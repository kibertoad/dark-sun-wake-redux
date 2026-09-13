using System.Text;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class GffCharacterCatalogTests
{
    [Fact]
    public void CorrelatesSameNumberResourcesInCharacterNumberOrder()
    {
        using var stream = new MemoryStream(Archive(
            ("PSIN", 7, new byte[] { 4 }),
            ("CHAR", 7, Character("Scholar")),
            ("CHAR", 3, Character("Hero")),
            ("PSIN", 3, new byte[] { 7 })));

        var catalog = GffCharacterCatalog.Read(GffArchive.Read(stream, "synthetic.gff"),
            "synthetic.gff");

        Assert.Equal([3U, 7U], catalog.Select(item => item.ResourceNumber));
        Assert.All(catalog, item => Assert.Equal(0, item.TailRecordCount));
        Assert.Equal(["Hero", "Scholar"], catalog.Select(item => item.Name));
        Assert.All(catalog, item => Assert.Equal(15, item.AbilityScores.Strength));
        Assert.Equal([7, 4], catalog.Select(item => item.RawPsionicMask));
    }

    [Fact]
    public void RejectsCharacterWithoutSelectionCompanion()
    {
        using var stream = new MemoryStream(Archive(
            ("CHAR", 3, Character("Hero"))));

        var exception = Assert.Throws<InvalidDataException>(() =>
            GffCharacterCatalog.Read(GffArchive.Read(stream, "synthetic.gff"),
                "synthetic.gff"));

        Assert.Contains("CHAR #3", exception.Message);
        Assert.Contains("no matching PSIN", exception.Message);
    }

    [Fact]
    public void RejectsSelectionWithoutCharacterCompanion()
    {
        using var stream = new MemoryStream(Archive(
            ("PSIN", 9, new byte[] { 1 })));

        var exception = Assert.Throws<InvalidDataException>(() =>
            GffCharacterCatalog.Read(GffArchive.Read(stream, "synthetic.gff"),
                "synthetic.gff"));

        Assert.Contains("PSIN resources", exception.Message);
        Assert.Contains("9", exception.Message);
    }

    private static byte[] Character(string name)
    {
        var data = new byte[GffCharacterRecordEnvelope.FixedHeaderSize];
        data[0] = GffCharacterRecordEnvelope.SupportedVersion;
        Array.Fill(data, (byte)15, GffCharacterAbilityScores.ScoresOffset,
            GffCharacterAbilityScores.ScoreCount);
        Encoding.ASCII.GetBytes(name).CopyTo(data, GffCharacterIdentity.NameOffset);
        return data;
    }

    private static byte[] Archive(params (string Tag, uint Number, byte[] Data)[] resources)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        var indexOffset = GffArchive.HeaderSize + resources.Sum(resource => resource.Data.Length);
        WriteTag(writer, "GFFI");
        writer.Write(0x0003_0000U);
        writer.Write((uint)GffArchive.HeaderSize);
        writer.Write((uint)indexOffset);
        writer.Write(0U);
        writer.Write(0U);
        writer.Write(1U);

        foreach (var resource in resources) writer.Write(resource.Data);
        writer.Write(8U);
        writer.Write(0U);
        var groups = resources.GroupBy(resource => resource.Tag).OrderBy(group => group.Key).ToArray();
        writer.Write(checked((ushort)groups.Length));
        var offset = GffArchive.HeaderSize;
        var offsets = resources.Select(resource =>
        {
            var current = offset;
            offset += resource.Data.Length;
            return current;
        }).ToArray();
        foreach (var group in groups)
        {
            WriteTag(writer, group.Key);
            var entries = group.Select(item => (Item: item, Index: Array.IndexOf(resources, item))).ToArray();
            writer.Write((uint)entries.Length);
            foreach (var entry in entries)
            {
                writer.Write(entry.Item.Number);
                writer.Write((uint)offsets[entry.Index]);
                writer.Write((uint)entry.Item.Data.Length);
            }
        }
        return stream.ToArray();
    }

    private static void WriteTag(BinaryWriter writer, string tag) =>
        writer.Write(Encoding.ASCII.GetBytes(tag));
}
