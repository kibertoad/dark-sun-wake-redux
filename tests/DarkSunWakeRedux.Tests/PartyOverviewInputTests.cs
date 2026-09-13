using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class PartyOverviewInputTests
{
    [Fact]
    public void ResolvesExactFullCanvasApplicationSurface()
    {
        var surface = PartyOverviewInput.Resolve(Catalog());

        Assert.Equal(new PartyOverviewInteractionSurface(2099, 0, 0, 319, 199, 0), surface);
        Assert.True(PartyOverviewInput.Contains(surface, 0, 0));
        Assert.True(PartyOverviewInput.Contains(surface, 318, 198));
        Assert.False(PartyOverviewInput.Contains(surface, 319, 198));
        Assert.False(PartyOverviewInput.Contains(surface, 318, 199));
        Assert.False(PartyOverviewInput.Contains(surface, -1, 0));
    }

    [Fact]
    public void RejectsChangedWindowOrButtonContracts()
    {
        var catalog = Catalog();
        Assert.Throws<InvalidDataException>(() => PartyOverviewInput.Resolve(catalog with
        {
            Windows = [catalog.Windows[0] with { ImageResourceNumber = 0 }]
        }));
        Assert.Throws<InvalidDataException>(() => PartyOverviewInput.Resolve(catalog with
        {
            Buttons = [catalog.Buttons[0] with { Width = 320 }]
        }));
        Assert.Throws<InvalidDataException>(() => PartyOverviewInput.Resolve(catalog with
        {
            Windows = [catalog.Windows[0] with { Children = [] }]
        }));
    }

    private static PackedUiCatalog Catalog() => new(
        [new UiWindowResource(19502, 19004, 320, 200,
            [new UiChildReference("BUTN", 2099, 0, 0)])],
        [new UiButtonResource(2099, 319, 199, 0, 0)], [], []);
}
