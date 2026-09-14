using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class FirstTyrDialogueProjectionTests
{
    [Fact]
    public void ProjectsPortraitSpeechVariantsAndMenuWithoutExecutingConditions()
    {
        var projection = FirstTyrDialogueProjectionReader.Read(Script());

        Assert.Equal(18u, projection.PortraitResourceNumber);
        Assert.Equal(["First greeting", "Second greeting"],
            projection.SpeechVariants.Select(item => item.Text));
        Assert.Equal(2, projection.InitialChoices.Count);
        Assert.Equal(GplDialogueTextSourceKind.Literal,
            projection.InitialChoices[0].Label.Kind);
        Assert.Equal("Literal choice", projection.InitialChoices[0].Label.Text);
        Assert.Equal(1017, projection.InitialChoices[0].TargetOffset);
        Assert.Equal(new(GplDialogueConditionKind.LocalFlag, 0, 1),
            projection.InitialChoices[0].Condition);
        Assert.Equal(GplDialogueTextSourceKind.Variable,
            projection.InitialChoices[1].Label.Kind);
        Assert.Equal((6, 5),
            (projection.InitialChoices[1].Label.VariableType,
                projection.InitialChoices[1].Label.VariableId));
        Assert.Equal(2905, projection.InitialChoices[1].TargetOffset);
        Assert.Equal(new(GplDialogueConditionKind.LocalNumberEquals, 0, 2),
            projection.InitialChoices[1].Condition);
    }

    [Fact]
    public void RejectsWrongIdentityOrDriftedObservedInstructions()
    {
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueProjectionReader.Read(Script() with { ResourceNumber = 134 }));

        var drifted = Script();
        drifted.Bytecode[FirstTyrDialogueProjectionReader.PortraitInstructionOffset] = 0;
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueProjectionReader.Read(drifted));
    }

    [Fact]
    public void RejectsUnterminatedMenusAndUnsupportedExpressions()
    {
        var unterminated = Script();
        Array.Fill(unterminated.Bytecode, (byte)0,
            FirstTyrDialogueProjectionReader.InitialMenuInstructionOffset,
            unterminated.Bytecode.Length -
                FirstTyrDialogueProjectionReader.InitialMenuInstructionOffset);
        unterminated.Bytecode[FirstTyrDialogueProjectionReader.InitialMenuInstructionOffset] =
            FirstTyrDialogueProjectionReader.MenuOpcode;
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueProjectionReader.Read(unterminated));

        var unsupported = Script();
        unsupported.Bytecode[FirstTyrDialogueProjectionReader.InitialMenuInstructionOffset + 1] =
            0xb0;
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueProjectionReader.Read(unsupported));
    }

    [Fact]
    public void ProjectsTheBoundedOpeningCompletionBranch()
    {
        var script = ScriptWithCompletion();

        var projection = FirstTyrDialogueCompletionProjectionReader.Read(script);

        Assert.Equal(7, projection.SourceChoiceIndex);
        Assert.Equal(2905, projection.EntryOffset);
        Assert.Equal((GplDialogueTextSourceKind.Variable, (byte)6, (ushort)5),
            (projection.SpokenResponse.Kind, projection.SpokenResponse.VariableType,
                projection.SpokenResponse.VariableId));
        Assert.Equal([(14, true), (4, true)], projection.LocalFlagAssignments
            .Select(assignment => ((int)assignment.VariableId, assignment.Value)));
        Assert.True(projection.ReturnsFromLocalBranch);
    }

    [Fact]
    public void RejectsCompletionBranchDriftAndTruncation()
    {
        var drifted = ScriptWithCompletion();
        drifted.Bytecode[FirstTyrDialogueCompletionProjectionReader.EntryOffset + 9] = 15;
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueCompletionProjectionReader.Read(drifted));
        var complete = ScriptWithCompletion();
        var truncated = complete with
        {
            Bytecode = complete.Bytecode[..FirstTyrDialogueCompletionProjectionReader.ReturnOffset]
        };
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueCompletionProjectionReader.Read(truncated));
    }

    private static PackedGplScript Script()
    {
        var bytes = new byte[500];
        Write(bytes, FirstTyrDialogueProjectionReader.PortraitInstructionOffset,
            [FirstTyrDialogueProjectionReader.ShowPortraitOpcode, 0x8f, 18]);
        WritePrint(bytes, FirstTyrDialogueProjectionReader.FirstSpeechInstructionOffset,
            "First greeting");
        WritePrint(bytes, FirstTyrDialogueProjectionReader.SecondSpeechInstructionOffset,
            "Second greeting");

        var menu = new List<byte>
        {
            FirstTyrDialogueProjectionReader.MenuOpcode,
            0x86, 4
        };
        menu.AddRange(TextExpression("Literal choice"));
        menu.AddRange([0x03, 0xf9, 0x8e, 0]);
        menu.AddRange([0x86, 5, 0x0b, 0x59]);
        menu.AddRange([0xe2, 0x82, 0, 0xd7, 0x8f, 2, 0xe1]);
        menu.Add(FirstTyrDialogueProjectionReader.MenuTerminator);
        Write(bytes, FirstTyrDialogueProjectionReader.InitialMenuInstructionOffset, menu);
        return new(OriginalContent.FirstTyrDialogueScriptResourceNumber, bytes);
    }

    private static PackedGplScript ScriptWithCompletion()
    {
        var script = Script();
        var bytes = new byte[FirstTyrDialogueCompletionProjectionReader.ReturnOffset + 1];
        script.Bytecode.CopyTo(bytes, 0);
        Write(bytes, FirstTyrDialogueCompletionProjectionReader.EntryOffset,
        [
            0x4f, 0x00, 0x73, 0x86, 0x05,
            0x16, 0x8f, 0x01, 0x8e, 0x0e,
            0x16, 0x8f, 0x01, 0x8e, 0x04,
            0x15
        ]);
        return script with { Bytecode = bytes };
    }

    private static void WritePrint(byte[] bytes, int offset, string text)
    {
        var instruction = new List<byte>
        {
            FirstTyrDialogueProjectionReader.PrintStringOpcode,
            0, 115
        };
        instruction.AddRange(TextExpression(text));
        Write(bytes, offset, instruction);
    }

    private static byte[] TextExpression(string text) =>
        [0x92, .. Encode(text)];

    private static byte[] Encode(string text)
    {
        var values = text.Select(character => checked((byte)character))
            .Append(GplPackedString.Terminator).ToArray();
        var bits = new List<int>(values.Length * 7);
        foreach (var value in values)
            for (var bit = 6; bit >= 0; bit--) bits.Add((value >> bit) & 1);
        var packed = new List<byte> { GplPackedString.CompressedMarker };
        for (var offset = 0; offset < bits.Count; offset += 8)
        {
            byte value = 0;
            for (var bit = 0; bit < 8 && offset + bit < bits.Count; bit++)
                value |= checked((byte)(bits[offset + bit] << (7 - bit)));
            packed.Add(value);
        }
        return packed.ToArray();
    }

    private static void Write(byte[] destination, int offset, IEnumerable<byte> source)
    {
        foreach (var value in source) destination[offset++] = value;
    }
}
