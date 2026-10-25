---
id: RULE-SAVE-002
title: The number and file name of a saved game
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SAVE-004, FND-SAVE-005, FND-SAVE-006, FND-SAVE-007, FND-SAVE-008, FND-SAVE-009, FND-SAVE-015, FND-SAVE-020, FND-SAVE-021, FND-SAVE-022, FND-SAVE-023, FND-SAVE-024]
conflicting: []
split_with: []
related: []
---

## Summary

The game keeps at most ten saved games, numbered 1 to 10. Saved game n is the file `SAVEnn.SAV`
in the game's directory, with n in two digits, and its game state is `GREQ` resource n
(FMT-SAVE-002). The Load Game and Save Game screens list the saved games from a first shown one,
and a slot on the screen gives the number by adding its position to that first one. The list
reader requests `STXT/1` from an available `SAVEnn.SAV` archive for its description
(FND-SAVE-007). Saving updates the working `DARKRUN.GFF` archive's `SAVE` resource group and
`STXT/1` description, then copies it to the numbered file (FND-SAVE-008).
It then selects `CHARSAVE.GFF` for the `PREF/100` and `GREQ/n` writes
(FND-SAVE-009). A `SAVEnn.SAV` therefore holds `SAVE/1` to `SAVE/61`, `STXT/1`, `ETME/0` and, for
each region the party has left, `SAVE/(61r + 1)` to `SAVE/(61r + 17)` (FND-SAVE-015,
FND-SAVE-021), with script entry points written as `GPLI/1` numbers and mapped back on every load
(FND-SAVE-023). Loading copies the file back over `DARKRUN.GFF`, reads its 61 resources and then
`PREF/100` and `GREQ/n` from `CHARSAVE.GFF` (FND-SAVE-022). The screens find saved games by
searching the directory for `SAVE??.SAV` and place each in the list by its two digits
(FND-SAVE-020).

## When it runs

When the player saves a game from a slot of the Save Game screen or loads one from a slot of the
Load Game screen (FND-SAVE-004, FND-SAVE-005).

## Parameters

`slot`, the position on the screen of the saved game the player chose, counted from 0.

## Inputs

`save_list_top`.

## Procedure

```text
define saved_game_number(slot):
    return save_list_top + slot + 1

define saved_game_file(slot):
    return sprintf("SAVE%02d.SAV", saved_game_number(slot))
```

## Outputs

`saved_game_number` gives the number of the saved game, which is also the number of its `GREQ`
resource. `saved_game_file` gives the name of its file, which the game appends to its directory,
keeping at most 80 characters.

## Edge cases

Neither routine checks that the number is from 1 to 10; a number above 99 would give a file name
of more than two digits. The list search does not bound the number it reads from a file name, so a
`SAVE11.SAV` or higher in the directory is stored past the ten list records (FND-SAVE-020). A load
whose copy fails still applies the slot's `PREF` and `GREQ` (FND-SAVE-022).

`save_list_top` is 0 at start and only the stored-character list writes it, leaving its last page
start there (0, 10, 20 or 30, kept below the count of stored characters less 9). With 11 or more
stored characters, paging down in that list and then opening Save or Load gives saved games
numbered from 11, past the ten list records (FND-SAVE-024). The original's pattern is `SAVE%.2d.SAV`, which gives the same name as
`%02d` for any number that is not negative.

## What the sources say

SRC-MANUAL-1994, page 14: the Save Game screen has slots, of which a free one says
`<available>`, and the player types a description of the saved game.

## Differences between builds

None known.

## Open questions

None known.
