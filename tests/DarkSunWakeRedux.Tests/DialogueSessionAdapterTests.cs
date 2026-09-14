using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class DialogueSessionAdapterTests
{
    [Fact]
    public void CreatesObservedFiveResponsePageWithOriginalBranchTargets()
    {
        var projection = new FirstTyrDialogueProjection(18, [],
            Enumerable.Range(0, 8).Select(index => new GplDialogueChoice(
                new(GplDialogueTextSourceKind.Literal, $"choice {index}", 0, 0),
                1000 + index,
                new(index < 4
                        ? GplDialogueConditionKind.LocalFlag
                        : index == 7
                            ? GplDialogueConditionKind.Constant
                            : GplDialogueConditionKind.LocalFlag,
                    checked((ushort)index), 1))).ToArray());

        var snapshot = DialogueSessionAdapter.Create(
            projection, FirstTyrDialogueObservedState.Create(), 5).Snapshot();

        Assert.Equal([0, 1, 2, 3, 7],
            snapshot.Choices.Select(choice => choice.SourceIndex));
        Assert.Equal([1000, 1001, 1002, 1003, 1007],
            snapshot.Choices.Select(choice => choice.BranchTargetOffset));
        Assert.Equal([0, 1, 2, 3], snapshot.Variables.LocalFlags
            .Where(pair => pair.Value).Select(pair => (int)pair.Key).Order());
        Assert.Equal(0, snapshot.Variables.LocalNumbers[0]);
    }

    [Fact]
    public void MapsBoundedCompletionEffectsWithoutResourceDependenciesInCore()
    {
        var projection = new FirstTyrDialogueCompletionProjection(7, 2905,
            new(GplDialogueTextSourceKind.Variable, string.Empty, 6, 5),
            [new(14, true), new(4, true)], true);

        var result = DialogueSessionAdapter.ToCore(projection);

        Assert.Equal((7, 2905), (result.SourceIndex, result.BranchTargetOffset));
        Assert.Equal(new Dictionary<ushort, bool> { [14] = true, [4] = true },
            result.LocalFlagAssignments);
        Assert.Throws<InvalidDataException>(() =>
            DialogueSessionAdapter.ToCore(projection with { ReturnsFromLocalBranch = false }));
    }

    [Fact]
    public void MapsReturnedResponseEffectsBackToTheCallingMenu()
    {
        var projection = new FirstTyrDialogueResponseProjection(0, 1017, [],
            [new(0, false)], [], true);

        var result = DialogueSessionAdapter.ToCore(projection);

        Assert.Equal(DialogueBranchDisposition.ReturnThroughFirstTyrMenu,
            result.Disposition);
        Assert.False(result.LocalFlagAssignments[0]);
        Assert.Empty(result.LocalNumberIncrements);
        Assert.Throws<InvalidDataException>(() =>
            DialogueSessionAdapter.ToCore(projection with { ReturnsToMenu = false }));
    }

    [Fact]
    public void MapsConditionalAndGlobalResponseEffects()
    {
        var projection = new FirstTyrDialogueResponseProjection(1, 1597, [],
            [new(1, false)], [], true)
        {
            GlobalFlagAssignments = [new(357, true)],
            ConditionalLocalFlagAssignments =
            [
                new(new(357, false), 6, true),
                new(new(357, false), 7, true)
            ]
        };

        var result = DialogueSessionAdapter.ToCore(projection);

        Assert.True(result.GlobalFlagAssignments[357]);
        Assert.Equal([(357, false, 6, true), (357, false, 7, true)],
            result.ConditionalLocalFlagAssignments.Select(assignment => (
                (int)assignment.Condition.VariableId, assignment.Condition.Value,
                (int)assignment.VariableId, assignment.Value)));
    }

    [Fact]
    public void MapsLocalNumberAssignmentsSeparatelyFromIncrements()
    {
        var projection = new FirstTyrDialogueResponseProjection(4, 1232, [],
            [new(9, true)], [], true)
        {
            LocalNumberAssignments = [new(0, 0)]
        };

        var result = DialogueSessionAdapter.ToCore(projection);

        Assert.Equal(0, result.LocalNumberAssignments[0]);
        Assert.Empty(result.LocalNumberIncrements);
    }

    [Fact]
    public void MapsConditionalEffectsFromLocalFlags()
    {
        var projection = new FirstTyrDialogueResponseProjection(1, 1825, [],
            [new(6, false)], [], true)
        {
            ConditionalLocalFlagAssignmentsFromLocalFlags =
            [
                new(new(16, false), 10, true)
            ]
        };

        var result = DialogueSessionAdapter.ToCore(projection);

        var assignment = Assert.Single(
            result.ConditionalLocalFlagAssignmentsFromLocalFlags);
        Assert.Equal((16, false, 10, true), (
            (int)assignment.Condition.VariableId, assignment.Condition.Value,
            (int)assignment.VariableId, assignment.Value));
    }
}
