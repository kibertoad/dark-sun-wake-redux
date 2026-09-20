using DarkSunWakeRedux.Core;

namespace DarkSunWakeRedux.Game;

public sealed class ExplorationRightMouseInput
{
    public const int PanNumerator = 13;
    public const int PanDenominator = 10;

    private bool _tracking;
    private bool _dragged;
    private GridPoint? _previousPointer;
    private int _remainderX;
    private int _remainderY;

    public ExplorationCommand? Update(
        bool worldActive,
        bool pressed,
        GridPoint? logicalPointer)
    {
        if (!worldActive)
        {
            Reset();
            return null;
        }
        if (!pressed)
        {
            if (!_tracking) return null;
            var cycle = !_dragged;
            Reset();
            return cycle
                ? new(ExplorationCommandKind.CycleCursorMode)
                : null;
        }
        if (!_tracking)
        {
            _tracking = true;
            _dragged = false;
            _previousPointer = logicalPointer;
            return null;
        }
        if (logicalPointer is null)
        {
            _previousPointer = null;
            _remainderX = 0;
            _remainderY = 0;
            return null;
        }
        if (_previousPointer is not { } previous)
        {
            _previousPointer = logicalPointer;
            return null;
        }

        var deltaX = (long)logicalPointer.Value.X - previous.X;
        var deltaY = (long)logicalPointer.Value.Y - previous.Y;
        _previousPointer = logicalPointer;
        if (deltaX == 0 && deltaY == 0) return null;
        _dragged = true;
        return ExplorationCommand.Pan(
            Scale(-deltaX, ref _remainderX),
            Scale(-deltaY, ref _remainderY));
    }

    private static int Scale(long delta, ref int remainder)
    {
        var numerator = delta * PanNumerator + remainder;
        var result = numerator / PanDenominator;
        remainder = checked((int)(numerator % PanDenominator));
        return checked((int)Math.Clamp(result, int.MinValue, int.MaxValue));
    }

    private void Reset()
    {
        _tracking = false;
        _dragged = false;
        _previousPointer = null;
        _remainderX = 0;
        _remainderY = 0;
    }
}
