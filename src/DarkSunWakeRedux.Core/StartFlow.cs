namespace DarkSunWakeRedux.Core;

public enum StartFlowScreen
{
    StartWindow,
    PartyOverview,
    EmptySlotMenu,
    CharacterGeneration,
    AddExistingCharacter,
    OccupiedSlotMenu,
    DualClassSelection,
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
public enum OccupiedSlotChoice { Edit, Drop, Dual }

public enum PartyOrigin { None, ShippedPartyUnresolved, Created }

public sealed class StartFlow
{
    private readonly List<CharacterDraft> _storedCharacters = [];
    public StartFlowScreen Screen { get; private set; } = StartFlowScreen.StartWindow;
    public PartyOrigin PartyOrigin { get; private set; }
    public int? ActiveMemberIndex { get; private set; }
    public Party Party { get; }
    public IReadOnlyList<CharacterDraft> StoredCharacters => _storedCharacters;

    public StartFlow() => Party = new();

    internal StartFlow(StartFlowSnapshot snapshot)
    {
        Screen = snapshot.Screen;
        PartyOrigin = snapshot.PartyOrigin;
        ActiveMemberIndex = snapshot.ActiveMemberIndex;
        Party = new(snapshot.PartyMembers);
        _storedCharacters.AddRange(snapshot.StoredCharacters);
    }

    public IReadOnlyList<PartyDiagnostic> Choose(StartWindowChoice choice)
    {
        if (Screen != StartFlowScreen.StartWindow) return WrongScreen("start_window_choice");
        switch (choice)
        {
            case StartWindowChoice.StartGame:
                PartyOrigin = PartyOrigin.ShippedPartyUnresolved;
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

    public IReadOnlyList<PartyDiagnostic> OpenOccupiedSlotMenu(int index)
    {
        if (Screen != StartFlowScreen.PartyOverview) return WrongScreen("open_occupied_slot");
        if ((uint)index >= (uint)Party.Members.Count)
            return [new("party_member_missing", "The selected party member does not exist.")];
        ActiveMemberIndex = index;
        Screen = StartFlowScreen.OccupiedSlotMenu;
        return [];
    }

    public IReadOnlyList<PartyDiagnostic> Choose(OccupiedSlotChoice choice)
    {
        if (Screen != StartFlowScreen.OccupiedSlotMenu || ActiveMemberIndex is null)
            return WrongScreen("occupied_slot_choice");
        switch (choice)
        {
            case OccupiedSlotChoice.Edit:
                Screen = StartFlowScreen.CharacterGeneration;
                return [];
            case OccupiedSlotChoice.Drop:
                _storedCharacters.Add(Party.RemoveAt(ActiveMemberIndex.Value));
                ActiveMemberIndex = null;
                Screen = StartFlowScreen.PartyOverview;
                return [];
            case OccupiedSlotChoice.Dual:
                var member = Party.Members[ActiveMemberIndex.Value];
                if (member.ClassProgression is null)
                    return [new("dual_class_progression_missing",
                        "Dual-classing requires class-level progression state.")];
                var diagnostics = member.ClassProgression.ValidateCanStartNext(member.Origin);
                if (diagnostics.Count == 0) Screen = StartFlowScreen.DualClassSelection;
                return diagnostics;
            default:
                return [new("occupied_slot_choice_invalid", "The selected party-member action is invalid.")];
        }
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
        if (Screen != StartFlowScreen.CharacterGeneration)
            return WrongScreen("complete_character");
        var diagnostics = ActiveMemberIndex is { } index
            ? Party.ReplaceAt(index, character)
            : Party.Add(character);
        if (diagnostics.Count == 0)
        {
            ActiveMemberIndex = null;
            Screen = StartFlowScreen.PartyOverview;
        }
        return diagnostics;
    }

    public IReadOnlyList<PartyDiagnostic> AddStoredCharacter(int index)
    {
        if (Screen != StartFlowScreen.AddExistingCharacter) return WrongScreen("add_stored_character");
        if ((uint)index >= (uint)_storedCharacters.Count)
            return [new("stored_character_missing", "The selected stored character does not exist.")];
        var diagnostics = Party.Add(_storedCharacters[index]);
        if (diagnostics.Count == 0)
        {
            _storedCharacters.RemoveAt(index);
            Screen = StartFlowScreen.PartyOverview;
        }
        return diagnostics;
    }

    public IReadOnlyList<PartyDiagnostic> ChooseDualClass(CharacterClass nextClass)
    {
        if (Screen != StartFlowScreen.DualClassSelection || ActiveMemberIndex is null)
            return WrongScreen("choose_dual_class");
        var member = Party.Members[ActiveMemberIndex.Value];
        if (member.ClassProgression is null)
            return [new("dual_class_progression_missing",
                "Dual-classing requires class-level progression state.")];
        var transition = member.ClassProgression.TryStartNext(member.Origin, nextClass);
        if (!transition.Accepted) return transition.Diagnostics;
        var updated = member with
        {
            Classes = transition.Progression.Careers.Select(item => item.CharacterClass).ToArray(),
            ClassProgression = transition.Progression
        };
        var diagnostics = Party.ReplaceAt(ActiveMemberIndex.Value, updated);
        if (diagnostics.Count == 0)
        {
            ActiveMemberIndex = null;
            Screen = StartFlowScreen.PartyOverview;
        }
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
            case StartFlowScreen.OccupiedSlotMenu:
            case StartFlowScreen.DualClassSelection:
                ActiveMemberIndex = null;
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
