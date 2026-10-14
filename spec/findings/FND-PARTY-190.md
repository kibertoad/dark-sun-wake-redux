---
id: FND-PARTY-190
title: Opened by NEW from Create Characters and from View Character in play, the generation screen has its own handler at DS:A0F1, 0 at DS:A0F5 and DS:A0F9 and DS:1429 at 4E4F:0029, and its sheet prints the Fighter threshold for a level 7 Fighter
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession, rust-dos v1.7.2 debug server (memory reads, screenshots), tools/research/rust-dos-runs/run_plan.py with plans/generation-new.json and plans/generation-new-in-play.json
environment: Windows 11 Pro 10.0.26200; rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 5, DMA 1, HDMA 5, no Ultrasound
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D952..0x0006D978
---

## Observation

A segment `S` of the repository's load image is `S - 0A46` in the run, so `DS:A0F1` is
`4D9A:A0F1` and `4E4F:0029` is `4409:0029`.

**From Create Characters.** `plans/generation-new.json` starts from the checkpoint `start` of
`plans/start.json` (the title screen), clicks Create Characters at `(160, 93)`, right-clicks the
empty third box at `(70, 107)`, which opens a window titled `INACTIVE CHARACTER` with `NEW`,
`ADD` and `CANCEL`, and clicks `NEW` at `(115, 117)`.

**From play.** `plans/generation-new-in-play.json` starts from the checkpoint `talk` of
`plans/start-talk.json`, clicks once in the conversation, opens the USE screen at `(23, 18)` and
View Character at `(50, 171)`, right-clicks the fourth box (Gerakis) at `(121, 107)`, which
opens a window with `EDIT NAME`, `DROP` and `CANCEL`, clicks `DROP` at `(165, 129)`, which asks
`DROP THIS CHARACTER?` with `YES`, `NO` and `CANCEL`, right-clicks the box again, clicks `YES` at
`(166, 117)`, which leaves the box empty, right-clicks the empty box, which opens the
`INACTIVE CHARACTER` window, and clicks `NEW` at `(166, 117)`.

**What memory holds.** Before the window opens, View Character has `4D83:0043` at `DS:A0F1` and
`4CD9:0057` at `DS:A0F5`; while the `INACTIVE CHARACTER` window is open, `4C24:0052` at
`DS:A0F1` and 0 at `DS:A0F5` and `DS:A0F9`. Three to six seconds after `NEW`, on both routes:

| Address | Bytes | Reading |
| --- | --- | --- |
| `DS:A0F1` | `43 00 97 4C` | `4C97:0043`, which is `56DD:0043`, the trampoline that enters overlay 184 `+0DA7` |
| `DS:A0F5`, `DS:A0F9` | all 0 | no handler |
| `DS:1429` | `29 00 09 44` | `4409:0029`, that is `4E4F:0029` |
| `4E4F:0029 +0x00` | `00 FA 00 00` | 64000 |
| `4E4F:0029 +0x1B..+0x20` | `03 00 00 07 00 00` | class bytes 3, 0, 0; levels 7, 0, 0 |

**What the screen shows.** The generation screen lists the classes Cleric to Thief with Fighter
marked, and its sheet prints `FIGHTER`, `7` and `EXP:64000 (125000)`.

## Interpretation

On both routes into generation, the screen's events go to its own handler only, and the record it
prints is the working record with class code 3, Fighter's place in the 8-name table. Fighter is
code 9 in the 17-code numbering, so the 125000, Fighter's level 8 threshold in `DATA` 1000
(FND-PARTY-058), shows the sheet indexed the table by the 8-name code. The handlers at
`DS:A0F5` and `DS:A0F9` that Q-PARTY-076 asks about are 0 here: the `INACTIVE CHARACTER` window
had already cleared them before `NEW`.

## Alternatives

- The runs read the slots three to six seconds after `NEW`, once, and pick no class or score.
  A slot set later while the screen is up would not show. Other routes into generation (overlay
  184 `+1606`, the edit path of FND-PARTY-073) were not run.
- An attempt to click other classes in the list missed its positions and is not recorded.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `GAME_DIR` set to the
installed build and the checkpoints of `plans/start.json` in `artifacts/runs/2026-10-14-start6`
and of `plans/start-talk.json` in `artifacts/runs/2026-10-14-talk2`: run
`tools/research/rust-dos-runs/run_plan.py artifacts/runs/<dir>
tools/research/rust-dos-runs/plans/generation-new.json` and the same with
`generation-new-in-play.json`, and read the screenshots `gen.png`, `gen2.png`, `menu4.png` and
`dropped.png` and the memory each step prints.
