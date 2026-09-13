using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public sealed record PartyOverviewInteractionSurface(
    uint ButtonResourceNumber,
    int X,
    int Y,
    int Width,
    int Height,
    ushort EventMask);

public static class PartyOverviewInput
{
    public const uint WindowResourceNumber = 19502;
    public const uint ButtonResourceNumber = 2099;

    public static PartyOverviewInteractionSurface Resolve(PackedUiCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var window = UiWindowGraphResolver.Resolve(catalog, WindowResourceNumber);
        if (window.ImageResourceNumber != 19004 || window.Width != 320 || window.Height != 200)
            throw Error($"WIND #{WindowResourceNumber} has an unexpected shell contract");
        var control = window.Controls.Count == 1
            ? window.Controls[0]
            : throw Error($"WIND #{WindowResourceNumber} must contain exactly one control");
        if (control.Kind != ResolvedUiControlKind.Button ||
            control.ResourceNumber != ButtonResourceNumber ||
            control.ImageResourceNumber != 0 || control.X != 0 || control.Y != 0 ||
            control.Width != 319 || control.Height != 199 || control.EventMask != 0)
            throw Error($"BUTN #{ButtonResourceNumber} does not match the application-surface contract");
        return new(control.ResourceNumber, control.X, control.Y,
            control.Width, control.Height, control.EventMask);
    }

    public static bool Contains(PartyOverviewInteractionSurface surface, int logicalX, int logicalY)
    {
        ArgumentNullException.ThrowIfNull(surface);
        return logicalX >= surface.X && logicalX < surface.X + surface.Width &&
            logicalY >= surface.Y && logicalY < surface.Y + surface.Height;
    }

    private static InvalidDataException Error(string message) => new($"{message}.");
}
