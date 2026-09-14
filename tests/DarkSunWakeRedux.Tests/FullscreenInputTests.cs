using DarkSunWakeRedux.Game;
using Microsoft.Xna.Framework.Input;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class FullscreenInputTests
{
    [Theory]
    [InlineData(Keys.LeftAlt)]
    [InlineData(Keys.RightAlt)]
    public void TogglesWhenEitherAltCompletesTheEnterChord(Keys alt) =>
        Assert.True(FullscreenInput.ShouldToggle(
            new(alt, Keys.Enter), new(Keys.Enter)));

    [Fact]
    public void TogglesWhenEnterCompletesAnAltChord() =>
        Assert.True(FullscreenInput.ShouldToggle(
            new(Keys.LeftAlt, Keys.Enter), new(Keys.LeftAlt)));

    [Fact]
    public void HeldChordDoesNotToggleRepeatedly() =>
        Assert.False(FullscreenInput.ShouldToggle(
            new(Keys.LeftAlt, Keys.Enter), new(Keys.LeftAlt, Keys.Enter)));

    [Theory]
    [InlineData(Keys.Enter)]
    [InlineData(Keys.LeftAlt)]
    [InlineData(Keys.RightAlt)]
    public void PartialChordDoesNotToggle(Keys key) =>
        Assert.False(FullscreenInput.ShouldToggle(new(key), new()));

    [Fact]
    public void FullscreenUsesThePhysicalDisplayAndWindowedModeRestoresItsSize()
    {
        Assert.Equal(new(true, 1920, 1080),
            FullscreenInput.ResolveTarget(false, 1920, 1080));
        Assert.Equal(new(false, 960, 600),
            FullscreenInput.ResolveTarget(true, 1920, 1080));
    }

    [Theory]
    [InlineData(0, 1080)]
    [InlineData(1920, 0)]
    public void RejectsInvalidDisplayDimensions(int width, int height) =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FullscreenInput.ResolveTarget(false, width, height));
}
