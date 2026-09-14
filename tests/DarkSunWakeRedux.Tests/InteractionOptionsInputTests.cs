using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class InteractionOptionsInputTests
{
    [Fact]
    public void ResolvesObservedHostilePanelInStoredButtonOrder()
    {
        var controls = InteractionOptionsInput.Resolve(Catalog());

        Assert.Equal(
        [
            ExplorationInteractionAction.PickUp,
            ExplorationInteractionAction.Use,
            ExplorationInteractionAction.Talk,
            ExplorationInteractionAction.Dismiss
        ], controls.Select(control => control.Action));
        Assert.Equal([(91, 104), (111, 104), (71, 104), (129, 105)],
            controls.Select(control => (control.X, control.Y)));
        Assert.All(controls.Take(3), control => Assert.False(control.Enabled));
        Assert.True(controls[^1].Enabled);
    }

    [Fact]
    public void HitTestingUsesExclusiveControlEdges()
    {
        var controls = InteractionOptionsInput.Resolve(Catalog());
        var talk = Assert.Single(controls,
            control => control.Action == ExplorationInteractionAction.Talk);

        Assert.Equal(talk, InteractionOptionsInput.HitTest(controls, talk.X, talk.Y));
        Assert.Equal(talk, InteractionOptionsInput.HitTest(
            controls, talk.X + talk.Width - 1, talk.Y + talk.Height - 1));
        Assert.Null(InteractionOptionsInput.HitTest(
            controls, talk.X + talk.Width, talk.Y + talk.Height));
    }

    [Fact]
    public void RejectsDriftedApplicationFrameOrUnexpectedButton()
    {
        var catalog = Catalog();
        var frame = catalog.ApplicationFrames[0] with { Width = 144 };
        Assert.Throws<InvalidDataException>(() => InteractionOptionsInput.Resolve(
            catalog with { ApplicationFrames = [frame] }));
        var children = catalog.Windows[0].Children.ToArray();
        children[0] = children[0] with { ResourceNumber = 999 };
        Assert.Throws<InvalidDataException>(() => InteractionOptionsInput.Resolve(
            catalog with { Windows = [catalog.Windows[0] with { Children = children }] }));
    }

    private static PackedUiCatalog Catalog() => new(
        [new UiWindowResource(3020, 0, 92, 77,
        [
            new("BUTN", 15308, 23, 59),
            new("BUTN", 15307, 43, 59),
            new("BUTN", 15306, 3, 59),
            new("BUTN", 15309, 61, 60),
            new("APFM", 15200, 0, 0)
        ])],
        [
            new UiButtonResource(15306, 15, 15, 15105, 0),
            new UiButtonResource(15307, 15, 15, 15106, 0),
            new UiButtonResource(15308, 15, 15, 15107, 0),
            new UiButtonResource(15309, 27, 11, 15109, 84)
        ],
        [new UiApplicationFrameResource(15200, 145, 87, 486)], []);
}
