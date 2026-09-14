using System.Collections.ObjectModel;

namespace DarkSunWakeRedux.Core;

public enum DialoguePhase
{
    AwaitingChoice,
    BranchSelected,
    Completed
}

public sealed record DialogueChoiceIdentity(int SourceIndex, int BranchTargetOffset);

public enum DialogueCommandKind
{
    SelectResponse,
    CompleteBranch
}

public sealed record DialogueBranchResult
{
    public DialogueBranchResult(
        int sourceIndex,
        int branchTargetOffset,
        IReadOnlyDictionary<ushort, bool> localFlagAssignments)
    {
        if (sourceIndex < 0 || branchTargetOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(sourceIndex));
        ArgumentNullException.ThrowIfNull(localFlagAssignments);
        SourceIndex = sourceIndex;
        BranchTargetOffset = branchTargetOffset;
        LocalFlagAssignments = new ReadOnlyDictionary<ushort, bool>(
            new Dictionary<ushort, bool>(localFlagAssignments));
    }

    public int SourceIndex { get; }
    public int BranchTargetOffset { get; }
    public IReadOnlyDictionary<ushort, bool> LocalFlagAssignments { get; }
}

public sealed record DialogueCommand(
    DialogueCommandKind Kind,
    int ResponseIndex,
    DialogueBranchResult? BranchResult)
{
    public static DialogueCommand Select(int responseIndex) =>
        new(DialogueCommandKind.SelectResponse, responseIndex, null);

    public static DialogueCommand Complete(DialogueBranchResult result) =>
        new(DialogueCommandKind.CompleteBranch, -1,
            result ?? throw new ArgumentNullException(nameof(result)));
}

public sealed record DialogueSnapshot(
    uint ScriptResourceNumber,
    DialoguePhase Phase,
    IReadOnlyList<DialogueChoiceIdentity> Choices,
    int? SelectedSourceIndex,
    int? BranchTargetOffset,
    DialogueVariableSnapshot Variables);

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
        IReadOnlyList<DialogueChoiceIdentity> choices,
        DialogueVariableSnapshot? variables = null)
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
        variables ??= DialogueVariableSnapshot.Empty;
        ArgumentNullException.ThrowIfNull(variables.LocalFlags);
        ArgumentNullException.ThrowIfNull(variables.LocalNumbers);
        var stableVariables = new DialogueVariableSnapshot(
            new ReadOnlyDictionary<ushort, bool>(
                new Dictionary<ushort, bool>(variables.LocalFlags)),
            new ReadOnlyDictionary<ushort, int>(
                new Dictionary<ushort, int>(variables.LocalNumbers)));
        _snapshot = new(scriptResourceNumber, DialoguePhase.AwaitingChoice,
            stableChoices, null, null, stableVariables);
    }

    public DialogueSnapshot Snapshot() => _snapshot;

    public DialogueTransition Execute(DialogueCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (!Enum.IsDefined(command.Kind))
            throw new ArgumentOutOfRangeException(nameof(command), command.Kind,
                "Unknown dialogue command kind.");
        Validate(command);
        var before = _snapshot;
        if (command.Kind == DialogueCommandKind.SelectResponse &&
            _snapshot.Phase == DialoguePhase.AwaitingChoice)
        {
            var choice = _snapshot.Choices[command.ResponseIndex];
            _snapshot = _snapshot with
            {
                Phase = DialoguePhase.BranchSelected,
                SelectedSourceIndex = choice.SourceIndex,
                BranchTargetOffset = choice.BranchTargetOffset
            };
        }
        else if (command.Kind == DialogueCommandKind.CompleteBranch &&
            _snapshot.Phase == DialoguePhase.BranchSelected)
        {
            var result = command.BranchResult!;
            if (result.SourceIndex != _snapshot.SelectedSourceIndex ||
                result.BranchTargetOffset != _snapshot.BranchTargetOffset)
                throw new InvalidOperationException(
                    "The completed dialogue branch does not match the selected choice.");
            var flags = new Dictionary<ushort, bool>(_snapshot.Variables.LocalFlags);
            foreach (var assignment in result.LocalFlagAssignments)
                flags[assignment.Key] = assignment.Value;
            _snapshot = _snapshot with
            {
                Phase = DialoguePhase.Completed,
                Variables = _snapshot.Variables with
                {
                    LocalFlags = new ReadOnlyDictionary<ushort, bool>(flags)
                }
            };
        }
        return new(before, command, _snapshot, before != _snapshot);
    }

    private void Validate(DialogueCommand command)
    {
        if (command.Kind == DialogueCommandKind.SelectResponse &&
            command.BranchResult is null &&
            (command.ResponseIndex < 0 || command.ResponseIndex >= _snapshot.Choices.Count))
            throw new ArgumentOutOfRangeException(nameof(command),
                "The dialogue response index is outside the current page.");
        var valid = command.Kind switch
        {
            DialogueCommandKind.SelectResponse => command.BranchResult is null &&
                command.ResponseIndex >= 0 && command.ResponseIndex < _snapshot.Choices.Count,
            DialogueCommandKind.CompleteBranch => command.ResponseIndex == -1 &&
                command.BranchResult is not null,
            _ => false
        };
        if (!valid)
            throw new ArgumentException(
                "The dialogue command payload does not match its kind.", nameof(command));
    }
}
