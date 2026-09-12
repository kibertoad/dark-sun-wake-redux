namespace DarkSunWakeRedux.Core;

public enum StartFlowScreen
{
    StartWindow,
    PartyOverview,
    EmptySlotMenu,
    CharacterGeneration,
    AddExistingCharacter,
    LoadSavedGame,
    Gameplay,
    ExitRequested
}

public enum StartWindowChoice
{
    StartGame,
    CreateCharacters,
    LoadSavedGame,
    ExitToDos
}

public enum EmptySlotChoice { New, Add, Cancel }

public enum PartyOrigin { None, Pregenerated, Created }

public sealed class StartFlow
{
    public StartFlowScreen Screen { get; private set; } = StartFlowScreen.StartWindow;
    public PartyOrigin PartyOrigin { get; private set; }
    public Party Party { get; }

    public StartFlow() => Party = new();

    internal StartFlow(StartFlowSnapshot snapshot)
    {
        Screen = snapshot.Screen;
        PartyOrigin = snapshot.PartyOrigin;
        Party = new(snapshot.PartyMembers);
    }

    public IReadOnlyList<PartyDiagnostic> Choose(StartWindowChoice choice)
    {
        if (Screen != StartFlowScreen.StartWindow) return WrongScreen("start_window_choice");
        switch (choice)
        {
            case StartWindowChoice.StartGame:
                PartyOrigin = PartyOrigin.Pregenerated;
                Screen = StartFlowScreen.Gameplay;
                break;
            case StartWindowChoice.CreateCharacters:
                PartyOrigin = PartyOrigin.Created;
                Screen = StartFlowScreen.PartyOverview;
                break;
            case StartWindowChoice.LoadSavedGame:
                Screen = StartFlowScreen.LoadSavedGame;
                break;
            case StartWindowChoice.ExitToDos:
                Screen = StartFlowScreen.ExitRequested;
                break;
            default:
                return [new("start_choice_invalid", "The selected start-window action is invalid.")];
        }
        return [];
    }

    public IReadOnlyList<PartyDiagnostic> OpenEmptySlotMenu()
    {
        if (Screen != StartFlowScreen.PartyOverview) return WrongScreen("open_empty_slot");
        if (Party.Members.Count == Party.MaximumSize)
            return [new("party_full", "The party already contains four characters.")];
        Screen = StartFlowScreen.EmptySlotMenu;
        return [];
    }

    public IReadOnlyList<PartyDiagnostic> Choose(EmptySlotChoice choice)
    {
        if (Screen != StartFlowScreen.EmptySlotMenu) return WrongScreen("empty_slot_choice");
        switch (choice)
        {
            case EmptySlotChoice.New:
                Screen = StartFlowScreen.CharacterGeneration;
                break;
            case EmptySlotChoice.Add:
                Screen = StartFlowScreen.AddExistingCharacter;
                break;
            case EmptySlotChoice.Cancel:
                Screen = StartFlowScreen.PartyOverview;
                break;
            default:
                return [new("empty_slot_choice_invalid", "The selected empty-slot action is invalid.")];
        }
        return [];
    }

    public IReadOnlyList<PartyDiagnostic> CompleteCharacter(CharacterDraft character)
    {
        if (Screen is not (StartFlowScreen.CharacterGeneration or StartFlowScreen.AddExistingCharacter))
            return WrongScreen("complete_character");
        var diagnostics = Party.Add(character);
        if (diagnostics.Count == 0) Screen = StartFlowScreen.PartyOverview;
        return diagnostics;
    }

    public IReadOnlyList<PartyDiagnostic> BeginCreatedParty()
    {
        if (Screen != StartFlowScreen.PartyOverview) return WrongScreen("begin_created_party");
        if (!Party.CanStart)
            return [new("party_empty", "At least one character is required before starting.")];
        Screen = StartFlowScreen.Gameplay;
        return [];
    }

    public IReadOnlyList<PartyDiagnostic> Cancel()
    {
        switch (Screen)
        {
            case StartFlowScreen.EmptySlotMenu:
            case StartFlowScreen.CharacterGeneration:
            case StartFlowScreen.AddExistingCharacter:
                Screen = StartFlowScreen.PartyOverview;
                return [];
            case StartFlowScreen.LoadSavedGame:
                Screen = StartFlowScreen.StartWindow;
                return [];
            default:
                return WrongScreen("cancel");
        }
    }

    private IReadOnlyList<PartyDiagnostic> WrongScreen(string action) =>
        [new("start_flow_action_unavailable", $"Action '{action}' is unavailable on {Screen}.")];
}
