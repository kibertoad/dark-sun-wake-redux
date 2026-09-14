using DarkSunWakeRedux.Core;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class FirstTyrDialogueFlowTests
{
    [Fact]
    public void CounterZeroAdvancesAndInitializesTheSecondMenuFlags()
    {
        var continuation = FirstTyrDialogueFlow.ContinueOpeningMenu(
            FirstTyrDialogueObservedState.Create());

        Assert.True(continuation.AdvancesToSecondMenu);
        Assert.True(continuation.Variables.LocalFlags[4]);
        Assert.True(continuation.Variables.LocalFlags[1]);
        Assert.True(continuation.Variables.LocalFlags[8]);
        Assert.Equal(1, continuation.Variables.GlobalNumbers[22]);
        Assert.False(continuation.Variables.LocalFlags.ContainsKey(6));
        Assert.False(continuation.Variables.LocalFlags.ContainsKey(7));
    }

    [Fact]
    public void CounterTwoStaysOnTheFirstMenuAndEnablesItsFollowUp()
    {
        var variables = FirstTyrDialogueObservedState.Create() with
        {
            LocalFlags = new Dictionary<ushort, bool>
            {
                [0] = true, [1] = true, [2] = false, [3] = false,
                [4] = false, [5] = false, [9] = false
            },
            LocalNumbers = new Dictionary<ushort, int> { [0] = 2 }
        };

        var continuation = FirstTyrDialogueFlow.ContinueOpeningMenu(variables);

        Assert.False(continuation.AdvancesToSecondMenu);
        Assert.True(continuation.Variables.LocalFlags[0]);
        Assert.True(continuation.Variables.LocalFlags[5]);
    }

    [Fact]
    public void RequiresEveryOpeningVariableEvenWhenAnEarlierFlagIsTrue()
    {
        var variables = FirstTyrDialogueObservedState.Create() with
        {
            LocalFlags = new Dictionary<ushort, bool>
            {
                [0] = true, [1] = true, [3] = true, [4] = false, [5] = false,
                [9] = false
            }
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            FirstTyrDialogueFlow.ContinueOpeningMenu(variables));

        Assert.Contains("flag #2", exception.Message);
    }

    [Fact]
    public void SessionChangesDefinitionPagesOnlyAfterAValidatedContinuation()
    {
        var first = new[]
        {
            Definition(0, 1017, 0), Definition(1, 1597, 1),
            Definition(2, 1148, 2), Definition(3, 1183, 3)
        };
        var second = new[]
        {
            Definition(0, 1597, 1), Definition(1, 1825, 6),
            Definition(3, 1996, 7), Definition(5, 2415, 8),
            new DialogueChoiceDefinition(6, 2905,
                new(DialogueConditionKind.Constant, 0, 1))
        };
        var session = new DialogueSession(135, first,
            FirstTyrDialogueObservedState.Create(), 5, second);
        session.Execute(DialogueCommand.Select(0));

        var transition = session.Execute(DialogueCommand.Apply(new(0, 1017,
            DialogueBranchDisposition.ReturnThroughFirstTyrMenu,
            new Dictionary<ushort, bool> { [0] = false })));

        Assert.Equal([0, 5, 6],
            transition.After.Choices.Select(choice => choice.SourceIndex));
        Assert.Equal([1597, 2415, 2905],
            transition.After.Choices.Select(choice => choice.BranchTargetOffset));
    }

    [Fact]
    public void ReturningBranchOnSecondPageDoesNotRerunTheOpeningContinuation()
    {
        var first = new[] { Definition(0, 1017, 0) };
        var second = new[]
        {
            Definition(0, 1597, 1),
            new DialogueChoiceDefinition(1, 2905,
                new(DialogueConditionKind.Constant, 0, 1))
        };
        var session = new DialogueSession(135, first,
            FirstTyrDialogueObservedState.Create(), 5, second);
        session.Execute(DialogueCommand.Select(0));
        session.Execute(DialogueCommand.Apply(new(0, 1017,
            DialogueBranchDisposition.ReturnThroughFirstTyrMenu,
            new Dictionary<ushort, bool> { [0] = false })));
        session.Execute(DialogueCommand.Select(0));

        var transition = session.Execute(DialogueCommand.Apply(new(0, 1597,
            DialogueBranchDisposition.ReturnThroughFirstTyrMenu,
            new Dictionary<ushort, bool> { [1] = false },
            localNumberAssignments: new Dictionary<ushort, int> { [0] = 2 })));

        Assert.Equal([1], transition.After.Choices.Select(choice => choice.SourceIndex));
        Assert.False(transition.After.Variables.LocalFlags[5]);
    }

    private static DialogueChoiceDefinition Definition(
        int sourceIndex,
        int target,
        ushort flag) => new(sourceIndex, target,
            new(DialogueConditionKind.LocalFlag, flag, 1));
}
