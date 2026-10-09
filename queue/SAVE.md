# SAVE

Next ID: Q-SAVE-003

## Static

- Q-SAVE-001. RULE-SAVE-001, RULE-SAVE-002, SCR-UI-013, SCR-UI-014: What does a `SAVEnn.SAV`
  file hold beyond the traced resource group, what effects do the key
  dispatcher's indirect callback and the exit cleanup have, and which
  windows, lists and controls make up the Load Game and Save Game screens?
  Settles it: inventory the working archive's other resources and trace the
  indirect callback, cleanup callees, `SAVE??.SAV` search and list-base writes
  (FND-SAVE-004, FND-SAVE-006). Tried:
  FND-UI-036 identifies the shared `WIND/18500` window, ten list rows and the
  LOAD/SAVE image switch. FND-UI-037 traces row selection and the action
  branches to the save/load routines. FND-SAVE-007 identifies `STXT/1` as
  the source of an available slot's description. FND-SAVE-008 traces `SAVE`
  resource and `STXT/1` writes to `DARKRUN.GFF`, then its copy to the numbered
  file. FND-SAVE-009 traces the later `PREF`/`GREQ` target to
  `CHARSAVE.GFF`. FND-UI-038 traces event-6 row navigation and identifies
  cross-overlay writes to the list-base word; its entry value, physical
  input mapping, other resources and transitions remain open. FND-SAVE-010
  traces the `F1` to `F3` branches. FND-SAVE-012 establishes that the
  exit byte makes the resident loop return and traces startup cleanup to
  the DOS terminate call; indirect cleanup and the key callback remain
  open. Blocks:
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
