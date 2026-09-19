namespace DarkSunWakeRedux.Resources;

public sealed record GplLocalFlagAssignment(ushort VariableId, bool Value);

public sealed record FirstTyrDialogueCompletionProjection(
    int SourceChoiceIndex,
    int EntryOffset,
    GplDialogueTextSource SpokenResponse,
    IReadOnlyList<GplLocalFlagAssignment> LocalFlagAssignments,
    bool ReturnsFromLocalBranch);

public static class FirstTyrDialogueCompletionProjectionReader
{
    public const int SourceChoiceIndex = 7;
    public const int EntryOffset = 2905;
    public const int ReturnOffset = 2920;
    public const byte PrintStringOpcode = 0x4f;
    public const byte LoadVariableOpcode = 0x16;
    public const byte LocalReturnOpcode = 0x15;

    public static FirstTyrDialogueCompletionProjection Read(PackedGplScript script)
    {
        ArgumentNullException.ThrowIfNull(script);
        if (script.SourceTag != PackedGplScript.GplTag)
            throw Error($"expected source tag {PackedGplScript.GplTag.Trim()}, not {script.SourceTag.Trim()}");
        if (script.ResourceNumber != OriginalContent.FirstTyrDialogueScriptResourceNumber)
            throw Error($"expected GPL #{OriginalContent.FirstTyrDialogueScriptResourceNumber}, " +
                $"not #{script.ResourceNumber}");
        var bytes = script.Bytecode.AsSpan();
        var position = EntryOffset;
        RequireOpcode(bytes, ref position, PrintStringOpcode, "opening response print");
        RequireImmediate14(bytes, ref position, 115, "opening response print destination");
        var spoken = ReadVariable(bytes, ref position, "opening response text");
        if (spoken.VariableType != 6 || spoken.VariableId != 5)
            throw Error("the completion response does not use GSTRING #5");
        var assignments = new[]
        {
            ReadFlagAssignment(bytes, ref position, 14),
            ReadFlagAssignment(bytes, ref position, 4)
        };
        if (position != ReturnOffset)
            throw Error($"the completion assignments end at {position}, not {ReturnOffset}");
        RequireOpcode(bytes, ref position, LocalReturnOpcode, "completion return");
        return new(SourceChoiceIndex, EntryOffset, spoken, assignments, true);
    }

    private static GplLocalFlagAssignment ReadFlagAssignment(
        ReadOnlySpan<byte> bytes,
        ref int position,
        ushort expectedId)
    {
        RequireOpcode(bytes, ref position, LoadVariableOpcode,
            $"local flag #{expectedId} assignment");
        Require(bytes, position, 4, $"local flag #{expectedId} assignment");
        if (bytes[position++] != 0x8f || bytes[position++] != 1 ||
            bytes[position++] != 0x8e || bytes[position++] != expectedId)
            throw Error($"local flag #{expectedId} is not assigned immediate 1");
        return new(expectedId, true);
    }

    private static GplDialogueTextSource ReadVariable(
        ReadOnlySpan<byte> bytes,
        ref int position,
        string description)
    {
        Require(bytes, position, 2, description);
        var marker = bytes[position++];
        var type = checked((byte)(marker & 0x7f));
        if (marker < 0x80 || type > 0x0e || (type & 0x40) != 0)
            throw Error($"{description} is not a short variable expression");
        return new(GplDialogueTextSourceKind.Variable, string.Empty,
            type, bytes[position++]);
    }

    private static void RequireImmediate14(
        ReadOnlySpan<byte> bytes,
        ref int position,
        int expected,
        string description)
    {
        Require(bytes, position, 2, description);
        var first = bytes[position++];
        var value = (first << 8) | bytes[position++];
        if (first >= 0x80 || value != expected)
            throw Error($"{description} is not immediate {expected}");
    }

    private static void RequireOpcode(
        ReadOnlySpan<byte> bytes,
        ref int position,
        byte expected,
        string description)
    {
        Require(bytes, position, 1, description);
        if (bytes[position++] != expected)
            throw Error($"{description} does not use opcode 0x{expected:X2}");
    }

    private static void Require(
        ReadOnlySpan<byte> bytes,
        int position,
        int length,
        string description)
    {
        if (position < 0 || length < 0 || position > bytes.Length - length)
            throw Error($"{description} is truncated");
    }

    private static InvalidDataException Error(string message) =>
        new($"First Tyr dialogue completion projection: {message}.");
}
