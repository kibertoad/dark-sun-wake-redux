using DarkSunWakeRedux.Core;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class StartFlowTests
{
    [Theory]
    [InlineData(StartWindowChoice.StartGame, StartFlowScreen.Gameplay, PartyOrigin.Pregenerated)]
    [InlineData(StartWindowChoice.CreateCharacters, StartFlowScreen.PartyOverview, PartyOrigin.Created)]
    [InlineData(StartWindowChoice.LoadSavedGame, StartFlowScreen.LoadSavedGame, PartyOrigin.None)]
    [InlineData(StartWindowChoice.ExitToDos, StartFlowScreen.ExitRequested, PartyOrigin.None)]
    public void StartWindowChoicesHaveDocumentedDestinations(
        StartWindowChoice choice, StartFlowScreen expectedScreen, PartyOrigin expectedOrigin)
    {
        var flow = new StartFlow();

        Assert.Empty(flow.Choose(choice));

        Assert.Equal(expectedScreen, flow.Screen);
        Assert.Equal(expectedOrigin, flow.PartyOrigin);
    }

    [Theory]
    [InlineData(EmptySlotChoice.New, StartFlowScreen.CharacterGeneration)]
    [InlineData(EmptySlotChoice.Add, StartFlowScreen.AddExistingCharacter)]
    [InlineData(EmptySlotChoice.Cancel, StartFlowScreen.PartyOverview)]
    public void EmptyPartySlotOffersNewAddAndCancel(
        EmptySlotChoice choice, StartFlowScreen expectedScreen)
    {
        var flow = CreatedPartyFlow();
        Assert.Empty(flow.OpenEmptySlotMenu());

        Assert.Empty(flow.Choose(choice));

        Assert.Equal(expectedScreen, flow.Screen);
    }

    [Fact]
    public void ValidNewCharacterReturnsToPartyOverview()
    {
        var flow = CreatedPartyFlow();
        flow.OpenEmptySlotMenu();
        flow.Choose(EmptySlotChoice.New);

        var diagnostics = flow.CompleteCharacter(Draft());

        Assert.Empty(diagnostics);
        Assert.Equal(StartFlowScreen.PartyOverview, flow.Screen);
        Assert.Single(flow.Party.Members);
    }

    [Fact]
    public void InvalidCharacterStaysInGeneration()
    {
        var flow = CreatedPartyFlow();
        flow.OpenEmptySlotMenu();
        flow.Choose(EmptySlotChoice.New);

        var diagnostics = flow.CompleteCharacter(Draft(name: ""));

        Assert.Contains(diagnostics, item => item.Code == "character_name_missing");
        Assert.Equal(StartFlowScreen.CharacterGeneration, flow.Screen);
        Assert.Empty(flow.Party.Members);
    }

    [Fact]
    public void CreatedPartyCannotStartEmptyAndCanStartWithOneMember()
    {
        var flow = CreatedPartyFlow();

        Assert.Equal("party_empty", Assert.Single(flow.BeginCreatedParty()).Code);
        Assert.Equal(StartFlowScreen.PartyOverview, flow.Screen);

        flow.OpenEmptySlotMenu();
        flow.Choose(EmptySlotChoice.New);
        flow.CompleteCharacter(Draft());
        Assert.Empty(flow.BeginCreatedParty());
        Assert.Equal(StartFlowScreen.Gameplay, flow.Screen);
    }

    [Fact]
    public void CancelReturnsToOwningScreen()
    {
        var creation = CreatedPartyFlow();
        creation.OpenEmptySlotMenu();
        creation.Choose(EmptySlotChoice.New);
        Assert.Empty(creation.Cancel());
        Assert.Equal(StartFlowScreen.PartyOverview, creation.Screen);

        var load = new StartFlow();
        load.Choose(StartWindowChoice.LoadSavedGame);
        Assert.Empty(load.Cancel());
        Assert.Equal(StartFlowScreen.StartWindow, load.Screen);
    }

    [Fact]
    public void CommandsAreRejectedOutsideTheirScreenWithoutMutation()
    {
        var flow = new StartFlow();

        var diagnostic = Assert.Single(flow.OpenEmptySlotMenu());

        Assert.Equal("start_flow_action_unavailable", diagnostic.Code);
        Assert.Equal(StartFlowScreen.StartWindow, flow.Screen);
    }

    private static StartFlow CreatedPartyFlow()
    {
        var flow = new StartFlow();
        flow.Choose(StartWindowChoice.CreateCharacters);
        return flow;
    }

    private static CharacterDraft Draft(string name = "Rikus") =>
        new(name, CharacterRace.Human, CharacterSex.Male, CharacterAlignment.NeutralGood,
            new(15, 15, 15, 15, 15, 15), [CharacterClass.Fighter]);
}
