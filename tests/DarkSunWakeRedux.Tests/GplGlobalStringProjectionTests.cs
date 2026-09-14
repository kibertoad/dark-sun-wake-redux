using DarkSunWakeRedux.Resources;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class GplGlobalStringProjectionTests
{
    [Fact]
    public void ReadsBoundedSyntheticGlobalExitLabel()
    {
        var projection = GplGlobalStringProjectionReader.Read(new(
            OriginalContent.DialogueGlobalStringsScriptResourceNumber,
            StartupAssetTestArchives.GlobalStringScript()));

        Assert.Equal(new GplDialogueVariable(6, 5), projection.Destination);
        Assert.Equal("Depart!!", projection.Text);
    }

    [Fact]
    public void RejectsWrongIdentityAndInstructionDrift()
    {
        Assert.Throws<InvalidDataException>(() =>
            GplGlobalStringProjectionReader.Read(new(98,
                StartupAssetTestArchives.GlobalStringScript())));
        var bytes = StartupAssetTestArchives.GlobalStringScript();
        bytes[GplGlobalStringProjectionReader.AssignmentInstructionOffset + 2] = 6;
        Assert.Throws<InvalidDataException>(() =>
            GplGlobalStringProjectionReader.Read(new(99, bytes)));
    }
}
