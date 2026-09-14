using System.Collections.ObjectModel;

namespace DarkSunWakeRedux.Core;

public enum DialoguePhase { AwaitingChoice, BranchSelected, Completed }

public sealed record DialogueChoiceIdentity(int SourceIndex, int BranchTargetOffset);

public sealed record DialogueChoiceDefinition(
    int SourceIndex,
    int BranchTargetOffset,
    DialogueCondition Condition);

public enum DialogueBranchDisposition { ReturnToChoices, Completed }

public sealed record DialogueBranchResult
{
    public DialogueBranchResult(
        int sourceIndex,
        int branchTargetOffset,
        DialogueBranchDisposition disposition,
        IReadOnlyDictionary<ushort, bool> localFlagAssignments,
        IReadOnlyDictionary<ushort, int>? localNumberIncrements = null)
    {
        if (sourceIndex < 0) throw new ArgumentOutOfRangeException(nameof(sourceIndex));
        if (branchTargetOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(branchTargetOffset));
        if (!Enum.IsDefined(disposition))
            throw new ArgumentOutOfRangeException(nameof(disposition));
        ArgumentNullException.ThrowIfNull(localFlagAssignments);
        localNumberIncrements ??= new Dictionary<ushort, int>();
        if (localNumberIncrements.Any(pair => pair.Value == 0))
            throw new ArgumentException("Dialogue number increments cannot be zero.",
                nameof(localNumberIncrements));
        SourceIndex = sourceIndex;
        BranchTargetOffset = branchTargetOffset;
        Disposition = disposition;
        LocalFlagAssignments = ReadOnlyCopy(localFlagAssignments);
        LocalNumberIncrements = ReadOnlyCopy(localNumberIncrements);
    }

    public int SourceIndex { get; }
    public int BranchTargetOffset { get; }
    public DialogueBranchDisposition Disposition { get; }
    public IReadOnlyDictionary<ushort, bool> LocalFlagAssignments { get; }
    public IReadOnlyDictionary<ushort, int> LocalNumberIncrements { get; }

    private static IReadOnlyDictionary<TKey, TValue> ReadOnlyCopy<TKey, TValue>(
        IReadOnlyDictionary<TKey, TValue> source) where TKey : notnull =>
        new ReadOnlyDictionary<TKey, TValue>(new Dictionary<TKey, TValue>(source));
}

public enum DialogueCommandKind { SelectResponse, ApplyBranch }

public sealed record DialogueCommand(
    DialogueCommandKind Kind,
    int ResponseIndex,
    DialogueBranchResult? BranchResult)
{
    public static DialogueCommand Select(int responseIndex) =>
        new(DialogueCommandKind.SelectResponse, responseIndex, null);

    public static DialogueCommand Apply(DialogueBranchResult result) =>
        new(DialogueCommandKind.ApplyBranch, -1,
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
    private readonly IReadOnlyList<DialogueChoiceDefinition> _definitions;
    private readonly int _maximumChoices;
    private DialogueSnapshot _snapshot;

    public DialogueSession(
        uint scriptResourceNumber,
        IReadOnlyList<DialogueChoiceDefinition> definitions,
        DialogueVariableSnapshot variables,
        int maximumChoices)
    {
        if (scriptResourceNumber == 0)
            throw new ArgumentOutOfRangeException(nameof(scriptResourceNumber));
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(variables);
        ArgumentNullException.ThrowIfNull(variables.LocalFlags);
        ArgumentNullException.ThrowIfNull(variables.LocalNumbers);
        if (maximumChoices <= 0) throw new ArgumentOutOfRangeException(nameof(maximumChoices));
        if (definitions.Count == 0)
            throw new ArgumentException("A dialogue requires at least one choice definition.",
                nameof(definitions));
        if (definitions.Any(definition => definition is null || definition.SourceIndex < 0 ||
                definition.BranchTargetOffset < 0 || definition.Condition is null) ||
            definitions.Select(definition => definition.SourceIndex).Distinct().Count() !=
                definitions.Count)
            throw new ArgumentException(
                "Dialogue definitions require unique non-negative source indexes, " +
                "branch targets, and conditions.", nameof(definitions));
        _definitions = Array.AsReadOnly(definitions.ToArray());
        _maximumChoices = maximumChoices;
        var stableVariables = Copy(variables);
        _snapshot = new(scriptResourceNumber, DialoguePhase.AwaitingChoice,
            SelectVisible(stableVariables), null, null, stableVariables);
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
        else if (command.Kind == DialogueCommandKind.ApplyBranch &&
            _snapshot.Phase == DialoguePhase.BranchSelected)
            ApplyResult(command.BranchResult!);
        return new(before, command, _snapshot, before != _snapshot);
    }

    private void ApplyResult(DialogueBranchResult result)
    {
        if (result.SourceIndex != _snapshot.SelectedSourceIndex ||
            result.BranchTargetOffset != _snapshot.BranchTargetOffset)
            throw new InvalidOperationException(
                "The applied dialogue branch does not match the selected choice.");
        var flags = new Dictionary<ushort, bool>(_snapshot.Variables.LocalFlags);
        foreach (var assignment in result.LocalFlagAssignments)
            flags[assignment.Key] = assignment.Value;
        var numbers = new Dictionary<ushort, int>(_snapshot.Variables.LocalNumbers);
        foreach (var increment in result.LocalNumberIncrements)
        {
            if (!numbers.TryGetValue(increment.Key, out var value))
                throw new InvalidOperationException(
                    $"Local dialogue number #{increment.Key} cannot be incremented while unknown.");
            numbers[increment.Key] = checked(value + increment.Value);
        }
        var variables = Copy(new(flags, numbers));
        _snapshot = result.Disposition switch
        {
            DialogueBranchDisposition.ReturnToChoices => _snapshot with
            {
                Phase = DialoguePhase.AwaitingChoice,
                Choices = SelectVisible(variables),
                SelectedSourceIndex = null,
                BranchTargetOffset = null,
                Variables = variables
            },
            DialogueBranchDisposition.Completed => _snapshot with
            {
                Phase = DialoguePhase.Completed,
                Variables = variables
            },
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Disposition,
                "Unknown dialogue branch disposition.")
        };
    }

    private IReadOnlyList<DialogueChoiceIdentity> SelectVisible(
        DialogueVariableSnapshot variables)
    {
        var selected = DialogueChoiceSelector.SelectVisible(
            _definitions.Select(definition => definition.Condition).ToArray(),
            variables, _maximumChoices);
        return Array.AsReadOnly(selected.Select(index =>
        {
            var definition = _definitions[index];
            return new DialogueChoiceIdentity(
                definition.SourceIndex, definition.BranchTargetOffset);
        }).ToArray());
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
            DialogueCommandKind.ApplyBranch => command.ResponseIndex == -1 &&
                command.BranchResult is not null,
            _ => false
        };
        if (!valid)
            throw new ArgumentException(
                "The dialogue command payload does not match its kind.", nameof(command));
    }

    private static DialogueVariableSnapshot Copy(DialogueVariableSnapshot variables) => new(
        new ReadOnlyDictionary<ushort, bool>(
            new Dictionary<ushort, bool>(variables.LocalFlags)),
        new ReadOnlyDictionary<ushort, int>(
            new Dictionary<ushort, int>(variables.LocalNumbers)));
}
