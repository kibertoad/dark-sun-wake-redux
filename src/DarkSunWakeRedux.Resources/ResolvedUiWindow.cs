namespace DarkSunWakeRedux.Resources;

public enum ResolvedUiControlKind
{
    Button,
    ApplicationFrame,
    EditBox
}

public sealed record ResolvedUiControl(
    ResolvedUiControlKind Kind,
    uint ResourceNumber,
    short X,
    short Y,
    ushort Width,
    ushort Height,
    ushort EventMask,
    uint? ImageResourceNumber);

public sealed record ResolvedUiWindow(
    uint ResourceNumber,
    uint ImageResourceNumber,
    ushort Width,
    ushort Height,
    IReadOnlyList<ResolvedUiControl> Controls);

public static class UiWindowGraphResolver
{
    public static ResolvedUiWindow Resolve(PackedUiCatalog catalog, uint windowResourceNumber)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var windows = Unique(catalog.Windows, item => item.ResourceNumber, "WIND");
        var buttons = Unique(catalog.Buttons, item => item.ResourceNumber, "BUTN");
        var frames = Unique(catalog.ApplicationFrames, item => item.ResourceNumber, "APFM");
        var editBoxes = Unique(catalog.EditBoxes, item => item.ResourceNumber, "EBOX");

        if (!windows.TryGetValue(windowResourceNumber, out var window))
            throw Error($"UI catalog does not contain WIND #{windowResourceNumber}");
        RequireDimensions(window.Width, window.Height, $"WIND #{windowResourceNumber}");
        if (window.Children is null || window.Children.Count > UiWindowResource.MaximumChildren)
            throw Error($"WIND #{windowResourceNumber} has an invalid child count");

        var controls = new ResolvedUiControl[window.Children.Count];
        for (var index = 0; index < controls.Length; index++)
        {
            var child = window.Children[index];
            controls[index] = child.Tag switch
            {
                "BUTN" when buttons.TryGetValue(child.ResourceNumber, out var button) =>
                    new(ResolvedUiControlKind.Button, child.ResourceNumber, child.X, child.Y,
                        button.Width, button.Height, button.EventMask, button.ImageResourceNumber),
                "APFM" when frames.TryGetValue(child.ResourceNumber, out var frame) =>
                    new(ResolvedUiControlKind.ApplicationFrame, child.ResourceNumber, child.X, child.Y,
                        frame.Width, frame.Height, frame.EventMask, null),
                "EBOX" when editBoxes.TryGetValue(child.ResourceNumber, out var editBox) =>
                    new(ResolvedUiControlKind.EditBox, child.ResourceNumber, child.X, child.Y,
                        editBox.Width, editBox.Height, editBox.EventMask, null),
                "BUTN" or "APFM" or "EBOX" => throw Error(
                    $"WIND #{windowResourceNumber} references missing {child.Tag} #{child.ResourceNumber}"),
                _ => throw Error(
                    $"WIND #{windowResourceNumber} contains unsupported child tag '{child.Tag}'")
            };
            RequireDimensions(controls[index].Width, controls[index].Height,
                $"{child.Tag} #{child.ResourceNumber}");
        }

        return new(window.ResourceNumber, window.ImageResourceNumber, window.Width, window.Height, controls);
    }

    private static Dictionary<uint, T> Unique<T>(
        IReadOnlyList<T>? records,
        Func<T, uint> resourceNumber,
        string tag)
    {
        if (records is null) throw Error($"UI catalog has a null {tag} collection");
        var result = new Dictionary<uint, T>(records.Count);
        foreach (var record in records)
        {
            var number = resourceNumber(record);
            if (!result.TryAdd(number, record))
                throw Error($"UI catalog contains duplicate {tag} #{number}");
        }
        return result;
    }

    private static InvalidDataException Error(string message) => new($"{message}.");

    private static void RequireDimensions(ushort width, ushort height, string resource)
    {
        if (width is 0 or > IndexedImage.MaximumDimension ||
            height is 0 or > IndexedImage.MaximumDimension)
            throw Error($"{resource} has invalid dimensions {width}x{height}");
    }
}
