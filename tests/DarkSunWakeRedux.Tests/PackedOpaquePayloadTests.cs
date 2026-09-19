using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class PackedOpaquePayloadTests
{
    [Fact]
    public void RoundTripsLosslessBytes()
    {
        var expected = PackedOpaquePayload.From([0x00, 0x01, 0xff, 0x80]);
        using var stream = new MemoryStream();

        expected.Write(stream);
        stream.Position = 0;
        var actual = PackedOpaquePayload.Read(stream);

        Assert.Equal(expected.Bytes, actual.Bytes);
    }

    [Theory]
    [InlineData(0, 0x58)]
    [InlineData(4, 0x02)]
    [InlineData(6, 0xff)]
    public void RejectsInvalidEnvelopeFields(int offset, byte replacement)
    {
        using var valid = new MemoryStream();
        PackedOpaquePayload.From([0x19]).Write(valid);
        var bytes = valid.ToArray();
        bytes[offset] = replacement;

        Assert.Throws<InvalidDataException>(() =>
            PackedOpaquePayload.Read(new MemoryStream(bytes)));
    }

    [Fact]
    public void RejectsTruncationTrailingBytesAndOversizedLength()
    {
        using var valid = new MemoryStream();
        PackedOpaquePayload.From([0x19]).Write(valid);
        var bytes = valid.ToArray();

        Assert.Throws<InvalidDataException>(() =>
            PackedOpaquePayload.Read(new MemoryStream(bytes[..^1])));
        Assert.Throws<InvalidDataException>(() =>
            PackedOpaquePayload.Read(new MemoryStream(bytes.Append((byte)0).ToArray())));
        bytes[6] = 0xff;
        bytes[7] = 0xff;
        bytes[8] = 0xff;
        bytes[9] = 0xff;
        Assert.Throws<InvalidDataException>(() =>
            PackedOpaquePayload.Read(new MemoryStream(bytes)));
    }
}
