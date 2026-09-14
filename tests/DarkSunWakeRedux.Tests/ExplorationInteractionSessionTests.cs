using DarkSunWakeRedux.Core;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class ExplorationInteractionSessionTests
{
    [Fact]
    public void HostileCreatureOpensInformationPanelWithDisabledTalk()
    {
        var session = new ExplorationInteractionSession();
        var target = Target(ExplorationInteractionTargetKind.Creature,
            ExplorationInteractionCapabilities.None, hostile: true);

        var opened = session.Open(target);
        var disabledTalk = session.Select(ExplorationInteractionAction.Talk);

        Assert.True(opened.Applied);
        Assert.Equal(target, session.Snapshot().Target);
        Assert.False(disabledTalk.Applied);
        Assert.Equal(ExplorationInteractionOutcome.None, disabledTalk.Outcome);
    }

    [Theory]
    [InlineData(ExplorationInteractionCapabilities.Talk,
        ExplorationInteractionAction.Talk, ExplorationInteractionOutcome.TalkRequested)]
    [InlineData(ExplorationInteractionCapabilities.PickUp,
        ExplorationInteractionAction.PickUp, ExplorationInteractionOutcome.PickUpRequested)]
    [InlineData(ExplorationInteractionCapabilities.Use,
        ExplorationInteractionAction.Use, ExplorationInteractionOutcome.UseRequested)]
    public void SoleObjectActionExecutesWithoutOpeningPanel(
        ExplorationInteractionCapabilities capability,
        ExplorationInteractionAction action,
        ExplorationInteractionOutcome outcome)
    {
        var session = new ExplorationInteractionSession();

        var transition = session.Open(Target(
            ExplorationInteractionTargetKind.Object, capability));

        Assert.True(transition.Applied);
        Assert.Equal(action, transition.Action);
        Assert.Equal(outcome, transition.Outcome);
        Assert.False(session.Snapshot().IsOpen);
    }

    [Fact]
    public void MultipleObjectActionsOpenPanelAndEnabledSelectionClosesIt()
    {
        var session = new ExplorationInteractionSession();
        session.Open(Target(ExplorationInteractionTargetKind.Object,
            ExplorationInteractionCapabilities.Talk |
            ExplorationInteractionCapabilities.Use));

        var transition = session.Select(ExplorationInteractionAction.Use);

        Assert.True(transition.Applied);
        Assert.Equal(ExplorationInteractionOutcome.UseRequested, transition.Outcome);
        Assert.False(transition.After.IsOpen);
    }

    [Fact]
    public void DismissClosesPanelAndSelectionWhileClosedIsInert()
    {
        var session = new ExplorationInteractionSession();
        session.Open(Target(ExplorationInteractionTargetKind.Creature,
            ExplorationInteractionCapabilities.None));

        var dismissed = session.Select(ExplorationInteractionAction.Dismiss);
        var repeated = session.Select(ExplorationInteractionAction.Dismiss);

        Assert.Equal(ExplorationInteractionOutcome.Dismissed, dismissed.Outcome);
        Assert.True(dismissed.Applied);
        Assert.False(repeated.Applied);
        Assert.Equal(ExplorationInteractionOutcome.None, repeated.Outcome);
    }

    [Theory]
    [InlineData(0U, "Draxan", 10)]
    [InlineData(9258U, "", 10)]
    [InlineData(9258U, "Draxan", 0)]
    [InlineData(9258U, "Draxan", 100)]
    public void RejectsInvalidTargetState(uint resourceNumber, string name, int level)
    {
        var session = new ExplorationInteractionSession();

        Assert.ThrowsAny<ArgumentException>(() => session.Open(new(
            resourceNumber, name, ExplorationInteractionTargetKind.Creature,
            level, true, ExplorationInteractionCapabilities.None)));
    }

    private static ExplorationInteractionTarget Target(
        ExplorationInteractionTargetKind kind,
        ExplorationInteractionCapabilities capabilities,
        bool hostile = false) => new(9258, "Draxan", kind, 10, hostile, capabilities);
}
