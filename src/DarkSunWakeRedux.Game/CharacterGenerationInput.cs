using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public sealed record CharacterGenerationControl(
    uint ResourceNumber,
    uint? SourceImageResourceNumber,
    string? DisplayAssetPath,
    int X,
    int Y,
    int Width,
    int Height,
    ushort EventMask);

public static class CharacterGenerationInput
{
    public const uint WindowResourceNumber = 19503;
    public const uint ExitButtonResourceNumber = 18302;

    private sealed record ExpectedControl(
        string Tag,
        uint ResourceNumber,
        uint? SourceImageResourceNumber,
        string? DisplayAssetPath,
        short X,
        short Y,
        ushort Width,
        ushort Height,
        ushort EventMask);

    private static readonly IReadOnlyList<ExpectedControl> Expected =
    [
        Button(2001, 0, 0, 100, 116),
        ImageButton(2002), ImageButton(2003), ImageButton(2004), ImageButton(2005),
        ImageButton(2006), ImageButton(2007), ImageButton(2008), ImageButton(2009),
        Button(2010, 135, 75, 50, 42), Button(2027, 135, 20, 45, 35),
        ImageButton(ExitButtonResourceNumber), ImageButton(19304),
        Button(2011, 79, 145, 82, 7),
        Button(2012, 4, 139, 50, 5), Button(2013, 4, 146, 50, 5),
        Button(2014, 4, 153, 50, 5), Button(2015, 4, 160, 50, 5),
        Button(2016, 4, 167, 50, 5), Button(2017, 4, 174, 50, 5),
        Button(2018, 79, 174, 58, 5),
        new("EBOX", 4003, null, null, 40, 125, 95, 8, 10)
    ];

    public static IReadOnlyList<CharacterGenerationControl> Resolve(PackedUiCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var window = UiWindowGraphResolver.Resolve(catalog, WindowResourceNumber);
        if (window.ImageResourceNumber != 19004 || window.Width != 320 || window.Height != 200)
            throw Error($"WIND #{WindowResourceNumber} has an unexpected shell contract");
        if (window.Controls.Count != Expected.Count)
            throw Error($"WIND #{WindowResourceNumber} must contain exactly {Expected.Count} controls");

        var result = new CharacterGenerationControl[Expected.Count];
        for (var index = 0; index < result.Length; index++)
        {
            var actual = window.Controls[index];
            var expected = Expected[index];
            var expectedKind = expected.Tag == "EBOX"
                ? ResolvedUiControlKind.EditBox
                : ResolvedUiControlKind.Button;
            if (actual.Kind != expectedKind || actual.ResourceNumber != expected.ResourceNumber ||
                actual.ImageResourceNumber != expected.SourceImageResourceNumber ||
                actual.X != expected.X || actual.Y != expected.Y || actual.Width != expected.Width ||
                actual.Height != expected.Height || actual.EventMask != expected.EventMask)
                throw Error($"WIND #{WindowResourceNumber} control {index} does not match " +
                    $"the expected {expected.Tag} #{expected.ResourceNumber} contract");
            result[index] = new(actual.ResourceNumber, actual.ImageResourceNumber,
                expected.DisplayAssetPath, actual.X, actual.Y, actual.Width, actual.Height,
                actual.EventMask);
        }
        return result;
    }

    public static CharacterGenerationControl? HitTest(
        IReadOnlyList<CharacterGenerationControl> controls,
        int logicalX,
        int logicalY)
    {
        ArgumentNullException.ThrowIfNull(controls);
        foreach (var control in controls)
        {
            if (control.DisplayAssetPath is null)
                continue;
            if (logicalX >= control.X && logicalX < control.X + control.Width &&
                logicalY >= control.Y && logicalY < control.Y + control.Height)
                return control;
        }
        return null;
    }

    public static StartFlowCommand? CommandFor(CharacterGenerationControl control)
    {
        ArgumentNullException.ThrowIfNull(control);
        return control.ResourceNumber == ExitButtonResourceNumber
            ? StartFlowCommand.Cancel()
            : null;
    }

    private static ExpectedControl Button(
        uint resourceNumber,
        short x,
        short y,
        ushort width,
        ushort height) =>
        new("BUTN", resourceNumber, 0, null, x, y, width, height, 0);

    private static ExpectedControl ImageButton(uint resourceNumber)
    {
        var asset = OriginalContent.CharacterGenerationButtons.Single(
            item => item.ButtonResourceNumber == resourceNumber);
        return new("BUTN", resourceNumber, asset.ImageResourceNumber, asset.Path,
            checked((short)asset.X), checked((short)asset.Y),
            checked((ushort)asset.ControlWidth), checked((ushort)asset.ControlHeight), 0);
    }

    private static InvalidDataException Error(string message) => new($"{message}.");
}
