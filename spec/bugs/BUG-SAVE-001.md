---
id: BUG-SAVE-001
title: After Save is answered to the F3 exit choice and the Save Game screen is left with EXIT, the next save or load quits the game
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
impact: rules
intent: unintended
player_reliance: unknown
evidence: [FND-SAVE-018, FND-SAVE-025, FND-SAVE-012]
conflicting: []
split_with: []
related: [RULE-SAVE-001, SCR-UI-014, SCR-UI-013]
---

## Symptom

The player presses F3, answers Save to "EXIT: SAVE GAME?", and then leaves the Save Game screen
with EXIT instead of saving. Play goes on. The next time the player saves with F1 or loads with
F2, the game shows "GAME SAVED" or loads the game and then returns to DOS (FND-SAVE-025).

## Trigger conditions

- The Save answer of the F3 exit choice in overlay 190, or of overlay 181's copy of that choice,
  which stores 1 in `DS:627C` (FND-SAVE-018).
- The Save Game screen left without saving, so the game does not end there.
- A later save or load completed from any Save or Load screen.

## Mechanism

The exit choice marks the save as the last action by storing 1 in `DS:627C` before it opens Save
Game. The Save and Load window's callback, after a save or load, stores 0 in `DS:1462` and returns
-1 when `DS:627C` is not 0, which makes the main loop return and the game exit
(FND-SAVE-012). No code clears `DS:627C`, so leaving the screen with EXIT keeps the mark for the
next save or load (FND-SAVE-018, FND-SAVE-025).

## Frequency

Every time the trigger conditions hold, until the game exits.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

None known.
