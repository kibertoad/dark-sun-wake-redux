using System.Text;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class GffObjectFrameCatalogTests
{
    [Fact]
    public void ReadsRequestedDefinitionsAndCachesDecodedImages()
    {
        var archive = Archive(ObjectArchive(
            (11, ObjectFrame(0x12, -4, 17, 0x8000, 0x1234, 256, 5)),
            (10, ObjectFrame(2, 7, 63, 0xabcd, 4096, 0, 5))));

        var catalog = GffObjectFrameCatalog.Read(archive, [11, 10], "synthetic-objects.gff");

        Assert.Equal([10U, 11U], catalog.Entries.Select(entry => entry.ResourceNumber));
        Assert.Equal(new object[] { (ushort)2, (short)7, (short)63, (ushort)0xabcd,
            (ushort)4096, (ushort)0, 5U },
            new object[] { catalog.Entries[0].RawWord0, catalog.Entries[0].XOffset,
                catalog.Entries[0].YOffset, catalog.Entries[0].RawWord6,
                catalog.Entries[0].RawWord8, catalog.Entries[0].RawWord10,
                catalog.Entries[0].ImageResourceNumber });
        Assert.Single(catalog.Entries[0].Image.Frames);
        Assert.Same(catalog.Entries[0].Image, catalog.Entries[1].Image);
    }

    [Fact]
    public void ReadsCompleteCatalogWhenNoSubsetIsSpecified()
    {
        var archive = Archive(ObjectArchive(
            (21, ObjectFrame(0, 0, 0, 0, 0, 0, 6)),
            (10, ObjectFrame(0, 0, 0, 0, 0, 0, 5))));

        var catalog = GffObjectFrameCatalog.Read(archive, sourceName: "all.gff");

        Assert.Equal([10U, 21U], catalog.Entries.Select(entry => entry.ResourceNumber));
        Assert.Equal([5U, 6U], catalog.Entries.Select(entry => entry.ImageResourceNumber));
    }

    [Fact]
    public void RejectsMalformedRecordReservedWordAndMissingImage()
    {
        Assert.Contains("exactly 16 bytes", Error(ObjectArchive(
            (10, ObjectFrame(0, 0, 0, 0, 0, 0, 5)[..^1]))));

        var reserved = ObjectFrame(0, 0, 0, 0, 0, 0, 5);
        reserved[14] = 1;
        Assert.Contains("reserved word", Error(ObjectArchive((10, reserved))));

        Assert.Contains("missing BMP #7", Error(ObjectArchive(
            [(10, ObjectFrame(0, 0, 0, 0, 0, 0, 7))], [])));
    }

    [Fact]
    public void RejectsEmptyImageAndMissingRequestedDefinition()
    {
        var emptyImage = ObjectArchive(
            [(10, ObjectFrame(0, 0, 0, 0, 0, 0, 5))],
            [("BMP ", 5U, EmptyImage())]);
        Assert.Contains("empty BMP #5", Error(emptyImage));

        var archive = Archive(ObjectArchive(
            (10, ObjectFrame(0, 0, 0, 0, 0, 0, 5))));
        Assert.Contains("requested OJFF #11 does not exist",
            Assert.Throws<InvalidDataException>(() =>
                GffObjectFrameCatalog.Read(archive, [11], "missing.gff")).Message);
    }

    private static string Error(byte[] bytes)
    {
        var archive = Archive(bytes);
        return Assert.Throws<InvalidDataException>(() =>
            GffObjectFrameCatalog.Read(archive, sourceName: "bad.gff")).Message;
    }

    private static GffArchive Archive(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return GffArchive.Read(stream, "synthetic.gff");
    }

    private static byte[] ObjectArchive(
        params (uint Number, byte[] Bytes)[] objects) =>
        ObjectArchive(objects, objects.Select(item => ("BMP ",
            Number: (uint)BitConverter.ToUInt16(item.Bytes, 12), Bytes: TransparentImage())).ToArray());

    private static byte[] ObjectArchive(
        IReadOnlyList<(uint Number, byte[] Bytes)> objects,
        IReadOnlyList<(string Tag, uint Number, byte[] Bytes)> images)
    {
        return WriteArchive(objects.Select(item => ("OJFF", item.Number, item.Bytes))
            .Concat(images).GroupBy(item => (item.Item1, item.Item2))
            .Select(group => group.First()).ToArray());
    }

    private static byte[] ObjectFrame(
        ushort raw0, short x, short y, ushort raw6, ushort raw8, ushort raw10, ushort imageNumber)
    {
        var bytes = new byte[GffObjectFrameCatalog.RecordSize];
        BitConverter.GetBytes(raw0).CopyTo(bytes, 0);
        BitConverter.GetBytes(x).CopyTo(bytes, 2);
        BitConverter.GetBytes(y).CopyTo(bytes, 4);
        BitConverter.GetBytes(raw6).CopyTo(bytes, 6);
        BitConverter.GetBytes(raw8).CopyTo(bytes, 8);
        BitConverter.GetBytes(raw10).CopyTo(bytes, 10);
        BitConverter.GetBytes(imageNumber).CopyTo(bytes, 12);
        return bytes;
    }

    private static byte[] TransparentImage()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(15U);
        writer.Write((ushort)1);
        writer.Write(10U);
        writer.Write((ushort)16);
        writer.Write((ushort)16);
        writer.Write((byte)0xff);
        return stream.ToArray();
    }

    private static byte[] EmptyImage() => [6, 0, 0, 0, 0, 0];

    private static byte[] WriteArchive(params (string Tag, uint Number, byte[] Bytes)[] resources)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("GFFI"u8);
        writer.Write(0x0003_0000U);
        writer.Write((uint)GffArchive.HeaderSize);
        writer.Write(0U);
        writer.Write(new byte[12]);
        var offsets = new uint[resources.Length];
        for (var index = 0; index < resources.Length; index++)
        {
            offsets[index] = checked((uint)stream.Position);
            writer.Write(resources[index].Bytes);
        }
        var indexOffset = checked((uint)stream.Position);
        writer.Write(0U);
        writer.Write(0U);
        var groups = resources.Select((resource, index) => (resource, index))
            .GroupBy(item => item.resource.Tag).ToArray();
        writer.Write(checked((ushort)groups.Length));
        foreach (var group in groups)
        {
            writer.Write(Encoding.ASCII.GetBytes(group.Key));
            writer.Write(checked((uint)group.Count()));
            foreach (var item in group)
            {
                writer.Write(item.resource.Number);
                writer.Write(offsets[item.index]);
                writer.Write(checked((uint)item.resource.Bytes.Length));
            }
        }
        var bytes = stream.ToArray();
        BitConverter.GetBytes(indexOffset).CopyTo(bytes, 12);
        return bytes;
    }
}
