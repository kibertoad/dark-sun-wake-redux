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

    [Fact]
    public void VisibleChoiceSelectionIsOrderedBoundedAndFailClosed()
    {
        var conditions = new DialogueCondition[]
        {
            new(DialogueConditionKind.LocalFlag, 9, 1),
            new(DialogueConditionKind.Constant, 0, 1),
            new(DialogueConditionKind.LocalFlag, 1, 1),
            new(DialogueConditionKind.LocalNumberEquals, 3, 7),
            new(DialogueConditionKind.Constant, 0, 1)
        };

        Assert.Equal([1, 2],
            DialogueChoiceSelector.SelectVisible(conditions, Variables, 2));
        Assert.Equal([1, 2, 3, 4],
            DialogueChoiceSelector.SelectVisible(conditions, Variables, 5));
        Assert.Equal([1, 4], DialogueChoiceSelector.SelectVisible(
            conditions, DialogueVariableSnapshot.Empty, 5));
    }

    [Fact]
    public void VisibleChoiceSelectionRejectsInvalidInputs()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DialogueChoiceSelector.SelectVisible([], Variables, 0));
        Assert.Throws<ArgumentException>(() =>
            DialogueChoiceSelector.SelectVisible([null!], Variables, 1));
    }
}
