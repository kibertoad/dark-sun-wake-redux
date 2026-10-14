---
id: FND-PARTY-204
title: On the generation screen a left click steps the strength, alignment and hit point buttons forward and a right click back, with the wraps RULE-PARTY-010 to RULE-PARTY-012 give, and a middle click, which reaches the game's mouse handler as bits 0x20 and 0x40, changes nothing
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession, rust-dos v1.7.2 debug server (breakpoints, memory reads, screenshots, mouse input), tools/research/rust-dos-runs/run_plan.py with plans/generation-buttons.json, generation-strength-points.json, generation-buttons-middle.json and mouse-event-bits.json
environment: Windows 11 Pro 10.0.26200; rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 5, DMA 1, HDMA 5, no Ultrasound
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44D0:0053..44D0:009E
---

## Observation

Run addresses are the static ones less `0x0A46` (the load segment of this run), so the run's
`4409:0029` is `4E4F:0029`, the working details record (FND-PARTY-187), and `3A8A:0053` is the
mouse event handler `44D0:0053` (FND-INPUT-005). Coordinates are in the 320 by 200 screen.

Each plan starts from the checkpoint `start` of `artifacts/runs/2026-10-14-start6`, clicks Create
Characters (160, 93), right-clicks the first box (70, 107) and clicks NEW (115, 117), which opens
the generation screen on a male human fighter of level 7 with strength 17, dexterity 17,
constitution 16, intelligence 15, wisdom 16, charisma 16, true neutral and 7 hit points out of 7
(the screen shows `7/7` and `23/23`). It then clicks one point at a time three seconds apart, and
after each reads 0x90 bytes from `4409:0029` and takes a screenshot. In those bytes the word at
`0x08` is the hit points the screen shows, the byte at `0x14` the alignment counted from 1, and the
bytes `0x5B` to `0x60` the six scores.

`plans/generation-buttons.json`, at (8, 139) for strength, (81, 146) for alignment and (81, 174)
for hit points:

| Click | Strength | Alignment byte, as shown | Hit points |
|---|---|---|---|
| none | 17 | 5, TRUE NEUTRAL | 7 |
| left on strength | 18 | 5 | 7 |
| right on strength | 17 | 5 | 7 |
| right on strength | 20 | 5 | 7 |
| right on strength | 19 | 5 | 7 |
| left on alignment | 19 | 7, CHAOTIC GOOD | 7 |
| right on alignment | 19 | 5, TRUE NEUTRAL | 7 |
| right on alignment | 19 | 4, NEUTRAL GOOD | 7 |
| left on hit points | 19 | 4 | 84 |
| right on hit points | 19 | 4 | 83 |
| left on hit points | 19 | 4 | 84 |
| left on hit points | 19 | 4 | 21 |

`plans/generation-strength-points.json` left-clicks the strength row and gives 18 at (8, 139), 19 at (4, 141), 20 at
(30, 139) and 17 at (45, 139); clicks at (3, 139), (4, 136), (5, 135) and (5, 138) changed
nothing. The screenshots show the same values.

`plans/generation-buttons-middle.json` makes the same start and middle-clicks strength twice,
alignment once and hit points once at the same points: no byte read changes from the first
reading, and the screen stays at strength 17, true neutral and `7/7`.

`plans/mouse-event-bits.json` makes the same start and clicks strength at (8, 139) with the left,
the right and the middle button, with a breakpoint on `3A8A:0053` for each. The handler stops twice
per click, with `AX` 3 and `BX` 1 then `AX` 4 and `BX` 0 for the left button, `AX` 8 and `BX` 2
then `AX` `0x10` and `BX` 0 for the right, and `AX` `0x20` and `BX` 4 then `AX` `0x40` and `BX` 0
for the middle; strength goes 17, 18, 17, 17.

## Interpretation

The left button steps strength, alignment and hit points forward and the right one back. Past the
top, strength wraps to 17, the fighter's minimum, and below it to 20; alignment skips neutral evil
both ways; hit points below the low bound of 21 go to the high bound of 84 and above it to 21. Those
are RULE-PARTY-010's `step_score`, RULE-PARTY-011's `step_alignment` and RULE-PARTY-012's
`step_hit_points` for a level 7 fighter with constitution 16. The middle button reaches the game's
handler with its own event bits and presses none of the three buttons, as FND-PARTY-205 reads.

## Alternatives

- Only the strength button among the six score buttons was clicked; the others share its handler
  (FND-PARTY-071) but were not pressed.
- The run's mouse driver is rust-dos's; a DOS driver on other hardware may report the buttons with
  other bits, which was not tried.
- The click areas of the buttons were not read, so why the points that changed nothing on the
  strength row missed is not known.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `GAME_DIR` set to the
installed build and the checkpoint `start` in `artifacts/runs/2026-10-14-start6`: run
`tools/research/rust-dos-runs/run_plan.py artifacts/runs/<dir>
tools/research/rust-dos-runs/plans/<plan>` for each of the four plans, read the `mem 4409:0029`
and `break` lines of its output and the screenshots.
