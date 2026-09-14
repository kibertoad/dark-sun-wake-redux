using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public static class DialogueConditionAdapter
{
    public static DialogueCondition ToCore(GplDialogueCondition condition)
    {
        ArgumentNullException.ThrowIfNull(condition);
        var kind = condition.Kind switch
        {
            GplDialogueConditionKind.Constant => DialogueConditionKind.Constant,
            GplDialogueConditionKind.LocalFlag => DialogueConditionKind.LocalFlag,
            GplDialogueConditionKind.LocalNumberEquals =>
                DialogueConditionKind.LocalNumberEquals,
            _ => throw new ArgumentOutOfRangeException(
                nameof(condition), condition.Kind, "Unknown GPL dialogue condition kind.")
        };
        return new(kind, condition.VariableId, condition.Value);
    }
}
