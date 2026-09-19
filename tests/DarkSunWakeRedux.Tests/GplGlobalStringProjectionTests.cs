using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class GplGlobalStringProjectionTests
{
    [Fact]
    public void ReadsBoundedSyntheticGlobalExitLabel()
    {
        var projection = GplGlobalStringProjectionReader.Read(PackedGplScript.FromOwned(
            PackedGplScript.MasTag, OriginalContent.DialogueGlobalStringsScriptResourceNumber,
            StartupAssetTestArchives.GlobalStringScript()));

        Assert.Equal(new GplDialogueVariable(6, 5), projection.Destination);
        Assert.Equal("Depart!!", projection.Text);

        var thirdMenuExit = GplGlobalStringProjectionReader.ReadThirdMenuExit(
            PackedGplScript.FromOwned(PackedGplScript.MasTag,
                OriginalContent.DialogueGlobalStringsScriptResourceNumber,
                StartupAssetTestArchives.GlobalStringScript()));
        Assert.Equal(new GplDialogueVariable(6, 6), thirdMenuExit.Destination);
        Assert.Equal("Continue exploring Tyr!!!", thirdMenuExit.Text);
    }

    [Fact]
    public void RejectsWrongIdentityAndInstructionDrift()
    {
        Assert.Throws<InvalidDataException>(() =>
            GplGlobalStringProjectionReader.Read(PackedGplScript.FromOwned(
                PackedGplScript.MasTag, 98, StartupAssetTestArchives.GlobalStringScript())));
        var bytes = StartupAssetTestArchives.GlobalStringScript();
        bytes[GplGlobalStringProjectionReader.AssignmentInstructionOffset + 2] = 6;
        Assert.Throws<InvalidDataException>(() =>
            GplGlobalStringProjectionReader.Read(PackedGplScript.FromOwned(
                PackedGplScript.MasTag, 99, bytes)));
        var thirdMenuDrift = StartupAssetTestArchives.GlobalStringScript();
        thirdMenuDrift[
            GplGlobalStringProjectionReader.ThirdMenuExitAssignmentInstructionOffset + 2] = 5;
        Assert.Throws<InvalidDataException>(() =>
            GplGlobalStringProjectionReader.ReadThirdMenuExit(PackedGplScript.FromOwned(
                PackedGplScript.MasTag, 99, thirdMenuDrift)));
        Assert.Throws<InvalidDataException>(() =>
            GplGlobalStringProjectionReader.Read(new PackedGplScript(99,
                StartupAssetTestArchives.GlobalStringScript())));
    }
}
