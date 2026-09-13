using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class GffCharacterIdentityTests
{
    [Fact]
    public void ReadsNameAndIgnoresBytesAfterFirstTerminator()
    {
        var data = Resource("Hero");
        data[GffCharacterIdentity.NameOffset + 9] = (byte)'p';
        data[GffCharacterIdentity.NameOffset + 11] = (byte)'r';

        var identity = GffCharacterIdentity.Read(data, "synthetic:CHAR#7");

        Assert.Equal("Hero", identity.Name);
    }

    [Fact]
    public void AcceptsMaximumFifteenCharacterName()
    {
        var identity = GffCharacterIdentity.Read(Resource("123456789012345"), "synthetic");

        Assert.Equal("123456789012345", identity.Name);
    }

    [Theory]
    [InlineData(GffCharacterIdentity.MinimumResourceSize - 1)]
    [InlineData(GffCharacterIdentity.MaximumResourceSize + 1)]
    public void RejectsResourceOutsideBounds(int size)
    {
        var exception = Assert.Throws<InvalidDataException>(() =>
            GffCharacterIdentity.Read(new byte[size], "synthetic"));

        Assert.Contains("synthetic", exception.Message);
    }

    [Fact]
    public void RejectsNameWithoutTerminator()
    {
        var data = Resource("Hero");
        Array.Fill(data, (byte)'A', GffCharacterIdentity.NameOffset,
            GffCharacterIdentity.NameSlotLength);

        var exception = Assert.Throws<InvalidDataException>(() =>
            GffCharacterIdentity.Read(data, "synthetic"));

        Assert.Contains("not terminated", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(31)]
    [InlineData(127)]
    [InlineData(128)]
    public void RejectsEmptyOrNonPrintableName(byte firstByte)
    {
        var data = Resource("Hero");
        data[GffCharacterIdentity.NameOffset] = firstByte;

        Assert.Throws<InvalidDataException>(() =>
            GffCharacterIdentity.Read(data, "synthetic"));
    }

    private static byte[] Resource(string name)
    {
        var data = new byte[GffCharacterIdentity.MinimumResourceSize];
        var encoded = System.Text.Encoding.ASCII.GetBytes(name);
        encoded.CopyTo(data, GffCharacterIdentity.NameOffset);
        return data;
    }
}
