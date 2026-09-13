using System.Text;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class GffRegionTests
{
    [Fact]
    public void ReadsBoundedRegionPlanesTilesAndEntityReferences()
    {
        var region = GffRegion.Read(
            Archive(RegionArchive()), Archive(ObjectArchive()), "synthetic-region.gff");

        Assert.Equal(50U, region.ResourceNumber);
        Assert.Equal("Tyr", region.Name);
        Assert.Equal(GffRegion.MapByteCount, region.TileMap.Length);
        Assert.Equal(2, region.Tiles.Count);
        Assert.All(region.Tiles.Values,
            tile => Assert.Equal((16, 16), (tile.Width, tile.Height)));
        Assert.Equal([new RegionEntityReference(24, 40, -3, 0x80, 10),
            new RegionEntityReference(72, 88, 4, 0x21, -11)], region.Entities);
        Assert.Equal([10U, 11U], region.Entities.Select(entity => entity.ObjectResourceNumber));
    }

    [Fact]
    public void RejectsMalformedNameAndPlaneLengths()
    {
        var objects = Archive(ObjectArchive());

        Assert.Contains("NUL-terminated", Assert.Throws<InvalidDataException>(() =>
            GffRegion.Read(Archive(RegionArchive(name: "Tyr!")), objects, "bad-name.gff")).Message);
        Assert.Contains("12544 bytes", Assert.Throws<InvalidDataException>(() =>
            GffRegion.Read(Archive(RegionArchive(mapLength: GffRegion.MapByteCount - 1)),
                objects, "bad-map.gff")).Message);
    }

    [Fact]
    public void RejectsMissingTileAndObjectReferences()
    {
        Assert.Contains("missing TILE #3", Assert.Throws<InvalidDataException>(() =>
            GffRegion.Read(Archive(RegionArchive(mapValue: 3)), Archive(ObjectArchive()),
                "missing-tile.gff")).Message);
        Assert.Contains("missing OJFF #11", Assert.Throws<InvalidDataException>(() =>
            GffRegion.Read(Archive(RegionArchive()), Archive(ObjectArchive(includeEleven: false)),
                "missing-object.gff")).Message);
    }

    [Fact]
    public void RejectsPartialEntityRecordAndNonTileImage()
    {
        var partial = EntityBytes()[..^1];
        Assert.Contains("8-byte records", Assert.Throws<InvalidDataException>(() =>
            GffRegion.Read(Archive(RegionArchive(entityBytes: partial)), Archive(ObjectArchive()),
                "partial-etab.gff")).Message);

        Assert.Contains("16x16", Assert.Throws<InvalidDataException>(() =>
            GffRegion.Read(Archive(RegionArchive(tileWidth: 15)), Archive(ObjectArchive()),
                "bad-tile.gff")).Message);
    }

    private static GffArchive Archive(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return GffArchive.Read(stream, "synthetic.gff");
    }

    internal static byte[] RegionArchive(
        string name = "Tyr\0",
        int mapLength = GffRegion.MapByteCount,
        byte mapValue = 2,
        byte[]? entityBytes = null,
        ushort tileWidth = 16)
    {
        var map = Enumerable.Repeat(mapValue, mapLength).ToArray();
        if (map.Length > 1) map[^1] = 7;
        return WriteArchive(
            ("RNME", 50, Encoding.ASCII.GetBytes(name)),
            ("PAL ", 50, new byte[IndexedPalette.EncodedLength]),
            ("MAP ", 50, map),
            ("GMAP", 50, new byte[GffRegion.MapByteCount]),
            ("ETAB", 50, entityBytes ?? EntityBytes()),
            ("TILE", 2, TileImage(tileWidth)),
            ("TILE", 7, TileImage(16)));
    }

    internal static byte[] ObjectArchive(bool includeEleven = true)
    {
        var resources = new List<(string Tag, uint Number, byte[] Bytes)>
        {
            ("OJFF", 10, ObjectFrame(5)),
            ("BMP ", 5, TileImage(16))
        };
        if (includeEleven) resources.Add(("OJFF", 11, ObjectFrame(5)));
        return WriteArchive(resources.ToArray());
    }

    private static byte[] ObjectFrame(ushort imageNumber)
    {
        var bytes = new byte[GffObjectFrameCatalog.RecordSize];
        BitConverter.GetBytes(imageNumber).CopyTo(bytes, 12);
        return bytes;
    }

    private static byte[] EntityBytes()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        WriteEntity(writer, 24, 40, -3, 0x80, 10);
        WriteEntity(writer, 72, 88, 4, 0x21, -11);
        return stream.ToArray();
    }

    private static void WriteEntity(
        BinaryWriter writer, short x, short y, sbyte verticalOffset, byte flags, short objectNumber)
    {
        writer.Write(x);
        writer.Write(y);
        writer.Write(verticalOffset);
        writer.Write(flags);
        writer.Write(objectNumber);
    }

    private static byte[] TileImage(ushort width)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(15U);
        writer.Write((ushort)1);
        writer.Write(10U);
        writer.Write(width);
        writer.Write((ushort)16);
        writer.Write((byte)0xff);
        return stream.ToArray();
    }

    private static byte[] WriteArchive(params (string Tag, uint Number, byte[] Bytes)[] resources)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        WriteTag(writer, "GFFI");
        writer.Write(0x0003_0000U);
        writer.Write((uint)GffArchive.HeaderSize);
        writer.Write(0U);
        writer.Write(0U);
        writer.Write(0U);
        writer.Write(1U);

        var offsets = new uint[resources.Length];
        for (var index = 0; index < resources.Length; index++)
        {
            offsets[index] = checked((uint)stream.Position);
            writer.Write(resources[index].Bytes);
        }
        var indexOffset = checked((uint)stream.Position);
        writer.Write(8U);
        writer.Write(0U);
        var groups = resources.Select((resource, index) => (resource, index))
            .GroupBy(item => item.resource.Tag).ToArray();
        writer.Write(checked((ushort)groups.Length));
        foreach (var group in groups)
        {
            WriteTag(writer, group.Key);
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

    private static void WriteTag(BinaryWriter writer, string tag) =>
        writer.Write(Encoding.ASCII.GetBytes(tag));
}
