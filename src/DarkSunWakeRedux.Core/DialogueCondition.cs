namespace DarkSunWakeRedux.Core;

public enum DialogueConditionKind
{
    Constant,
    LocalFlag,
    LocalNumberEquals
}

public enum DialogueConditionResult
{
    False,
    True,
    Unknown
}

public sealed record DialogueCondition(
    DialogueConditionKind Kind,
    ushort VariableId,
    int Value);

public sealed record DialogueVariableSnapshot(
    IReadOnlyDictionary<ushort, bool> LocalFlags,
    IReadOnlyDictionary<ushort, int> LocalNumbers);

public static class DialogueConditionEvaluator
{
    public static DialogueConditionResult Evaluate(
        DialogueCondition condition,
        DialogueVariableSnapshot variables)
    {
        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(variables);
        ArgumentNullException.ThrowIfNull(variables.LocalFlags);
        ArgumentNullException.ThrowIfNull(variables.LocalNumbers);
        return condition.Kind switch
        {
            DialogueConditionKind.Constant => condition.Value == 0
                ? DialogueConditionResult.False
                : DialogueConditionResult.True,
            DialogueConditionKind.LocalFlag =>
                variables.LocalFlags.TryGetValue(condition.VariableId, out var flag)
                    ? flag ? DialogueConditionResult.True : DialogueConditionResult.False
                    : DialogueConditionResult.Unknown,
            DialogueConditionKind.LocalNumberEquals =>
                variables.LocalNumbers.TryGetValue(condition.VariableId, out var number)
                    ? number == condition.Value
                        ? DialogueConditionResult.True
                        : DialogueConditionResult.False
                    : DialogueConditionResult.Unknown,
            _ => throw new ArgumentOutOfRangeException(
                nameof(condition), condition.Kind, "Unknown dialogue condition kind.")
        };
    }
}
