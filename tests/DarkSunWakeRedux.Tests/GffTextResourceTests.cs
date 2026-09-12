using System.Text;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class GffTextResourceTests
{
    [Fact]
    public void ReadsCrlfTerminatedAsciiLines()
    {
        var text = GffTextResource.Read(Encoding.ASCII.GetBytes("First\r\n\r\nThird\r\n"), "synthetic TEXT");

        Assert.Equal(["First", "", "Third"], text.Lines);
    }

    [Theory]
    [InlineData("missing terminator")]
    [InlineData("bare feed\n")]
    [InlineData("bare return\r")]
    public void RejectsInvalidLineEndings(string value) =>
        Assert.Throws<InvalidDataException>(() =>
            GffTextResource.Read(Encoding.ASCII.GetBytes(value), "broken TEXT"));

    [Fact]
    public void RejectsUnsupportedControlAndNonAsciiBytes()
    {
        Assert.Throws<InvalidDataException>(() =>
            GffTextResource.Read(new byte[] { 1, 13, 10 }, "control TEXT"));
        Assert.Throws<InvalidDataException>(() =>
            GffTextResource.Read(new byte[] { 128, 13, 10 }, "extended TEXT"));
    }

    [Fact]
    public void RejectsOversizedLine()
    {
        var bytes = Enumerable.Repeat((byte)'A', GffTextResource.MaximumLineBytes + 1)
            .Concat(new byte[] { 13, 10 }).ToArray();

        Assert.Throws<InvalidDataException>(() => GffTextResource.Read(bytes, "long TEXT"));
    }
}
