using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ExplorationRightMouseInputTests
{
    [Fact]
    public void SingleRightClickStillCyclesTheCursorMode()
    {
        var input = new ExplorationRightMouseInput();

        Assert.Null(input.Update(true, true, new(100, 80)));
        var released = input.Update(true, false, new(100, 80));

        Assert.Equal(ExplorationCommandKind.CycleCursorMode, released!.Kind);
    }

    [Fact]
    public void HeldRightButtonPansOppositeEachLogicalPointerDelta()
    {
        var input = new ExplorationRightMouseInput();
        input.Update(true, true, new(100, 80));

        var first = input.Update(true, true, new(112, 75));
        var second = input.Update(true, true, new(110, 90));
        var released = input.Update(true, false, new(110, 90));

        Assert.Equal((-15, 6), (first!.DeltaX, first.DeltaY));
        Assert.Equal((2, -19), (second!.DeltaX, second.DeltaY));
        Assert.Null(released);
    }

    [Fact]
    public void FractionalRemainderMakesTenSinglePixelMovesProduceThirteenPixels()
    {
        var input = new ExplorationRightMouseInput();
        input.Update(true, true, new(100, 80));

        var deltas = Enumerable.Range(1, 10)
            .Select(offset => input.Update(true, true, new(100 + offset, 80))!.DeltaX)
            .ToArray();

        Assert.Equal(-13, deltas.Sum());
    }

    [Fact]
    public void ReversedSubpixelDragDoesNotEmitAnInvalidZeroPan()
    {
        var input = new ExplorationRightMouseInput();
        input.Update(true, true, new(100, 80));

        Assert.Equal(1, input.Update(true, true, new(99, 80))!.DeltaX);
        Assert.Equal(1, input.Update(true, true, new(98, 80))!.DeltaX);
        Assert.Equal(1, input.Update(true, true, new(97, 80))!.DeltaX);

        Assert.Null(input.Update(true, true, new(98, 80)));
        Assert.Null(input.Update(true, false, new(98, 80)));
    }

    [Fact]
    public void ExtremePointerCoordinatesSaturateInsteadOfWrappingPanDeltas()
    {
        var input = new ExplorationRightMouseInput();
        input.Update(true, true, new(int.MinValue, int.MinValue));

        var pan = input.Update(true, true, new(int.MaxValue, int.MaxValue));

        Assert.Equal((int.MinValue, int.MinValue), (pan!.DeltaX, pan.DeltaY));
    }

    [Fact]
    public void LeavingTheCanvasReanchorsWithoutCameraJump()
    {
        var input = new ExplorationRightMouseInput();
        input.Update(true, true, new(20, 20));
        Assert.Null(input.Update(true, true, null));
        Assert.Null(input.Update(true, true, new(200, 150)));

        var pan = input.Update(true, true, new(198, 151));

        Assert.Equal((2, -1), (pan!.DeltaX, pan.DeltaY));
    }

    [Fact]
    public void InactiveWorldCancelsAStartedGesture()
    {
        var input = new ExplorationRightMouseInput();
        input.Update(true, true, new(20, 20));

        Assert.Null(input.Update(false, true, new(30, 30)));
        Assert.Null(input.Update(true, false, new(30, 30)));
    }
}
