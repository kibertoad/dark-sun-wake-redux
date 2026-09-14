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
        Assert.Equal(2, projection.SecondChoices.Count);
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

    [Fact]
    public void ProjectsFirstStraightLineResponseWithoutOriginalTextFixtures()
    {
        var projection = FirstTyrDialogueResponseProjectionReader.ReadFirst(
            ScriptWithFirstResponse());

        Assert.Equal((0, 1017),
            (projection.SourceChoiceIndex, projection.EntryOffset));
        Assert.Equal([
                GplDialogueOutputKind.Text,
                GplDialogueOutputKind.NewLine,
                GplDialogueOutputKind.NewLine,
                GplDialogueOutputKind.Text,
                GplDialogueOutputKind.Text
            ], projection.Output.Select(output => output.Kind));
        Assert.Equal([23, 74, 23], projection.Output
            .Where(output => output.Text is not null)
            .Select(output => output.Text!.Text.Length));
        Assert.Equal([(0, false)], projection.LocalFlagAssignments
            .Select(assignment => ((int)assignment.VariableId, assignment.Value)));
        Assert.Empty(projection.LocalNumberIncrements);
        Assert.True(projection.ReturnsToOpeningMenu);
    }

    [Fact]
    public void RejectsFirstResponseOffsetOpcodeAndAssignmentDrift()
    {
        var driftedPrint = ScriptWithFirstResponse();
        driftedPrint.Bytecode[1043] = 0;
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadFirst(driftedPrint));
        var driftedFlag = ScriptWithFirstResponse();
        driftedFlag.Bytecode[1146] = 1;
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadFirst(driftedFlag));
        var truncated = ScriptWithFirstResponse();
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadFirst(truncated with
            {
                Bytecode = truncated.Bytecode[..1147]
            }));
    }

    [Fact]
    public void ProjectsThirdAndFourthResponsesWithCounterProgression()
    {
        var script = ScriptWithReturningResponses();

        var third = FirstTyrDialogueResponseProjectionReader.ReadThird(script);
        var fourth = FirstTyrDialogueResponseProjectionReader.ReadFourth(script);

        Assert.Equal((2, 1148, 23), (third.SourceChoiceIndex,
            third.EntryOffset, third.Output.Single().Text!.Text.Length));
        Assert.Equal([(2, false)], third.LocalFlagAssignments
            .Select(assignment => ((int)assignment.VariableId, assignment.Value)));
        Assert.Equal([(0, 1)], third.LocalNumberIncrements
            .Select(increment => ((int)increment.VariableId, increment.Amount)));
        Assert.Equal((3, 1183, 38), (fourth.SourceChoiceIndex,
            fourth.EntryOffset, fourth.Output.Single().Text!.Text.Length));
        Assert.Equal([(3, false)], fourth.LocalFlagAssignments
            .Select(assignment => ((int)assignment.VariableId, assignment.Value)));
        Assert.Equal([(0, 1)], fourth.LocalNumberIncrements
            .Select(increment => ((int)increment.VariableId, increment.Amount)));
        Assert.True(third.ReturnsToOpeningMenu);
        Assert.True(fourth.ReturnsToOpeningMenu);
    }

    [Fact]
    public void RejectsThirdAndFourthResponseIncrementAndReturnDrift()
    {
        var driftedIncrement = ScriptWithReturningResponses();
        driftedIncrement.Bytecode[1180] = 0x83;
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadThird(driftedIncrement));
        var driftedReturn = ScriptWithReturningResponses();
        driftedReturn.Bytecode[1231] = 0;
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadFourth(driftedReturn));
        var truncated = ScriptWithReturningResponses();
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadFourth(truncated with
            {
                Bytecode = truncated.Bytecode[..1231]
            }));
    }

    [Fact]
    public void ProjectsSecondResponseConditionalGlobalFlagEffects()
    {
        var projection = FirstTyrDialogueResponseProjectionReader.ReadSecond(
            ScriptWithSecondResponse());

        Assert.Equal((1, 1597),
            (projection.SourceChoiceIndex, projection.EntryOffset));
        Assert.Equal([67, 72, 63], projection.Output
            .Select(output => output.Text!.Text.Length));
        Assert.Equal([(1, false)], projection.LocalFlagAssignments
            .Select(assignment => ((int)assignment.VariableId, assignment.Value)));
        Assert.Equal([(357, true)], projection.GlobalFlagAssignments
            .Select(assignment => ((int)assignment.VariableId, assignment.Value)));
        Assert.Equal([(357, false, 6, true), (357, false, 7, true)],
            projection.ConditionalLocalFlagAssignments.Select(assignment => (
                (int)assignment.Condition.VariableId, assignment.Condition.Value,
                (int)assignment.VariableId, assignment.Value)));
    }

    [Fact]
    public void RejectsSecondResponseConditionalAndGlobalAssignmentDrift()
    {
        var driftedCondition = ScriptWithSecondResponse();
        driftedCondition.Bytecode[1798] = 0x8d;
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadSecond(driftedCondition));
        var driftedAssignment = ScriptWithSecondResponse();
        driftedAssignment.Bytecode[1821] = 0x01;
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadSecond(driftedAssignment));
        var truncated = ScriptWithSecondResponse();
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadSecond(truncated with
            {
                Bytecode = truncated.Bytecode[..1824]
            }));
    }

    [Fact]
    public void ProjectsFifthResponseFlagAndCounterReset()
    {
        var projection = FirstTyrDialogueResponseProjectionReader.ReadFifth(
            ScriptWithFifthResponse());

        Assert.Equal((4, 1232),
            (projection.SourceChoiceIndex, projection.EntryOffset));
        Assert.Equal([61, 69, 21], projection.Output
            .Select(output => output.Text!.Text.Length));
        Assert.Equal([(9, true)], projection.LocalFlagAssignments
            .Select(assignment => ((int)assignment.VariableId, assignment.Value)));
        Assert.Equal([(0, 0)], projection.LocalNumberAssignments
            .Select(assignment => ((int)assignment.VariableId, assignment.Value)));
    }

    [Fact]
    public void RejectsFifthResponseCounterAndReturnDrift()
    {
        var drifted = ScriptWithFifthResponse();
        drifted.Bytecode[1392] = 0x83;
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadFifth(drifted));
        var truncated = ScriptWithFifthResponse();
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadFifth(truncated with
            {
                Bytecode = truncated.Bytecode[..1394]
            }));
    }

    private static PackedGplScript Script()
    {
        var bytes = new byte[1000];
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
        var secondMenu = new List<byte>
        {
            FirstTyrDialogueProjectionReader.MenuOpcode,
            0x86, 4
        };
        secondMenu.AddRange(TextExpression("Second choice"));
        secondMenu.AddRange([0x03, 0xe9, 0x8e, 6]);
        secondMenu.AddRange(TextExpression("Leave"));
        secondMenu.AddRange([0x0b, 0x59, 0x00, 0x01]);
        secondMenu.Add(FirstTyrDialogueProjectionReader.MenuTerminator);
        Write(bytes, FirstTyrDialogueProjectionReader.SecondMenuInstructionOffset, secondMenu);
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

    private static PackedGplScript ScriptWithFirstResponse()
    {
        var script = Script();
        var bytes = new byte[1148];
        script.Bytecode.CopyTo(bytes, 0);
        WritePrint(bytes, 1017, new string('A', 23));
        bytes[1043] = 0x51;
        bytes[1044] = 0x51;
        WritePrint(bytes, 1045, new string('B', 74));
        WritePrint(bytes, 1116, new string('C', 23));
        Write(bytes, 1142, [0x16, 0x8f, 0x00, 0x8e, 0x00, 0x15]);
        return script with { Bytecode = bytes };
    }

    private static PackedGplScript ScriptWithReturningResponses()
    {
        var first = ScriptWithFirstResponse();
        var bytes = new byte[1232];
        first.Bytecode.CopyTo(bytes, 0);
        WritePrint(bytes, 1148, new string('D', 23));
        Write(bytes, 1174, [0x16, 0x8f, 0x00, 0x8e, 0x02, 0x06, 0x82, 0x00, 0x15]);
        WritePrint(bytes, 1183, new string('E', 38));
        Write(bytes, 1223, [0x16, 0x8f, 0x00, 0x8e, 0x03, 0x06, 0x82, 0x00, 0x15]);
        return first with { Bytecode = bytes };
    }

    private static PackedGplScript ScriptWithSecondResponse()
    {
        var script = Script();
        var bytes = new byte[1825];
        script.Bytecode.CopyTo(bytes, 0);
        WritePrint(bytes, 1597, new string('F', 67));
        WritePrint(bytes, 1662, new string('G', 72));
        WritePrint(bytes, 1731, new string('H', 63));
        Write(bytes, 1792,
        [
            0x16, 0x8f, 0x00, 0x8e, 0x01,
            0x18, 0xcd, 0x01, 0x65, 0xd7, 0x8f, 0x00,
            0x3e, 0x07, 0x19,
            0x16, 0x8f, 0x01, 0x8e, 0x06,
            0x16, 0x8f, 0x01, 0x8e, 0x07,
            0x67,
            0x16, 0x8f, 0x01, 0xcd, 0x01, 0x65,
            0x15
        ]);
        return script with { Bytecode = bytes };
    }

    private static PackedGplScript ScriptWithFifthResponse()
    {
        var script = Script();
        var bytes = new byte[1395];
        script.Bytecode.CopyTo(bytes, 0);
        WritePrint(bytes, 1232, new string('I', 61));
        WritePrint(bytes, 1292, new string('J', 69));
        WritePrint(bytes, 1359, new string('K', 21));
        Write(bytes, 1384,
            [0x16, 0x8f, 0x01, 0x8e, 0x09, 0x16, 0x8f, 0x00, 0x82, 0x00, 0x15]);
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
