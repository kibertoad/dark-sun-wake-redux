using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public sealed record DialogueResponseControl(
    int Index,
    uint ResourceNumber,
    int X,
    int Y,
    int Width,
    int Height,
    string AssetPath);

public sealed record DialogueOverlayRectangle(
    int X,
    int Y,
    int Width,
    int Height,
    Rgb24 FillColor);

public sealed record DialogueOverlayImage(
    uint ResourceNumber,
    string AssetPath,
    int X,
    int Y);

public sealed record DialogueOverlayLayout(
    DialogueOverlayRectangle SpeechWindow,
    DialogueOverlayRectangle ResponseWindow,
    int PortraitX,
    int PortraitY,
    IReadOnlyList<DialogueOverlayImage> Images);

public static class DialogueInput
{
    public const int SpeechOriginX = 1;
    public const int SpeechOriginY = 0;
    public const int ResponseOriginX = 1;
    public const int ResponseOriginY = 142;
    public static readonly Rgb24 SpeechFillColor = new(0, 0, 0);
    public static readonly Rgb24 ResponseFillColor = new(125, 125, 125);

    private static readonly IReadOnlyDictionary<uint, (int Index, string AssetName)> Rows =
        new Dictionary<uint, (int, string)>
        {
            [2076] = (0, "dialogue-response-1"),
            [2077] = (1, "dialogue-response-2"),
            [2078] = (2, "dialogue-response-3"),
            [2079] = (3, "dialogue-response-4"),
            [2080] = (4, "dialogue-response-5")
        };

    public static IReadOnlyList<DialogueResponseControl> ResolveResponses(
        PackedUiCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ValidateSpeechWindow(catalog);
        var window = UiWindowGraphResolver.Resolve(
            catalog, OriginalContent.DialogueResponseWindowResourceNumber);
        if (window.Width != 318 || window.Height != 58 || window.ImageResourceNumber != 0)
            throw Error("The dialogue response window has unexpected geometry or image");
        var buttons = window.Controls.Where(control =>
            control.Kind == ResolvedUiControlKind.Button).ToArray();
        if (buttons.Length != 7)
            throw Error("The dialogue response window has an unexpected button count");
        var assets = OriginalContent.InteractionButtonAssets.ToDictionary(
            asset => asset.Name, StringComparer.Ordinal);
        var rows = new List<DialogueResponseControl>(Rows.Count);
        foreach (var button in buttons)
        {
            if (!Rows.TryGetValue(button.ResourceNumber, out var mapping)) continue;
            var asset = assets[mapping.AssetName];
            if (button.ImageResourceNumber != asset.ImageResourceNumber ||
                button.Width != 300 || button.Height != 10)
                throw Error($"Dialogue response BUTN #{button.ResourceNumber} has drifted");
            rows.Add(new(mapping.Index, button.ResourceNumber,
                ResponseOriginX + button.X, ResponseOriginY + button.Y,
                button.Width, button.Height, asset.Path));
        }
        if (rows.Count != Rows.Count ||
            !rows.OrderBy(row => row.Index).Select(row => row.Index)
                .SequenceEqual(Enumerable.Range(0, Rows.Count)))
            throw Error("The dialogue response rows are incomplete");
        return rows.OrderBy(row => row.Index).ToArray();
    }

    public static DialogueResponseControl? HitTest(
        IReadOnlyList<DialogueResponseControl> controls,
        int logicalX,
        int logicalY) => controls.LastOrDefault(control =>
            logicalX >= control.X && logicalX < control.X + control.Width &&
            logicalY >= control.Y && logicalY < control.Y + control.Height);

    public static DialogueOverlayLayout ResolveOverlay(PackedUiCatalog catalog)
    {
        var responses = ResolveResponses(catalog);
        var speech = UiWindowGraphResolver.Resolve(
            catalog, OriginalContent.DialogueSpeechWindowResourceNumber);
        var response = UiWindowGraphResolver.Resolve(
            catalog, OriginalContent.DialogueResponseWindowResourceNumber);
        var interactionAssets = OriginalContent.InteractionButtonAssets.ToDictionary(
            asset => asset.Name, StringComparer.Ordinal);
        var scrollUp = OriginalContent.AddExistingCharacterAssets.Single(
            asset => asset.Name == "scroll-up");
        var more = interactionAssets["dialogue-more"];

        var upperScroll = RequireButton(speech, 2093, scrollUp.ResourceNumber, 305, 4);
        var upperMore = RequireButton(speech, 2094, more.ImageResourceNumber, 305, 18);
        var lowerScroll = RequireButton(response, 2095, scrollUp.ResourceNumber, 305, 4);
        var lowerMore = RequireButton(response, 2096, more.ImageResourceNumber, 305, 18);
        var images = new List<DialogueOverlayImage>(4 + responses.Count)
        {
            Place(upperScroll, scrollUp.Path, SpeechOriginX, SpeechOriginY),
            Place(upperMore, more.Path, SpeechOriginX, SpeechOriginY),
            Place(lowerScroll, scrollUp.Path, ResponseOriginX, ResponseOriginY),
            Place(lowerMore, more.Path, ResponseOriginX, ResponseOriginY)
        };
        images.AddRange(responses.Select(row =>
            new DialogueOverlayImage(row.ResourceNumber, row.AssetPath, row.X, row.Y)));
        return new(
            new(SpeechOriginX, SpeechOriginY, speech.Width, speech.Height, SpeechFillColor),
            new(ResponseOriginX, ResponseOriginY, response.Width, response.Height,
                ResponseFillColor),
            SpeechOriginX, SpeechOriginY,
            images);
    }

    private static void ValidateSpeechWindow(PackedUiCatalog catalog)
    {
        var window = UiWindowGraphResolver.Resolve(
            catalog, OriginalContent.DialogueSpeechWindowResourceNumber);
        if (window.Width != 318 || window.Height != 72 || window.ImageResourceNumber != 0 ||
            window.Controls.Count != 4)
            throw Error("The dialogue speech window has unexpected geometry or controls");
        var editBox = window.Controls.SingleOrDefault(control =>
            control.Kind == ResolvedUiControlKind.EditBox);
        if (editBox is null || editBox.ResourceNumber != 12400 ||
            editBox.X != 75 || editBox.Y != 6 || editBox.Width != 236 ||
            editBox.Height != 46 || editBox.EventMask != 4)
            throw Error("The dialogue speech edit box has drifted");
    }

    private static ResolvedUiControl RequireButton(
        ResolvedUiWindow window,
        uint resourceNumber,
        uint imageResourceNumber,
        int x,
        int y)
    {
        var button = window.Controls.SingleOrDefault(control =>
            control.Kind == ResolvedUiControlKind.Button &&
            control.ResourceNumber == resourceNumber);
        if (button is null || button.ImageResourceNumber != imageResourceNumber ||
            button.X != x || button.Y != y)
            throw Error($"Dialogue BUTN #{resourceNumber} has drifted");
        return button;
    }

    private static DialogueOverlayImage Place(
        ResolvedUiControl control,
        string assetPath,
        int originX,
        int originY) => new(
            control.ResourceNumber, assetPath,
            originX + control.X, originY + control.Y);

    private static InvalidDataException Error(string message) => new($"{message}.");
}
