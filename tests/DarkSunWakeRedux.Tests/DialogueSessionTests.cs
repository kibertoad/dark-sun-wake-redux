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
    public void CompletesTheSelectedBranchAndAppliesLocalFlagsAtomically()
    {
        var variables = new DialogueVariableSnapshot(
            new Dictionary<ushort, bool> { [0] = true },
            new Dictionary<ushort, int>());
        var session = new DialogueSession(135, [new(7, 2905)], variables);
        session.Execute(DialogueCommand.Select(0));
        var result = new DialogueBranchResult(7, 2905,
            new Dictionary<ushort, bool> { [14] = true, [4] = true });

        var transition = session.Execute(DialogueCommand.Complete(result));

        Assert.True(transition.Applied);
        Assert.Equal(DialoguePhase.Completed, transition.After.Phase);
        Assert.True(transition.After.Variables.LocalFlags[0]);
        Assert.True(transition.After.Variables.LocalFlags[14]);
        Assert.True(transition.After.Variables.LocalFlags[4]);
        Assert.False(session.Execute(DialogueCommand.Complete(result)).Applied);
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

        var other = new DialogueBranchResult(6, 2905,
            new Dictionary<ushort, bool> { [4] = true });
        session.Execute(DialogueCommand.Select(0));
        Assert.Throws<InvalidOperationException>(() =>
            session.Execute(DialogueCommand.Complete(other)));
        Assert.Equal(DialoguePhase.BranchSelected, session.Snapshot().Phase);
    }

    [Fact]
    public void CopiesCallerOwnedCollectionsAtDeterministicBoundaries()
    {
        var choices = new[] { new DialogueChoiceIdentity(7, 2905) };
        var flags = new Dictionary<ushort, bool> { [0] = true };
        var assignments = new Dictionary<ushort, bool> { [4] = true };
        var session = new DialogueSession(135, choices,
            new(flags, new Dictionary<ushort, int>()));
        var result = new DialogueBranchResult(7, 2905, assignments);
        choices[0] = new(1, 2);
        flags[0] = false;
        assignments[4] = false;

        session.Execute(DialogueCommand.Select(0));
        session.Execute(DialogueCommand.Complete(result));

        Assert.Equal(7, session.Snapshot().SelectedSourceIndex);
        Assert.True(session.Snapshot().Variables.LocalFlags[0]);
        Assert.True(session.Snapshot().Variables.LocalFlags[4]);
    }
}
