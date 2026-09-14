namespace DarkSunWakeRedux.Resources;

public readonly record struct GplDialogueVariable(byte Type, ushort Id);

public sealed record GplGlobalStringProjection(
    GplDialogueVariable Destination,
    string Text);

public static class GplGlobalStringProjectionReader
{
    public const int AssignmentInstructionOffset = 20;
    public const int NextInstructionOffset = 33;
    public const int ThirdMenuExitAssignmentInstructionOffset = 66;
    public const int ThirdMenuExitNextInstructionOffset = 94;
    public const byte StringCopyOpcode = 0x0a;
    public const byte GlobalStringMarker = 0x86;
    public const byte PackedStringMarker = 0x92;
    public const byte GlobalStringType = 6;
    public const ushort ExitLabelVariableId = 5;
    public const ushort ThirdMenuExitLabelVariableId = 6;

    public static GplGlobalStringProjection Read(PackedGplScript script)
    {
        ValidateScript(script);
        return ReadAt(script.Bytecode, AssignmentInstructionOffset,
            NextInstructionOffset, ExitLabelVariableId);
    }

    public static GplGlobalStringProjection ReadThirdMenuExit(PackedGplScript script)
    {
        ValidateScript(script);
        return ReadAt(script.Bytecode, ThirdMenuExitAssignmentInstructionOffset,
            ThirdMenuExitNextInstructionOffset, ThirdMenuExitLabelVariableId);
    }

    private static void ValidateScript(PackedGplScript script)
    {
        ArgumentNullException.ThrowIfNull(script);
        if (script.ResourceNumber != OriginalContent.DialogueGlobalStringsScriptResourceNumber)
            throw Error($"expected MAS #{OriginalContent.DialogueGlobalStringsScriptResourceNumber}, " +
                $"not #{script.ResourceNumber}");
    }

    private static GplGlobalStringProjection ReadAt(
        ReadOnlySpan<byte> bytes,
        int instructionOffset,
        int nextInstructionOffset,
        ushort variableId)
    {
        Require(bytes, instructionOffset, 5, variableId);
        var position = instructionOffset;
        if (bytes[position++] != StringCopyOpcode)
            throw Error($"offset {instructionOffset} is not a string-copy instruction");
        if (bytes[position++] != GlobalStringMarker || bytes[position++] != variableId)
            throw Error($"the assignment destination is not GSTRING #{variableId}");
        if (bytes[position++] != PackedStringMarker)
            throw Error($"the GSTRING #{variableId} source is not packed literal text");
        var packed = GplPackedString.Read(bytes[position..],
            $"MAS #99 GSTRING #{variableId} text");
        if (packed.Kind != GplPackedStringKind.Compressed || string.IsNullOrWhiteSpace(packed.Value))
            throw Error($"the GSTRING #{variableId} source is not non-empty literal text");
        position += packed.BytesConsumed;
        if (position != nextInstructionOffset)
            throw Error($"the GSTRING #{variableId} assignment ends at {position}, " +
                $"not {nextInstructionOffset}");
        return new(new(GlobalStringType, variableId), packed.Value);
    }

    private static void Require(
        ReadOnlySpan<byte> bytes,
        int offset,
        int length,
        ushort variableId)
    {
        if (offset < 0 || length < 0 || offset > bytes.Length - length)
            throw Error($"the GSTRING #{variableId} assignment is truncated");
    }

    private static InvalidDataException Error(string message) =>
        new($"GPL global-string projection: {message}.");
}
