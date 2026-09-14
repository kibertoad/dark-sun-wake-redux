using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class DialogueConditionTests
{
    private static readonly DialogueVariableSnapshot Variables = new(
        new Dictionary<ushort, bool> { [1] = true, [2] = false },
        new Dictionary<ushort, int> { [3] = 7 });

    [Theory]
    [InlineData(0, DialogueConditionResult.False)]
    [InlineData(1, DialogueConditionResult.True)]
    [InlineData(-1, DialogueConditionResult.True)]
    public void EvaluatesConstants(int value, DialogueConditionResult expected) =>
        Assert.Equal(expected, DialogueConditionEvaluator.Evaluate(
            new(DialogueConditionKind.Constant, 0, value), Variables));

    [Theory]
    [InlineData(1, DialogueConditionResult.True)]
    [InlineData(2, DialogueConditionResult.False)]
    [InlineData(9, DialogueConditionResult.Unknown)]
    public void EvaluatesKnownAndUnknownLocalFlags(
        ushort id,
        DialogueConditionResult expected) =>
        Assert.Equal(expected, DialogueConditionEvaluator.Evaluate(
            new(DialogueConditionKind.LocalFlag, id, 1), Variables));

    [Theory]
    [InlineData(3, 7, DialogueConditionResult.True)]
    [InlineData(3, 8, DialogueConditionResult.False)]
    [InlineData(4, 0, DialogueConditionResult.Unknown)]
    public void EvaluatesKnownAndUnknownLocalNumbers(
        ushort id,
        int expectedValue,
        DialogueConditionResult expected) =>
        Assert.Equal(expected, DialogueConditionEvaluator.Evaluate(
            new(DialogueConditionKind.LocalNumberEquals, id, expectedValue), Variables));

    [Fact]
    public void AdapterPreservesTheParsedConditionContract()
    {
        var parsed = new GplDialogueCondition(
            GplDialogueConditionKind.LocalNumberEquals, 4, 12);

        Assert.Equal(new(DialogueConditionKind.LocalNumberEquals, 4, 12),
            DialogueConditionAdapter.ToCore(parsed));
    }
}
