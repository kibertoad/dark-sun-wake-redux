using System.Buffers.Binary;

namespace DarkSunWakeRedux.Resources;

/// <summary>
/// Restores the MZ executable an LZEXE 0.91 compressor packed, so that static analysis can read
/// the program at the addresses it runs at. The output keeps the packed file's load module, entry
/// point, stack and relocations, and gives it a fresh header: the signature, the sizes, the
/// relocation count and a header of whole paragraphs, the memory the packed file asked for less
/// the load module's growth, the packed file's maximum allocation, SS:SP, a zero checksum, CS:IP,
/// the relocation table at 0x1C, overlay 0, then the relocations and zero padding to the next
/// paragraph. Addresses in the load module do not depend on that header.
/// </summary>
public static class LzexeUnpacker
{
    public const long MaximumPackedBytes = 1024 * 1024;
    public const long MaximumLoadModuleBytes = 1024 * 1024;
    public const int MaximumRelocations = 16_384;
    private const int HeaderLength = 0x1C;
    private const int PackerHeaderLength = 16;
    private const int RelocationTableOffset = 0x158;

    public static bool IsLzexe091(ReadOnlySpan<byte> file) =>
        file.Length >= 0x20 && file[0] == (byte)'M' && file[1] == (byte)'Z' &&
        file[0x1C] == (byte)'L' && file[0x1D] == (byte)'Z' && file[0x1E] == (byte)'9' && file[0x1F] == (byte)'1';

    public static byte[] Unpack(ReadOnlySpan<byte> file, string sourceName = "executable")
    {
        if (file.Length > MaximumPackedBytes)
            throw Error(sourceName, $"exceeds the {MaximumPackedBytes}-byte limit");
        if (!IsLzexe091(file))
            throw Error(sourceName, "is not an MZ executable packed by LZEXE 0.91");

        var headerParagraphs = Word(file, 0x08);
        var minimumAllocation = Word(file, 0x0A);
        var maximumAllocation = Word(file, 0x0C);
        var packerSegment = Word(file, 0x16);
        var finalPageBytes = Word(file, 0x02);
        var pageCount = Word(file, 0x04);
        var mzLength = finalPageBytes == 0 ? pageCount * 512L : (pageCount - 1L) * 512 + finalPageBytes;
        if (pageCount == 0 || finalPageBytes > 512 || mzLength > file.Length)
            throw Error(sourceName, "has invalid MZ page counts");

        var packerHeaderOffset = ((long)headerParagraphs + packerSegment) * 16;
        Require(packerHeaderOffset, PackerHeaderLength, mzLength, sourceName, "packer header");
        var packerHeader = file.Slice((int)packerHeaderOffset, PackerHeaderLength);
        var entryIp = Word(packerHeader, 0);
        var entryCs = Word(packerHeader, 2);
        var stackSp = Word(packerHeader, 4);
        var stackSs = Word(packerHeader, 6);
        var packedParagraphs = Word(packerHeader, 8);
        if (packedParagraphs > packerSegment)
            throw Error(sourceName, "declares more packed data than precedes its decompressor");

        var packedOffset = ((long)headerParagraphs + packerSegment - packedParagraphs) * 16;
        var loadModule = Decompress(file[(int)packedOffset..(int)packerHeaderOffset], sourceName);
        var relocations = ReadRelocations(file, packerHeaderOffset + RelocationTableOffset, mzLength, sourceName);

        var headerBytes = Align16(HeaderLength + relocations.Count * 4L);
        var outputLength = headerBytes + loadModule.Length;
        var packedMemory = (mzLength - (long)headerParagraphs * 16 + 15) / 16 + minimumAllocation;
        var outputMemory = ((long)loadModule.Length + 15) / 16;
        var outputMinimum = (ushort)Math.Clamp(packedMemory - outputMemory, 0, ushort.MaxValue);

        var output = new byte[outputLength];
        output[0] = (byte)'M';
        output[1] = (byte)'Z';
        Put(output, 0x02, (ushort)(outputLength % 512));
        Put(output, 0x04, (ushort)((outputLength + 511) / 512));
        Put(output, 0x06, (ushort)relocations.Count);
        Put(output, 0x08, (ushort)(headerBytes / 16));
        Put(output, 0x0A, outputMinimum);
        Put(output, 0x0C, maximumAllocation);
        Put(output, 0x0E, stackSs);
        Put(output, 0x10, stackSp);
        Put(output, 0x14, entryIp);
        Put(output, 0x16, entryCs);
        Put(output, 0x18, HeaderLength);
        for (var index = 0; index < relocations.Count; index++)
        {
            Put(output, HeaderLength + index * 4, relocations[index].Offset);
            Put(output, HeaderLength + index * 4 + 2, relocations[index].Segment);
        }
        loadModule.CopyTo(output.AsSpan((int)headerBytes));
        return output;
    }

    private static byte[] Decompress(ReadOnlySpan<byte> packedSpan, string sourceName)
    {
        var packed = packedSpan.ToArray();
        var output = new List<byte>();
        var position = 0;
        byte ReadByte()
        {
            if (position >= packed.Length) throw Error(sourceName, "ends inside its packed load module");
            return packed[position++];
        }
        ushort ReadWord()
        {
            var low = ReadByte();
            return (ushort)(low | (ReadByte() << 8));
        }

        var bits = ReadWord();
        var bitsLeft = 16;
        int ReadBit()
        {
            var bit = bits & 1;
            if (--bitsLeft == 0)
            {
                bits = ReadWord();
                bitsLeft = 16;
            }
            else
            {
                bits >>= 1;
            }
            return bit;
        }

        while (true)
        {
            if (ReadBit() == 1)
            {
                if (output.Count >= MaximumLoadModuleBytes)
                    throw Error(sourceName, $"unpacks beyond the {MaximumLoadModuleBytes}-byte limit");
                output.Add(ReadByte());
                continue;
            }

            int length;
            int span;
            if (ReadBit() == 0)
            {
                length = (ReadBit() << 1) | ReadBit();
                length += 2;
                span = (short)(ReadByte() | 0xFF00);
            }
            else
            {
                var low = ReadByte();
                var high = ReadByte();
                span = (short)(low | ((high & ~0x07) << 5) | 0xE000);
                length = (high & 0x07) + 2;
                if (length == 2)
                {
                    length = ReadByte();
                    if (length == 0) break;
                    if (length == 1) continue;
                    length++;
                }
            }

            if (output.Count + span < 0) throw Error(sourceName, "copies from before its load module");
            if (output.Count + length > MaximumLoadModuleBytes)
                throw Error(sourceName, $"unpacks beyond the {MaximumLoadModuleBytes}-byte limit");
            for (; length > 0; length--) output.Add(output[output.Count + span]);
        }
        return output.ToArray();
    }

    private static List<(ushort Offset, ushort Segment)> ReadRelocations(
        ReadOnlySpan<byte> fileSpan, long offset, long limit, string sourceName)
    {
        var file = fileSpan.ToArray();
        var relocations = new List<(ushort, ushort)>();
        var position = offset;
        byte ReadByte(byte[] bytes)
        {
            if (position >= limit) throw Error(sourceName, "ends inside its relocation table");
            return bytes[(int)position++];
        }

        var relocationOffset = 0;
        var relocationSegment = 0;
        while (true)
        {
            int span = ReadByte(file);
            if (span == 0)
            {
                span = ReadByte(file) | (ReadByte(file) << 8);
                if (span == 0)
                {
                    relocationSegment += 0x0FFF;
                    continue;
                }
                if (span == 1) break;
            }
            relocationOffset += span;
            relocationSegment += (relocationOffset & ~0x0F) >> 4;
            relocationOffset &= 0x0F;
            if (relocationSegment > ushort.MaxValue) throw Error(sourceName, "has a relocation beyond segment FFFF");
            if (relocations.Count == MaximumRelocations)
                throw Error(sourceName, $"has more than {MaximumRelocations} relocations");
            relocations.Add(((ushort)relocationOffset, (ushort)relocationSegment));
        }
        return relocations;
    }

    private static long Align16(long value) => (value + 15) / 16 * 16;

    private static ushort Word(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);

    private static void Put(byte[] bytes, int offset, ushort value) =>
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset), value);

    private static void Require(long offset, long length, long limit, string sourceName, string part)
    {
        if (offset < 0 || length < 0 || offset > limit - length)
            throw Error(sourceName, $"has its {part} outside the file");
    }

    private static InvalidDataException Error(string sourceName, string message) =>
        new($"{sourceName} {message}.");
}
