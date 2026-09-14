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
    public void MapsReturnedResponseEffectsBackToTheOpeningMenu()
    {
        var projection = new FirstTyrDialogueResponseProjection(0, 1017, [],
            [new(0, false)], [], true);

        var result = DialogueSessionAdapter.ToCore(projection);

        Assert.Equal(DialogueBranchDisposition.ReturnToChoices, result.Disposition);
        Assert.False(result.LocalFlagAssignments[0]);
        Assert.Empty(result.LocalNumberIncrements);
        Assert.Throws<InvalidDataException>(() =>
            DialogueSessionAdapter.ToCore(projection with { ReturnsToOpeningMenu = false }));
    }
}
