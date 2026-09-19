using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class DialogueInputTests
{
    [Fact]
    public void ResolvesFiveObservedRowsAtBottomOfLogicalCanvas()
    {
        var rows = DialogueInput.ResolveResponses(Catalog());

        Assert.Equal([0, 1, 2, 3, 4], rows.Select(row => row.Index));
        Assert.Equal([(4, 155), (4, 163), (4, 171), (4, 179), (4, 187)],
            rows.Select(row => (row.X, row.Y)));
        Assert.All(rows, row => Assert.Equal((300, 10), (row.Width, row.Height)));
    }

    [Fact]
    public void HitTestingUsesExclusiveEdgesAndTopmostOverlap()
    {
        var rows = DialogueInput.ResolveResponses(Catalog());

        Assert.Equal(0, DialogueInput.HitTest(rows, 4, 155)?.Index);
        Assert.Equal(1, DialogueInput.HitTest(rows, 303, 164)?.Index);
        Assert.Null(DialogueInput.HitTest(rows, 304, 165));
    }

    [Fact]
    public void ResolvesObservedDialogueChromeOnTheFixedLogicalCanvas()
    {
        var overlay = DialogueInput.ResolveOverlay(Catalog());

        Assert.Equal(new(1, 0, 318, 72, new Rgb24(0, 0, 0)), overlay.SpeechWindow);
        Assert.Equal(new(1, 142, 318, 58, new Rgb24(125, 125, 125)),
            overlay.ResponseWindow);
        Assert.Equal((1, 0), (overlay.PortraitX, overlay.PortraitY));
        Assert.Equal(9, overlay.Images.Count);
        Assert.Equal(
        [
            (2093u, 306, 4), (2094u, 306, 18),
            (2095u, 306, 146), (2096u, 306, 160),
            (2076u, 4, 155), (2077u, 4, 163), (2078u, 4, 171),
            (2079u, 4, 179), (2080u, 4, 187)
        ], overlay.Images.Select(image => (image.ResourceNumber, image.X, image.Y)));
    }

    [Fact]
    public void RejectsDriftedSpeechOrResponseContracts()
    {
        var catalog = Catalog();
        var editBox = catalog.EditBoxes[0] with { EventMask = 0 };
        Assert.Throws<InvalidDataException>(() => DialogueInput.ResolveResponses(
            catalog with { EditBoxes = [editBox] }));
        var button = catalog.Buttons.Single(item => item.ResourceNumber == 2078);
        Assert.Throws<InvalidDataException>(() => DialogueInput.ResolveResponses(
            catalog with { Buttons = catalog.Buttons.Select(item =>
                item.ResourceNumber == 2078 ? button with { ImageResourceNumber = 12104 } : item)
                .ToArray() }));
    }

    private static PackedUiCatalog Catalog() => new(
    [
        new(12500, 0, 318, 72, StartupAssetTestArchives.DialogueSpeechChildren()),
        new(12501, 0, 318, 58, StartupAssetTestArchives.DialogueResponseChildren())
    ],
    [
        new(12300, 300, 58, 0, 0),
        new(2093, 14, 14, 12102, 0),
        new(2094, 14, 35, 12100, 0),
        new(2096, 14, 35, 12100, 0),
        new(2076, 300, 10, 12104, 0),
        new(2077, 300, 10, 12105, 0),
        new(2078, 300, 10, 12106, 0),
        new(2079, 300, 10, 12107, 0),
        new(2080, 300, 10, 12108, 0),
        new(2095, 14, 14, 12102, 0)
    ], [], [new(12400, 236, 46, 4)]);
}
