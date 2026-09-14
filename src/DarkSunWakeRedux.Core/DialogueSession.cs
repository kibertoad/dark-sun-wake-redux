using System.Collections.ObjectModel;

namespace DarkSunWakeRedux.Core;

public enum DialoguePhase { AwaitingChoice, BranchSelected, Completed }

public sealed record DialogueChoiceIdentity(int SourceIndex, int BranchTargetOffset);

public sealed record DialogueChoiceDefinition(
    int SourceIndex,
    int BranchTargetOffset,
    DialogueCondition Condition);

public enum DialogueBranchDisposition
{
    ReturnToChoices,
    ReturnThroughFirstTyrMenu,
    Completed
}

public sealed record DialogueGlobalFlagCondition(ushort VariableId, bool Value);

public sealed record DialogueLocalFlagCondition(ushort VariableId, bool Value);

public sealed record DialogueConditionalLocalFlagAssignment(
    DialogueGlobalFlagCondition Condition,
    ushort VariableId,
    bool Value);

public sealed record DialogueConditionalLocalFlagAssignmentFromLocalFlag(
    DialogueLocalFlagCondition Condition,
    ushort VariableId,
    bool Value);

public sealed record DialogueBranchResult
{
    public DialogueBranchResult(
        int sourceIndex,
        int branchTargetOffset,
        DialogueBranchDisposition disposition,
        IReadOnlyDictionary<ushort, bool> localFlagAssignments,
        IReadOnlyDictionary<ushort, int>? localNumberIncrements = null,
        IReadOnlyDictionary<ushort, bool>? globalFlagAssignments = null,
        IReadOnlyList<DialogueConditionalLocalFlagAssignment>?
            conditionalLocalFlagAssignments = null,
        IReadOnlyDictionary<ushort, int>? localNumberAssignments = null,
        IReadOnlyList<DialogueConditionalLocalFlagAssignmentFromLocalFlag>?
            conditionalLocalFlagAssignmentsFromLocalFlags = null)
    {
        if (sourceIndex < 0) throw new ArgumentOutOfRangeException(nameof(sourceIndex));
        if (branchTargetOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(branchTargetOffset));
        if (!Enum.IsDefined(disposition))
            throw new ArgumentOutOfRangeException(nameof(disposition));
        ArgumentNullException.ThrowIfNull(localFlagAssignments);
        localNumberIncrements ??= new Dictionary<ushort, int>();
        globalFlagAssignments ??= new Dictionary<ushort, bool>();
        conditionalLocalFlagAssignments ??= [];
        localNumberAssignments ??= new Dictionary<ushort, int>();
        conditionalLocalFlagAssignmentsFromLocalFlags ??= [];
        if (localNumberIncrements.Any(pair => pair.Value == 0))
            throw new ArgumentException("Dialogue number increments cannot be zero.",
                nameof(localNumberIncrements));
        if (localNumberAssignments.Keys.Intersect(localNumberIncrements.Keys).Any())
            throw new ArgumentException(
                "A dialogue result cannot assign and increment the same local number.",
                nameof(localNumberAssignments));
        if (conditionalLocalFlagAssignments.Any(assignment =>
                assignment is null || assignment.Condition is null))
            throw new ArgumentException(
                "Conditional dialogue assignments require conditions.",
                nameof(conditionalLocalFlagAssignments));
        if (conditionalLocalFlagAssignmentsFromLocalFlags.Any(assignment =>
                assignment is null || assignment.Condition is null))
            throw new ArgumentException(
                "Conditional dialogue assignments require local-flag conditions.",
                nameof(conditionalLocalFlagAssignmentsFromLocalFlags));
        SourceIndex = sourceIndex;
        BranchTargetOffset = branchTargetOffset;
        Disposition = disposition;
        LocalFlagAssignments = ReadOnlyCopy(localFlagAssignments);
        LocalNumberIncrements = ReadOnlyCopy(localNumberIncrements);
        GlobalFlagAssignments = ReadOnlyCopy(globalFlagAssignments);
        ConditionalLocalFlagAssignments = Array.AsReadOnly(
            conditionalLocalFlagAssignments.ToArray());
        LocalNumberAssignments = ReadOnlyCopy(localNumberAssignments);
        ConditionalLocalFlagAssignmentsFromLocalFlags = Array.AsReadOnly(
            conditionalLocalFlagAssignmentsFromLocalFlags.ToArray());
    }

    public int SourceIndex { get; }
    public int BranchTargetOffset { get; }
    public DialogueBranchDisposition Disposition { get; }
    public IReadOnlyDictionary<ushort, bool> LocalFlagAssignments { get; }
    public IReadOnlyDictionary<ushort, int> LocalNumberIncrements { get; }
    public IReadOnlyDictionary<ushort, bool> GlobalFlagAssignments { get; }
    public IReadOnlyList<DialogueConditionalLocalFlagAssignment>
        ConditionalLocalFlagAssignments { get; }
    public IReadOnlyDictionary<ushort, int> LocalNumberAssignments { get; }
    public IReadOnlyList<DialogueConditionalLocalFlagAssignmentFromLocalFlag>
        ConditionalLocalFlagAssignmentsFromLocalFlags { get; }

    public DialogueBranchResult ForSourceIndex(int sourceIndex) => new(
        sourceIndex,
        BranchTargetOffset,
        Disposition,
        LocalFlagAssignments,
        LocalNumberIncrements,
        GlobalFlagAssignments,
        ConditionalLocalFlagAssignments,
        LocalNumberAssignments,
        ConditionalLocalFlagAssignmentsFromLocalFlags);

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
    private IReadOnlyList<DialogueChoiceDefinition> _definitions;
    private readonly IReadOnlyList<DialogueChoiceDefinition>? _secondPageDefinitions;
    private readonly int _maximumChoices;
    private bool _onSecondPage;
    private DialogueSnapshot _snapshot;

    public DialogueSession(
        uint scriptResourceNumber,
        IReadOnlyList<DialogueChoiceDefinition> definitions,
        DialogueVariableSnapshot variables,
        int maximumChoices,
        IReadOnlyList<DialogueChoiceDefinition>? secondPageDefinitions = null)
    {
        if (scriptResourceNumber == 0)
            throw new ArgumentOutOfRangeException(nameof(scriptResourceNumber));
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(variables);
        ArgumentNullException.ThrowIfNull(variables.LocalFlags);
        ArgumentNullException.ThrowIfNull(variables.LocalNumbers);
        ArgumentNullException.ThrowIfNull(variables.GlobalFlags);
        if (maximumChoices <= 0) throw new ArgumentOutOfRangeException(nameof(maximumChoices));
        _definitions = ValidateDefinitions(definitions, nameof(definitions));
        _secondPageDefinitions = secondPageDefinitions is null
            ? null
            : ValidateDefinitions(secondPageDefinitions, nameof(secondPageDefinitions));
        _maximumChoices = maximumChoices;
        var stableVariables = Copy(variables);
        _snapshot = new(scriptResourceNumber, DialoguePhase.AwaitingChoice,
            SelectVisible(stableVariables, _definitions), null, null, stableVariables);
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
        var conditionalAssignments = new List<DialogueConditionalLocalFlagAssignment>();
        foreach (var assignment in result.ConditionalLocalFlagAssignments)
        {
            if (!_snapshot.Variables.GlobalFlags.TryGetValue(
                    assignment.Condition.VariableId, out var value))
                throw new InvalidOperationException(
                    $"Global dialogue flag #{assignment.Condition.VariableId} is unknown.");
            if (value == assignment.Condition.Value)
                conditionalAssignments.Add(assignment);
        }
        var conditionalLocalAssignments =
            new List<DialogueConditionalLocalFlagAssignmentFromLocalFlag>();
        foreach (var assignment in result.ConditionalLocalFlagAssignmentsFromLocalFlags)
        {
            if (!_snapshot.Variables.LocalFlags.TryGetValue(
                    assignment.Condition.VariableId, out var value))
                throw new InvalidOperationException(
                    $"Local dialogue flag #{assignment.Condition.VariableId} is unknown.");
            if (value == assignment.Condition.Value)
                conditionalLocalAssignments.Add(assignment);
        }
        var flags = new Dictionary<ushort, bool>(_snapshot.Variables.LocalFlags);
        foreach (var assignment in result.LocalFlagAssignments)
            flags[assignment.Key] = assignment.Value;
        foreach (var assignment in conditionalAssignments)
            flags[assignment.VariableId] = assignment.Value;
        foreach (var assignment in conditionalLocalAssignments)
            flags[assignment.VariableId] = assignment.Value;
        var numbers = new Dictionary<ushort, int>(_snapshot.Variables.LocalNumbers);
        foreach (var assignment in result.LocalNumberAssignments)
            numbers[assignment.Key] = assignment.Value;
        foreach (var increment in result.LocalNumberIncrements)
        {
            if (!numbers.TryGetValue(increment.Key, out var value))
                throw new InvalidOperationException(
                    $"Local dialogue number #{increment.Key} cannot be incremented while unknown.");
            numbers[increment.Key] = checked(value + increment.Value);
        }
        var globalFlags = new Dictionary<ushort, bool>(_snapshot.Variables.GlobalFlags);
        foreach (var assignment in result.GlobalFlagAssignments)
            globalFlags[assignment.Key] = assignment.Value;
        var variables = Copy(new(flags, numbers) { GlobalFlags = globalFlags });
        var nextDefinitions = _definitions;
        var onSecondPage = _onSecondPage;
        if (result.Disposition == DialogueBranchDisposition.ReturnThroughFirstTyrMenu &&
            !_onSecondPage)
        {
            var continuation = FirstTyrDialogueFlow.ContinueOpeningMenu(variables);
            variables = continuation.Variables;
            if (continuation.AdvancesToSecondMenu)
            {
                nextDefinitions = _secondPageDefinitions ?? throw new InvalidOperationException(
                    "The second dialogue page is unavailable.");
                onSecondPage = true;
            }
        }
        _snapshot = result.Disposition switch
        {
            DialogueBranchDisposition.ReturnToChoices or
                DialogueBranchDisposition.ReturnThroughFirstTyrMenu => _snapshot with
            {
                Phase = DialoguePhase.AwaitingChoice,
                Choices = SelectVisible(variables, nextDefinitions),
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
        _definitions = nextDefinitions;
        _onSecondPage = onSecondPage;
    }

    private IReadOnlyList<DialogueChoiceIdentity> SelectVisible(
        DialogueVariableSnapshot variables,
        IReadOnlyList<DialogueChoiceDefinition> definitions)
    {
        var selected = DialogueChoiceSelector.SelectVisible(
            definitions.Select(definition => definition.Condition).ToArray(),
            variables, _maximumChoices);
        return Array.AsReadOnly(selected.Select(index =>
        {
            var definition = definitions[index];
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
            new Dictionary<ushort, int>(variables.LocalNumbers)))
    {
        GlobalFlags = new ReadOnlyDictionary<ushort, bool>(
            new Dictionary<ushort, bool>(variables.GlobalFlags))
    };

    private static IReadOnlyList<DialogueChoiceDefinition> ValidateDefinitions(
        IReadOnlyList<DialogueChoiceDefinition> definitions,
        string parameterName)
    {
        if (definitions.Count == 0)
            throw new ArgumentException("A dialogue requires at least one choice definition.",
                parameterName);
        if (definitions.Any(definition => definition is null || definition.SourceIndex < 0 ||
                definition.BranchTargetOffset < 0 || definition.Condition is null) ||
            definitions.Select(definition => definition.SourceIndex).Distinct().Count() !=
                definitions.Count)
            throw new ArgumentException(
                "Dialogue definitions require unique non-negative source indexes, " +
                "branch targets, and conditions.", parameterName);
        return Array.AsReadOnly(definitions.ToArray());
    }
}
