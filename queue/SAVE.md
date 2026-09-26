# SAVE

Next ID: Q-SAVE-003

## Static

- Q-SAVE-001. RULE-SAVE-001, RULE-SAVE-002, SCR-UI-013, SCR-UI-014: What does a `SAVEnn.SAV`
  file hold, which archive do `PREF/100` and the `GREQ` resources go to, what do `F1`, `F2` and
  `F3` do, and which windows, lists and controls make up the Load Game and Save Game screens?
  Settles it: the far routine overlay 192 calls with the saved game's record, its search for
  `SAVE??.SAV`, and the code that sets `save_list_top` (FND-SAVE-004, FND-SAVE-006). Blocks:
  slice 5.
- Q-SAVE-002. FMT-SAVE-001, FMT-SAVE-002: What do the four words and the byte of a `GREQ` resource
  hold, what is the tenth byte the routines keep beside them, and where does a character's `CACT`
  identifier come from? Settles it: the code that reads and writes the globals FND-SAVE-004 lists,
  and the code that gives a new character its identifier. Tried: comparing every 16-bit window of
  both families with the `CHAR` resource numbers, which finds none (FND-SAVE-002). Blocks: slice 5.

## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

None.

## Blocked

None.
