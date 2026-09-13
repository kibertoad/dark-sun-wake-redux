using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public enum AddExistingControlKind
{
    Title,
    Add,
    Exit,
    Delete,
    Row,
    ScrollUp,
    ScrollDown,
    EditBox
}

public sealed record AddExistingControl(
    AddExistingControlKind Kind,
    int? RowIndex,
    uint ResourceNumber,
    uint? SourceImageResourceNumber,
    string? DisplayAssetPath,
    int X,
    int Y,
    int Width,
    int Height,
    ushort EventMask);

public static class AddExistingCharacterInput
{
    public const uint WindowResourceNumber = 18501;

    private sealed record ExpectedControl(
        AddExistingControlKind Kind,
        int? RowIndex,
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
        new(AddExistingControlKind.Title, null, 18300, 18107,
            "images/add-existing/title.dsix", 110, 0, 67, 23, 4),
        new(AddExistingControlKind.Add, null, 18301, 18108,
            "images/add-existing/add.dsix", 231, 30, 44, 15, 0),
        new(AddExistingControlKind.Exit, null, 18302, 18109,
            "images/character-generation/exit.dsix", 231, 50, 44, 15, 0),
        new(AddExistingControlKind.Delete, null, 18303, 18110,
            "images/add-existing/delete.dsix", 215, 148, 62, 15, 0),
        Row(0, 18304, 31), Row(1, 18305, 42), Row(2, 18306, 53),
        Row(3, 18307, 64), Row(4, 18308, 75), Row(5, 18309, 86),
        Row(6, 18310, 97), Row(7, 18311, 108), Row(8, 18312, 119),
        Row(9, 18313, 130),
        new(AddExistingControlKind.ScrollUp, null, 10314, 12102,
            "images/add-existing/scroll-up.dsix", 215, 30, 14, 14, 0),
        new(AddExistingControlKind.ScrollDown, null, 10315, 12101,
            "images/add-existing/scroll-down.dsix", 215, 130, 14, 14, 0),
        new(AddExistingControlKind.EditBox, null, 18401, null, null,
            49, 147, 164, 12, 10)
    ];

    public static IReadOnlyList<AddExistingControl> Resolve(PackedUiCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var window = UiWindowGraphResolver.Resolve(catalog, WindowResourceNumber);
        if (window.ImageResourceNumber != 10002 || window.Width != 320 || window.Height != 181)
            throw Error($"WIND #{WindowResourceNumber} has an unexpected shell contract");
        if (window.Controls.Count != Expected.Count)
            throw Error($"WIND #{WindowResourceNumber} must contain exactly {Expected.Count} controls");

        var result = new AddExistingControl[Expected.Count];
        for (var index = 0; index < result.Length; index++)
        {
            var actual = window.Controls[index];
            var expected = Expected[index];
            var expectedKind = expected.Kind == AddExistingControlKind.EditBox
                ? ResolvedUiControlKind.EditBox
                : ResolvedUiControlKind.Button;
            if (actual.Kind != expectedKind || actual.ResourceNumber != expected.ResourceNumber ||
                actual.ImageResourceNumber != expected.SourceImageResourceNumber ||
                actual.X != expected.X || actual.Y != expected.Y || actual.Width != expected.Width ||
                actual.Height != expected.Height || actual.EventMask != expected.EventMask)
                throw Error($"WIND #{WindowResourceNumber} control {index} does not match " +
                    $"the expected {expected.Kind} contract");
            result[index] = new(expected.Kind, expected.RowIndex, actual.ResourceNumber,
                actual.ImageResourceNumber, expected.DisplayAssetPath, actual.X, actual.Y,
                actual.Width, actual.Height, actual.EventMask);
        }
        return result;
    }

    public static AddExistingControl? HitTest(
        IReadOnlyList<AddExistingControl> controls,
        int logicalX,
        int logicalY)
    {
        ArgumentNullException.ThrowIfNull(controls);
        foreach (var control in controls)
        {
            if (control.Kind is AddExistingControlKind.Title or AddExistingControlKind.EditBox)
                continue;
            if (logicalX >= control.X && logicalX < control.X + control.Width &&
                logicalY >= control.Y && logicalY < control.Y + control.Height)
                return control;
        }
        return null;
    }

    public static StartFlowCommand? CommandFor(AddExistingControl control)
    {
        ArgumentNullException.ThrowIfNull(control);
        return control.Kind == AddExistingControlKind.Exit
            ? StartFlowCommand.Cancel()
            : null;
    }

    private static ExpectedControl Row(int index, uint resourceNumber, short y) =>
        new(AddExistingControlKind.Row, index, resourceNumber, 18100,
            "images/add-existing/row.dsix", 46, y, 163, 11, 208);

    private static InvalidDataException Error(string message) => new($"{message}.");
}
