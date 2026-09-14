using DarkSunWakeRedux.Core;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class DialogueSessionTests
{
    [Fact]
    public void SelectsPhysicalResponseAndRetainsSourceBranchIdentity()
    {
        var session = new DialogueSession(135,
        [
            new(0, 1017),
            new(3, 1183),
            new(7, 2905)
        ]);

        var transition = session.Execute(DialogueCommand.Select(1));

        Assert.True(transition.Applied);
        Assert.Equal(DialoguePhase.AwaitingChoice, transition.Before.Phase);
        Assert.Equal(DialoguePhase.BranchSelected, transition.After.Phase);
        Assert.Equal(3, transition.After.SelectedSourceIndex);
        Assert.Equal(1183, transition.After.BranchTargetOffset);
        Assert.False(session.Execute(DialogueCommand.Select(2)).Applied);
        Assert.Equal(3, session.Snapshot().SelectedSourceIndex);
    }

    [Fact]
    public void RejectsInvalidConstructionAndSelectionWithoutMutation()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DialogueSession(0, [new(0, 1)]));
        Assert.Throws<ArgumentException>(() => new DialogueSession(1, []));
        Assert.Throws<ArgumentException>(() => new DialogueSession(1, [new(0, 1), new(0, 2)]));
        Assert.Throws<ArgumentException>(() => new DialogueSession(1, [new(-1, 1)]));
        Assert.Throws<ArgumentException>(() => new DialogueSession(1, [new(0, -1)]));
        var session = new DialogueSession(135, [new(7, 2905)]);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            session.Execute(DialogueCommand.Select(1)));
        Assert.Equal(DialoguePhase.AwaitingChoice, session.Snapshot().Phase);
    }
}
