using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed partial class FirstTyrDialogueProjectionTests
{
    [Fact]
    public void ProjectsAlternateThirdMenuResponseFlagTransition()
    {
        var projection = FirstTyrDialogueResponseProjectionReader.ReadThirdMenuFifth(
            ScriptWithThirdMenuFifthResponse());

        Assert.Equal((4, 3686),
            (projection.SourceChoiceIndex, projection.EntryOffset));
        Assert.Equal([68, 19],
            projection.Output.Select(item => item.Text!.Text.Length));
        Assert.Equal([(17, false), (18, true)], projection.LocalFlagAssignments
            .Select(assignment => ((int)assignment.VariableId, assignment.Value)));
    }

    [Fact]
    public void RejectsAlternateThirdMenuResponseDrift()
    {
        var drifted = ScriptWithThirdMenuFifthResponse();
        drifted.Bytecode[3784] = 17;
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadThirdMenuFifth(drifted));
        var truncated = ScriptWithThirdMenuFifthResponse();
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadThirdMenuFifth(truncated with
            {
                Bytecode = truncated.Bytecode[..3785]
            }));
    }

    [Fact]
    public void ProjectsFinalAlternateThirdMenuResponseFlagClear()
    {
        var projection = FirstTyrDialogueResponseProjectionReader.ReadThirdMenuSixth(
            ScriptWithThirdMenuSixthResponse());

        Assert.Equal((5, 3786),
            (projection.SourceChoiceIndex, projection.EntryOffset));
        Assert.Equal([71, 68, 50],
            projection.Output.Select(item => item.Text!.Text.Length));
        Assert.Equal([(18, false)], projection.LocalFlagAssignments
            .Select(assignment => ((int)assignment.VariableId, assignment.Value)));
    }

    [Fact]
    public void RejectsFinalAlternateThirdMenuResponseDrift()
    {
        var drifted = ScriptWithThirdMenuSixthResponse();
        drifted.Bytecode[3974] = 17;
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadThirdMenuSixth(drifted));
        var truncated = ScriptWithThirdMenuSixthResponse();
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadThirdMenuSixth(truncated with
            {
                Bytecode = truncated.Bytecode[..3975]
            }));
    }

    [Fact]
    public void ProjectsThirdMenuExitWithPostAssignmentCompletionCondition()
    {
        var projection = FirstTyrDialogueResponseProjectionReader.ReadThirdMenuExit(
            ScriptWithThirdMenuExit());

        Assert.Equal((6, 3976, true), (projection.SourceChoiceIndex,
            projection.EntryOffset, projection.ReturnsFromLocalBranch));
        Assert.Equal([9, 39], projection.CompletedOutput
            .Select(item => item.Text!.Text.Length));
        Assert.Equal([35], projection.EarlyOutput
            .Select(item => item.Text!.Text.Length));
        Assert.Equal([(8, false)], projection.LocalFlagAssignments
            .Select(assignment => ((int)assignment.VariableId, assignment.Value)));
        var conditional = Assert.Single(
            projection.ConditionalLocalFlagAssignmentsAfterLocalFlags);
        Assert.Equal([1, 6, 7, 11, 10, 8], conditional.Conditions
            .Select(condition => (int)condition.VariableId));
        Assert.Equal((14, true), ((int)conditional.VariableId, conditional.Value));
    }

    [Fact]
    public void RejectsThirdMenuExitControlFlowAndHelperDrift()
    {
        var driftedBranch = ScriptWithThirdMenuExit();
        driftedBranch.Bytecode[3989] = 0;
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadThirdMenuExit(driftedBranch));
        var driftedHelper = ScriptWithThirdMenuExit();
        driftedHelper.Bytecode[4069] = 9;
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadThirdMenuExit(driftedHelper));
        var truncated = ScriptWithThirdMenuExit();
        Assert.Throws<InvalidDataException>(() =>
            FirstTyrDialogueResponseProjectionReader.ReadThirdMenuExit(truncated with
            {
                Bytecode = truncated.Bytecode[..4122]
            }));
    }
}
