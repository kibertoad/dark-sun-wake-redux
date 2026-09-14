namespace DarkSunWakeRedux.Core;

public enum ExplorationInteractionTargetKind
{
    Creature,
    Object
}

[Flags]
public enum ExplorationInteractionCapabilities
{
    None = 0,
    Talk = 1,
    PickUp = 2,
    Use = 4
}

public enum ExplorationInteractionAction
{
    Talk,
    PickUp,
    Use,
    Dismiss
}

public enum ExplorationInteractionOutcome
{
    None,
    TalkRequested,
    PickUpRequested,
    UseRequested,
    Dismissed
}

public sealed record ExplorationInteractionTarget(
    uint ObjectResourceNumber,
    string Name,
    ExplorationInteractionTargetKind Kind,
    int? Level,
    bool Hostile,
    ExplorationInteractionCapabilities Capabilities);

public sealed record ExplorationInteractionSnapshot(ExplorationInteractionTarget? Target)
{
    public bool IsOpen => Target is not null;
}

public sealed record ExplorationInteractionTransition(
    ExplorationInteractionSnapshot Before,
    ExplorationInteractionSnapshot After,
    ExplorationInteractionAction? Action,
    ExplorationInteractionOutcome Outcome,
    bool Applied);

public sealed class ExplorationInteractionSession
{
    private ExplorationInteractionSnapshot _snapshot = new(null);

    public ExplorationInteractionSnapshot Snapshot() => _snapshot;

    public ExplorationInteractionTransition Open(ExplorationInteractionTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        Validate(target);
        var before = _snapshot;
        var soleAction = target.Kind == ExplorationInteractionTargetKind.Object
            ? SoleEnabledAction(target.Capabilities)
            : null;
        if (soleAction is { } action)
            return new(before, before, action, OutcomeFor(action), true);

        _snapshot = new(target);
        return new(before, _snapshot, null, ExplorationInteractionOutcome.None,
            before != _snapshot);
    }

    public ExplorationInteractionTransition Select(ExplorationInteractionAction action)
    {
        if (!Enum.IsDefined(action))
            throw new ArgumentOutOfRangeException(nameof(action));
        var before = _snapshot;
        if (before.Target is null)
            return new(before, before, action, ExplorationInteractionOutcome.None, false);
        if (action != ExplorationInteractionAction.Dismiss &&
            !Enabled(before.Target.Capabilities, action))
            return new(before, before, action, ExplorationInteractionOutcome.None, false);

        _snapshot = new(null);
        return new(before, _snapshot, action, OutcomeFor(action), true);
    }

    private static void Validate(ExplorationInteractionTarget target)
    {
        if (target.ObjectResourceNumber == 0)
            throw new ArgumentOutOfRangeException(nameof(target),
                "An interaction target must identify an object resource.");
        if (string.IsNullOrWhiteSpace(target.Name) || target.Name.Length > 64 ||
            target.Name.Any(character => character is < ' ' or > '~'))
            throw new ArgumentException(
                "An interaction target name must be 1-64 printable ASCII characters.",
                nameof(target));
        if (target.Level is <= 0 or > 99)
            throw new ArgumentOutOfRangeException(nameof(target),
                "A known interaction target level must be between 1 and 99.");
        const ExplorationInteractionCapabilities all =
            ExplorationInteractionCapabilities.Talk |
            ExplorationInteractionCapabilities.PickUp |
            ExplorationInteractionCapabilities.Use;
        if ((target.Capabilities & ~all) != 0 || !Enum.IsDefined(target.Kind))
            throw new ArgumentException("The interaction target contains unsupported values.",
                nameof(target));
    }

    private static ExplorationInteractionAction? SoleEnabledAction(
        ExplorationInteractionCapabilities capabilities)
    {
        var actions = new[]
        {
            ExplorationInteractionAction.Talk,
            ExplorationInteractionAction.PickUp,
            ExplorationInteractionAction.Use
        }.Where(action => Enabled(capabilities, action)).ToArray();
        return actions.Length == 1 ? actions[0] : null;
    }

    public static bool Enabled(
        ExplorationInteractionCapabilities capabilities,
        ExplorationInteractionAction action) => action switch
    {
        ExplorationInteractionAction.Talk =>
            capabilities.HasFlag(ExplorationInteractionCapabilities.Talk),
        ExplorationInteractionAction.PickUp =>
            capabilities.HasFlag(ExplorationInteractionCapabilities.PickUp),
        ExplorationInteractionAction.Use =>
            capabilities.HasFlag(ExplorationInteractionCapabilities.Use),
        ExplorationInteractionAction.Dismiss => true,
        _ => throw new ArgumentOutOfRangeException(nameof(action))
    };

    private static ExplorationInteractionOutcome OutcomeFor(
        ExplorationInteractionAction action) => action switch
    {
        ExplorationInteractionAction.Talk => ExplorationInteractionOutcome.TalkRequested,
        ExplorationInteractionAction.PickUp => ExplorationInteractionOutcome.PickUpRequested,
        ExplorationInteractionAction.Use => ExplorationInteractionOutcome.UseRequested,
        ExplorationInteractionAction.Dismiss => ExplorationInteractionOutcome.Dismissed,
        _ => throw new ArgumentOutOfRangeException(nameof(action))
    };
}
