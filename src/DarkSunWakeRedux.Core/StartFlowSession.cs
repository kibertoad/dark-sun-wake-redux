using System.Security.Cryptography;
using System.Text;

namespace DarkSunWakeRedux.Core;

public enum StartFlowCommandKind
{
    ChooseStart,
    OpenEmptySlot,
    ChooseEmptySlot,
    CompleteCharacter,
    BeginCreatedParty,
    Cancel
}

public sealed record StartFlowCommand(
    StartFlowCommandKind Kind,
    StartWindowChoice? StartChoice = null,
    EmptySlotChoice? EmptySlotChoice = null,
    CharacterDraft? Character = null)
{
    public static StartFlowCommand Choose(StartWindowChoice choice) => new(StartFlowCommandKind.ChooseStart, choice);
    public static StartFlowCommand OpenEmptySlot() => new(StartFlowCommandKind.OpenEmptySlot);
    public static StartFlowCommand Choose(EmptySlotChoice choice) =>
        new(StartFlowCommandKind.ChooseEmptySlot, EmptySlotChoice: choice);
    public static StartFlowCommand Complete(CharacterDraft character) =>
        new(StartFlowCommandKind.CompleteCharacter, Character: character);
    public static StartFlowCommand BeginParty() => new(StartFlowCommandKind.BeginCreatedParty);
    public static StartFlowCommand Cancel() => new(StartFlowCommandKind.Cancel);
}

public sealed record StartFlowSnapshot(
    int SchemaVersion,
    int Seed,
    long Sequence,
    StartFlowScreen Screen,
    PartyOrigin PartyOrigin,
    IReadOnlyList<CharacterDraft> PartyMembers)
{
    public const int CurrentSchemaVersion = 2;
}

public sealed record StartFlowEvent(
    long Sequence,
    StartFlowCommandKind Command,
    StartFlowScreen Before,
    StartFlowScreen After,
    IReadOnlyList<PartyDiagnostic> Diagnostics)
{
    public bool Accepted => Diagnostics.Count == 0;
}

public sealed record StartFlowReplayEntry(StartFlowCommand Command, string ExpectedStateSha256);
public sealed record StartFlowReplay(int FormatVersion, int Seed, IReadOnlyList<StartFlowReplayEntry> Entries)
{
    public const int CurrentFormatVersion = 1;
}

public sealed class StartFlowSession
{
    private StartFlow _flow;

    public int Seed { get; }
    public long Sequence { get; private set; }

    public StartFlowSession(int seed)
    {
        Seed = seed;
        _flow = new();
    }

    private StartFlowSession(StartFlowSnapshot snapshot)
    {
        Validate(snapshot);
        Seed = snapshot.Seed;
        Sequence = snapshot.Sequence;
        _flow = new(snapshot);
    }

    public StartFlowEvent Execute(StartFlowCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        var before = _flow.Screen;
        var diagnostics = !HasValidPayload(command)
            ? [new PartyDiagnostic("start_flow_command_invalid", "The command payload does not match its kind.")]
            : command.Kind switch
        {
            StartFlowCommandKind.ChooseStart => _flow.Choose(command.StartChoice!.Value),
            StartFlowCommandKind.OpenEmptySlot => _flow.OpenEmptySlotMenu(),
            StartFlowCommandKind.ChooseEmptySlot => _flow.Choose(command.EmptySlotChoice!.Value),
            StartFlowCommandKind.CompleteCharacter => _flow.CompleteCharacter(command.Character!),
            StartFlowCommandKind.BeginCreatedParty => _flow.BeginCreatedParty(),
            StartFlowCommandKind.Cancel => _flow.Cancel(),
            _ => [new("start_flow_command_invalid", "The command payload does not match its kind.")]
        };
        Sequence = checked(Sequence + 1);
        return new(Sequence, command.Kind, before, _flow.Screen, diagnostics.ToArray());
    }

    private static bool HasValidPayload(StartFlowCommand command) => command.Kind switch
    {
        StartFlowCommandKind.ChooseStart => command.StartChoice is not null &&
            command.EmptySlotChoice is null && command.Character is null,
        StartFlowCommandKind.ChooseEmptySlot => command.StartChoice is null &&
            command.EmptySlotChoice is not null && command.Character is null,
        StartFlowCommandKind.CompleteCharacter => command.StartChoice is null &&
            command.EmptySlotChoice is null && command.Character is not null,
        StartFlowCommandKind.OpenEmptySlot or StartFlowCommandKind.BeginCreatedParty or
            StartFlowCommandKind.Cancel => command.StartChoice is null &&
            command.EmptySlotChoice is null && command.Character is null,
        _ => false
    };

    public StartFlowSnapshot Snapshot() => new(
        StartFlowSnapshot.CurrentSchemaVersion,
        Seed,
        Sequence,
        _flow.Screen,
        _flow.PartyOrigin,
        _flow.Party.Members.Select(Copy).ToArray());

    public string StateSha256() => Convert.ToHexStringLower(SHA256.HashData(Encode(Snapshot())));

    public static StartFlowSession Restore(StartFlowSnapshot snapshot) => new(snapshot);

    public static StartFlowSession Replay(StartFlowReplay replay)
    {
        ArgumentNullException.ThrowIfNull(replay);
        if (replay.FormatVersion != StartFlowReplay.CurrentFormatVersion)
            throw new InvalidDataException($"Unsupported start-flow replay version {replay.FormatVersion}.");
        var session = new StartFlowSession(replay.Seed);
        foreach (var (entry, index) in replay.Entries.Select((entry, index) => (entry, index)))
        {
            session.Execute(entry.Command);
            var actual = session.StateSha256();
            if (!actual.Equals(entry.ExpectedStateSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Start-flow replay diverged after command {index + 1}.");
        }
        return session;
    }

    private static byte[] Encode(StartFlowSnapshot snapshot)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(snapshot.SchemaVersion);
        writer.Write(snapshot.Seed);
        writer.Write(snapshot.Sequence);
        writer.Write((int)snapshot.Screen);
        writer.Write((int)snapshot.PartyOrigin);
        writer.Write(snapshot.PartyMembers.Count);
        foreach (var member in snapshot.PartyMembers)
        {
            WriteString(writer, member.Name);
            writer.Write((int)member.Race);
            writer.Write((int)member.Sex);
            writer.Write((int)member.Alignment);
            foreach (var (_, value) in member.Abilities.All()) writer.Write(value);
            writer.Write(member.Classes.Count);
            foreach (var characterClass in member.Classes) writer.Write((int)characterClass);
            writer.Write((int)member.PsionicDisciplines);
            writer.Write(member.ClericalSphere is null ? -1 : (int)member.ClericalSphere.Value);
        }
        return stream.ToArray();
    }

    private static void WriteString(BinaryWriter writer, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }

    private static CharacterDraft Copy(CharacterDraft member) => member with
    {
        Classes = member.Classes.ToArray()
    };

    private static void Validate(StartFlowSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.SchemaVersion != StartFlowSnapshot.CurrentSchemaVersion)
            throw new InvalidDataException($"Unsupported start-flow snapshot version {snapshot.SchemaVersion}.");
        if (snapshot.Sequence < 0) throw new InvalidDataException("Snapshot sequence cannot be negative.");
        if (!Enum.IsDefined(snapshot.Screen) || !Enum.IsDefined(snapshot.PartyOrigin))
            throw new InvalidDataException("Snapshot contains an invalid start-flow state.");
        if (snapshot.PartyMembers is null || snapshot.PartyMembers.Count > Party.MaximumSize)
            throw new InvalidDataException("Snapshot contains an invalid party size.");
        foreach (var member in snapshot.PartyMembers)
            if (PartyCreationRules.Validate(member).Count != 0)
                throw new InvalidDataException("Snapshot contains an invalid party member.");
    }
}
