# SAVE

Next ID: Q-SAVE-003

## Static

- Q-SAVE-001. RULE-SAVE-001, RULE-SAVE-002, SCR-UI-013, SCR-UI-014: What does a `SAVEnn.SAV`
  file hold beyond the traced resource group, what do `F1`, `F2` and
  `F3` do, and which windows, lists and controls make up the Load Game and Save Game screens?
  Settles it: inventory the working archive's other resources and trace the
  `F1` to `F3` handlers, `SAVE??.SAV` search and `save_list_top` writes
  (FND-SAVE-004, FND-SAVE-006). Tried:
  FND-UI-036 identifies the shared `WIND/18500` window, ten list rows and the
  LOAD/SAVE image switch. FND-UI-037 traces row selection and the action
  branches to the save/load routines. FND-SAVE-007 identifies `STXT/1` as
  the source of an available slot's description. FND-SAVE-008 traces `SAVE`
  resource and `STXT/1` writes to `DARKRUN.GFF`, then its copy to the numbered
  file. FND-SAVE-009 traces the later `PREF`/`GREQ` target to
  `CHARSAVE.GFF`; other resources and transitions remain open. Blocks:
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
