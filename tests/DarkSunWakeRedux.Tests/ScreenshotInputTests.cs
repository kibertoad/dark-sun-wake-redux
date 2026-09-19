using DarkSunWakeRedux.Game;
using Microsoft.Xna.Framework.Input;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ScreenshotInputTests
{
    [Fact]
    public void F12RequestsOneScreenshotOnItsRisingEdge()
    {
        Assert.True(ScreenshotInput.ShouldCapture(new(Keys.F12), new()));
        Assert.False(ScreenshotInput.ShouldCapture(new(Keys.F12), new(Keys.F12)));
        Assert.False(ScreenshotInput.ShouldCapture(new(), new(Keys.F12)));
    }
}
