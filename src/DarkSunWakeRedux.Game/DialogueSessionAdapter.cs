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
        var visible = DialogueConditionAdapter.SelectVisibleChoices(
            projection.InitialChoices, variables, maximumResponses);
        return new(OriginalContent.FirstTyrDialogueScriptResourceNumber,
            visible.Select(index => new DialogueChoiceIdentity(
                index, projection.InitialChoices[index].TargetOffset)).ToArray());
    }
}
