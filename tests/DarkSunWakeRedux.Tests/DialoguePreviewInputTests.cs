using DarkSunWakeRedux.Game;
using Microsoft.Xna.Framework.Input;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class DialoguePreviewInputTests
{
    [Fact]
    public void F9TogglesOnlyOnTheRisingEdge()
    {
        Assert.True(DialoguePreviewInput.ShouldToggle(
            new KeyboardState(Keys.F9), new KeyboardState()));
        Assert.False(DialoguePreviewInput.ShouldToggle(
            new KeyboardState(Keys.F9), new KeyboardState(Keys.F9)));
        Assert.False(DialoguePreviewInput.ShouldToggle(
            new KeyboardState(), new KeyboardState(Keys.F9)));
    }
}
