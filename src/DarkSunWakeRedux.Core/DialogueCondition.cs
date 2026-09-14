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
    IReadOnlyDictionary<ushort, int> LocalNumbers)
{
    public static DialogueVariableSnapshot Empty { get; } = new(
        new Dictionary<ushort, bool>(),
        new Dictionary<ushort, int>());
}

public static class FirstTyrDialogueObservedState
{
    public static DialogueVariableSnapshot Create() => new(
        new Dictionary<ushort, bool>
        {
            [0] = true,
            [1] = true,
            [2] = true,
            [3] = true
        },
        new Dictionary<ushort, int> { [0] = 0 });
}

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

public static class DialogueChoiceSelector
{
    public static IReadOnlyList<int> SelectVisible(
        IReadOnlyList<DialogueCondition> conditions,
        DialogueVariableSnapshot variables,
        int maximumChoices)
    {
        ArgumentNullException.ThrowIfNull(conditions);
        ArgumentNullException.ThrowIfNull(variables);
        if (maximumChoices <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumChoices));

        var visible = new List<int>(Math.Min(conditions.Count, maximumChoices));
        for (var index = 0; index < conditions.Count && visible.Count < maximumChoices; index++)
        {
            if (conditions[index] is null)
                throw new ArgumentException("Dialogue conditions cannot contain null entries.",
                    nameof(conditions));
            if (DialogueConditionEvaluator.Evaluate(conditions[index], variables) ==
                DialogueConditionResult.True)
                visible.Add(index);
        }
        return visible;
    }
}
