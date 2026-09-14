using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public static class DialogueSessionAdapter
{
    public static DialogueSession Create(
        FirstTyrDialogueProjection projection,
        DialogueVariableSnapshot variables,
        int maximumResponses)
    {
        ArgumentNullException.ThrowIfNull(projection);
        var definitions = projection.InitialChoices.Select((choice, index) =>
            new DialogueChoiceDefinition(index, choice.TargetOffset,
                DialogueConditionAdapter.ToCore(choice.Condition))).ToArray();
        var secondDefinitions = projection.SecondChoices.Count == 0
            ? null
            : projection.SecondChoices.Select((choice, index) =>
                new DialogueChoiceDefinition(index, choice.TargetOffset,
                    DialogueConditionAdapter.ToCore(choice.Condition))).ToArray();
        var thirdDefinitions = projection.ThirdChoices.Count == 0
            ? null
            : projection.ThirdChoices.Select((choice, index) =>
                new DialogueChoiceDefinition(index, choice.TargetOffset,
                    DialogueConditionAdapter.ToCore(choice.Condition))).ToArray();
        return new(OriginalContent.FirstTyrDialogueScriptResourceNumber,
            definitions, variables, maximumResponses, secondDefinitions, thirdDefinitions);
    }

    public static DialogueBranchResult ToCore(
        FirstTyrDialogueCompletionProjection projection)
    {
        ArgumentNullException.ThrowIfNull(projection);
        if (!projection.ReturnsFromLocalBranch)
            throw new InvalidDataException("The dialogue completion branch does not return.");
        return new(projection.SourceChoiceIndex, projection.EntryOffset,
            DialogueBranchDisposition.Completed,
            projection.LocalFlagAssignments.ToDictionary(
                assignment => assignment.VariableId,
                assignment => assignment.Value));
    }

    public static DialogueBranchResult ToCore(
        FirstTyrDialogueResponseProjection projection)
    {
        ArgumentNullException.ThrowIfNull(projection);
        if (projection.ReturnsToMenu == projection.AdvancesToThirdMenu)
            throw new InvalidDataException(
                "The dialogue response must have exactly one supported menu destination.");
        return new(projection.SourceChoiceIndex, projection.EntryOffset,
            projection.AdvancesToThirdMenu
                ? DialogueBranchDisposition.AdvanceToThirdPage
                : DialogueBranchDisposition.ReturnThroughFirstTyrMenu,
            projection.LocalFlagAssignments.ToDictionary(
                assignment => assignment.VariableId,
                assignment => assignment.Value),
            projection.LocalNumberIncrements.ToDictionary(
                increment => increment.VariableId,
                increment => increment.Amount),
            projection.GlobalFlagAssignments.ToDictionary(
                assignment => assignment.VariableId,
                assignment => assignment.Value),
            projection.ConditionalLocalFlagAssignments.Select(assignment =>
                new DialogueConditionalLocalFlagAssignment(
                    new(assignment.Condition.VariableId, assignment.Condition.Value),
                    assignment.VariableId, assignment.Value)).ToArray(),
            projection.LocalNumberAssignments.ToDictionary(
                assignment => assignment.VariableId,
                assignment => assignment.Value),
            projection.ConditionalLocalFlagAssignmentsFromLocalFlags.Select(assignment =>
                new DialogueConditionalLocalFlagAssignmentFromLocalFlag(
                    new(assignment.Condition.VariableId, assignment.Condition.Value),
                    assignment.VariableId, assignment.Value)).ToArray(),
            projection.RequiredGlobalNumberConditions.Select(condition =>
                new DialogueGlobalNumberCondition(condition.VariableId, condition.Value,
                    ToCore(condition.Comparison))).ToArray(),
            conditionalLocalFlagAssignmentsFromGlobalNumbers:
                projection.ConditionalLocalFlagAssignmentsFromGlobalNumbers
                    .Select(assignment =>
                        new DialogueConditionalLocalFlagAssignmentFromGlobalNumber(
                            new(assignment.Condition.VariableId,
                                assignment.Condition.Value,
                                ToCore(assignment.Condition.Comparison)),
                            assignment.VariableId, assignment.Value)).ToArray(),
            conditionalGlobalNumberOrAssignments:
                projection.ConditionalGlobalNumberOrAssignments.Select(assignment =>
                    new DialogueConditionalGlobalNumberOrAssignment(
                        new(assignment.Condition.VariableId,
                            assignment.Condition.Value,
                            ToCore(assignment.Condition.Comparison)),
                        assignment.VariableId, assignment.Mask)).ToArray());
    }

    public static DialogueBranchResult ToCore(
        FirstTyrDialogueThirdCompletionProjection projection)
    {
        ArgumentNullException.ThrowIfNull(projection);
        if (!projection.ReturnsFromLocalBranch)
            throw new InvalidDataException(
                "The third-menu completion branch does not return.");
        return new(projection.SourceChoiceIndex, projection.EntryOffset,
            DialogueBranchDisposition.Completed,
            projection.LocalFlagAssignments.ToDictionary(
                assignment => assignment.VariableId,
                assignment => assignment.Value),
            conditionalLocalFlagAssignmentsAfterLocalFlags:
                projection.ConditionalLocalFlagAssignmentsAfterLocalFlags
                    .Select(assignment =>
                        new DialogueConditionalLocalFlagAssignmentAfterLocalFlags(
                            assignment.Conditions.Select(condition =>
                                new DialogueLocalFlagCondition(
                                    condition.VariableId, condition.Value)).ToArray(),
                            assignment.VariableId, assignment.Value))
                    .ToArray());
    }

    public static IReadOnlyList<GplDialogueOutput> ResolveOutput(
        FirstTyrDialogueResponseProjection projection,
        DialogueVariableSnapshot variables)
    {
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(variables);
        if (projection.OutputPaths.Count == 0) return projection.Output;
        foreach (var path in projection.OutputPaths)
        {
            var matches = true;
            foreach (var condition in path.Conditions)
            {
                if (!variables.GlobalNumbers.TryGetValue(condition.VariableId,
                        out var value))
                    throw new InvalidOperationException(
                        $"Global dialogue number #{condition.VariableId} is unknown.");
                matches &= condition.Comparison switch
                {
                    GplGlobalNumberComparison.Equal => value == condition.Value,
                    GplGlobalNumberComparison.NotEqual => value != condition.Value,
                    GplGlobalNumberComparison.BitwiseAndNonZero =>
                        (value & condition.Value) != 0,
                    GplGlobalNumberComparison.BitwiseAndZero =>
                        (value & condition.Value) == 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(condition),
                        condition.Comparison, "Unknown global-number comparison.")
                };
                if (!matches) break;
            }
            if (matches) return path.Output;
        }
        throw new InvalidOperationException(
            "No dialogue output path matches the known global-number state.");
    }

    private static DialogueGlobalNumberComparison ToCore(
        GplGlobalNumberComparison comparison) => comparison switch
        {
            GplGlobalNumberComparison.Equal => DialogueGlobalNumberComparison.Equal,
            GplGlobalNumberComparison.NotEqual => DialogueGlobalNumberComparison.NotEqual,
            GplGlobalNumberComparison.BitwiseAndNonZero =>
                DialogueGlobalNumberComparison.BitwiseAndNonZero,
            GplGlobalNumberComparison.BitwiseAndZero =>
                DialogueGlobalNumberComparison.BitwiseAndZero,
            _ => throw new ArgumentOutOfRangeException(nameof(comparison), comparison,
                "Unknown GPL global-number comparison.")
        };
}
