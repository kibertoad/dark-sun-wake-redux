using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class PackedTextCatalogTests
{
    [Fact]
    public void RoundTripsResourceIdsLinesAndEmptyLines()
    {
        var catalog = new PackedTextCatalog(new Dictionary<uint, IReadOnlyList<string>>
        {
            [100] = ["Alpha", "", "Omega"],
            [2] = ["Two"]
        });
        using var stream = new MemoryStream();
        catalog.Write(stream);
        stream.Position = 0;

        var decoded = PackedTextCatalog.Read(stream, "synthetic.dstx");

        Assert.Equal(["Two"], decoded.Resources[2]);
        Assert.Equal(["Alpha", "", "Omega"], decoded.Resources[100]);
    }

    [Fact]
    public void WriteIsDeterministicAcrossDictionaryOrder()
    {
        var first = Bytes(new(new Dictionary<uint, IReadOnlyList<string>> { [9] = ["Nine"], [1] = ["One"] }));
        var second = Bytes(new(new Dictionary<uint, IReadOnlyList<string>> { [1] = ["One"], [9] = ["Nine"] }));

        Assert.Equal(first, second);
    }

    [Fact]
    public void RejectsSignatureTruncationAndTrailingData()
    {
        var bytes = Bytes(new(new Dictionary<uint, IReadOnlyList<string>> { [1] = ["One"] }));
        bytes[0] = (byte)'X';
        Assert.Throws<InvalidDataException>(() => Read(bytes));

        bytes = Bytes(new(new Dictionary<uint, IReadOnlyList<string>> { [1] = ["One"] }));
        Assert.Throws<InvalidDataException>(() => Read(bytes[..^1]));
        Assert.Throws<InvalidDataException>(() => Read([.. bytes, 0]));
    }

    [Fact]
    public void RejectsNonAsciiTextWhenWriting() =>
        Assert.Throws<InvalidDataException>(() =>
            new PackedTextCatalog(new Dictionary<uint, IReadOnlyList<string>> { [1] = ["Café"] })
                .Write(new MemoryStream()));

    private static byte[] Bytes(PackedTextCatalog catalog)
    {
        using var stream = new MemoryStream();
        catalog.Write(stream);
        return stream.ToArray();
    }

    private static PackedTextCatalog Read(byte[] bytes) =>
        PackedTextCatalog.Read(new MemoryStream(bytes), "broken.dstx");
}
