---
id: RULE-SAVE-001
title: The F1 and F2 screens and F3 exit choice
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SAVE-010, FND-SAVE-012, FND-SAVE-018, FND-SAVE-027, FND-SAVE-028, FND-SAVE-030, FND-SAVE-031, FND-SAVE-032, FND-SAVE-033, FND-COMBAT-124, FND-EXE-398, FND-EXE-558, FND-EXE-559, FND-SAVE-025, FND-UI-035, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-013, SCR-UI-014]
---

## Summary

In overlay 190's key dispatcher, `F1` opens Save Game when combat is
inactive and reports that saving is refused during combat. `F2` opens
Load Game. `F3` opens an exit choice; when combat is inactive, that
choice offers Save, Quit and Cancel (FND-SAVE-010).

## When it runs

When overlay 190 handles a key word (FND-COMBAT-025).

## Parameters

`key_word`, the BIOS keyboard word: `0x3B00` for `F1`, `0x3C00` for
`F2` and `0x3D00` for `F3` (FND-COMBAT-025).

## Inputs

The combat-state word at `4C10:0019` and the exit-control byte at
`DS:1462`.

Archive cleanup also consumes `archive_pending_metadata`, the byte at
runtime archive record offset `0x2C`: zero skips preparation's file
requests, while nonzero selects its checked header/directory write and
commit sequence. Success clears it; a failed check retains it without
rolling back earlier requests (FND-SAVE-028, FND-COMBAT-124).

## Procedure

```text
define save_load_key(key_word):
    if key_word == 0x3B00:
        if combat_state != 0:
            show "CAN'T SAVE DURING COMBAT"
        else:
            show SCR-UI-014
    else if key_word == 0x3C00:
        show SCR-UI-013
    else if key_word == 0x3D00:
        show exit choice, with "SAVE" offered when combat_state == 0
        if choice == 1:
            show SCR-UI-014
        else if choice == 2:
            exit_control_byte = 0
```

## Outputs

The Save Game or Load Game screen, a combat refusal message, or an exit
choice whose Quit branch clears `DS:1462`. That zero value makes the
resident main loop return at its next continuation check; the startup
path then runs the shutdown routine registered with `atexit`, which closes
sound, video, the timer and the archives, and submits a DOS terminate
request (FND-SAVE-012, FND-SAVE-027). Shutdown can request pending
archive header/directory writes when a closed record is dirty. A separately
installed runtime stream hook can also request buffered writes for its
selected records. These conditional mechanisms do not by themselves create
a new gameplay snapshot or prove successful persistence; actual state,
handle targets and other callback effects remain open
(FND-SAVE-028, FND-SAVE-030, Q-SAVE-007).
The known registered library cleanup also dispatches populated slots and
requests deletion of its stored path. One documented slot producer installs
a cleanup target that requests positioning, a zero-count write and close;
it ignores the positioning and write results before closing. Those requests
do not establish the affected live file, a successful deletion, or the OS's
zero-count behavior (FND-EXE-398, FND-EXE-558, FND-EXE-559).
One fixed saved-map Quit run observed five clean records at archive
preparation and unchanged hashes for DARKRUN.GFF, CHARSAVE.GFF,
SAVE02.SAV and RGN032.GFF before the termination request. No monitored
write site hit in that interval; this does not establish other shutdown
states or all file effects (FND-SAVE-031).
A second fixed run observed the actual type-three slot call and normal
register rejoin, and zero counts for all four streams at their flush-count
tests, with the same clean-archive and file-hash result (FND-SAVE-032).
The fixed callee-entry extension also verified that the observed transfer
entered that documented target with the same argument and expected return
frame before its normal rejoin (FND-SAVE-033).
Answering Save sets `DS:627C`, after which the Save Game screen ends the
game once it has saved (FND-SAVE-018).

## Edge cases

The screen that runs the key consumer can pass a routine that releases
its windows and buffers; the consumer calls it before `F1` or `F2` opens
a screen, on Esc, and on the Save and Quit answers. With such a routine,
Esc closes that screen instead of showing the exit choice (FND-SAVE-018).
The exit choice omits its `SAVE` label when the combat-state word is
nonzero (FND-SAVE-010).

Nothing clears `DS:627C`: after Save is answered and the Save Game screen
is left with EXIT, the next save or load ends the game (FND-SAVE-018,
FND-SAVE-025, BUG-SAVE-001).

## What the sources say

SRC-MANUAL-1994, page 77, hotkeys: `F1` saves game, `F2` loads game, `F3` quits game. It also
lists `Alt+X`, which quits the game, and `Esc`, which leaves every menu, or the game when no menu
is shown.

## Differences between builds

None known.

## Open questions

- Which saved-game or working-archive files, if any, are affected by the
  known pending-write and registered slot/delete routes on actual Quit
  paths, and do other callback targets produce saves? Actual dirty records,
  stream state, slot targets/arguments, handle/path identity and native
  outcomes remain unresolved. The type-four slot's installed cleanup
  target and zero-count request sequence are already read; their live
  bindings and external effects are not (FND-SAVE-027, FND-SAVE-028,
  FND-SAVE-030, FND-EXE-398, FND-EXE-559, Q-SAVE-007).
