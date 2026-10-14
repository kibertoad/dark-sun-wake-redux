---
id: FND-PARTY-187
title: In play, View Character prints after each supplied character's experience the threshold of its own classes' rows, since the record at 4E4F:0029 it reads holds class codes 1 to 8 that overlay 184 +1E5C wrote from the 17 codes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006EE5E..0x0006EEF4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FB4C..0x0006FBC3
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession, rust-dos v1.7.2 debug server (breakpoints, watchpoints, memory reads, screenshots)
environment: Windows 11 Pro 10.0.26200; rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 5, DMA 1, HDMA 5, no Ultrasound
---

## Observation

A segment `S` of the repository's load image is `S - 0A46` in the run; overlay code runs where
the overlay manager loads it and is identified by its offset and bytes in the file. Overlay 184's
code starts at file `0x6D090` and overlay 186's at `0x6F700`.

The runs start from the checkpoint `talk` of `start-talk.json`: START GAME chosen first, the
opening conversation answered. They click once in the conversation, click the button at the top
left of the play screen, which opens the USE screen with the four members, click its first icon
in the bottom row, which turns it into View Character with box 1 (Ar'Anda) selected, and then
click boxes 2, 3 and 4 in turn (`(121, 47)`, `(70, 107)`, `(121, 107)` in the game's 320 x 200
pixels).

**What the screen shows.** Each sheet prints after `EXP:` the character's experience and a figure
in brackets:

| Box | Name | Classes on the sheet | Levels | `EXP:` |
| --- | --- | --- | --- | --- |
| 1 | Ar'Anda | Preserver/Psionic/Thief | 6/6/7 | 40000 (55000) |
| 2 | Terrannus | Cleric | 7 | 55000 (110000) |
| 3 | Thy'rokh | Fighter/Druid | 6/6 | 32000 (35000) |
| 4 | Gerakis | Gladiator | 7 | 75000 (150000) |

**The calls.** A breakpoint on the trampoline `4CA0:0020` (`56E6:0020`, which enters overlay 185
`+0000`) stops with the far return `+0484` in the code that matches overlay 186 `+047F`, so the
caller is overlay 186 `+0421` (FND-PARTY-058). The words after the return are `1000`, then the
class byte less 1 and the level byte the caller pushed. The record argument `[bp + 0x0A]` is
`4409:0029`, that is `4E4F:0029`, at every stop. Each sheet makes these calls (some twice, as the
sheet is drawn twice), and the record's bytes read after the selection hold:

| Name | `+0x00` | `+0x12` | `+0x1B..+0x1D` | `+0x1E..+0x20` | Calls (class less 1, level) |
| --- | --- | --- | --- | --- | --- |
| Ar'Anda | 40000 | 3 | 5, 6, 8 | 6, 6, 7 | (4, 6), (5, 6), (7, 7) |
| Terrannus | 55000 | 1 | 1, 0, 0 | 7, 0, 0 | (0, 7) |
| Thy'rokh | 32000 | 8 | 3, 2, 0 | 6, 6, 0 | (2, 6), (1, 6) |
| Gerakis | 75000 | 5 | 4, 0, 0 | 7, 0, 0 | (3, 7) |

In the four 66-byte details records at `761B:0004` (FND-PARTY-186) the same characters' class
bytes are 11, 12, 17; 2; 9, 7; and 10, as `CHAR` records 40 to 43 hold them.

**The writes.** With watchpoints on bytes `+0x1B..+0x1D` of `4E4F:0029`, opening View Character
first changes them from the previous contents to 11, 12, 17 through `1000:3F6F`, called from the
code that matches overlay 184 `+1DD7..+1DE8` (`push 0x42`, the details record, `4E4F:0029`), and
then to 5, 6 and 8, one byte at a time, by the instruction ending at overlay 184 `+1E5C`. Each
later selection repeats the copy and the three stores.

With `DATA` 1000 as FND-PARTY-058 tabulates it, the words the calls read are 600, 550 and 700
for Ar'Anda, 1100 for Terrannus, 640 and 350 for Thy'rokh and 1500 for Gerakis; the least of each
character's, times 100, is the figure in brackets.

## Interpretation

On this path the sheet's record holds the classes' positions in the 8-name table (Preserver 5,
Psionicist 6, Thief 8, Cleric 1, Fighter 3, Druid 2, Gladiator 4), which overlay 184 `+1DCE`
writes back from the 17 codes through the pairs at `4E4F:009C` as FND-PARTY-073 reads it. Overlay
186 `+0421` then reads each class's own row of `DATA` 1000: the figure is the least threshold of
the character's next levels, and not one from another class's table. A supplied Cleric at level
7 shows 110000, not 60000.

## Alternatives

- The runs show only the View Character path from the play screen, for the four supplied
  characters. Other routes to the sheet (the Create Characters screen, a character being made)
  are not run here. FND-PARTY-073 reads the generation buttons storing codes 1 to 8 and `+1DCE`
  converting a stored character's codes back, both into the same record.
- `+1DCE` takes the class bytes from the details record numbered by the word at `4E71:0B44`
  (FND-PARTY-073), not from the record it copies. In these runs the converted codes were those of
  the character selected each time, so the two agreed.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `GAME_DIR` set to the
installed build and the checkpoint of `plans/start-talk.json` in `artifacts/runs/2026-10-14-talk2`:
run `tools/research/rust-dos-runs/run_plan.py` with a plan from that checkpoint whose steps are
clicks at `(160, 40)`, `(23, 18)` and `(50, 171)` at 283,000, 286,000 and 289,000 ms, then at
`(121, 47)`, `(70, 107)` and `(121, 107)` 3,000 ms apart, each step with `"breaks":
["4CA0:0020"]`, a screenshot and a read of `4409:0029` for 0x60 bytes; then the same plan
without the breaks and with `"watch": [["4409:0044", 2], ["4409:0046", 1]]`. Match the code
before each far return with the file through `tools/research/exec-census/overlay_listing.py`.
