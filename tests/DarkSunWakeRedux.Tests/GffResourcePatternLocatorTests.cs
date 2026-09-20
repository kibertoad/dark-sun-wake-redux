using System.Text;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class GffResourcePatternLocatorTests
{
    [Fact]
    public void FindsFirstOccurrenceInEachResourceWithDescriptorMetadataOnly()
    {
        var archive = Archive(
            ("DATA", 8U, "needle before needle"),
            ("TEXT", 3U, "not here"),
            ("TEXT", 4U, "xxneedle"));

        var matches = GffResourcePatternLocator.FindAscii(archive, "needle");

        Assert.Equal(
        [
            new GffResourcePatternMatch("DATA", 8, 20, 0),
            new GffResourcePatternMatch("TEXT", 4, 8, 2)
        ], matches);
    }

    [Theory]
    [InlineData("")]
    [InlineData("has\nnewline")]
    public void RejectsBlankOrNonPrintablePatterns(string pattern)
    {
        var archive = Archive(("DATA", 1U, "contents"));

        Assert.Throws<ArgumentException>(() =>
            GffResourcePatternLocator.FindAscii(archive, pattern));
    }

    [Fact]
    public void RejectsPatternPastSafetyLimit()
    {
        var archive = Archive(("DATA", 1U, "contents"));

        Assert.Throws<ArgumentException>(() => GffResourcePatternLocator.FindAscii(
            archive, new string('x', GffResourcePatternLocator.MaximumPatternLength + 1)));
    }

    private static GffArchive Archive(params (string Tag, uint Number, string Text)[] resources)
    {
        var payloads = resources.Select(resource => Encoding.ASCII.GetBytes(resource.Text)).ToArray();
        var indexOffset = checked(GffArchive.HeaderSize + payloads.Sum(payload => payload.Length));
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("GFFI"u8);
        writer.Write(0x0003_0000U);
        writer.Write((uint)GffArchive.HeaderSize);
        writer.Write((uint)indexOffset);
        writer.Write(new byte[12]);
        foreach (var payload in payloads) writer.Write(payload);
        writer.Write(0U);
        writer.Write(0U);
        var groups = resources.Select((resource, index) => (resource, index))
            .GroupBy(item => item.resource.Tag).OrderBy(group => group.Key).ToArray();
        writer.Write(checked((ushort)groups.Length));
        var offsets = new uint[resources.Length];
        var offset = GffArchive.HeaderSize;
        for (var index = 0; index < payloads.Length; index++)
        {
            offsets[index] = checked((uint)offset);
            offset += payloads[index].Length;
        }
        foreach (var group in groups)
        {
            writer.Write(Encoding.ASCII.GetBytes(group.Key));
            writer.Write((uint)group.Count());
            foreach (var item in group)
            {
                writer.Write(item.resource.Number);
                writer.Write(offsets[item.index]);
                writer.Write((uint)payloads[item.index].Length);
            }
        }
        stream.Position = 0;
        return GffArchive.Read(stream, "synthetic-pattern.gff");
    }
}
