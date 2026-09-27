---
id: RULE-SAVE-002
title: The number and file name of a saved game
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SAVE-004, FND-SAVE-005, FND-SAVE-006, FND-SAVE-007]
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
(FND-SAVE-007).

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
of more than two digits. The original's pattern is `SAVE%.2d.SAV`, which gives the same name as
`%02d` for any number that is not negative.

## What the sources say

SRC-MANUAL-1994, page 14: the Save Game screen has slots, of which a free one says
`<available>`, and the player types a description of the saved game.

## Differences between builds

None known.

## Open questions

- What a `SAVEnn.SAV` file holds beyond `STXT/1`, and whether the game also
  copies `DARKRUN.GFF` into it (FND-SAVE-006, FND-SAVE-007, Q-SAVE-001).
- How many slots the screens show and how `save_list_top` changes (Q-SAVE-001).
