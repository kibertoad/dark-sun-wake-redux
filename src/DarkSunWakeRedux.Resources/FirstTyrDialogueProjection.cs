namespace DarkSunWakeRedux.Resources;

public enum GplDialogueTextSourceKind
{
    Literal,
    Variable,
    ActiveCharacterName
}

public sealed record GplDialogueTextSource(
    GplDialogueTextSourceKind Kind,
    string Text,
    byte VariableType,
    ushort VariableId);

public sealed record GplDialogueChoice(
    GplDialogueTextSource Label,
    int TargetOffset,
    GplDialogueCondition Condition);

public enum GplDialogueConditionKind
{
    Constant,
    LocalFlag,
    LocalNumberEquals
}

public sealed record GplDialogueCondition(
    GplDialogueConditionKind Kind,
    ushort VariableId,
    int Value);

public sealed record FirstTyrDialogueProjection(
    uint PortraitResourceNumber,
    IReadOnlyList<GplDialogueTextSource> SpeechVariants,
    IReadOnlyList<GplDialogueChoice> InitialChoices)
{
    public IReadOnlyList<GplDialogueChoice> SecondChoices { get; init; } = [];
    public IReadOnlyList<GplDialogueChoice> ThirdChoices { get; init; } = [];
}

public static class FirstTyrDialogueProjectionReader
{
    public const int PortraitInstructionOffset = 16;
    public const int FirstSpeechInstructionOffset = 118;
    public const int SecondSpeechInstructionOffset = 199;
    public const int InitialMenuInstructionOffset = 253;
    public const int SecondMenuInstructionOffset = 750;
    public const int ThirdMenuInstructionOffset = 2616;
    public const byte ShowPortraitOpcode = 0x54;
    public const byte PrintStringOpcode = 0x4f;
    public const byte MenuOpcode = 0x48;
    public const byte MenuTerminator = 0x4a;
    private const int MaximumMenuChoices = 24;

    public static FirstTyrDialogueProjection Read(PackedGplScript script)
    {
        ArgumentNullException.ThrowIfNull(script);
        if (script.SourceTag != PackedGplScript.GplTag)
            throw Error($"expected source tag {PackedGplScript.GplTag.Trim()}, not {script.SourceTag.Trim()}");
        if (script.ResourceNumber != OriginalContent.FirstTyrDialogueScriptResourceNumber)
            throw Error($"expected GPL #{OriginalContent.FirstTyrDialogueScriptResourceNumber}, " +
                $"not #{script.ResourceNumber}");
        var bytes = script.Bytecode.AsSpan();
        var portrait = ReadPortrait(bytes, PortraitInstructionOffset);
        var speech = new[]
        {
            ReadPrintedText(bytes, FirstSpeechInstructionOffset),
            ReadPrintedText(bytes, SecondSpeechInstructionOffset)
        };
        var choices = ReadMenu(bytes, InitialMenuInstructionOffset);
        var secondChoices = ReadMenu(bytes, SecondMenuInstructionOffset);
        var thirdChoices = ReadMenu(bytes, ThirdMenuInstructionOffset);
        return new(portrait, speech, choices)
        {
            SecondChoices = secondChoices,
            ThirdChoices = thirdChoices
        };
    }

    private static uint ReadPortrait(ReadOnlySpan<byte> bytes, int offset)
    {
        var position = RequireOpcode(bytes, offset, ShowPortraitOpcode);
        var expression = ReadExpression(bytes, ref position, "portrait resource");
        if (expression.Kind != ExpressionKind.Immediate || expression.Number <= 0)
            throw Error("the opening portrait operand is not a positive immediate");
        return checked((uint)expression.Number);
    }

    private static GplDialogueTextSource ReadPrintedText(
        ReadOnlySpan<byte> bytes,
        int offset)
    {
        var position = RequireOpcode(bytes, offset, PrintStringOpcode);
        _ = ReadExpression(bytes, ref position, "print destination");
        return ToTextSource(ReadExpression(bytes, ref position, "printed text"));
    }

    private static IReadOnlyList<GplDialogueChoice> ReadMenu(
        ReadOnlySpan<byte> bytes,
        int offset)
    {
        var position = RequireOpcode(bytes, offset, MenuOpcode);
        _ = ReadExpression(bytes, ref position, "menu selection destination");
        var choices = new List<GplDialogueChoice>();
        while (position < bytes.Length && bytes[position] != MenuTerminator)
        {
            if (choices.Count == MaximumMenuChoices)
                throw Error($"the opening menu exceeds {MaximumMenuChoices} choices");
            var label = ToTextSource(ReadExpression(bytes, ref position, "menu label"));
            var target = ReadExpression(bytes, ref position, "menu target");
            if (target.Kind != ExpressionKind.Immediate || target.Number < 0)
                throw Error("an opening-menu target is not a non-negative immediate offset");
            var conditionStart = position;
            _ = ReadExpression(bytes, ref position, "menu condition");
            var condition = ReadCondition(bytes[conditionStart..position]);
            choices.Add(new(label, target.Number, condition));
        }
        if (position >= bytes.Length || bytes[position] != MenuTerminator)
            throw Error("the opening menu has no terminator");
        if (choices.Count == 0)
            throw Error("the opening menu has no choices");
        return choices;
    }

    private static GplDialogueCondition ReadCondition(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 2 && bytes[0] < 0x80)
            return new(GplDialogueConditionKind.Constant, 0,
                (bytes[0] << 8) | bytes[1]);
        if (bytes.Length == 2 && bytes[0] == 0x8e)
            return new(GplDialogueConditionKind.LocalFlag, bytes[1], 1);
        if (bytes.Length == 7 && bytes[0] == 0xe2 && bytes[1] == 0x82 &&
            bytes[3] == 0xd7 && bytes[4] == 0x8f && bytes[6] == 0xe1)
            return new(GplDialogueConditionKind.LocalNumberEquals,
                bytes[2], unchecked((sbyte)bytes[5]));
        throw Error("an opening-menu condition has an unsupported shape");
    }

    private static int RequireOpcode(ReadOnlySpan<byte> bytes, int offset, byte opcode)
    {
        if ((uint)offset >= (uint)bytes.Length)
            throw Error($"instruction offset {offset} is outside the script");
        if (bytes[offset] != opcode)
            throw Error($"offset {offset} contains opcode 0x{bytes[offset]:X2}, " +
                $"not 0x{opcode:X2}");
        return offset + 1;
    }

    private static GplDialogueTextSource ToTextSource(ExpressionValue expression) =>
        expression.Kind switch
        {
            ExpressionKind.Text => new(GplDialogueTextSourceKind.Literal,
                expression.Text, 0, 0),
            ExpressionKind.ActiveCharacterName => new(
                GplDialogueTextSourceKind.ActiveCharacterName, string.Empty, 0, 0),
            ExpressionKind.Variable => new(GplDialogueTextSourceKind.Variable,
                string.Empty, expression.VariableType, expression.VariableId),
            _ => throw Error("dialogue text is neither a literal nor a string source")
        };

    private static ExpressionValue ReadExpression(
        ReadOnlySpan<byte> bytes,
        ref int position,
        string description)
    {
        var parentheses = 0;
        var compound = false;
        ExpressionValue? first = null;
        while (true)
        {
            if (position >= bytes.Length)
                throw Error($"{description} is truncated");
            var marker = bytes[position++];
            if (marker == 0xe2)
            {
                parentheses++;
                compound = true;
                continue;
            }
            if (marker == 0xe1)
            {
                if (parentheses == 0)
                    throw Error($"{description} has an unmatched closing parenthesis");
                parentheses--;
            }
            else
            {
                var atom = ReadAtom(bytes, marker, ref position, description);
                first ??= atom;
            }

            if (position < bytes.Length && bytes[position] is > 0xd0 and <= 0xdf)
            {
                position++;
                compound = true;
                continue;
            }
            if (parentheses > 0) continue;
            if (first is null) throw Error($"{description} is empty");
            return compound ? first with { Kind = ExpressionKind.Compound } : first;
        }
    }

    private static ExpressionValue ReadAtom(
        ReadOnlySpan<byte> bytes,
        byte marker,
        ref int position,
        string description)
    {
        if (marker < 0x80)
        {
            Require(bytes, position, 1, description);
            return new(ExpressionKind.Immediate, (marker << 8) | bytes[position++],
                string.Empty, 0, 0);
        }
        var type = checked((byte)(marker & 0x7f));
        var extended = (type & 0x40) != 0;
        if (extended) type = checked((byte)(type & ~0x40));
        if (type <= 0x0e)
        {
            var length = extended ? 2 : 1;
            Require(bytes, position, length, description);
            var id = extended
                ? checked((ushort)((bytes[position] << 8) | bytes[position + 1]))
                : bytes[position];
            position += length;
            return new(ExpressionKind.Variable, 0, string.Empty, type, id);
        }
        if (type == 0x0f)
        {
            Require(bytes, position, 1, description);
            return new(ExpressionKind.Immediate, unchecked((sbyte)bytes[position++]),
                string.Empty, 0, 0);
        }
        if (type == 0x11)
        {
            Require(bytes, position, 2, description);
            var value = -((bytes[position] << 8) | bytes[position + 1]);
            position += 2;
            return new(ExpressionKind.Immediate, value, string.Empty, 0, 0);
        }
        if (type == 0x12)
        {
            var packed = GplPackedString.Read(bytes[position..], description);
            position += packed.BytesConsumed;
            return packed.Kind == GplPackedStringKind.ActiveCharacterName
                ? new(ExpressionKind.ActiveCharacterName, 0, string.Empty, 0, 0)
                : new(ExpressionKind.Text, 0, packed.Value, 0, 0);
        }
        throw Error($"{description} uses unsupported expression marker 0x{marker:X2}");
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

    private enum ExpressionKind
    {
        Immediate,
        Variable,
        Text,
        ActiveCharacterName,
        Compound
    }

    private sealed record ExpressionValue(
        ExpressionKind Kind,
        int Number,
        string Text,
        byte VariableType,
        ushort VariableId);

    private static InvalidDataException Error(string message) =>
        new($"First Tyr dialogue projection: {message}.");
}
