using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class CharacterGenerationInputTests
{
    [Fact]
    public void ResolvesTheCompleteOwnedGraphWhileExposingOnlyImageBackedControls()
    {
        var controls = CharacterGenerationInput.Resolve(Catalog());

        Assert.Equal(22, controls.Count);
        Assert.Equal([2001U, 2002U, 2003U, 2004U, 2005U, 2006U, 2007U, 2008U,
            2009U, 2010U, 2027U, 18302U, 19304U, 2011U, 2012U, 2013U, 2014U,
            2015U, 2016U, 2017U, 2018U, 4003U], controls.Select(control => control.ResourceNumber));
        var images = controls.Where(control => control.DisplayAssetPath is not null).ToArray();
        Assert.Equal(10, images.Length);
        Assert.Equal(OriginalContent.CharacterGenerationButtons.Select(asset => asset.Path),
            images.Select(control => control.DisplayAssetPath));
        var cleric = Assert.Single(controls, control => control.ResourceNumber == 2002);
        Assert.Equal((217, 10, 64, 6, 2002U),
            (cleric.X, cleric.Y, cleric.Width, cleric.Height,
                cleric.SourceImageResourceNumber));
    }

    [Fact]
    public void HitTestingUsesRecordedExclusiveImageBounds()
    {
        var controls = CharacterGenerationInput.Resolve(Catalog());
        var cleric = Assert.Single(controls, control => control.ResourceNumber == 2002);

        Assert.Equal(cleric, CharacterGenerationInput.HitTest(controls, 217, 10));
        Assert.Equal(cleric, CharacterGenerationInput.HitTest(controls, 280, 15));
        Assert.Null(CharacterGenerationInput.HitTest(controls, 281, 10));
        Assert.Null(CharacterGenerationInput.HitTest(controls, 0, 0));
    }

    [Fact]
    public void OnlyTheEvidencedExitControlCancelsTheCurrentFlow()
    {
        var controls = CharacterGenerationInput.Resolve(Catalog());
        var exit = Assert.Single(controls,
            control => control.ResourceNumber == CharacterGenerationInput.ExitButtonResourceNumber);
        var done = Assert.Single(controls, control => control.ResourceNumber == 19304);

        Assert.Equal(StartFlowCommand.Cancel(), CharacterGenerationInput.CommandFor(exit));
        Assert.Null(CharacterGenerationInput.CommandFor(done));

        var session = new StartFlowSession(0);
        session.Execute(StartFlowCommand.Choose(StartWindowChoice.CreateCharacters));
        session.Execute(StartFlowCommand.OpenEmptySlot());
        session.Execute(StartFlowCommand.Choose(EmptySlotChoice.New));
        var result = session.Execute(CharacterGenerationInput.CommandFor(exit)!);

        Assert.True(result.Accepted);
        Assert.Equal(StartFlowScreen.PartyOverview, session.Snapshot().Screen);
    }

    [Fact]
    public void RejectsChangedOrReorderedGraphContracts()
    {
        var catalog = Catalog();
        Assert.Throws<InvalidDataException>(() => CharacterGenerationInput.Resolve(catalog with
        {
            Windows = [catalog.Windows[0] with { Children = catalog.Windows[0].Children.Reverse().ToArray() }]
        }));
        Assert.Throws<InvalidDataException>(() => CharacterGenerationInput.Resolve(catalog with
        {
            Buttons = catalog.Buttons.Select(button => button.ResourceNumber == 2001
                ? button with { Width = 101 }
                : button).ToArray()
        }));
    }

    private static PackedUiCatalog Catalog()
    {
        var children = new List<UiChildReference>
        {
            new("BUTN", 2001, 0, 0)
        };
        children.AddRange(OriginalContent.CharacterGenerationButtons.Take(8).Select(asset =>
            new UiChildReference("BUTN", asset.ButtonResourceNumber,
                checked((short)asset.X), checked((short)asset.Y))));
        children.AddRange(
        [
            new UiChildReference("BUTN", 2010, 135, 75),
            new UiChildReference("BUTN", 2027, 135, 20),
            new UiChildReference("BUTN", 18302, 258, 154),
            new UiChildReference("BUTN", 19304, 243, 174),
            new UiChildReference("BUTN", 2011, 79, 145),
            new UiChildReference("BUTN", 2012, 4, 139),
            new UiChildReference("BUTN", 2013, 4, 146),
            new UiChildReference("BUTN", 2014, 4, 153),
            new UiChildReference("BUTN", 2015, 4, 160),
            new UiChildReference("BUTN", 2016, 4, 167),
            new UiChildReference("BUTN", 2017, 4, 174),
            new UiChildReference("BUTN", 2018, 79, 174),
            new UiChildReference("EBOX", 4003, 40, 125)
        ]);

        var buttons = new List<UiButtonResource>
        {
            new(2001, 100, 116, 0, 0), new(2010, 50, 42, 0, 0),
            new(2027, 45, 35, 0, 0), new(2011, 82, 7, 0, 0),
            new(2012, 50, 5, 0, 0), new(2013, 50, 5, 0, 0),
            new(2014, 50, 5, 0, 0), new(2015, 50, 5, 0, 0),
            new(2016, 50, 5, 0, 0), new(2017, 50, 5, 0, 0), new(2018, 58, 5, 0, 0)
        };
        buttons.AddRange(OriginalContent.CharacterGenerationButtons.Select(asset =>
            new UiButtonResource(asset.ButtonResourceNumber,
                checked((ushort)asset.ControlWidth), checked((ushort)asset.ControlHeight),
                asset.ImageResourceNumber, 0)));
        return new([new(CharacterGenerationInput.WindowResourceNumber, 19004, 320, 200, children)],
            buttons, [], [new UiEditBoxResource(4003, 95, 8, 10)]);
    }
}
