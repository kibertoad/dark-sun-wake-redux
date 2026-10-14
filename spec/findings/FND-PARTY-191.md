---
id: FND-PARTY-191
title: For a character just made with DONE, EDIT opens generation with its own handler only and DS:1429 back at 4E4F:0029, and the DUAL window prints from 4E4F:0029 with class code 3 and the word at 4E71:0B44 naming that character's slot; both print 125000 for a level 7 Fighter
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession, rust-dos v1.7.2 debug server (memory reads, screenshots), tools/research/rust-dos-runs/run_plan.py with plans/generation-edit.json and plans/dual.json
environment: Windows 11 Pro 10.0.26200; rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 5, DMA 1, HDMA 5, no Ultrasound
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E696..0x0006E6D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009463D..0x00094683
---

## Observation

A segment `S` of the repository's load image is `S - 0A46` in the run, so `DS:A0F1` is
`4D9A:A0F1`, `4E71:0B44` is `442B:0B44`, `4E4F:0029` is `4409:0029` and the details record of
slot 2 in the table at `DS:19C5` is `761B:0088` (FND-PARTY-186).

Both plans repeat `plans/generation-new.json` (FND-PARTY-190): from the title screen, Create
Characters, a right-click on the empty third box and `NEW`, which opens generation with a level
7 human Fighter. They then click `DONE` at `(270, 183)`, which closes generation and shows the
character in the third box with `New` under it, and right-click the box, which opens a window
with `EDIT`, `DROP` and `DUAL`. `plans/generation-edit.json` clicks `EDIT` at `(115, 117)` and
`plans/dual.json` clicks `DUAL` at `(115, 141)`.

| Moment | `DS:A0F1` | `DS:A0F5`, `DS:A0F9` | `DS:1429` | `4E71:0B44` | Class bytes at `4E4F:0029 +0x1B` | Class bytes at `761B:0088 +0x1B` |
| --- | --- | --- | --- | --- | --- | --- |
| after `DONE` | `4D83:0043` | `4CD9:0057`, 0 | `761B:0088` | 2 | 3, 0, 0 | 9, 0, 0 |
| window open | `4C24:0052` | 0, 0 | `761B:0088` | 2 | 3, 0, 0 | 9, 0, 0 |
| after `EDIT` | `4C97:0043` | 0, 0 | `4409:0029` | 2 | 3, 0, 0 | 9, 0, 0 |
| after `DUAL` | `4D6A:0048` | 0, 0 | `761B:0088` | 2 | 3, 0, 0 | 9, 0, 0 |

Both records hold experience 64000 and level 7. In `plans/dual.json`, which also reads the word at
`DS:0DAB` (`4D9A:0DAB`), that word is 0 at every step, from Create Characters to after `DUAL`. After `EDIT` the screen is the generation screen
with Fighter marked and the sheet printing `FIGHTER`, `7` and `EXP:64000 (125000)`. After `DUAL`
a window titled `DUAL` lists the classes and prints `LEVEL 7`, `Fighter` and
`EXP:64000 (125000)`.

## Interpretation

`DONE` stores the character with the 17 codes (Fighter 9) and leaves `DS:1429` at the stored
record, as FND-PARTY-189 reads `+1029`. `EDIT` goes through overlay 184 `+1606` and `+07D8`, which
point `DS:1429` back at the working record before generation's handler takes over, and the
working record holds code 3 (FND-PARTY-073). `4C97:0043` is `56DD:0043`, generation's handler `+0DA7`. The `DUAL`
window's handler is `4D6A:0048`, that is `57B0:0048`, an overlay 209 trampoline. Its sheet, overlay
209 `+151E` (FND-PARTY-188), prints from `4E4F:0029`, where overlay 184 `+1DCE` writes codes 1 to 8
for the slot in `4E71:0B44`. Here that word is 2, the slot of the character whose records were
copied, so the class code and the record agree. The 125000 is Fighter's own next threshold
(FND-PARTY-058, row 2), which code 9 would not give.

## Alternatives

- The DUAL run shows the word at `4E71:0B44` naming the character's own slot on one route, for a
  character made just before. It does not show that no route changes the word before overlay 209
  `+14FF` (Q-PARTY-077). `DS:0DAB` read 0 at each step; a change between the reads would not
  show.
- The class bytes at `4E4F:0029` already held 3 before `DUAL`. The run does not separate what
  `+1DCE` wrote from what generation left there.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `GAME_DIR` set to the
installed build and the checkpoint of `plans/start.json` in `artifacts/runs/2026-10-14-start6`:
run `tools/research/rust-dos-runs/run_plan.py artifacts/runs/<dir>
tools/research/rust-dos-runs/plans/generation-edit.json` and the same with `dual.json`, and read
the screenshots `done.png`, `menu.png`, `edit.png` and `dual.png` and the memory each step prints.
