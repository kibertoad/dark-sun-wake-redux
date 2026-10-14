---
id: FND-PARTY-178
title: In play with characters in boxes 1 and 3, the keys 2 and 4 make their slot the leader through 28C9:0EBD and the keys 1 and 3, whose slots are empty, write nothing to leader
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0EBD..28C9:0EC2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3DB4..2D40:3DB9
tool: dinorefurb-dosbox-session 0.9.0 RustDosSession, rust-dos v1.7.2 debug server (watchpoint, memory reads, key input)
environment: Windows 11 Pro 10.0.26200; rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 7, DMA 1, HDMA 5
---

## Observation

A segment `S` of the repository's load image is `S - 0A46` in the run.

`tools/research/rust-dos-runs/plans/two-play.json` repeats EXP-PARTY-002's run 1 (Clk'whrr in
box 1, Meggyn in box 3, START GAME) and saves a checkpoint 90,000 ms after START GAME, with the
room on screen, `object_slot_kinds` 0, 2, 0, 2, `object_slot_combatants` 0, 1, 0, 3 and `leader`
1. From it, `plans/leader-keys.json` presses the keys 4, 1, 2, 3 and 4, one every 4,000 ms of
emulated time, with a watchpoint on `leader` (`4C13:0369`), and reads `leader` before the next key:

| Key | Slot | Writes to `leader` | `leader` after |
| --- | --- | --- | --- |
| 4 | 3 | one, of 3 | 3 |
| 1 | 0, empty | none | 3 |
| 2 | 1 | one, of 1 | 1 |
| 3 | 2, empty | none | 1 |
| 4 | 3 | one, of 3 | 3 |

Each write stops after `mov es:[0369], si` at `2D40:3DB4`. Its BP chain gives the returns
overlay 182 `+1D99` (in `+1D3E`), `28C9:0EC2` (after the far call at `28C9:0EBD`), `39D1:083F`,
`39D1:0742`, `277B:055B` (the main loop of `277D:0004`) and `1000:015D`. `object_slot_kinds` stays
0, 2, 0, 2. The screen is the same frame (`53bc28736610911212c97d8e140fb99d`) at every read. The
run made 13 draws, all by script instruction `0x52` (RULE-SCRIPT-006), and every result matched
the original's.

## Interpretation

Outside combat the keys 1 to 4 make the character in their slot the leader, through the key code
of FND-PARTY-176 and the setter of FND-PARTY-171, and a key whose slot has no character leaves the
leader as it was. The run did not read the combatant records or the `object_slots` entries, so it
does not show which of FND-PARTY-176's tests turned the keys 1 and 3 away.

## Alternatives

- The run presses keys with the room on screen and no combat; a key during combat, or on another
  screen, was not tried.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `GAME_DIR` set to the
installed build and the start checkpoint of `plans/start.json` in `artifacts/runs/2026-10-14-start5`:
run `tools/research/rust-dos-runs/run_plan.py artifacts/runs/2026-10-14-twoplay
tools/research/rust-dos-runs/plans/two-play.json`, then `run_plan.py artifacts/runs/<new>
tools/research/rust-dos-runs/plans/leader-keys.json`, and read its `mem` and `write` lines.
