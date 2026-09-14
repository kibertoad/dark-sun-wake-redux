using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ExplorationDestinationInputTests
{
    [Theory]
    [InlineData(ExplorationView.ViewCharacter, 43, 155, 253, 155)]
    [InlineData(ExplorationView.ViewInventory, 163, 181, 288, 181)]
    [InlineData(ExplorationView.CastSpellsOrUsePsionics, 43, 155, 253, 155)]
    [InlineData(ExplorationView.CurrentSpellEffects, 43, 155, 253, 155)]
    public void ResolvesSharedNavigationAsPageRelativeControls(
        ExplorationView page, int firstX, int firstY, int returnX, int returnY)
    {
        var controls = ExplorationDestinationInput.Resolve(Catalog(), page);

        Assert.Equal(5, controls.Count);
        Assert.Equal((ExplorationView.ViewCharacter, 10300U, firstX, firstY),
            (controls[0].Destination, controls[0].ResourceNumber,
                controls[0].X, controls[0].Y));
        Assert.Equal((ExplorationView.World, 10308U, returnX, returnY),
            (controls[^1].Destination, controls[^1].ResourceNumber,
                controls[^1].X, controls[^1].Y));
    }

    [Fact]
    public void HitTestingUsesExclusiveEdgesAndLastControlWins()
    {
        var controls = ExplorationDestinationInput.Resolve(
            Catalog(), ExplorationView.ViewInventory);
        var inventory = controls.Single(control =>
            control.Destination == ExplorationView.ViewInventory);

        Assert.Equal(inventory,
            ExplorationDestinationInput.HitTest(controls, inventory.X, inventory.Y));
        Assert.Equal(inventory, ExplorationDestinationInput.HitTest(controls,
            inventory.X + inventory.Width - 1, inventory.Y + inventory.Height - 1));
        Assert.Null(ExplorationDestinationInput.HitTest(controls,
            inventory.X + inventory.Width, inventory.Y));
    }

    [Fact]
    public void CommandsOpenDestinationsAndReturnThroughCoreEscape()
    {
        var controls = ExplorationDestinationInput.Resolve(
            Catalog(), ExplorationView.ViewCharacter);

        Assert.Equal(ExplorationCommand.Open(ExplorationView.ViewInventory),
            ExplorationDestinationInput.CommandFor(controls[1]));
        Assert.Equal(new(ExplorationCommandKind.Escape),
            ExplorationDestinationInput.CommandFor(controls[^1]));
    }

    [Fact]
    public void RejectsUnmappedPagesAndDriftedGeometry()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExplorationDestinationInput.Resolve(Catalog(), ExplorationView.OverheadMap));
        var catalog = Catalog();
        var bad = catalog with
        {
            Windows = catalog.Windows.Select(window => window.ResourceNumber == 13500
                ? window with { Width = 319 }
                : window).ToArray()
        };
        Assert.Throws<InvalidDataException>(() =>
            ExplorationDestinationInput.Resolve(bad, ExplorationView.ViewInventory));
    }

    private static PackedUiCatalog Catalog()
    {
        var resourceNumbers = new uint[] { 10300, 11304, 11305, 11306, 10308 };
        var buttons = resourceNumbers.Select(number =>
        {
            var asset = OriginalContent.GameMenuButtons.Single(candidate =>
                candidate.ButtonResourceNumber == number);
            return new UiButtonResource(number, checked((ushort)asset.FrameWidth),
                checked((ushort)asset.FrameHeight), asset.ImageResourceNumber, 0);
        }).ToArray();
        return new(
        [
            Window(11500, 320, 189,
                [(43, 155), (67, 155), (91, 155), (114, 155), (253, 155)]),
            Window(13500, 320, 200,
                [(163, 181), (187, 181), (211, 181), (235, 181), (288, 181)])
        ], buttons, [], []);

        UiWindowResource Window(
            uint number, ushort width, ushort height,
            IReadOnlyList<(short X, short Y)> positions) =>
            new(number, 0, width, height, resourceNumbers.Select((resource, index) =>
                new UiChildReference("BUTN", resource,
                    positions[index].X, positions[index].Y)).ToArray());
    }
}
