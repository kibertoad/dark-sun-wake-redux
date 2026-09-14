using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class GplPackedStringTests
{
    [Theory]
    [InlineData("")]
    [InlineData("Hello!")]
    [InlineData("EightBit")]
    [InlineData("Dialogue text with punctuation: yes?")]
    public void DecodesSevenBitPackedText(string text)
    {
        var encoded = Encode(text);

        var value = GplPackedString.Read(encoded);

        Assert.Equal(GplPackedStringKind.Compressed, value.Kind);
        Assert.Equal(text, value.Value);
        Assert.Equal(encoded.Length, value.BytesConsumed);
    }

    [Fact]
    public void PreservesNonPrintableSevenBitValues()
    {
        var value = GplPackedString.Read(Encode("A\tB\n"));

        Assert.Equal("A\tB\n", value.Value);
    }

    [Fact]
    public void RecognizesActiveCharacterNamePlaceholder()
    {
        var value = GplPackedString.Read(
            [GplPackedString.ActiveCharacterNameMarker, 0xff]);

        Assert.Equal(GplPackedStringKind.ActiveCharacterName, value.Kind);
        Assert.Equal(string.Empty, value.Value);
        Assert.Equal(1, value.BytesConsumed);
    }

    public static IEnumerable<object[]> InvalidPayloads()
    {
        yield return [Array.Empty<byte>()];
        yield return [new byte[] { 0x02 }];
        yield return [new byte[] { 0xff }];
        yield return [new byte[] { 0x05, 0x82 }];
    }

    [Theory]
    [MemberData(nameof(InvalidPayloads))]
    public void RejectsUnsupportedOrTruncatedPayloads(byte[] bytes)
    {
        Assert.Throws<InvalidDataException>(() => GplPackedString.Read(bytes, "synthetic"));
    }

    private static byte[] Encode(string text)
    {
        var values = text.Select(character => checked((byte)character))
            .Append(GplPackedString.Terminator).ToArray();
        Assert.All(values, value => Assert.InRange(value, (byte)0, (byte)0x7f));
        var bits = new List<int>(values.Length * 7);
        foreach (var value in values)
            for (var bit = 6; bit >= 0; bit--) bits.Add((value >> bit) & 1);
        var packed = new List<byte> { GplPackedString.CompressedMarker };
        for (var offset = 0; offset < bits.Count; offset += 8)
        {
            byte value = 0;
            for (var bit = 0; bit < 8 && offset + bit < bits.Count; bit++)
                value |= checked((byte)(bits[offset + bit] << (7 - bit)));
            packed.Add(value);
        }
        return packed.ToArray();
    }
}
