using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public sealed record ExplorationDestinationControl(
    ExplorationView Destination,
    uint ResourceNumber,
    int X,
    int Y,
    int Width,
    int Height,
    string AssetPath);

public static class ExplorationDestinationInput
{
    private sealed record Page(
        uint WindowResourceNumber,
        int Width,
        int Height,
        IReadOnlyDictionary<uint, (int X, int Y)> Positions);

    private static readonly IReadOnlyDictionary<ExplorationView, Page> Pages =
        new Dictionary<ExplorationView, Page>
        {
            [ExplorationView.ViewCharacter] = CharacterPage(),
            [ExplorationView.ViewInventory] = new(13500, 320, 200,
                new Dictionary<uint, (int, int)>
                {
                    [10300] = (163, 181),
                    [11304] = (187, 181),
                    [11305] = (211, 181),
                    [11306] = (235, 181),
                    [10308] = (288, 181)
                }),
            [ExplorationView.CastSpellsOrUsePsionics] = CharacterPage(),
            [ExplorationView.CurrentSpellEffects] = CharacterPage()
        };

    private static Page CharacterPage() => new(11500, 320, 189,
        new Dictionary<uint, (int, int)>
        {
            [10300] = (43, 155),
            [11304] = (67, 155),
            [11305] = (91, 155),
            [11306] = (114, 155),
            [10308] = (253, 155)
        });

    private static readonly IReadOnlyDictionary<uint, ExplorationView> Destinations =
        new Dictionary<uint, ExplorationView>
        {
            [10300] = ExplorationView.ViewCharacter,
            [11304] = ExplorationView.ViewInventory,
            [11305] = ExplorationView.CastSpellsOrUsePsionics,
            [11306] = ExplorationView.CurrentSpellEffects,
            [10308] = ExplorationView.World
        };

    public static IReadOnlyList<ExplorationDestinationControl> Resolve(
        PackedUiCatalog catalog,
        ExplorationView pageView)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        if (!Pages.TryGetValue(pageView, out var page))
            throw new ArgumentOutOfRangeException(nameof(pageView), pageView,
                "The requested exploration destination page has no mapped UI graph.");
        var window = UiWindowGraphResolver.Resolve(catalog, page.WindowResourceNumber);
        if (window.Width != page.Width || window.Height != page.Height)
            throw new InvalidDataException(
                $"Exploration destination WIND #{window.ResourceNumber} has unexpected geometry.");

        var controls = new List<ExplorationDestinationControl>(page.Positions.Count);
        foreach (var (resourceNumber, position) in page.Positions)
        {
            var matches = window.Controls.Where(control =>
                control.Kind == ResolvedUiControlKind.Button &&
                control.ResourceNumber == resourceNumber).ToArray();
            if (matches.Length != 1)
                throw new InvalidDataException(
                    $"WIND #{window.ResourceNumber} must contain exactly one BUTN #{resourceNumber}.");
            var control = matches[0];
            var asset = OriginalContent.GameMenuButtons.Single(candidate =>
                candidate.ButtonResourceNumber == resourceNumber);
            if (control.X != position.X || control.Y != position.Y ||
                control.Width != asset.FrameWidth || control.Height != asset.FrameHeight ||
                control.ImageResourceNumber != asset.ImageResourceNumber)
                throw new InvalidDataException(
                    $"WIND #{window.ResourceNumber} BUTN #{resourceNumber} does not match its mapped contract.");
            controls.Add(new(Destinations[resourceNumber], resourceNumber,
                control.X, control.Y, control.Width, control.Height, asset.Path));
        }
        return controls;
    }

    public static ExplorationDestinationControl? HitTest(
        IReadOnlyList<ExplorationDestinationControl> controls,
        int logicalX,
        int logicalY) => controls.LastOrDefault(control =>
            logicalX >= control.X && logicalX < control.X + control.Width &&
            logicalY >= control.Y && logicalY < control.Y + control.Height);

    public static ExplorationCommand CommandFor(ExplorationDestinationControl control) =>
        control.Destination == ExplorationView.World
            ? new(ExplorationCommandKind.Escape)
            : ExplorationCommand.Open(control.Destination);
}
