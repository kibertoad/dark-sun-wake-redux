using Microsoft.Xna.Framework.Input;

namespace DarkSunWakeRedux.Game;

/// <summary>The options the launch options screen lists, in their order (DEV-UI-001).</summary>
public enum LaunchOption
{
    /// <summary>DEV-EXPLORE-001.</summary>
    WideMapView
}

public enum LaunchOptionsOutcome { Open, Confirmed, ExitRequested }

public enum LaunchOptionsCommandKind { Previous, Next, Change, Activate, Exit, Hover, Click }

public readonly record struct LaunchOptionsCommand(LaunchOptionsCommandKind Kind, int Item = -1)
{
    public static LaunchOptionsCommand Previous() => new(LaunchOptionsCommandKind.Previous);
    public static LaunchOptionsCommand Next() => new(LaunchOptionsCommandKind.Next);
    public static LaunchOptionsCommand Change() => new(LaunchOptionsCommandKind.Change);
    public static LaunchOptionsCommand Activate() => new(LaunchOptionsCommandKind.Activate);
    public static LaunchOptionsCommand Exit() => new(LaunchOptionsCommandKind.Exit);
    public static LaunchOptionsCommand Hover(int item) => new(LaunchOptionsCommandKind.Hover, item);
    public static LaunchOptionsCommand Click(int item) => new(LaunchOptionsCommandKind.Click, item);
}

/// <summary>
/// The state of the launch options screen: the values being edited, the highlighted item and
/// whether the player has confirmed or quit. Items are the options in
/// <see cref="Options"/> order, then Confirm, then Exit.
/// </summary>
public sealed class LaunchOptionsSession(LaunchSettings initial)
{
    public static IReadOnlyList<LaunchOption> Options { get; } = [LaunchOption.WideMapView];
    public static int ConfirmItem => Options.Count;
    public static int ExitItem => Options.Count + 1;
    public static int ItemCount => Options.Count + 2;

    public LaunchSettings Settings { get; private set; } =
        initial ?? throw new ArgumentNullException(nameof(initial));
    public int Selected { get; private set; }
    public LaunchOptionsOutcome Outcome { get; private set; } = LaunchOptionsOutcome.Open;

    public void Execute(LaunchOptionsCommand command)
    {
        if (Outcome != LaunchOptionsOutcome.Open) return;
        switch (command.Kind)
        {
            case LaunchOptionsCommandKind.Previous:
                Selected = (Selected + ItemCount - 1) % ItemCount;
                break;
            case LaunchOptionsCommandKind.Next:
                Selected = (Selected + 1) % ItemCount;
                break;
            case LaunchOptionsCommandKind.Change:
                if (Selected < Options.Count) ChangeOption(Options[Selected]);
                break;
            case LaunchOptionsCommandKind.Activate:
                Activate();
                break;
            case LaunchOptionsCommandKind.Exit:
                Outcome = LaunchOptionsOutcome.ExitRequested;
                break;
            case LaunchOptionsCommandKind.Hover when IsItem(command.Item):
                Selected = command.Item;
                break;
            case LaunchOptionsCommandKind.Click when IsItem(command.Item):
                Selected = command.Item;
                Activate();
                break;
        }
    }

    public static string Label(LaunchOption option) => option switch
    {
        LaunchOption.WideMapView => "Wide map view",
        _ => throw new ArgumentOutOfRangeException(nameof(option))
    };

    public static string ValueText(LaunchSettings settings, LaunchOption option) => option switch
    {
        LaunchOption.WideMapView => settings.WideMapView ? "On" : "Off",
        _ => throw new ArgumentOutOfRangeException(nameof(option))
    };

    public static IReadOnlyList<string> Description(LaunchOption option) => option switch
    {
        LaunchOption.WideMapView =>
        [
            "On: the map fills the whole display.",
            "Off: the original 320x200 view, with bars."
        ],
        _ => throw new ArgumentOutOfRangeException(nameof(option))
    };

    private static bool IsItem(int item) => item >= 0 && item < ItemCount;

    private void Activate()
    {
        if (Selected < Options.Count) ChangeOption(Options[Selected]);
        else if (Selected == ConfirmItem) Outcome = LaunchOptionsOutcome.Confirmed;
        else Outcome = LaunchOptionsOutcome.ExitRequested;
    }

    private void ChangeOption(LaunchOption option) => Settings = option switch
    {
        LaunchOption.WideMapView => Settings with { WideMapView = !Settings.WideMapView },
        _ => throw new ArgumentOutOfRangeException(nameof(option))
    };
}

/// <summary>Maps keys and the pointer to launch options commands.</summary>
public static class LaunchOptionsInput
{
    public static IReadOnlyList<LaunchOptionsCommand> Resolve(
        KeyboardState current,
        KeyboardState previous,
        int? pointerItem,
        bool pointerMoved,
        bool leftClicked)
    {
        var commands = new List<LaunchOptionsCommand>();
        if (pointerItem is { } hovered && pointerMoved)
            commands.Add(LaunchOptionsCommand.Hover(hovered));
        if (Pressed(Keys.Up)) commands.Add(LaunchOptionsCommand.Previous());
        if (Pressed(Keys.Down) || Pressed(Keys.Tab)) commands.Add(LaunchOptionsCommand.Next());
        if (Pressed(Keys.Left) || Pressed(Keys.Right)) commands.Add(LaunchOptionsCommand.Change());
        // Alt+Enter belongs to the full-screen toggle (DEV-INPUT-001).
        var altDown = current.IsKeyDown(Keys.LeftAlt) || current.IsKeyDown(Keys.RightAlt);
        if ((Pressed(Keys.Enter) && !altDown) || Pressed(Keys.Space))
            commands.Add(LaunchOptionsCommand.Activate());
        if (Pressed(Keys.Escape)) commands.Add(LaunchOptionsCommand.Exit());
        if (leftClicked && pointerItem is { } clicked)
            commands.Add(LaunchOptionsCommand.Click(clicked));
        return commands;

        bool Pressed(Keys key) => current.IsKeyDown(key) && previous.IsKeyUp(key);
    }
}

public readonly record struct LaunchOptionsBounds(int X, int Y, int Width, int Height)
{
    public bool Contains(int x, int y) =>
        x >= X && y >= Y && x < X + Width && y < Y + Height;
}

/// <summary>
/// Where the launch options screen puts its parts on the 320x200 logical canvas. The layout is
/// the rebuild's own and follows no screen of the original.
/// </summary>
public static class LaunchOptionsLayout
{
    public const int TitleY = 10;
    public const int SubtitleY = 24;
    public static LaunchOptionsBounds Panel { get; } = new(20, 40, 280, 96);
    public const int OptionRowTop = 48;
    public const int OptionRowHeight = 14;
    public const int OptionTextLeft = 32;
    public const int OptionTextRight = 288;
    public const int DescriptionTop = 108;
    public const int DescriptionLineHeight = 11;
    public const int ButtonTop = 150;
    public const int ButtonWidth = 72;
    public const int ButtonHeight = 16;
    public const int HintY = 180;
    public const int MaximumTextHeight = 11;

    public static LaunchOptionsBounds Item(int item)
    {
        if (item < 0 || item >= LaunchOptionsSession.ItemCount)
            throw new ArgumentOutOfRangeException(nameof(item));
        if (item < LaunchOptionsSession.Options.Count)
            return new(Panel.X + 4, OptionRowTop + item * OptionRowHeight,
                Panel.Width - 8, OptionRowHeight);
        var center = LogicalCanvasTransform.LogicalWidth / 2;
        var left = item == LaunchOptionsSession.ConfirmItem
            ? center - 8 - ButtonWidth
            : center + 8;
        return new(left, ButtonTop, ButtonWidth, ButtonHeight);
    }

    public static int? HitTest(int logicalX, int logicalY)
    {
        for (var item = 0; item < LaunchOptionsSession.ItemCount; item++)
            if (Item(item).Contains(logicalX, logicalY))
                return item;
        return null;
    }
}
