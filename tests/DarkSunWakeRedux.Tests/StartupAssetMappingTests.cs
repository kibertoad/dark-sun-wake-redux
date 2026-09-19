using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed partial class StartupAssetExtractorTests
{
    [Fact]
    public void StartMenuAssetMappingsAreUniqueAndBounded()
    {
        Assert.Equal(4, OriginalContent.StartMenuButtons.Count);
        Assert.Equal(2, OriginalContent.StartMenuLayers.Count);
        Assert.Equal(
        [
            new UiLayerAsset("stone-shell", "images/start-menu/stone-shell.dsix",
                20029, 3, 44, 314, 112),
            new UiLayerAsset("flame-ornament", "images/start-menu/flame-ornament.dsix",
                20028, 47, 24, 222, 33)
        ], OriginalContent.StartMenuLayers);
        Assert.Equal(
        [
            new UiLayerAsset("party-overview-base", "images/party-overview/base.dsix",
                11000, 0, 0, 320, 200),
            new UiLayerAsset("view-character-title",
                "images/party-overview/view-character-title.dsix", 20079, 55, 0, 210, 23)
        ], OriginalContent.PartyOverviewLayers);
        Assert.Equal(7, OriginalContent.AddExistingCharacterAssets.Count);
        Assert.Equal(17, OriginalContent.AddExistingCharacterPlacements.Count);
        Assert.Equal(7, OriginalContent.AddExistingCharacterAssets
            .Select(asset => asset.Path).Distinct().Count());
        Assert.All(OriginalContent.AddExistingCharacterAssets, asset =>
        {
            Assert.Contains(asset.Tag, new[] { "BMP ", "ICON" });
            Assert.InRange(asset.FrameWidth, 1, 320);
            Assert.InRange(asset.FrameHeight, 1, 200);
            Assert.InRange(asset.FrameCount, 1, 4);
        });
        var addAssetPaths = OriginalContent.AddExistingCharacterAssets
            .Select(asset => asset.Path)
            .Append("images/character-generation/exit.dsix")
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(OriginalContent.AddExistingCharacterPlacements, placement =>
            Assert.Contains(placement.AssetPath, addAssetPaths));
        Assert.All(OriginalContent.StartMenuLayers, layer =>
        {
            Assert.InRange(layer.X, 0, 319);
            Assert.InRange(layer.Y, 0, 199);
            Assert.InRange(layer.X + layer.FrameWidth, 1, 320);
            Assert.InRange(layer.Y + layer.FrameHeight, 1, 200);
        });
        Assert.Equal(4, OriginalContent.StartMenuButtons.Select(button => button.Path).Distinct().Count());
        Assert.Equal(4, OriginalContent.StartMenuButtons.Select(button => button.ButtonResourceNumber).Distinct().Count());
        Assert.Equal(4, OriginalContent.StartMenuButtons.Select(button => button.ImageResourceNumber).Distinct().Count());
        Assert.All(OriginalContent.StartMenuButtons, button =>
        {
            Assert.InRange(button.FrameWidth, 1, 320);
            Assert.InRange(button.FrameHeight, 1, 200);
        });
        var load = Assert.Single(OriginalContent.StartMenuButtons,
            button => button.Name == "load-saved-game");
        Assert.Equal((191, 13), (load.FrameWidth, load.FrameHeight));
        Assert.Equal(10, OriginalContent.CharacterGenerationButtons.Count);
        Assert.Equal(10, OriginalContent.CharacterGenerationButtons.Select(button => button.Path).Distinct().Count());
        Assert.All(OriginalContent.CharacterGenerationButtons, button =>
        {
            Assert.InRange(button.X + button.ControlWidth, 1, 320);
            Assert.InRange(button.Y + button.ControlHeight, 1, 200);
            Assert.InRange(button.X + button.FrameWidth, 1, 320);
            Assert.InRange(button.Y + button.FrameHeight, 1, 200);
        });
        Assert.Equal(10, OriginalContent.CharacterGenerationModalButtons.Count);
        Assert.Equal(10, OriginalContent.CharacterGenerationModalButtons
            .Select(button => button.Path).Distinct().Count());
        Assert.All(OriginalContent.CharacterGenerationModalButtons,
            button => Assert.Equal(3, button.FrameCount));
        Assert.Equal(new UiLayerAsset("game-menu", "images/game-menu/base.dsix",
            10000, 55, 42, 210, 116), OriginalContent.GameMenuLayer);
        Assert.Equal(new UiLayerAsset("inventory", "images/exploration/inventory-base.dsix",
            13001, 0, 0, 320, 200), OriginalContent.InventoryLayer);
        Assert.Equal(
        [
            new UiLayerAsset("effects-title", "images/exploration/effects-title.dsix",
                20075, 84, 0, 152, 23),
            new UiLayerAsset("use-title", "images/exploration/use-title.dsix",
                20080, 108, 0, 104, 23)
        ], OriginalContent.ExplorationDestinationTitleLayers);
        Assert.Equal(
        [
            new UiLayerAsset("speech-panel", "images/dialogue/panel.dsix",
                12003, 0, 0, 320, 200),
            new UiLayerAsset("speech-texture", "images/dialogue/speech-texture.dsix",
                12002, 75, 6, 243, 47),
            new UiLayerAsset("response-panel", "images/dialogue/panel.dsix",
                12003, 0, 140, 320, 200)
        ], OriginalContent.DialogueLayers);
        Assert.Equal([11500U, 13500U],
            OriginalContent.ExplorationDestinationWindowResourceNumbers);
        Assert.Equal(10, OriginalContent.ExplorationCursorAssets.Count);
        Assert.Equal(Enumerable.Range(19101, 10).Select(number => (uint)number),
            OriginalContent.ExplorationCursorAssets.Select(asset => asset.ResourceNumber));
        Assert.All(OriginalContent.ExplorationCursorAssets, asset =>
        {
            Assert.Equal("ICON", asset.Tag);
            Assert.Equal(1, asset.FrameCount);
            Assert.InRange(asset.FrameWidth, 10, 16);
            Assert.InRange(asset.FrameHeight, 13, 17);
        });
        Assert.Equal(14, OriginalContent.GameMenuButtons.Count);
        Assert.Equal(14, OriginalContent.GameMenuButtons
            .Select(button => button.Path).Distinct().Count());
        Assert.Equal(14, OriginalContent.GameMenuButtons
            .Select(button => button.ButtonResourceNumber).Distinct().Count());
        Assert.Equal(14, OriginalContent.GameMenuButtons
            .Select(button => button.ImageResourceNumber).Distinct().Count());
        Assert.All(OriginalContent.GameMenuButtons, button =>
        {
            Assert.InRange(button.X + button.FrameWidth, 1,
                OriginalContent.GameMenuLayer.FrameWidth);
            Assert.InRange(button.Y + button.FrameHeight, 1,
                OriginalContent.GameMenuLayer.FrameHeight);
            Assert.InRange(button.FrameCount, 2, 4);
        });
        Assert.Equal([10500U, 16500U], OriginalContent.GameMenuWindowResourceNumbers);
        Assert.Equal(13, OriginalContent.PreferencesButtons.Count);
        Assert.Equal(9, OriginalContent.PreferencesButtons
            .Select(button => button.Path).Distinct().Count());
        Assert.Equal(13, OriginalContent.PreferencesButtons
            .Select(button => button.ButtonResourceNumber).Distinct().Count());
        Assert.All(OriginalContent.PreferencesButtons, button =>
        {
            Assert.InRange(button.X + button.FrameWidth, 1,
                OriginalContent.GameMenuLayer.FrameWidth);
            Assert.InRange(button.Y + button.FrameHeight, 1,
                OriginalContent.GameMenuLayer.FrameHeight);
            Assert.InRange(button.FrameCount, 2, 4);
        });
    }
}
