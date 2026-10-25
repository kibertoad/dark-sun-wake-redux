---
id: FND-COMBAT-064
title: In the repeated combat-end passes a key is read only when it arrives in the 10 to 13 ms between a pass's flush_input and the next queue read; F1 or F2 pressed then opens its screen and stops the passes, and EXIT or a save stores 4C13:0369 in DS:426D at overlay 182 +1AFF, ending them
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/bug_exit.py with the configs counts/qc035-*.json, at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; SB16 at 0x220, IRQ 5, DMA 1, no Ultrasound; default muted host audio; checkpoint play-ready of run v175-play-ready-b-20261019, and checkpoint control-saved of run qsave006-c1
---

## Observation

Times are emulated ms after the restore; mouse positions are the game's 320 by 200 pixels. Run
addresses are given as the repository's (a resident segment `S` runs at `S - 0A46`); overlay code
was named by matching the bytes around the instruction pointer against `DSUN.EXE`. Every run
watches `DS:426D`, `4C10:0019`, `DS:0DAB`, `DS:0D9C` and the word `4F49:0A32`, which flips once per
pass (FND-COMBAT-061), and breaks on the trampoline of overlay 173 `+059A`.

**The state.** Each run from `play-ready` moves the pointer to (160, 199) at 500 ms and (160, 100)
at 2,500 ms and left-clicks (120, 118) at 3,000 ms. At 4,500 ms `DS:426D` is 9999, `4C10:0019` 0,
`DS:0DAB` 1, `DS:0D9C` 1, `DS:1440` 1 and `DS:0CE6` `FF FF`; the first pass starts at 3,249 ms.

**Keys in the state.** In `qc035-bug-f2` and `qc035-bug-f1`, F2 or F1 pressed at 5,000, 6,000 and
7,000 ms and `2` at 8,500 ms open nothing, change neither `DS:0DAB` nor `DS:426D`, and the key
routine `28C9:0CFF` is not entered. Breakpoints on the queue writer `4464:0230` and on
`flush_input` (`2C5F:0182`) show each key queued (F2 at 5,004 ms: write offset `DS:33C6` 0 to
0x0A at `4464:029C`) and cleared by the next pass's `flush_input` (5,061 ms, at `4464:009E`),
called from overlay 199 `+0CE8` inside `173 +059A`; the read offset `DS:33C4` does not move. In
`qc035-bug-window`, with no input, the flushes fall 57 to 72 ms apart (4,847 to 5,789 ms) and the
queue reader `4464:0382`, called from `39D1:09BD`, runs 10 to 13 ms after each. In the controls
`qc035-ctrl-f2` and `qc035-ctrl-f2-trace`, without the click, F2 at 5,000 ms is read at 5,004 ms,
`28C9:0CFF` and the dispatcher call `28C9:11F4` run, and Load Game opens.

**F2 or F1, then EXIT.** In `qc035-f2-exit` F2 is sent at 5,500 ms, queued after the flush at
5,503 ms and read at 5,516 ms, when `28C9:0CFF` and `28C9:11F4` are entered. At 5,708 ms overlay
182 stores 0 in `DS:0D9C` at `+19B0` and 2 in `DS:0DAB` at `+19E3`, from overlay 190 `+1839`. The
last pass starts at 5,457 ms. Load Game is on screen at 7,000 ms with `DS:426D` 9999 and
`DS:1440` 0. EXIT at (253, 67) at 7,400 ms gives, at 8,109 ms, from overlay 192 `+0CEA` (return
`+0CEF`):

| Site | Change |
| --- | --- |
| overlay 182 `+1AD5` | `DS:0D9C` 0 to 1 |
| overlay 182 `+1AFF` | `DS:426D` 9999 to 1, the value of `4C13:0369` |
| overlay 182 `+1B02` | `DS:0DAB` 2 to 1 |

From 8,400 to 12,800 ms no pass runs, `4C13:035B` rises by 1 every 667 ms or so (0x925 to 0x92B),
and `2` at 9,900 ms enters `28C9:0CFF` at 9,905 ms, leaving `DS:426D` at 1, already the leader.
`qc035-f1-exit` does the same with F1, which opens Save Game; the stores and readings after EXIT
are the same. In `qc035-bug-right-f2` a right click at 4,700 ms leaves `DS:1440` at 1, and F2 at
5,500 ms and EXIT give the same stores.

**Save and load.** In `qc035-save-load`, F1 at 5,500 ms opens Save Game; the name `BUG` is typed
in row 0 and SAVE clicked at (253, 47) at 9,400 ms. At 10,430 ms `+1AD5`, `+1AFF` (`DS:426D` 9999
to 1) and `+1B02` run, and no pass runs to the end of the run at 90,300 ms. F2 at 13,400 ms is read
at 13,404 ms; loading `BUG` stores 4 in `DS:0DAB` at `+160C` at 20,231 ms, and at 22,226 ms 1 in
`DS:0D9C` at `+1745`, 1 in `DS:0DAB` at `+1759` and, at `+1772`, `AX` 1 from `4C13:0369` in
`DS:426D`, over 1. In `qc035-load`, from `control-saved`, `DS:426D` is 1 at the restore and the
click at (120, 118) starts no pass. `DS:0CE6` reads `FF FF` after EXIT, the save and the load.

## Interpretation

In the state every pass empties the key queue, and the game reads it only once per pass, 10 to 13
ms after the flush, so a key or click is taken only when it arrives in that gap, roughly one press
in five to seven by the gaps measured. The leader keys of the recorded runs never reached
`28C9:0CFF`; they were dropped before `28C9:156C`'s test could refuse them. A taken F1 or F2 opens
its screen, which stops the passes, and EXIT or a completed save stores the leader in
`DS:426D`, as FND-COMBAT-063 reads, after which the passes do not come back and keys are read again.

## Alternatives

No run loaded a game while `DS:426D` held 9999: the drive of `play-ready` has no saved game, a
save made in the run ends the state first, and `control-saved` starts with `DS:426D` at 1. That
the load's `+1772` store would end the state rests on the store writing 1 over 1 here. The chance
per press is estimated from one second of gaps. A leader key other than the current leader was
not pressed after EXIT. Why the right click left the pointer mode as it was, unlike
FND-COMBAT-059, was not traced. The overlay 192 frame of the save's call of `+19F8` was named from
FND-SAVE-018, not from the run.

## How to reproduce

From the repository root, with `GAME_DIR` the installed BLD-GOG-EN-1.1 and the checkpoints above:
`artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/bug_exit.py
tools/research/rust-dos-runs/counts/qc035-<x>.json` for `bug-f2`, `bug-f2-trace`, `bug-f1`,
`bug-window`, `ctrl-f2`, `ctrl-f2-trace`, `f2-exit`, `f1-exit`, `bug-right-f2`, `save-load`,
`load` and `probe-f2`. Each run writes `log.json` under `D:/rust-dos-games/work/qc035/<name>`.
