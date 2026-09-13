using DarkSunWakeRedux.Core;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class StartFlowSessionTests
{
    [Fact]
    public void SameSeedAndCommandsProduceSameEventsAndHash()
    {
        var first = Run(42);
        var second = Run(42);

        Assert.Equal(first.Events, second.Events);
        Assert.Equal(first.Session.StateSha256(), second.Session.StateSha256());
    }

    [Fact]
    public void SnapshotRestoresStateAndSequence()
    {
        var original = Run(19).Session;

        var restored = StartFlowSession.Restore(original.Snapshot());

        Assert.Equal(original.Snapshot().SchemaVersion, restored.Snapshot().SchemaVersion);
        Assert.Equal(original.Snapshot().Seed, restored.Snapshot().Seed);
        Assert.Equal(original.Snapshot().Sequence, restored.Snapshot().Sequence);
        Assert.Equal(original.Snapshot().Screen, restored.Snapshot().Screen);
        Assert.Equal(original.Snapshot().PartyOrigin, restored.Snapshot().PartyOrigin);
        Assert.Equal(original.Snapshot().PartyMembers.Select(member => member.Name),
            restored.Snapshot().PartyMembers.Select(member => member.Name));
        Assert.Equal(original.StateSha256(), restored.StateSha256());
    }

    [Fact]
    public void ReplayVerifiesEveryExpectedStateHash()
    {
        var commands = Commands();
        var recording = new StartFlowSession(7);
        var entries = commands.Select(command =>
        {
            recording.Execute(command);
            return new StartFlowReplayEntry(command, recording.StateSha256());
        }).ToArray();

        var replayed = StartFlowSession.Replay(new(StartFlowReplay.CurrentFormatVersion, 7, entries));

        Assert.Equal(recording.StateSha256(), replayed.StateSha256());
    }

    [Fact]
    public void ReplayRejectsDivergence()
    {
        var replay = new StartFlowReplay(StartFlowReplay.CurrentFormatVersion, 1,
            [new(StartFlowCommand.Choose(StartWindowChoice.CreateCharacters), new string('0', 64))]);

        Assert.Throws<InvalidDataException>(() => StartFlowSession.Replay(replay));
    }

    [Fact]
    public void RejectedCommandStillProducesSequencedEvent()
    {
        var session = new StartFlowSession(3);

        var result = session.Execute(StartFlowCommand.BeginParty());

        Assert.False(result.Accepted);
        Assert.Equal(1, result.Sequence);
        Assert.Equal(StartFlowScreen.StartWindow, result.Before);
        Assert.Equal(result.Before, result.After);
        Assert.Equal(1, session.Snapshot().Sequence);
    }

    [Fact]
    public void MismatchedCommandPayloadIsRejectedWithoutStateMutation()
    {
        var session = new StartFlowSession(3);

        var result = session.Execute(new(StartFlowCommandKind.OpenEmptySlot,
            StartChoice: StartWindowChoice.StartGame));

        Assert.Equal("start_flow_command_invalid", Assert.Single(result.Diagnostics).Code);
        Assert.Equal(StartFlowScreen.StartWindow, session.Snapshot().Screen);
    }

    [Fact]
    public void ClericalSphereContributesToCanonicalStateHash()
    {
        var cleric = new CharacterDraft("Sadia", CharacterRace.Human, CharacterSex.Female,
            CharacterAlignment.LawfulGood, new(15, 15, 15, 15, 15, 15), [CharacterClass.Cleric]);

        var air = SessionWith(cleric with { ClericalSphere = ClericalSphere.Air });
        var fire = SessionWith(cleric with { ClericalSphere = ClericalSphere.Fire });

        Assert.NotEqual(air.StateSha256(), fire.StateSha256());
    }

    [Fact]
    public void DualClassCommandAndProgressionSurviveSnapshotRestore()
    {
        var fighter = new CharacterDraft("Rikus", CharacterRace.Human, CharacterSex.Male,
            CharacterAlignment.NeutralGood, new(15, 15, 15, 15, 15, 15), [CharacterClass.Fighter])
        {
            ClassProgression = DualClassProgression.Begin(CharacterClass.Fighter, 3)
        };
        var session = new StartFlowSession(11);
        session.Execute(StartFlowCommand.Choose(StartWindowChoice.CreateCharacters));
        session.Execute(StartFlowCommand.OpenEmptySlot());
        session.Execute(StartFlowCommand.Choose(EmptySlotChoice.New));
        session.Execute(StartFlowCommand.Complete(fighter));
        session.Execute(StartFlowCommand.OpenOccupiedSlot(0));
        session.Execute(StartFlowCommand.Choose(OccupiedSlotChoice.Dual));

        var result = session.Execute(StartFlowCommand.ChooseDualClass(CharacterClass.Preserver));
        var restored = StartFlowSession.Restore(session.Snapshot());

        Assert.True(result.Accepted);
        Assert.Equal(StartFlowSnapshot.CurrentSchemaVersion, restored.Snapshot().SchemaVersion);
        Assert.Equal([CharacterClass.Fighter, CharacterClass.Preserver],
            restored.Snapshot().PartyMembers[0].Classes);
        Assert.Equal([3, 1], restored.Snapshot().PartyMembers[0].ClassProgression!.Careers
            .Select(item => item.Level));
        Assert.Equal(session.StateSha256(), restored.StateSha256());
    }

    [Fact]
    public void DualClassSelectionSurvivesSnapshotRestore()
    {
        var fighter = new CharacterDraft("Rikus", CharacterRace.Human, CharacterSex.Male,
            CharacterAlignment.NeutralGood, new(15, 15, 15, 15, 15, 15), [CharacterClass.Fighter])
        {
            ClassProgression = DualClassProgression.Begin(CharacterClass.Fighter, 3)
        };
        var session = SessionWith(fighter);
        session.Execute(StartFlowCommand.OpenOccupiedSlot(0));
        session.Execute(StartFlowCommand.Choose(OccupiedSlotChoice.Dual));

        var restored = StartFlowSession.Restore(session.Snapshot());
        var result = restored.Execute(StartFlowCommand.ChooseDualClass(CharacterClass.Preserver));

        Assert.True(result.Accepted);
        Assert.Equal(StartFlowScreen.PartyOverview, restored.Snapshot().Screen);
        Assert.Equal([CharacterClass.Fighter, CharacterClass.Preserver],
            restored.Snapshot().PartyMembers[0].Classes);
    }

    [Fact]
    public void DualClassChoiceRequiresExactlyItsOwnPayload()
    {
        var session = new StartFlowSession(3);

        var result = session.Execute(new(StartFlowCommandKind.ChooseDualClass,
            StartChoice: StartWindowChoice.StartGame,
            DualClassChoice: CharacterClass.Preserver));

        Assert.Equal("start_flow_command_invalid", Assert.Single(result.Diagnostics).Code);
        Assert.Equal(StartFlowScreen.StartWindow, session.Snapshot().Screen);
    }

    [Fact]
    public void ClassProgressionContributesToCanonicalStateHash()
    {
        var fighter = new CharacterDraft("Rikus", CharacterRace.Human, CharacterSex.Male,
            CharacterAlignment.NeutralGood, new(15, 15, 15, 15, 15, 15), [CharacterClass.Fighter]);
        var levelThree = SessionWith(fighter with
        {
            ClassProgression = DualClassProgression.Begin(CharacterClass.Fighter, 3)
        });
        var levelFour = SessionWith(fighter with
        {
            ClassProgression = DualClassProgression.Begin(CharacterClass.Fighter, 4)
        });

        Assert.NotEqual(levelThree.StateSha256(), levelFour.StateSha256());
    }

    [Fact]
    public void SnapshotPreservesOccupiedSlotEditTarget()
    {
        var session = SessionWith(new("Rikus", CharacterRace.Human, CharacterSex.Male,
            CharacterAlignment.NeutralGood, new(15, 15, 15, 15, 15, 15), [CharacterClass.Fighter]));
        session.Execute(StartFlowCommand.OpenOccupiedSlot(0));
        session.Execute(StartFlowCommand.Choose(OccupiedSlotChoice.Edit));

        var restored = StartFlowSession.Restore(session.Snapshot());
        var result = restored.Execute(StartFlowCommand.Complete(
            session.Snapshot().PartyMembers[0] with { Name = "Edited" }));

        Assert.True(result.Accepted);
        Assert.Single(restored.Snapshot().PartyMembers);
        Assert.Equal("Edited", restored.Snapshot().PartyMembers[0].Name);
    }

    private static (StartFlowSession Session, StartFlowEvent[] Events) Run(int seed)
    {
        var session = new StartFlowSession(seed);
        return (session, Commands().Select(session.Execute).ToArray());
    }

    private static StartFlowSession SessionWith(CharacterDraft character)
    {
        var session = new StartFlowSession(11);
        session.Execute(StartFlowCommand.Choose(StartWindowChoice.CreateCharacters));
        session.Execute(StartFlowCommand.OpenEmptySlot());
        session.Execute(StartFlowCommand.Choose(EmptySlotChoice.New));
        Assert.True(session.Execute(StartFlowCommand.Complete(character)).Accepted);
        return session;
    }

    private static StartFlowCommand[] Commands() =>
    [
        StartFlowCommand.Choose(StartWindowChoice.CreateCharacters),
        StartFlowCommand.OpenEmptySlot(),
        StartFlowCommand.Choose(EmptySlotChoice.New),
        StartFlowCommand.Complete(new("Rikus", CharacterRace.Human, CharacterSex.Male,
            CharacterAlignment.NeutralGood, new(15, 15, 15, 15, 15, 15), [CharacterClass.Fighter])),
        StartFlowCommand.BeginParty()
    ];
}
