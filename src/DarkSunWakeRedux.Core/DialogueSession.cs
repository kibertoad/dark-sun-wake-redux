namespace DarkSunWakeRedux.Core;

public enum DialoguePhase
{
    AwaitingChoice,
    BranchSelected
}

public sealed record DialogueChoiceIdentity(int SourceIndex, int BranchTargetOffset);

public enum DialogueCommandKind
{
    SelectResponse
}

public sealed record DialogueCommand(DialogueCommandKind Kind, int ResponseIndex)
{
    public static DialogueCommand Select(int responseIndex) =>
        new(DialogueCommandKind.SelectResponse, responseIndex);
}

public sealed record DialogueSnapshot(
    uint ScriptResourceNumber,
    DialoguePhase Phase,
    IReadOnlyList<DialogueChoiceIdentity> Choices,
    int? SelectedSourceIndex,
    int? BranchTargetOffset);

public sealed record DialogueTransition(
    DialogueSnapshot Before,
    DialogueCommand Command,
    DialogueSnapshot After,
    bool Applied);

public sealed class DialogueSession
{
    private DialogueSnapshot _snapshot;

    public DialogueSession(
        uint scriptResourceNumber,
        IReadOnlyList<DialogueChoiceIdentity> choices)
    {
        if (scriptResourceNumber == 0)
            throw new ArgumentOutOfRangeException(nameof(scriptResourceNumber));
        ArgumentNullException.ThrowIfNull(choices);
        if (choices.Count == 0)
            throw new ArgumentException("A dialogue page requires at least one choice.",
                nameof(choices));
        if (choices.Any(choice => choice is null || choice.SourceIndex < 0 ||
                choice.BranchTargetOffset < 0) ||
            choices.Select(choice => choice.SourceIndex).Distinct().Count() != choices.Count)
            throw new ArgumentException(
                "Dialogue choices require unique non-negative source indexes and branch targets.",
                nameof(choices));
        var stableChoices = Array.AsReadOnly(choices.ToArray());
        _snapshot = new(scriptResourceNumber, DialoguePhase.AwaitingChoice,
            stableChoices, null, null);
    }

    public DialogueSnapshot Snapshot() => _snapshot;

    public DialogueTransition Execute(DialogueCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.Kind != DialogueCommandKind.SelectResponse)
            throw new ArgumentOutOfRangeException(nameof(command), command.Kind,
                "Unknown dialogue command kind.");
        if (command.ResponseIndex < 0 || command.ResponseIndex >= _snapshot.Choices.Count)
            throw new ArgumentOutOfRangeException(nameof(command),
                "The dialogue response index is outside the current page.");
        var before = _snapshot;
        if (_snapshot.Phase == DialoguePhase.AwaitingChoice)
        {
            var choice = _snapshot.Choices[command.ResponseIndex];
            _snapshot = _snapshot with
            {
                Phase = DialoguePhase.BranchSelected,
                SelectedSourceIndex = choice.SourceIndex,
                BranchTargetOffset = choice.BranchTargetOffset
            };
        }
        return new(before, command, _snapshot, before != _snapshot);
    }
}
