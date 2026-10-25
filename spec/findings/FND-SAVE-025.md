---
id: FND-SAVE-025
title: After Save is answered to the F3 exit choice and the Save Game screen is left with EXIT, DS:627C stays 1 and the next completed save or load ends the game
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/input_watch.py with the configs counts/qsave006-*.json, at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; SB16 at 0x220, IRQ 5, DMA 1, no Ultrasound; default muted host audio; checkpoint play-ready of run v175-play-ready-b-20261019, and checkpoint control-saved of run qsave006-c1
---

## Observation

Times are emulated ms after the restore; mouse positions are the game's 320 by 200 pixels. Every
run watches the byte `DS:627C` and the word `DS:1462` (run `4D9A:`) and logs each change with the
instruction that made it. At the restore `DS:627C` is 0 and `DS:1462` holds 1. F3 shows the exit
choice "EXIT: SAVE GAME?"; a click at (145, 92) answers Save. On the Save and Load screens the
clicks used are EXIT at (253, 67), SAVE or LOAD at (253, 47) and row 0 at (128, 47); typed text
fills the name box.

**`qsave006-a1`**, from `play-ready`: F3, then Save. At 2,792 ms `DS:627C` goes from 0 to
1 at overlay 190 `+1A25` (file `0x79D65`) and Save Game opens. EXIT at 4,200 ms returns to the play
view, and `DS:627C` still reads 1 at 5,500 and 7,700 ms. F1, row 0, `TEST` typed, SAVE at 11,000
ms: at 11,848 ms the low byte of `DS:1462` goes from 1 to 0 at overlay 192 `+0C16` (file
`0x7DF16`); "GAME SAVED" is shown, then the DOS prompt. `qsave006-a1b` repeats it with more
screenshots and puts the return to DOS between 13,750 and 14,000 ms.

**`qsave006-c1`**, from `play-ready`: the same F1 save without the F3 step writes
`SAVE01.SAV`; neither byte changes and the play view goes on to 20,000 ms. Its checkpoint
`control-saved` is taken after the save.

**`qsave006-a2c`**, from `control-saved`: F3, Save, the same store of 1 in `DS:627C` at
2,797 ms; EXIT; F2, row 0, LOAD. The load shows "LOADING GAME" and a picture for about 50 s; at
61,406 ms `DS:1462` goes to 0 at overlay 192 `+0C16`, and the DOS prompt is up by 64,000 ms. The
load does not change `DS:627C`.

**`qsave006-c2b`**, from `control-saved`: the same F2 load without the F3 step returns to
the play view by 64,000 ms with no change to either byte.

The runs `e1`, `e2`, `e3`, `a2`, `a2b` and `c2` of the same configs found the inputs. The install
directory's files, sizes and times were the same before and after the runs; the saves were written
only to the runs' own C: copies.

## Interpretation

Nothing between leaving the Save Game screen with EXIT and the next save or load clears
`DS:627C`. The Save and Load window's callback then ends the game after that action, as
FND-SAVE-018 reads it.

## Alternatives

The watchpoint reports changes; that nothing wrote an unchanged value in the controls was not
checked. The path from `DS:1462` = 0 to the return to DOS was not traced in these runs
(FND-SAVE-012 reads it). The exit choice was answered with the mouse only. The window's screen
position was measured from one screenshot.

## How to reproduce

From the repository root, with `GAME_DIR` the installed BLD-GOG-EN-1.1:
`artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/input_watch.py
tools/research/rust-dos-runs/counts/qsave006-<x>.json` for `c1` first, then `a1`, `a1b`, `a2c` and
`c2b`. Output goes to `D:/rust-dos-games/work/qsave006/<name>`.
