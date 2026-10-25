---
id: FND-INPUT-017
title: Grey Up as rust-dos delivers it reaches the key routine 28C9:0CFF as 0x4800 and steps the character north, as keypad Up with no character does; keypad Up typing 8 arrives as 0x4838 and does nothing
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/walk_drive.py with the config counts/walkruns-walk3b.json, at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; SB16 at 0x220, IRQ 5, DMA 1, no Ultrasound; default muted host audio; checkpoint fight of run fightprep (D:/rust-dos-games/work/walkruns)
---

## Observation

Times are emulated ms after restoring `fight` (FND-EXPLORE-022). The session package passes a key
event to rust-dos's `/api/input/key` as given. In rust-dos 1.7.5 the key `"up"` is grey Up
(scan `0x48`, extended, no character) and `"kp8"` keypad 8 (scan `0x48`, character `8`); a named key
sends the table's character unless the event gives `"ascii"`. `28C9:0CFF` compares the full key
word with the 24 words at `28C9:1201` and jumps through `28C9:1231`; `0x4800` is among them and
`0x4838` is not.

Run `walk3b` walks Thy'rokh three cells north, as FND-EXPLORE-022 records, then presses keys with a
breakpoint on `28C9:0CFF` reading 12 stack words. The BIOS shift flags `0040:0017` read `00`
throughout.

| At (ms) | Key event | Entered (ms) | Key word | Result |
| --- | --- | --- | --- | --- |
| 2,500 | `{"key": "up"}` | 2,503.606 | `0x4800` | `28C9:01CB(2, 0x49, 0x58)`; slot 2 word `+0x0C` 1432 to 1416, cell (73, 88) |
| 4,000 | `{"key": "kp8"}` | 4,003.554 | `0x4838` | no call of `28C9:01CB`; position and screen unchanged at 5,300 ms |
| 5,500 | `{"key": "kp8", "ascii": 0}` | 5,503.714 | `0x4800` | `28C9:01CB(2, 0x49, 0x57)`; 1416 to 1400, cell (73, 87) |

The words before the key word are 6, 0, 0, 1, 10 and 0, the event kind and key packet of
FND-INPUT-005.

## Interpretation

A key word of `0x4800` steps the character, whichever key produced it, and `0x4838` matches no
entry. So the grey arrow keys step as the keypad arrows do with NumLock off, and with NumLock on the
keypad arrows, which then type digits, do not step.

## Alternatives

rust-dos stores `0x4800` for grey Up in the key buffer directly; an enhanced BIOS stores `0x48E0`
and its service 0 returns `0x4800`, and that part rests on BIOS documentation. rust-dos types `8`
for `"kp8"` while its shift flags say NumLock is off, which a PC does not do, so the second row
stands for keypad Up with NumLock on only through its key word. Only Up was pressed, and only in
the fight.

## How to reproduce

From the repository root, with `GAME_DIR` the installed BLD-GOG-EN-1.1 and the checkpoint above:
`artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/walk_drive.py
tools/research/rust-dos-runs/counts/walkruns-walk3b.json`, which writes `log.json` under
`D:/rust-dos-games/work/walkruns/walk3b`.
