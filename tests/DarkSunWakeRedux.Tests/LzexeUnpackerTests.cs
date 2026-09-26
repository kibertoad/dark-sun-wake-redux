using System.Buffers.Binary;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class LzexeUnpackerTests
{
    // A 400-byte LZEXE 0.91 file: a 32-byte header, one paragraph of packed data that unpacks to
    // "ABABA" through two literals and one short back-reference, the packer's own header at
    // CS=0001, and a relocation table at CS:0158 holding one relocation, 0001:0000.
    private static byte[] SyntheticPackedFile()
    {
        var file = new byte[400];
        file[0] = (byte)'M';
        file[1] = (byte)'Z';
        Put(file, 0x02, 400);
        Put(file, 0x04, 1);
        Put(file, 0x08, 2);
        Put(file, 0x0A, 0x20);
        Put(file, 0x0C, 0xFFFF);
        Put(file, 0x14, 0x000E);
        Put(file, 0x16, 0x0001);
        Put(file, 0x18, 0x1C);
        "LZ91"u8.CopyTo(file.AsSpan(0x1C));
        byte[] packed = [0xA3, 0x00, (byte)'A', (byte)'B', 0xFE, 0x00, 0x00, 0x00];
        packed.CopyTo(file.AsSpan(32));
        Put(file, 48 + 0, 0x0003);
        Put(file, 48 + 2, 0x0000);
        Put(file, 48 + 4, 0x0100);
        Put(file, 48 + 6, 0x0001);
        Put(file, 48 + 8, 0x0001);
        byte[] relocations = [0x10, 0x00, 0x01, 0x00];
        relocations.CopyTo(file.AsSpan(48 + 0x158));
        return file;
    }

    private static void Put(byte[] bytes, int offset, ushort value) =>
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset), value);

    private static ushort Word(byte[] bytes, int offset) =>
        BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset));

    [Fact]
    public void RestoresTheLoadModuleEntryPointStackAndRelocations()
    {
        var output = LzexeUnpacker.Unpack(SyntheticPackedFile());

        Assert.Equal(37, output.Length);
        Assert.Equal("ABABA"u8.ToArray(), output[32..]);
        Assert.Equal((byte)'M', output[0]);
        Assert.Equal((byte)'Z', output[1]);
        Assert.Equal(37, Word(output, 0x02));
        Assert.Equal(1, Word(output, 0x04));
        Assert.Equal(1, Word(output, 0x06));
        Assert.Equal(2, Word(output, 0x08));
        Assert.Equal(0x0001, Word(output, 0x0E));
        Assert.Equal(0x0100, Word(output, 0x10));
        Assert.Equal(0x0003, Word(output, 0x14));
        Assert.Equal(0x0000, Word(output, 0x16));
        Assert.Equal(0x1C, Word(output, 0x18));
        Assert.Equal(0x0000, Word(output, 0x1C));
        Assert.Equal(0x0001, Word(output, 0x1E));
        Assert.Equal(0xFFFF, Word(output, 0x0C));
    }

    [Fact]
    public void RejectsAFileWithoutTheLzexe091Signature()
    {
        var file = SyntheticPackedFile();
        file[0x1F] = (byte)'0';

        Assert.Throws<InvalidDataException>(() => LzexeUnpacker.Unpack(file));
    }

    [Fact]
    public void RejectsABackReferenceBeforeTheLoadModule()
    {
        var file = SyntheticPackedFile();
        file[32 + 4] = 0xF0;

        Assert.Throws<InvalidDataException>(() => LzexeUnpacker.Unpack(file));
    }

    [Fact]
    public void RejectsPackedDataThatNeverEnds()
    {
        var file = SyntheticPackedFile();
        file[32 + 7] = 0x01;

        Assert.Throws<InvalidDataException>(() => LzexeUnpacker.Unpack(file));
    }
}
