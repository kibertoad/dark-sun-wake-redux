namespace DarkSunWakeRedux.Resources;

public readonly record struct GplDialogueVariable(byte Type, ushort Id);

public sealed record GplGlobalStringProjection(
    GplDialogueVariable Destination,
    string Text);

public static class GplGlobalStringProjectionReader
{
    public const int AssignmentInstructionOffset = 20;
    public const int NextInstructionOffset = 33;
    public const byte StringCopyOpcode = 0x0a;
    public const byte GlobalStringMarker = 0x86;
    public const byte PackedStringMarker = 0x92;
    public const byte GlobalStringType = 6;
    public const ushort ExitLabelVariableId = 5;

    public static GplGlobalStringProjection Read(PackedGplScript script)
    {
        ArgumentNullException.ThrowIfNull(script);
        if (script.ResourceNumber != OriginalContent.DialogueGlobalStringsScriptResourceNumber)
            throw Error($"expected MAS #{OriginalContent.DialogueGlobalStringsScriptResourceNumber}, " +
                $"not #{script.ResourceNumber}");
        var bytes = script.Bytecode.AsSpan();
        Require(bytes, AssignmentInstructionOffset, 5);
        var position = AssignmentInstructionOffset;
        if (bytes[position++] != StringCopyOpcode)
            throw Error($"offset {AssignmentInstructionOffset} is not a string-copy instruction");
        if (bytes[position++] != GlobalStringMarker || bytes[position++] != ExitLabelVariableId)
            throw Error("the assignment destination is not GSTRING #5");
        if (bytes[position++] != PackedStringMarker)
            throw Error("the GSTRING #5 source is not packed literal text");
        var packed = GplPackedString.Read(bytes[position..], "MAS #99 GSTRING #5 text");
        if (packed.Kind != GplPackedStringKind.Compressed || string.IsNullOrWhiteSpace(packed.Value))
            throw Error("the GSTRING #5 source is not non-empty literal text");
        position += packed.BytesConsumed;
        if (position != NextInstructionOffset)
            throw Error($"the GSTRING #5 assignment ends at {position}, not {NextInstructionOffset}");
        return new(new(GlobalStringType, ExitLabelVariableId), packed.Value);
    }

    private static void Require(ReadOnlySpan<byte> bytes, int offset, int length)
    {
        if (offset < 0 || length < 0 || offset > bytes.Length - length)
            throw Error("the GSTRING #5 assignment is truncated");
    }

    private static InvalidDataException Error(string message) =>
        new($"GPL global-string projection: {message}.");
}
