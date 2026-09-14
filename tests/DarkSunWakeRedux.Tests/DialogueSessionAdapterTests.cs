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
    }
}
