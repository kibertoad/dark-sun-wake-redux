using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class PackedGplScriptTests
{
    [Fact]
    public void RoundTripsResourceIdentityAndOpaqueBytecode()
    {
        var expected = new PackedGplScript(135, [0x4f, 0x00, 0x73, 0x92, 0x05, 0x03]);
        using var stream = new MemoryStream();

        expected.Write(stream);
        stream.Position = 0;
        var actual = PackedGplScript.Read(stream);

        Assert.Equal(expected.ResourceNumber, actual.ResourceNumber);
        Assert.Equal(expected.Bytecode, actual.Bytecode);
    }

    [Theory]
    [InlineData(0, 0xff)]
    [InlineData(4, 2)]
    [InlineData(5, 1)]
    [InlineData(10, 2)]
    public void RejectsInvalidEnvelopeFields(int offset, byte replacement)
    {
        using var valid = new MemoryStream();
        new PackedGplScript(135, [0x19]).Write(valid);
        var bytes = valid.ToArray();
        bytes[offset] = replacement;

        Assert.Throws<InvalidDataException>(() =>
            PackedGplScript.Read(new MemoryStream(bytes)));
    }

    [Fact]
    public void RejectsTrailingBytesAndInvalidWriteState()
    {
        using var valid = new MemoryStream();
        new PackedGplScript(135, [0x19]).Write(valid);
        var bytes = valid.ToArray().Append((byte)0).ToArray();

        Assert.Throws<InvalidDataException>(() =>
            PackedGplScript.Read(new MemoryStream(bytes)));
        Assert.Throws<InvalidDataException>(() =>
            new PackedGplScript(0, [0x19]).Write(new MemoryStream()));
        Assert.Throws<InvalidDataException>(() =>
            new PackedGplScript(135, []).Write(new MemoryStream()));
    }
}
