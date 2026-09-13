using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class PackedCharacterCatalogTests
{
    [Fact]
    public void RoundTripsInCanonicalResourceOrder()
    {
        var catalog = new PackedCharacterCatalog(
        [
            Character(9, "Scholar", 4),
            Character(3, "Hero", 7)
        ]);
        using var stream = new MemoryStream();

        catalog.Write(stream);
        stream.Position = 0;
        var restored = PackedCharacterCatalog.Read(stream, "synthetic.dsch");

        Assert.Equal([3U, 9U], restored.Characters.Select(character => character.ResourceNumber));
        Assert.Equal(["Hero", "Scholar"], restored.Characters.Select(character => character.Name));
        Assert.Equal(7, restored.Characters[0].RawPsionicMask);
        Assert.Equal(15, restored.Characters[0].AbilityScores.Wisdom);
    }

    [Fact]
    public void OutputIsDeterministicRegardlessOfInputOrder()
    {
        var first = Bytes(new([Character(7, "Hero", 1), Character(8, "Mage", 2)]));
        var second = Bytes(new([Character(8, "Mage", 2), Character(7, "Hero", 1)]));

        Assert.Equal(first, second);
    }

    [Fact]
    public void RejectsDuplicateResourcesOnWrite()
    {
        using var stream = new MemoryStream();

        var exception = Assert.Throws<InvalidDataException>(() =>
            new PackedCharacterCatalog([Character(7, "Hero", 1), Character(7, "Mage", 2)])
                .Write(stream));

        Assert.Contains("duplicates", exception.Message);
    }

    [Theory]
    [InlineData(0, "Hero", 1)]
    [InlineData(15, "", 1)]
    [InlineData(15, "1234567890123456", 1)]
    [InlineData(15, "Hero", 0)]
    [InlineData(15, "Hero", 8)]
    public void RejectsInvalidCharacterOnWrite(byte strength, string name, byte mask)
    {
        using var stream = new MemoryStream();
        var character = Character(7, name, mask) with
        {
            AbilityScores = new(strength, 15, 15, 15, 15, 15)
        };

        Assert.Throws<InvalidDataException>(() =>
            new PackedCharacterCatalog([character]).Write(stream));
    }

    [Fact]
    public void RejectsNonCanonicalOrderOnRead()
    {
        var bytes = Bytes(new([Character(7, "Hero", 1), Character(8, "Mage", 2)]));
        var firstNumber = BitConverter.ToUInt32(bytes, 10);
        BitConverter.GetBytes(9U).CopyTo(bytes, 10);
        var secondOffset = 10 + 6 + "Hero".Length + 7;
        BitConverter.GetBytes(firstNumber).CopyTo(bytes, secondOffset);

        using var stream = new MemoryStream(bytes);
        var exception = Assert.Throws<InvalidDataException>(() =>
            PackedCharacterCatalog.Read(stream, "synthetic.dsch"));

        Assert.Contains("canonical order", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void RejectsTruncationAndTrailingData(int sizeDifference)
    {
        var exact = Bytes(new([Character(7, "Hero", 1)]));
        var bytes = new byte[exact.Length + (sizeDifference == 0 ? -1 : 1)];
        exact.AsSpan(0, Math.Min(exact.Length, bytes.Length)).CopyTo(bytes);
        using var stream = new MemoryStream(bytes);

        Assert.Throws<InvalidDataException>(() =>
            PackedCharacterCatalog.Read(stream, "synthetic.dsch"));
    }

    private static PackedCharacterMetadata Character(uint number, string name, byte mask) =>
        new(number, 2, name, new(15, 15, 15, 15, 15, 15), mask);

    private static byte[] Bytes(PackedCharacterCatalog catalog)
    {
        using var stream = new MemoryStream();
        catalog.Write(stream);
        return stream.ToArray();
    }
}
