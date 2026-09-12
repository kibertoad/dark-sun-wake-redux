using System.Buffers.Binary;
using System.Text;

namespace DarkSunWakeRedux.Resources;

public sealed record PackedIndexedImage(
    IReadOnlyList<Rgb24> Palette,
    IReadOnlyList<IndexedImageFrame> Frames)
{
    public const ushort FormatVersion = 1;
    public const long MaximumFileBytes = 160L * 1024 * 1024;
    private const int HeaderBytes = 8;
    private const int PaletteBytes = IndexedPalette.ColorCount * 3;

    public static PackedIndexedImage From(IndexedImage image, IndexedPalette palette) =>
        new(palette.Colors, image.Frames);

    public void Write(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite) throw new ArgumentException("Pack image stream must be writable.", nameof(stream));
        Validate(Palette, Frames, "pack image");
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("DSIX"u8);
        writer.Write(FormatVersion);
        writer.Write(checked((ushort)Frames.Count));
        foreach (var color in Palette)
        {
            writer.Write(color.Red);
            writer.Write(color.Green);
            writer.Write(color.Blue);
        }
        foreach (var frame in Frames)
        {
            writer.Write(checked((ushort)frame.Width));
            writer.Write(checked((ushort)frame.Height));
            writer.Write(checked((uint)frame.Pixels.Length));
            writer.Write(frame.Pixels);
            writer.Write(frame.Alpha);
        }
    }

    public static PackedIndexedImage Read(Stream stream, string sourceName = "pack image")
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw Error(sourceName, "must be a readable, seekable stream");
        if (stream.Length < HeaderBytes + PaletteBytes)
            throw Error(sourceName, "is shorter than the header and palette");
        if (stream.Length > MaximumFileBytes)
            throw Error(sourceName, $"exceeds the {MaximumFileBytes}-byte safety limit");
        stream.Position = 0;
        var bytes = new byte[checked((int)stream.Length)];
        stream.ReadExactly(bytes);
        if (!bytes.AsSpan(0, 4).SequenceEqual("DSIX"u8))
            throw Error(sourceName, "has an invalid signature");
        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4, 2));
        if (version != FormatVersion)
            throw Error(sourceName, $"uses unsupported format version {version}");
        var frameCount = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(6, 2));
        if (frameCount is 0 or > IndexedImage.MaximumFrameCount)
            throw Error(sourceName, $"declares invalid frame count {frameCount}");

        var position = HeaderBytes;
        var palette = new Rgb24[IndexedPalette.ColorCount];
        for (var index = 0; index < palette.Length; index++)
        {
            palette[index] = new(bytes[position], bytes[position + 1], bytes[position + 2]);
            position += 3;
        }

        var frames = new IndexedImageFrame[frameCount];
        var totalPixels = 0L;
        for (var index = 0; index < frameCount; index++)
        {
            Ensure(bytes, position, 8, sourceName, $"frame {index} header");
            var width = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(position, 2));
            var height = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(position + 2, 2));
            var pixelCount = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(position + 4, 4));
            position += 8;
            ValidateDimensions(width, height, pixelCount, ref totalPixels, sourceName, index);
            var count = checked((int)pixelCount);
            Ensure(bytes, position, checked(count * 2), sourceName, $"frame {index} pixels and alpha");
            var pixels = bytes.AsSpan(position, count).ToArray();
            position += count;
            var alpha = bytes.AsSpan(position, count).ToArray();
            position += count;
            if (alpha.Any(value => value is not 0 and not 255))
                throw Error(sourceName, $"frame {index} contains non-binary alpha");
            frames[index] = new(width, height, pixels, alpha);
        }
        if (position != bytes.Length) throw Error(sourceName, "contains trailing bytes");
        Validate(palette, frames, sourceName);
        return new(palette, frames);
    }

    private static void Validate(
        IReadOnlyList<Rgb24> palette,
        IReadOnlyList<IndexedImageFrame> frames,
        string sourceName)
    {
        if (palette.Count != IndexedPalette.ColorCount)
            throw Error(sourceName, $"must contain {IndexedPalette.ColorCount} palette colors");
        if (frames.Count is 0 or > IndexedImage.MaximumFrameCount)
            throw Error(sourceName, $"contains invalid frame count {frames.Count}");
        var totalPixels = 0L;
        for (var index = 0; index < frames.Count; index++)
        {
            var frame = frames[index];
            var pixelCount = frame.Pixels?.Length ?? 0;
            ValidateDimensions(frame.Width, frame.Height, checked((uint)pixelCount),
                ref totalPixels, sourceName, index);
            if (frame.Alpha is null || frame.Alpha.Length != pixelCount)
                throw Error(sourceName, $"frame {index} pixel and alpha lengths differ");
            if (frame.Alpha.Any(value => value is not 0 and not 255))
                throw Error(sourceName, $"frame {index} contains non-binary alpha");
        }
    }

    private static void ValidateDimensions(
        int width, int height, uint pixelCount, ref long totalPixels, string sourceName, int frameIndex)
    {
        if (width is <= 0 or > IndexedImage.MaximumDimension ||
            height is <= 0 or > IndexedImage.MaximumDimension ||
            (long)width * height > IndexedImage.MaximumPixelsPerFrame)
            throw Error(sourceName, $"frame {frameIndex} has invalid dimensions {width}x{height}");
        var expected = checked((uint)(width * height));
        if (pixelCount != expected)
            throw Error(sourceName, $"frame {frameIndex} declares {pixelCount} pixels, expected {expected}");
        totalPixels += pixelCount;
        if (totalPixels > IndexedImage.MaximumPixelsPerImage)
            throw Error(sourceName, "exceeds the decoded pixel safety limit");
    }

    private static void Ensure(byte[] bytes, int offset, int length, string sourceName, string field)
    {
        if (offset < 0 || length < 0 || (long)offset + length > bytes.Length)
            throw Error(sourceName, $"is truncated while reading {field}");
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName}: {message}.");
}
