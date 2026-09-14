namespace DarkSunWakeRedux.Resources;

public enum GplDialogueOutputKind { Text, NewLine }

public sealed record GplDialogueOutput(
    GplDialogueOutputKind Kind,
    GplDialogueTextSource? Text);

public sealed record GplLocalNumberIncrement(ushort VariableId, int Amount);

public sealed record GplGlobalFlagCondition(ushort VariableId, bool Value);

public sealed record GplLocalFlagCondition(ushort VariableId, bool Value);

public sealed record GplGlobalNumberCondition(ushort VariableId, int Value);

public sealed record GplConditionalLocalFlagAssignment(
    GplGlobalFlagCondition Condition,
    ushort VariableId,
    bool Value);

public sealed record GplConditionalLocalFlagAssignmentFromLocalFlag(
    GplLocalFlagCondition Condition,
    ushort VariableId,
    bool Value);

public sealed record GplGlobalFlagAssignment(ushort VariableId, bool Value);

public sealed record FirstTyrDialogueResponseProjection(
    int SourceChoiceIndex,
    int EntryOffset,
    IReadOnlyList<GplDialogueOutput> Output,
    IReadOnlyList<GplLocalFlagAssignment> LocalFlagAssignments,
    IReadOnlyList<GplLocalNumberIncrement> LocalNumberIncrements,
    bool ReturnsToMenu)
{
    public IReadOnlyList<GplGlobalFlagAssignment> GlobalFlagAssignments { get; init; } = [];
    public IReadOnlyList<GplConditionalLocalFlagAssignment>
        ConditionalLocalFlagAssignments { get; init; } = [];
    public IReadOnlyList<GplLocalNumberAssignment> LocalNumberAssignments { get; init; } = [];
    public IReadOnlyList<GplConditionalLocalFlagAssignmentFromLocalFlag>
        ConditionalLocalFlagAssignmentsFromLocalFlags { get; init; } = [];
    public IReadOnlyList<GplGlobalNumberCondition> RequiredGlobalNumberConditions
        { get; init; } = [];
}

public sealed record GplLocalNumberAssignment(ushort VariableId, int Value);

public static class FirstTyrDialogueResponseProjectionReader
{
    public const int FirstChoiceIndex = 0;
    public const int FirstEntryOffset = 1017;
    public const int FirstReturnOffset = 1147;
    public const int SecondChoiceIndex = 1;
    public const int SecondEntryOffset = 1597;
    public const int SecondReturnOffset = 1824;
    public const int ThirdChoiceIndex = 2;
    public const int ThirdEntryOffset = 1148;
    public const int ThirdReturnOffset = 1182;
    public const int FourthChoiceIndex = 3;
    public const int FourthEntryOffset = 1183;
    public const int FourthReturnOffset = 1231;
    public const int FifthChoiceIndex = 4;
    public const int FifthEntryOffset = 1232;
    public const int FifthReturnOffset = 1394;
    public const int TroubleSourceChoiceIndex = 1;
    public const int TroubleEntryOffset = 1825;
    public const int TroubleReturnOffset = 1995;
    public const int KingSourceChoiceIndex = 2;
    public const int KingEntryOffset = 3479;
    public const int KingReturnOffset = 3685;
    public const int CaravanSourceChoiceIndex = 3;
    public const int CaravanEntryOffset = 1996;
    public const int CaravanReturnOffset = 2351;
    public const int AcarSourceChoiceIndex = 4;
    public const int AcarEntryOffset = 2352;
    public const int AcarReturnOffset = 2414;
    public const byte PrintStringOpcode = 0x4f;
    public const byte PrintNewLineOpcode = 0x51;
    public const byte LoadVariableOpcode = 0x16;
    public const byte WordIncrementOpcode = 0x06;
    public const byte LoadAccumulatorOpcode = 0x18;
    public const byte IfOpcode = 0x3e;
    public const byte ElseOpcode = 0x3f;
    public const byte EndIfOpcode = 0x67;
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

    public static FirstTyrDialogueResponseProjection ReadThird(
        PackedGplScript script) => ReadIncrementing(
        script, ThirdChoiceIndex, ThirdEntryOffset, 1174, ThirdReturnOffset, 2);

    public static FirstTyrDialogueResponseProjection ReadFourth(
        PackedGplScript script) => ReadIncrementing(
        script, FourthChoiceIndex, FourthEntryOffset, 1223, FourthReturnOffset, 3);

    public static FirstTyrDialogueResponseProjection ReadSecond(
        PackedGplScript script)
    {
        ValidateScript(script);
        var bytes = script.Bytecode.AsSpan();
        var position = SecondEntryOffset;
        var output = new[]
        {
            ReadLiteralPrint(bytes, ref position, 1662),
            ReadLiteralPrint(bytes, ref position, 1731),
            ReadLiteralPrint(bytes, ref position, 1792)
        };
        var assignment = ReadFlagAssignment(bytes, ref position, 1, false);
        RequireOpcode(bytes, ref position, LoadAccumulatorOpcode,
            "choice 1 global-flag condition");
        RequireSequence(bytes, ref position, [0xcd, 0x01, 0x65, 0xd7, 0x8f, 0x00],
            "global flag #357 equals zero condition");
        RequireOpcode(bytes, ref position, IfOpcode, "choice 1 conditional branch");
        RequireImmediate14(bytes, ref position, 1817, "choice 1 endif target");
        var conditional = new[]
        {
            new GplConditionalLocalFlagAssignment(new(357, false), 6, true),
            new GplConditionalLocalFlagAssignment(new(357, false), 7, true)
        };
        _ = ReadFlagAssignment(bytes, ref position, 6, true);
        _ = ReadFlagAssignment(bytes, ref position, 7, true);
        RequireOpcode(bytes, ref position, EndIfOpcode, "choice 1 endif");
        var globalAssignment = ReadGlobalFlagAssignment(bytes, ref position, 357, true);
        if (position != SecondReturnOffset)
            throw Error($"choice 1 effects end at {position}, not {SecondReturnOffset}");
        RequireOpcode(bytes, ref position, LocalReturnOpcode, "choice 1 local return");
        return new(SecondChoiceIndex, SecondEntryOffset, output, [assignment], [], true)
        {
            GlobalFlagAssignments = [globalAssignment],
            ConditionalLocalFlagAssignments = conditional
        };
    }

    public static FirstTyrDialogueResponseProjection ReadFifth(
        PackedGplScript script)
    {
        ValidateScript(script);
        var bytes = script.Bytecode.AsSpan();
        var position = FifthEntryOffset;
        var output = new[]
        {
            ReadLiteralPrint(bytes, ref position, 1292),
            ReadLiteralPrint(bytes, ref position, 1359),
            ReadLiteralPrint(bytes, ref position, 1384)
        };
        var flagAssignment = ReadFlagAssignment(bytes, ref position, 9, true);
        var numberAssignment = ReadNumberAssignment(bytes, ref position, 0, 0);
        if (position != FifthReturnOffset)
            throw Error($"choice 4 effects end at {position}, not {FifthReturnOffset}");
        RequireOpcode(bytes, ref position, LocalReturnOpcode, "choice 4 local return");
        return new(FifthChoiceIndex, FifthEntryOffset, output, [flagAssignment], [], true)
        {
            LocalNumberAssignments = [numberAssignment]
        };
    }

    public static FirstTyrDialogueResponseProjection ReadTrouble(
        PackedGplScript script)
    {
        ValidateScript(script);
        var bytes = script.Bytecode.AsSpan();
        var position = TroubleEntryOffset;
        var output = new[]
        {
            ReadLiteralPrint(bytes, ref position, 1893),
            ReadLiteralPrint(bytes, ref position, 1962),
            ReadLiteralPrint(bytes, ref position, 1975)
        };
        var assignment = ReadFlagAssignment(bytes, ref position, 6, false);
        RequireOpcode(bytes, ref position, LoadAccumulatorOpcode,
            "trouble response local-flag condition");
        RequireSequence(bytes, ref position, [0x8e, 0x10, 0xd7, 0x8f, 0x00],
            "local flag #16 equals zero condition");
        RequireOpcode(bytes, ref position, IfOpcode, "trouble response conditional branch");
        RequireImmediate14(bytes, ref position, 1994, "trouble response endif target");
        var conditional = new GplConditionalLocalFlagAssignmentFromLocalFlag(
            new(16, false), 10, true);
        _ = ReadFlagAssignment(bytes, ref position, 10, true);
        RequireOpcode(bytes, ref position, EndIfOpcode, "trouble response endif");
        if (position != TroubleReturnOffset)
            throw Error($"trouble response effects end at {position}, not {TroubleReturnOffset}");
        RequireOpcode(bytes, ref position, LocalReturnOpcode, "trouble response local return");
        return new(TroubleSourceChoiceIndex, TroubleEntryOffset, output,
            [assignment], [], true)
        {
            ConditionalLocalFlagAssignmentsFromLocalFlags = [conditional]
        };
    }

    public static FirstTyrDialogueResponseProjection ReadKing(
        PackedGplScript script)
    {
        ValidateScript(script);
        var bytes = script.Bytecode.AsSpan();
        var position = KingEntryOffset;
        var output = new[]
        {
            ReadLiteralPrint(bytes, ref position, 3544),
            ReadLiteralPrint(bytes, ref position, 3611),
            ReadLiteralPrint(bytes, ref position, 3675)
        };
        var assignments = new[]
        {
            ReadFlagAssignment(bytes, ref position, 16, true),
            ReadFlagAssignment(bytes, ref position, 10, false)
        };
        if (position != KingReturnOffset)
            throw Error($"king response effects end at {position}, not {KingReturnOffset}");
        RequireOpcode(bytes, ref position, LocalReturnOpcode, "king response local return");
        return new(KingSourceChoiceIndex, KingEntryOffset, output,
            assignments, [], true);
    }

    public static FirstTyrDialogueResponseProjection ReadOpeningCaravan(
        PackedGplScript script)
    {
        ValidateScript(script);
        var bytes = script.Bytecode.AsSpan();
        var position = CaravanEntryOffset;
        RequireOpcode(bytes, ref position, LoadAccumulatorOpcode,
            "caravan response opening-state condition");
        RequireSequence(bytes, ref position, [0x87, 0x16, 0xd7, 0x8f, 0x01],
            "global number #22 equals one condition");
        RequireOpcode(bytes, ref position, IfOpcode, "caravan response conditional branch");
        RequireImmediate14(bytes, ref position, 2096, "caravan response else target");
        var output = new[]
        {
            ReadLiteralPrint(bytes, ref position, 2069),
            ReadLiteralPrint(bytes, ref position, 2091)
        };
        var firstAssignment = ReadFlagAssignment(bytes, ref position, 11, true);
        RequireOpcode(bytes, ref position, ElseOpcode, "caravan response else");
        RequireImmediate14(bytes, ref position, 2345, "caravan response endif target");
        position = 2345;
        RequireOpcode(bytes, ref position, EndIfOpcode, "caravan response endif");
        var secondAssignment = ReadFlagAssignment(bytes, ref position, 7, false);
        if (position != CaravanReturnOffset)
            throw Error($"caravan response effects end at {position}, not {CaravanReturnOffset}");
        RequireOpcode(bytes, ref position, LocalReturnOpcode, "caravan response local return");
        return new(CaravanSourceChoiceIndex, CaravanEntryOffset, output,
            [firstAssignment, secondAssignment], [], true)
        {
            RequiredGlobalNumberConditions = [new(22, 1)]
        };
    }

    public static FirstTyrDialogueResponseProjection ReadAcar(
        PackedGplScript script)
    {
        ValidateScript(script);
        var bytes = script.Bytecode.AsSpan();
        var position = AcarEntryOffset;
        var output = new[] { ReadLiteralPrint(bytes, ref position, 2409) };
        var assignment = ReadFlagAssignment(bytes, ref position, 11, false);
        if (position != AcarReturnOffset)
            throw Error($"Acar response effects end at {position}, not {AcarReturnOffset}");
        RequireOpcode(bytes, ref position, LocalReturnOpcode, "Acar response local return");
        return new(AcarSourceChoiceIndex, AcarEntryOffset, output,
            [assignment], [], true);
    }

    private static FirstTyrDialogueResponseProjection ReadIncrementing(
        PackedGplScript script,
        int choiceIndex,
        int entryOffset,
        int printEndOffset,
        int returnOffset,
        ushort flagId)
    {
        ValidateScript(script);
        var bytes = script.Bytecode.AsSpan();
        var position = entryOffset;
        var output = new[] { ReadLiteralPrint(bytes, ref position, printEndOffset) };
        var assignment = ReadFlagAssignment(bytes, ref position, flagId, false);
        var increment = ReadNumberIncrement(bytes, ref position, 0);
        if (position != returnOffset)
            throw Error($"choice {choiceIndex} effects end at {position}, not {returnOffset}");
        RequireOpcode(bytes, ref position, LocalReturnOpcode,
            $"choice {choiceIndex} local return");
        return new(choiceIndex, entryOffset, output, [assignment], [increment], true);
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

    private static GplLocalNumberIncrement ReadNumberIncrement(
        ReadOnlySpan<byte> bytes,
        ref int position,
        ushort expectedId)
    {
        RequireOpcode(bytes, ref position, WordIncrementOpcode,
            $"local number #{expectedId} increment");
        Require(bytes, position, 2, $"local number #{expectedId} increment");
        if (bytes[position++] != 0x82 || bytes[position++] != expectedId)
            throw Error($"word increment does not target local number #{expectedId}");
        return new(expectedId, 1);
    }

    private static GplGlobalFlagAssignment ReadGlobalFlagAssignment(
        ReadOnlySpan<byte> bytes,
        ref int position,
        ushort expectedId,
        bool expectedValue)
    {
        RequireOpcode(bytes, ref position, LoadVariableOpcode,
            $"global flag #{expectedId} assignment");
        var value = expectedValue ? (byte)1 : (byte)0;
        RequireSequence(bytes, ref position,
            [0x8f, value, 0xcd, checked((byte)(expectedId >> 8)),
                checked((byte)(expectedId & 0xff))],
            $"global flag #{expectedId} assignment");
        return new(expectedId, expectedValue);
    }

    private static GplLocalNumberAssignment ReadNumberAssignment(
        ReadOnlySpan<byte> bytes,
        ref int position,
        ushort expectedId,
        int expectedValue)
    {
        RequireOpcode(bytes, ref position, LoadVariableOpcode,
            $"local number #{expectedId} assignment");
        RequireSequence(bytes, ref position,
            [0x8f, checked((byte)expectedValue), 0x82, checked((byte)expectedId)],
            $"local number #{expectedId} assignment");
        return new(expectedId, expectedValue);
    }

    private static void RequireSequence(
        ReadOnlySpan<byte> bytes,
        ref int position,
        ReadOnlySpan<byte> expected,
        string description)
    {
        Require(bytes, position, expected.Length, description);
        if (!bytes.Slice(position, expected.Length).SequenceEqual(expected))
            throw Error($"{description} has unexpected operands");
        position += expected.Length;
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
