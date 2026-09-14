namespace DarkSunWakeRedux.Resources;

public enum GplDialogueOutputKind { Text, NewLine }

public sealed record GplDialogueOutput(
    GplDialogueOutputKind Kind,
    GplDialogueTextSource? Text);

public sealed record GplLocalNumberIncrement(ushort VariableId, int Amount);

public sealed record FirstTyrDialogueResponseProjection(
    int SourceChoiceIndex,
    int EntryOffset,
    IReadOnlyList<GplDialogueOutput> Output,
    IReadOnlyList<GplLocalFlagAssignment> LocalFlagAssignments,
    IReadOnlyList<GplLocalNumberIncrement> LocalNumberIncrements,
    bool ReturnsToOpeningMenu);

public static class FirstTyrDialogueResponseProjectionReader
{
    public const int FirstChoiceIndex = 0;
    public const int FirstEntryOffset = 1017;
    public const int FirstReturnOffset = 1147;
    public const byte PrintStringOpcode = 0x4f;
    public const byte PrintNewLineOpcode = 0x51;
    public const byte LoadVariableOpcode = 0x16;
    public const byte LocalReturnOpcode = 0x15;

    public static FirstTyrDialogueResponseProjection ReadFirst(
        PackedGplScript script)
    {
        ValidateScript(script);
        var bytes = script.Bytecode.AsSpan();
        var position = FirstEntryOffset;
        var output = new List<GplDialogueOutput>
        {
            ReadLiteralPrint(bytes, ref position, 1043)
        };
        RequireOpcode(bytes, ref position, PrintNewLineOpcode, "first response newline");
        output.Add(new(GplDialogueOutputKind.NewLine, null));
        RequireOpcode(bytes, ref position, PrintNewLineOpcode, "second response newline");
        output.Add(new(GplDialogueOutputKind.NewLine, null));
        output.Add(ReadLiteralPrint(bytes, ref position, 1116));
        output.Add(ReadLiteralPrint(bytes, ref position, 1142));
        var assignment = ReadFlagAssignment(bytes, ref position, 0, false);
        if (position != FirstReturnOffset)
            throw Error($"choice 0 effects end at {position}, not {FirstReturnOffset}");
        RequireOpcode(bytes, ref position, LocalReturnOpcode, "choice 0 local return");
        return new(FirstChoiceIndex, FirstEntryOffset, output, [assignment], [], true);
    }

    private static GplDialogueOutput ReadLiteralPrint(
        ReadOnlySpan<byte> bytes,
        ref int position,
        int expectedNextOffset)
    {
        RequireOpcode(bytes, ref position, PrintStringOpcode, "response print");
        RequireImmediate14(bytes, ref position, 115, "response print destination");
        Require(bytes, position, 1, "response literal");
        if (bytes[position++] != 0x92)
            throw Error("response text is not an immediate packed string");
        var packed = GplPackedString.Read(bytes[position..], "first Tyr response text");
        if (packed.Kind != GplPackedStringKind.Compressed)
            throw Error("response text is not compressed literal text");
        position += packed.BytesConsumed;
        if (position != expectedNextOffset)
            throw Error($"response print ends at {position}, not {expectedNextOffset}");
        return new(GplDialogueOutputKind.Text,
            new(GplDialogueTextSourceKind.Literal, packed.Value, 0, 0));
    }

    private static GplLocalFlagAssignment ReadFlagAssignment(
        ReadOnlySpan<byte> bytes,
        ref int position,
        ushort expectedId,
        bool expectedValue)
    {
        RequireOpcode(bytes, ref position, LoadVariableOpcode,
            $"local flag #{expectedId} assignment");
        Require(bytes, position, 4, $"local flag #{expectedId} assignment");
        var value = expectedValue ? (byte)1 : (byte)0;
        if (bytes[position++] != 0x8f || bytes[position++] != value ||
            bytes[position++] != 0x8e || bytes[position++] != expectedId)
            throw Error($"local flag #{expectedId} is not assigned immediate {value}");
        return new(expectedId, expectedValue);
    }

    private static void ValidateScript(PackedGplScript script)
    {
        ArgumentNullException.ThrowIfNull(script);
        if (script.ResourceNumber != OriginalContent.FirstTyrDialogueScriptResourceNumber)
            throw Error($"expected GPL #{OriginalContent.FirstTyrDialogueScriptResourceNumber}, " +
                $"not #{script.ResourceNumber}");
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
        new($"First Tyr dialogue response projection: {message}.");
}
