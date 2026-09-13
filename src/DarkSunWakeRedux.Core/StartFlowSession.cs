using System.Security.Cryptography;
using System.Text;

namespace DarkSunWakeRedux.Core;

public enum StartFlowCommandKind
{
    ChooseStart,
    OpenEmptySlot,
    OpenOccupiedSlot,
    ChooseEmptySlot,
    ChooseOccupiedSlot,
    ChooseDualClass,
    AddStoredCharacter,
    CompleteCharacter,
    BeginCreatedParty,
    Cancel
}

public sealed record StartFlowCommand(
    StartFlowCommandKind Kind,
    StartWindowChoice? StartChoice = null,
    EmptySlotChoice? EmptySlotChoice = null,
    CharacterDraft? Character = null,
    int? MemberIndex = null,
    OccupiedSlotChoice? OccupiedSlotChoice = null,
    CharacterClass? DualClassChoice = null)
{
    public static StartFlowCommand Choose(StartWindowChoice choice) => new(StartFlowCommandKind.ChooseStart, choice);
    public static StartFlowCommand OpenEmptySlot() => new(StartFlowCommandKind.OpenEmptySlot);
    public static StartFlowCommand OpenOccupiedSlot(int index) =>
        new(StartFlowCommandKind.OpenOccupiedSlot, MemberIndex: index);
    public static StartFlowCommand Choose(EmptySlotChoice choice) =>
        new(StartFlowCommandKind.ChooseEmptySlot, EmptySlotChoice: choice);
    public static StartFlowCommand Complete(CharacterDraft character) =>
        new(StartFlowCommandKind.CompleteCharacter, Character: character);
    public static StartFlowCommand Choose(OccupiedSlotChoice choice) =>
        new(StartFlowCommandKind.ChooseOccupiedSlot, OccupiedSlotChoice: choice);
    public static StartFlowCommand ChooseDualClass(CharacterClass choice) =>
        new(StartFlowCommandKind.ChooseDualClass, DualClassChoice: choice);
    public static StartFlowCommand AddStoredCharacter(int index) =>
        new(StartFlowCommandKind.AddStoredCharacter, MemberIndex: index);
    public static StartFlowCommand BeginParty() => new(StartFlowCommandKind.BeginCreatedParty);
    public static StartFlowCommand Cancel() => new(StartFlowCommandKind.Cancel);
}

public sealed record StartFlowSnapshot(
    int SchemaVersion,
    int Seed,
    long Sequence,
    StartFlowScreen Screen,
    PartyOrigin PartyOrigin,
    IReadOnlyList<CharacterDraft> PartyMembers,
    int? ActiveMemberIndex,
    IReadOnlyList<CharacterDraft> StoredCharacters)
{
    public const int CurrentSchemaVersion = 4;
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
            StartFlowCommandKind.OpenOccupiedSlot => _flow.OpenOccupiedSlotMenu(command.MemberIndex!.Value),
            StartFlowCommandKind.ChooseEmptySlot => _flow.Choose(command.EmptySlotChoice!.Value),
            StartFlowCommandKind.ChooseOccupiedSlot => _flow.Choose(command.OccupiedSlotChoice!.Value),
            StartFlowCommandKind.ChooseDualClass => _flow.ChooseDualClass(command.DualClassChoice!.Value),
            StartFlowCommandKind.AddStoredCharacter => _flow.AddStoredCharacter(command.MemberIndex!.Value),
            StartFlowCommandKind.CompleteCharacter => _flow.CompleteCharacter(command.Character!),
            StartFlowCommandKind.BeginCreatedParty => _flow.BeginCreatedParty(),
            StartFlowCommandKind.Cancel => _flow.Cancel(),
            _ => [new("start_flow_command_invalid", "The command payload does not match its kind.")]
        };
        Sequence = checked(Sequence + 1);
        return new(Sequence, command.Kind, before, _flow.Screen, diagnostics.ToArray());
    }

    private static bool HasValidPayload(StartFlowCommand command)
    {
        var count = (command.StartChoice is null ? 0 : 1) + (command.EmptySlotChoice is null ? 0 : 1) +
            (command.Character is null ? 0 : 1) + (command.MemberIndex is null ? 0 : 1) +
            (command.OccupiedSlotChoice is null ? 0 : 1) + (command.DualClassChoice is null ? 0 : 1);
        return command.Kind switch
        {
            StartFlowCommandKind.ChooseStart => count == 1 && command.StartChoice is not null,
            StartFlowCommandKind.OpenOccupiedSlot or StartFlowCommandKind.AddStoredCharacter =>
                count == 1 && command.MemberIndex is not null,
            StartFlowCommandKind.ChooseEmptySlot => count == 1 && command.EmptySlotChoice is not null,
            StartFlowCommandKind.ChooseOccupiedSlot => count == 1 && command.OccupiedSlotChoice is not null,
            StartFlowCommandKind.ChooseDualClass => count == 1 && command.DualClassChoice is not null,
            StartFlowCommandKind.CompleteCharacter => count == 1 && command.Character is not null,
            StartFlowCommandKind.OpenEmptySlot or StartFlowCommandKind.BeginCreatedParty or
                StartFlowCommandKind.Cancel => count == 0,
            _ => false
        };
    }

    public StartFlowSnapshot Snapshot() => new(
        StartFlowSnapshot.CurrentSchemaVersion,
        Seed,
        Sequence,
        _flow.Screen,
        _flow.PartyOrigin,
        _flow.Party.Members.Select(Copy).ToArray(),
        _flow.ActiveMemberIndex,
        _flow.StoredCharacters.Select(Copy).ToArray());

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
        writer.Write(snapshot.ActiveMemberIndex ?? -1);
        WriteCharacters(writer, snapshot.PartyMembers);
        WriteCharacters(writer, snapshot.StoredCharacters);
        return stream.ToArray();
    }

    private static void WriteCharacters(BinaryWriter writer, IReadOnlyList<CharacterDraft> members)
    {
        writer.Write(members.Count);
        foreach (var member in members)
        {
            WriteString(writer, member.Name);
            writer.Write((int)member.Origin);
            writer.Write((int)member.Sex);
            writer.Write((int)member.Alignment);
            foreach (var (_, value) in member.Abilities.All()) writer.Write(value);
            writer.Write(member.Classes.Count);
            foreach (var characterClass in member.Classes) writer.Write((int)characterClass);
            writer.Write((int)member.PsionicDisciplines);
            writer.Write(member.ClericalSphere is null ? -1 : (int)member.ClericalSphere.Value);
            writer.Write(member.ClassProgression is not null);
            if (member.ClassProgression is not null)
            {
                writer.Write(member.ClassProgression.Careers.Count);
                foreach (var career in member.ClassProgression.Careers)
                {
                    writer.Write((int)career.CharacterClass);
                    writer.Write(career.Level);
                }
            }
        }
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
        if (snapshot.StoredCharacters is null || snapshot.StoredCharacters.Count > 1024)
            throw new InvalidDataException("Snapshot contains an invalid stored-character count.");
        if ((snapshot.Screen is StartFlowScreen.OccupiedSlotMenu or StartFlowScreen.DualClassSelection) &&
            snapshot.ActiveMemberIndex is null)
            throw new InvalidDataException("Snapshot member-action screen has no active party member.");
        if (snapshot.ActiveMemberIndex is { } index &&
            ((uint)index >= (uint)snapshot.PartyMembers.Count ||
             snapshot.Screen is not (StartFlowScreen.OccupiedSlotMenu or
                 StartFlowScreen.CharacterGeneration or StartFlowScreen.DualClassSelection)))
            throw new InvalidDataException("Snapshot contains an invalid active party member.");
        foreach (var member in snapshot.PartyMembers)
            if (PartyCreationRules.Validate(member).Count != 0)
                throw new InvalidDataException("Snapshot contains an invalid party member.");
        foreach (var member in snapshot.StoredCharacters)
            if (PartyCreationRules.Validate(member).Count != 0)
                throw new InvalidDataException("Snapshot contains an invalid stored character.");
        if (snapshot.Screen == StartFlowScreen.DualClassSelection &&
            snapshot.PartyMembers[snapshot.ActiveMemberIndex!.Value].ClassProgression is null)
            throw new InvalidDataException("Snapshot dual-class selection has no class progression.");
    }
}
