using System.Buffers.Binary;
using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record IndexedImageFrame(int Width, int Height, byte[] Pixels, byte[] Alpha);

public sealed record IndexedImage(IReadOnlyList<IndexedImageFrame> Frames)
{
    public const int MaximumPayloadBytes = 64 * 1024 * 1024;
    public const int MaximumFrameCount = 4_096;
    public const int MaximumDimension = 4_096;
    public const int MaximumPixelsPerFrame = 16 * 1024 * 1024;
    public const long MaximumPixelsPerImage = 64L * 1024 * 1024;

    public static IndexedImage Read(ReadOnlyMemory<byte> payload, string sourceName = "image resource")
    {
        if (payload.Length < 6) throw Error(sourceName, "is shorter than the image header");
        if (payload.Length > MaximumPayloadBytes)
            throw Error(sourceName, $"exceeds the {MaximumPayloadBytes}-byte safety limit");
        var bytes = payload.ToArray();
        var reader = new Reader(bytes, sourceName);
        var declaredSize = reader.UInt32(0, "declared image size");
        if (declaredSize != bytes.Length)
            throw Error(sourceName, $"declares size {declaredSize}, but contains {bytes.Length} bytes");
        var frameCount = reader.UInt16(4, "frame count");
        if (frameCount > MaximumFrameCount)
            throw Error(sourceName, $"declares {frameCount} frames, exceeding the safety limit");
        var tableEnd = checked(6 + frameCount * 4);
        reader.Ensure(0, tableEnd, "frame-offset table");

        var offsets = new int[frameCount];
        for (var index = 0; index < frameCount; index++)
        {
            var offset = reader.UInt32(6 + index * 4, "frame offset");
            if (offset < tableEnd || offset >= bytes.Length)
                throw Error(sourceName, $"frame {index} has out-of-range offset {offset}");
            offsets[index] = checked((int)offset);
            if (index > 0 && offsets[index] <= offsets[index - 1])
                throw Error(sourceName, "frame offsets are not strictly increasing");
        }

        var frames = new List<IndexedImageFrame>(frameCount);
        var totalPixels = 0L;
        for (var index = 0; index < frameCount; index++)
        {
            var end = index + 1 < frameCount ? offsets[index + 1] : bytes.Length;
            var frame = ReadFrame(reader, offsets[index], end, index);
            totalPixels += (long)frame.Width * frame.Height;
            if (totalPixels > MaximumPixelsPerImage)
                throw Error(sourceName, "decoded pixel count exceeds the per-image safety limit");
            frames.Add(frame);
        }
        return new(frames);
    }

    private static IndexedImageFrame ReadFrame(Reader reader, int start, int end, int frameIndex)
    {
        reader.Ensure(start, 4, $"frame {frameIndex} dimensions", end);
        var width = reader.UInt16(start, "frame width");
        var height = reader.UInt16(start + 2, "frame height");
        if (width > MaximumDimension || height > MaximumDimension || (long)width * height > MaximumPixelsPerFrame)
            throw Error(reader.SourceName, $"frame {frameIndex} dimensions {width}x{height} exceed safety limits");

        if (start + 9 <= end && reader.Byte(start + 4, "frame encoding marker") == 0xff)
        {
            var tag = reader.Ascii(start + 5, 4, "frame encoding tag");
            if (tag == "PLAN") return ReadPlanar(reader, start, end, width, height, false, frameIndex);
            if (tag == "PLNR") return ReadPlanar(reader, start, end, width, height, true, frameIndex);
        }
        return ReadRows(reader, start, end, width, height, frameIndex);
    }

    private static IndexedImageFrame ReadRows(
        Reader reader, int start, int end, int width, int height, int frameIndex)
    {
        var pixels = new byte[checked(width * height)];
        var alpha = new byte[pixels.Length];
        var seenRows = new HashSet<int>();
        var position = start + 4;
        while (seenRows.Count < height)
        {
            reader.Ensure(position, 1, $"frame {frameIndex} row number", end);
            var row = reader.Byte(position++, "row number");
            if (row == 0xff) break;
            if (row >= height)
                throw Error(reader.SourceName, $"frame {frameIndex} row {row} exceeds height {height}");
            if (!seenRows.Add(row))
                throw Error(reader.SourceName, $"frame {frameIndex} repeats row {row}");

            var lastRun = false;
            while (!lastRun)
            {
                reader.Ensure(position, 4, $"frame {frameIndex} row-run header", end);
                var startX = (int)reader.Byte(position++, "run start");
                var flags = reader.Byte(position++, "run flags");
                if ((flags & 0x01) != 0) startX += 256;
                lastRun = (flags & 0x80) != 0;
                var decodedLength = reader.Byte(position++, "run decoded length");
                var encodedLength = reader.Byte(position++, "run encoded length");
                reader.Ensure(position, encodedLength, $"frame {frameIndex} compressed pixels", end);
                if (startX + decodedLength > width)
                    throw Error(reader.SourceName, $"frame {frameIndex} row {row} run exceeds width {width}");
                DecodeRowRun(reader.Bytes.AsSpan(position, encodedLength),
                    pixels.AsSpan(row * width + startX, decodedLength), reader.SourceName, frameIndex);
                alpha.AsSpan(row * width + startX, decodedLength).Fill(255);
                position += encodedLength;
            }
        }
        return new(width, height, pixels, alpha);
    }

    private static void DecodeRowRun(
        ReadOnlySpan<byte> encoded, Span<byte> output, string sourceName, int frameIndex)
    {
        var inputPosition = 0;
        var outputPosition = 0;
        while (inputPosition < encoded.Length)
        {
            var code = encoded[inputPosition++];
            var runLength = code / 2 + 1;
            if (outputPosition + runLength > output.Length)
                throw Error(sourceName, $"frame {frameIndex} row run expands beyond its declared length");
            if ((code & 1) == 0)
            {
                if (inputPosition + runLength > encoded.Length)
                    throw Error(sourceName, $"frame {frameIndex} literal row run is truncated");
                encoded.Slice(inputPosition, runLength).CopyTo(output[outputPosition..]);
                inputPosition += runLength;
            }
            else
            {
                if (inputPosition >= encoded.Length)
                    throw Error(sourceName, $"frame {frameIndex} repeated row run is truncated");
                output.Slice(outputPosition, runLength).Fill(encoded[inputPosition++]);
            }
            outputPosition += runLength;
        }
        if (outputPosition != output.Length)
            throw Error(sourceName, $"frame {frameIndex} row run decoded {outputPosition} bytes, expected {output.Length}");
    }

    private static IndexedImageFrame ReadPlanar(
        Reader reader, int start, int end, int width, int height, bool repeatSymbols, int frameIndex)
    {
        reader.Ensure(start, 10, $"frame {frameIndex} planar header", end);
        var bitsPerSymbol = reader.Byte(start + 9, "bits per symbol");
        var pixels = new byte[checked(width * height)];
        var alpha = new byte[pixels.Length];
        // PLACEHOLDER: RULE-IMAGE-002 - what the original draws for a 0-bit frame is unknown.
        if (bitsPerSymbol == 0) return new(width, height, pixels, alpha);
        if (bitsPerSymbol > 8)
            throw Error(reader.SourceName, $"frame {frameIndex} uses invalid {bitsPerSymbol}-bit symbols");
        var dictionarySize = 1 << bitsPerSymbol;
        var dictionaryStart = start + 10;
        reader.Ensure(dictionaryStart, dictionarySize, $"frame {frameIndex} palette-index dictionary", end);
        var bitReader = new BitReader(reader.Bytes, dictionaryStart + dictionarySize, end, reader.SourceName, frameIndex);
        var lastSymbol = 0;
        var remaining = 0;
        for (var pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
        {
            int symbol;
            if (!repeatSymbols)
            {
                symbol = bitReader.Read(bitsPerSymbol);
            }
            else
            {
                if (remaining == 0)
                {
                    var first = bitReader.Read(bitsPerSymbol);
                    if (first == 0)
                    {
                        var second = bitReader.Read(bitsPerSymbol);
                        if (second == 0)
                        {
                            lastSymbol = 0;
                            remaining = 1;
                        }
                        else
                        {
                            remaining = second + 2;
                        }
                    }
                    else
                    {
                        lastSymbol = first;
                        remaining = 1;
                    }
                }
                symbol = lastSymbol;
                remaining--;
            }
            var value = reader.Byte(dictionaryStart + symbol, "palette-index dictionary value");
            pixels[pixelIndex] = value;
            if (value != 0) alpha[pixelIndex] = 255;
        }
        return new(width, height, pixels, alpha);
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");

    private sealed class Reader(byte[] bytes, string sourceName)
    {
        public byte[] Bytes => bytes;
        public string SourceName => sourceName;

        public byte Byte(int offset, string field)
        {
            Ensure(offset, 1, field);
            return bytes[offset];
        }

        public ushort UInt16(int offset, string field)
        {
            Ensure(offset, 2, field);
            return BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset, 2));
        }

        public uint UInt32(int offset, string field)
        {
            Ensure(offset, 4, field);
            return BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset, 4));
        }

        public string Ascii(int offset, int length, string field)
        {
            Ensure(offset, length, field);
            return Encoding.ASCII.GetString(bytes, offset, length);
        }

        public void Ensure(int offset, int length, string field, int? exclusiveEnd = null)
        {
            var end = exclusiveEnd ?? bytes.Length;
            if (offset < 0 || length < 0 || (long)offset + length > end || end > bytes.Length)
                throw Error(sourceName, $"is truncated while reading {field} at offset {offset}");
        }
    }

    private sealed class BitReader(
        byte[] bytes, int byteOffset, int exclusiveEnd, string sourceName, int frameIndex)
    {
        private int _bitOffset;

        public int Read(int bitCount)
        {
            var result = 0;
            for (var index = 0; index < bitCount; index++)
            {
                var position = byteOffset + _bitOffset / 8;
                if (position >= exclusiveEnd)
                    throw Error(sourceName, $"frame {frameIndex} planar bitstream is truncated");
                var bitInByte = _bitOffset % 8;
                result = (result << 1) | ((bytes[position] >> (7 - bitInByte)) & 1);
                _bitOffset++;
            }
            return result;
        }
    }
}
