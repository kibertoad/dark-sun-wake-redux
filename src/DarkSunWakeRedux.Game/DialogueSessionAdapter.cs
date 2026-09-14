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
        return new(OriginalContent.FirstTyrDialogueScriptResourceNumber,
            definitions, variables, maximumResponses);
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
        if (!projection.ReturnsToOpeningMenu)
            throw new InvalidDataException("The dialogue response does not return to its menu.");
        return new(projection.SourceChoiceIndex, projection.EntryOffset,
            DialogueBranchDisposition.ReturnToChoices,
            projection.LocalFlagAssignments.ToDictionary(
                assignment => assignment.VariableId,
                assignment => assignment.Value),
            projection.LocalNumberIncrements.ToDictionary(
                increment => increment.VariableId,
                increment => increment.Amount));
    }
}
