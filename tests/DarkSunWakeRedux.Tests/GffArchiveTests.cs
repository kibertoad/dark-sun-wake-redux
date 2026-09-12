using System.Text;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class GffArchiveTests
{
    [Fact]
    public void ReadsPrimaryTablesAndResourceBytes()
    {
        using var stream = new MemoryStream(PrimaryArchive());

        var archive = GffArchive.Read(stream, "synthetic-primary.gff");

        Assert.Equal(2, archive.Resources.Count);
        Assert.Equal([0x41, 0x42, 0x43], archive.GetResource("TEXT", 7).ToArray());
    }

    [Fact]
    public void PrimaryOnlyArchiveDoesNotRequireGffiTable()
    {
        var bytes = PrimaryArchive();
        var indexOffset = BitConverter.ToInt32(bytes, 12);
        Encoding.ASCII.GetBytes("DATA").CopyTo(bytes, indexOffset + 10);
        using var stream = new MemoryStream(bytes);

        var archive = GffArchive.Read(stream, "primary-only.gff");

        Assert.Equal([9, 8, 7, 6], archive.GetResource("DATA", 0).ToArray());
    }

    [Fact]
    public void ExpandsSecondaryTableNumberingSegments()
    {
        using var stream = new MemoryStream(SecondaryArchive());

        var archive = GffArchive.Read(stream, "synthetic-secondary.gff");

        Assert.Equal([1, 2], archive.GetResource("TEXT", 10).ToArray());
        Assert.Equal([3, 4], archive.GetResource("TEXT", 11).ToArray());
    }

    [Fact]
    public void RejectsTruncatedIndexWithSourceContext()
    {
        var bytes = PrimaryArchive();
        using var stream = new MemoryStream(bytes[..^1]);

        var exception = Assert.Throws<InvalidDataException>(() =>
            GffArchive.Read(stream, "truncated.gff"));

        Assert.Contains("truncated.gff", exception.Message, StringComparison.Ordinal);
        Assert.Contains("truncated", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RejectsResourceOutsideFile()
    {
        var bytes = PrimaryArchive();
        var indexOffset = BitConverter.ToInt32(bytes, 12);
        var textOffsetField = indexOffset + 10 + 8 + 12 + 8 + 4;
        BitConverter.GetBytes(uint.MaxValue).CopyTo(bytes, textOffsetField);
        using var stream = new MemoryStream(bytes);

        var exception = Assert.Throws<InvalidDataException>(() =>
            GffArchive.Read(stream, "outside.gff"));

        Assert.Contains("extends beyond", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsPartiallyOverlappingResources()
    {
        var bytes = PrimaryArchive();
        var indexOffset = BitConverter.ToInt32(bytes, 12);
        var textOffsetField = indexOffset + 10 + 8 + 12 + 8 + 4;
        BitConverter.GetBytes(30U).CopyTo(bytes, textOffsetField);
        var textSizeField = textOffsetField + 4;
        BitConverter.GetBytes(4U).CopyTo(bytes, textSizeField);
        using var stream = new MemoryStream(bytes);

        var exception = Assert.Throws<InvalidDataException>(() =>
            GffArchive.Read(stream, "overlap.gff"));

        Assert.Contains("partially overlap", exception.Message, StringComparison.Ordinal);
    }

    private static byte[] PrimaryArchive()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        WriteHeader(writer, 35);
        writer.Write(new byte[] { 9, 8, 7, 6, 0x41, 0x42, 0x43 });
        WriteIndexPrefix(writer, 2);
        WriteTag(writer, "GFFI");
        writer.Write(1U);
        WritePrimaryEntry(writer, 0, 28, 4);
        WriteTag(writer, "TEXT");
        writer.Write(1U);
        WritePrimaryEntry(writer, 7, 32, 3);
        return stream.ToArray();
    }

    private static byte[] SecondaryArchive()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        WriteHeader(writer, 52);
        writer.Write(2U);
        writer.Write(48U);
        writer.Write(2U);
        writer.Write(50U);
        writer.Write(2U);
        writer.Write(new byte[] { 1, 2, 3, 4 });
        WriteIndexPrefix(writer, 2);
        WriteTag(writer, "GFFI");
        writer.Write(1U);
        WritePrimaryEntry(writer, 0, 28, 20);
        WriteTag(writer, "TEXT");
        writer.Write(0x8000_0002U);
        writer.Write(2U);
        writer.Write(0U);
        writer.Write(1U);
        writer.Write(10U);
        writer.Write(2U);
        return stream.ToArray();
    }

    private static void WriteHeader(BinaryWriter writer, uint indexOffset)
    {
        WriteTag(writer, "GFFI");
        writer.Write(0x0003_0000U);
        writer.Write((uint)GffArchive.HeaderSize);
        writer.Write(indexOffset);
        writer.Write(0U);
        writer.Write(0U);
        writer.Write(1U);
    }

    private static void WriteIndexPrefix(BinaryWriter writer, ushort tagCount)
    {
        writer.Write(8U);
        writer.Write(0U);
        writer.Write(tagCount);
    }

    private static void WritePrimaryEntry(BinaryWriter writer, uint number, uint offset, uint size)
    {
        writer.Write(number);
        writer.Write(offset);
        writer.Write(size);
    }

    private static void WriteTag(BinaryWriter writer, string tag) =>
        writer.Write(Encoding.ASCII.GetBytes(tag));
}
