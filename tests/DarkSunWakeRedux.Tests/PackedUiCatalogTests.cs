using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class PackedUiCatalogTests
{
    [Fact]
    public void RoundTripsResolvedWindowGraphInDeterministicOrder()
    {
        var catalog = Catalog();
        using var first = new MemoryStream();
        catalog.Write(first);
        using var second = new MemoryStream();
        (catalog with { Buttons = catalog.Buttons.Reverse().ToArray() }).Write(second);
        Assert.Equal(first.ToArray(), second.ToArray());

        first.Position = 0;
        var decoded = PackedUiCatalog.Read(first, "start-flow.dsui");

        var window = Assert.Single(decoded.Windows);
        Assert.Equal(19501U, window.ResourceNumber);
        Assert.Equal(3, window.Children.Count);
        Assert.Equal(new UiChildReference("EBOX", 4003, 40, 125), window.Children[2]);
        Assert.Equal(2, decoded.Buttons.Count);
        Assert.Equal(2002U, decoded.Buttons[0].ResourceNumber);
        Assert.Equal((ushort)494, Assert.Single(decoded.ApplicationFrames).EventMask);
        Assert.Equal((ushort)10, Assert.Single(decoded.EditBoxes).EventMask);
    }

    [Fact]
    public void RejectsSignatureVersionTruncationAndTrailingData()
    {
        var bytes = Bytes(Catalog());
        bytes[0] = (byte)'X';
        Assert.Throws<InvalidDataException>(() => Read(bytes));

        bytes = Bytes(Catalog());
        bytes[4]++;
        Assert.Throws<InvalidDataException>(() => Read(bytes));

        bytes = Bytes(Catalog());
        Assert.Throws<InvalidDataException>(() => Read(bytes[..^1]));

        Assert.Throws<InvalidDataException>(() => Read([.. Bytes(Catalog()), 0]));
    }

    [Fact]
    public void RejectsMissingDuplicateAndUnsupportedChildren()
    {
        var catalog = Catalog();
        Assert.Throws<InvalidDataException>(() =>
            (catalog with { Buttons = [] }).Write(new MemoryStream()));
        Assert.Throws<InvalidDataException>(() =>
            (catalog with { Buttons = [catalog.Buttons[0], catalog.Buttons[0]] }).Write(new MemoryStream()));

        var window = catalog.Windows[0] with
        {
            Children = [new UiChildReference("TEXT", 1, 0, 0)]
        };
        Assert.Throws<InvalidDataException>(() =>
            (catalog with { Windows = [window] }).Write(new MemoryStream()));
    }

    [Fact]
    public void RejectsInvalidDimensions()
    {
        var catalog = Catalog();
        var button = catalog.Buttons[0] with { Width = 0 };

        Assert.Throws<InvalidDataException>(() =>
            (catalog with { Buttons = [button, catalog.Buttons[1]] }).Write(new MemoryStream()));
    }

    private static PackedUiCatalog Catalog() => new(
        [new UiWindowResource(19501, 19004, 320, 200,
        [
            new("BUTN", 2002, 217, 10),
            new("APFM", 19200, 0, 0),
            new("EBOX", 4003, 40, 125)
        ])],
        [new(2003, 56, 6, 2003, 0), new(2002, 64, 6, 2002, 0)],
        [new(19200, 320, 200, 494)],
        [new(4003, 95, 8, 10)]);

    private static byte[] Bytes(PackedUiCatalog catalog)
    {
        using var stream = new MemoryStream();
        catalog.Write(stream);
        return stream.ToArray();
    }

    private static PackedUiCatalog Read(byte[] bytes) =>
        PackedUiCatalog.Read(new MemoryStream(bytes), "broken.dsui");
}
