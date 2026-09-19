namespace DarkSunWakeRedux.Core;

/// <summary>
/// A player request whose game-specific resolution is owned by the combat slice.
/// </summary>
public enum CombatCommandKind
{
    Guard,
    TargetNext,
    TargetPrevious,
    EndTurn,
    Wait,
    DisableComputerControl
}

public sealed record CombatCommand
{
    public CombatCommand(CombatCommandKind kind)
    {
        if (!Enum.IsDefined(kind))
            throw new ArgumentOutOfRangeException(nameof(kind), kind,
                "Combat command kind is not defined.");
        Kind = kind;
    }

    public CombatCommandKind Kind { get; }
}
