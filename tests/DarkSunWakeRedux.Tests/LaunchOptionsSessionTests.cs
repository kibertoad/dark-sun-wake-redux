using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Microsoft.Xna.Framework.Input;
using Xunit;

namespace DarkSunWakeRedux.Tests;

// The launch options screen of DEV-UI-001 and its Wide map view option (DEV-EXPLORE-001).
public sealed class LaunchOptionsSessionTests
{
    [Fact]
    public void OpensOnTheFirstOptionWithTheLoadedValues()
    {
        var session = new LaunchOptionsSession(new(false));

        Assert.Equal(0, session.Selected);
        Assert.Equal(LaunchOptionsOutcome.Open, session.Outcome);
        Assert.Equal("Off", LaunchOptionsSession.ValueText(session.Settings,
            LaunchOption.WideMapView));
    }

    [Fact]
    public void NavigationWrapsThroughOptionsConfirmAndExit()
    {
        var session = new LaunchOptionsSession(LaunchSettings.Default);

        session.Execute(LaunchOptionsCommand.Next());
        Assert.Equal(LaunchOptionsSession.ConfirmItem, session.Selected);
        session.Execute(LaunchOptionsCommand.Next());
        Assert.Equal(LaunchOptionsSession.ExitItem, session.Selected);
        session.Execute(LaunchOptionsCommand.Next());
        Assert.Equal(0, session.Selected);
        session.Execute(LaunchOptionsCommand.Previous());
        Assert.Equal(LaunchOptionsSession.ExitItem, session.Selected);
    }

    [Fact]
    public void ChangeAndActivateToggleTheSelectedOption()
    {
        var session = new LaunchOptionsSession(LaunchSettings.Default);

        session.Execute(LaunchOptionsCommand.Change());
        Assert.False(session.Settings.WideMapView);
        session.Execute(LaunchOptionsCommand.Activate());
        Assert.True(session.Settings.WideMapView);
        Assert.Equal(LaunchOptionsOutcome.Open, session.Outcome);
    }

    [Fact]
    public void ChangeOnAButtonDoesNothing()
    {
        var session = new LaunchOptionsSession(LaunchSettings.Default);
        session.Execute(LaunchOptionsCommand.Hover(LaunchOptionsSession.ConfirmItem));

        session.Execute(LaunchOptionsCommand.Change());

        Assert.Equal(LaunchSettings.Default, session.Settings);
        Assert.Equal(LaunchOptionsOutcome.Open, session.Outcome);
    }

    [Fact]
    public void ConfirmKeepsTheChangedValues()
    {
        var session = new LaunchOptionsSession(LaunchSettings.Default);
        session.Execute(LaunchOptionsCommand.Change());
        session.Execute(LaunchOptionsCommand.Next());

        session.Execute(LaunchOptionsCommand.Activate());

        Assert.Equal(LaunchOptionsOutcome.Confirmed, session.Outcome);
        Assert.False(session.Settings.WideMapView);
    }

    [Fact]
    public void ExitButtonAndEscapeQuit()
    {
        var byButton = new LaunchOptionsSession(LaunchSettings.Default);
        byButton.Execute(LaunchOptionsCommand.Click(LaunchOptionsSession.ExitItem));
        var byKey = new LaunchOptionsSession(LaunchSettings.Default);
        byKey.Execute(LaunchOptionsCommand.Exit());

        Assert.Equal(LaunchOptionsOutcome.ExitRequested, byButton.Outcome);
        Assert.Equal(LaunchOptionsOutcome.ExitRequested, byKey.Outcome);
    }

    [Fact]
    public void CommandsAfterTheOutcomeAreIgnored()
    {
        var session = new LaunchOptionsSession(LaunchSettings.Default);
        session.Execute(LaunchOptionsCommand.Click(LaunchOptionsSession.ConfirmItem));

        session.Execute(LaunchOptionsCommand.Click(0));
        session.Execute(LaunchOptionsCommand.Exit());

        Assert.Equal(LaunchOptionsOutcome.Confirmed, session.Outcome);
        Assert.True(session.Settings.WideMapView);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void PointerCommandsOutsideTheItemsAreIgnored(int item)
    {
        var session = new LaunchOptionsSession(LaunchSettings.Default);

        session.Execute(LaunchOptionsCommand.Click(item));

        Assert.Equal((0, LaunchOptionsOutcome.Open, true),
            (session.Selected, session.Outcome, session.Settings.WideMapView));
    }

    [Fact]
    public void KeysMapToCommandsOnPress()
    {
        Assert.Equal([LaunchOptionsCommand.Next()], Resolve(Keys.Down));
        Assert.Equal([LaunchOptionsCommand.Next()], Resolve(Keys.Tab));
        Assert.Equal([LaunchOptionsCommand.Previous()], Resolve(Keys.Up));
        Assert.Equal([LaunchOptionsCommand.Change()], Resolve(Keys.Left));
        Assert.Equal([LaunchOptionsCommand.Change()], Resolve(Keys.Right));
        Assert.Equal([LaunchOptionsCommand.Activate()], Resolve(Keys.Enter));
        Assert.Equal([LaunchOptionsCommand.Activate()], Resolve(Keys.Space));
        Assert.Equal([LaunchOptionsCommand.Exit()], Resolve(Keys.Escape));
        Assert.Empty(LaunchOptionsInput.Resolve(
            new(Keys.Down), new(Keys.Down), null, false, false));

        static IReadOnlyList<LaunchOptionsCommand> Resolve(Keys key) =>
            LaunchOptionsInput.Resolve(new(key), new(), null, false, false);
    }

    [Fact]
    public void AltEnterIsLeftToTheFullScreenToggle() =>
        Assert.Empty(LaunchOptionsInput.Resolve(
            new(Keys.LeftAlt, Keys.Enter), new(Keys.LeftAlt), null, false, false));

    [Fact]
    public void PointerHoversWhenItMovesAndClicksTheItemUnderIt()
    {
        Assert.Equal([LaunchOptionsCommand.Hover(1)],
            LaunchOptionsInput.Resolve(new(), new(), 1, true, false));
        Assert.Equal([LaunchOptionsCommand.Click(2)],
            LaunchOptionsInput.Resolve(new(), new(), 2, false, true));
        Assert.Empty(LaunchOptionsInput.Resolve(new(), new(), null, true, true));
    }

    [Fact]
    public void LayoutHitTestsEachItemInsideTheCanvas()
    {
        for (var item = 0; item < LaunchOptionsSession.ItemCount; item++)
        {
            var bounds = LaunchOptionsLayout.Item(item);
            Assert.InRange(bounds.X, 0, LogicalCanvasTransform.LogicalWidth - bounds.Width);
            Assert.InRange(bounds.Y, 0, LogicalCanvasTransform.LogicalHeight - bounds.Height);
            Assert.Equal(item, LaunchOptionsLayout.HitTest(
                bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2));
        }
        Assert.Null(LaunchOptionsLayout.HitTest(0, 0));
    }

    [Fact]
    public void RasterizesEveryStateOnTheLogicalCanvas()
    {
        var font = Font(width: 4, height: 7);
        foreach (var wideMapView in new[] { true, false })
            for (var item = 0; item < LaunchOptionsSession.ItemCount; item++)
            {
                var session = new LaunchOptionsSession(new(wideMapView));
                session.Execute(LaunchOptionsCommand.Hover(item));

                var canvas = LaunchOptionsRasterizer.Rasterize(font, session);

                Assert.Equal((320, 200, 64_000),
                    (canvas.Width, canvas.Height, canvas.Pixels.Length));
                Assert.Contains(LaunchOptionsRasterizer.HighlightText, canvas.Pixels);
                Assert.All(canvas.Pixels, pixel =>
                    Assert.InRange(pixel, 0, LaunchOptionsRasterizer.Palette.Count - 1));
            }
    }

    [Fact]
    public void RasterizerRejectsTextThatDoesNotFit() =>
        Assert.Throws<InvalidDataException>(() => LaunchOptionsRasterizer.Rasterize(
            Font(width: 12, height: 7), new(LaunchSettings.Default)));

    [Fact]
    public void RasterizerRejectsAFontTallerThanItsRows() =>
        Assert.Throws<InvalidDataException>(() => LaunchOptionsRasterizer.Rasterize(
            Font(width: 1, height: LaunchOptionsLayout.MaximumTextHeight + 1),
            new(LaunchSettings.Default)));

    private static PackedIndexedBitmapFont Font(int width, int height)
    {
        var map = Enumerable.Range(0, 256).Select(value => (byte)value).ToArray();
        var glyphs = Enumerable.Range(0, 256)
            .Select(_ => new IndexedFontGlyph(width, height,
                Enumerable.Repeat((byte)1, width * height).ToArray()))
            .ToArray();
        return new(map, glyphs);
    }
}
