using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class GffCharacterRecordEnvelopeTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(255)]
    public void ReadsExactVersionedEnvelope(byte count)
    {
        var envelope = GffCharacterRecordEnvelope.Read(Resource(count), "synthetic:CHAR#7");

        Assert.Equal(count, envelope.TailRecordCount);
    }

    [Fact]
    public void RejectsUnsupportedVersion()
    {
        var data = Resource(0);
        data[0] = 2;

        var exception = Assert.Throws<InvalidDataException>(() =>
            GffCharacterRecordEnvelope.Read(data, "synthetic"));

        Assert.Contains("unsupported version", exception.Message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void RejectsSizeThatDoesNotMatchCount(int sizeDifference)
    {
        var exact = Resource(2);
        var data = new byte[exact.Length + sizeDifference];
        exact.AsSpan(0, Math.Min(exact.Length, data.Length)).CopyTo(data);

        var exception = Assert.Throws<InvalidDataException>(() =>
            GffCharacterRecordEnvelope.Read(data, "synthetic"));

        Assert.Contains("declares 2 tail records", exception.Message);
    }

    [Fact]
    public void RejectsTruncatedFixedHeader()
    {
        var exception = Assert.Throws<InvalidDataException>(() =>
            GffCharacterRecordEnvelope.Read(
                new byte[GffCharacterRecordEnvelope.FixedHeaderSize - 1], "synthetic"));

        Assert.Contains("fixed header", exception.Message);
    }

    private static byte[] Resource(byte count)
    {
        var data = new byte[GffCharacterRecordEnvelope.FixedHeaderSize +
            count * GffCharacterRecordEnvelope.TailRecordSize];
        data[0] = GffCharacterRecordEnvelope.SupportedVersion;
        data[1] = count;
        return data;
    }
}
