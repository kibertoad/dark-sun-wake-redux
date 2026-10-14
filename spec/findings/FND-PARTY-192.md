---
id: FND-PARTY-192
title: A character made with NEW and DONE has combat_mark 0 until Esc leaves View Character, which writes 1 from overlay 190 +1998, and START GAME with it as the only member makes its slot the leader
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession, rust-dos v1.7.2 debug server (watchpoints, memory reads, screenshots), tools/research/rust-dos-runs/run_plan.py with plans/new-start.json
environment: Windows 11 Pro 10.0.26200; rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 5, DMA 1, HDMA 5, no Ultrasound
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079C93..0x00079CE3
---

## Observation

A segment `S` of the repository's load image is `S - 0A46` in the run, so the combatant table at
`DS:19C9` is at `7662:0004` (49-byte records), slot `n`'s kind byte is at `4503:0C33 + 3n`
(FND-PARTY-171), and the
leader word `4C13:0369` is at `41CD:0369`. Byte `+0x14` of slot 2's combatant record, its
`combat_mark`, is at `7662:007A`.

`plans/new-start.json` starts from the checkpoint `start` of `plans/start.json` (the title
screen) with a watchpoint on `7662:007A`. It clicks Create Characters, right-clicks the empty
third box, clicks `NEW` and then `DONE` (FND-PARTY-190, FND-PARTY-191), presses Esc at 212,000
ms, which closes View Character, and Esc again at 217,000 ms, which shows the title screen. At
223,000 ms it clicks START GAME at `(157, 76)`, and at 275,000 ms it presses Esc during the
opening animation.

| Read at (ms) | Kinds of slots 0 to 3 | Marks of slots 0 to 3 | Hit points of slot 2 | Leader | `DS:0DAB` |
| --- | --- | --- | --- | --- | --- |
| 212,000 (after `DONE`) | 0, 0, 2, 0 | 0, 0, 0, 0 | 7 | 0 | 0 |
| 223,000 (title screen) | 0, 0, 2, 0 | 0, 0, 1, 0 | 7 | 0 | 0 |
| 233,000 and 253,000 | 0, 0, 2, 0 | 0, 0, 1, 0 | 7 | 0 | 0 |
| 275,000 | 0, 0, 2, 0 | 0, 0, 1, 0 | 7 | 0 | 0 |
| 285,000 and 300,000 | 0, 0, 2, 0 | 0, 0, 1, 0 | 7 | 2 | 1 |

The hit points are the record's word `+0x00` (FND-PARTY-050); the details record of slot 2 holds
7 at `+0x08` too, and the generation sheet showed `7/7`. The watchpoint stops once, at 212,014 ms
(the first Esc), on a write of 1 over 0 by the instruction that ends at run `6190:199D`. The 96
bytes read at `6190:1960` at 217,000 ms match overlay 190's code at `+1960` in the file, so the
write is the instruction at overlay 190 `+1998`. The other slots' marks stay 0, and their combatant
records are 0 in every byte read.

The screenshots at 233,000 and 253,000 ms show the opening animation; at 300,000 ms the game is
in play.

## Interpretation

`DONE` stores the new character with `combat_mark` 0, as overlay 184 `+1029` reads (FND-PARTY-189,
`+1391`). The Esc handler of View Character then gives it 1 (FND-PARTY-193). When play starts, the
leader is slot 2, the only slot with a character, as overlay 173 `+3CDA` picks it (FND-PARTY-185):
the party of one with a mark of 1 and 7 hit points counts as standing.

## Alternatives

- The run leaves the screens with Esc only. A route that reaches START GAME from View Character
  without its Esc handler would not pass `+1998`; overlay 182 `+1167`, which START GAME calls,
  holds the same loop (FND-PARTY-193), but this run cannot tell which of the two set the mark
  first, since the mark was already 1.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `GAME_DIR` set to the
installed build and the checkpoint of `plans/start.json` in `artifacts/runs/2026-10-14-start6`:
run `tools/research/rust-dos-runs/run_plan.py artifacts/runs/<dir>
tools/research/rust-dos-runs/plans/new-start.json` and read the memory each step prints, the
watchpoint report and the screenshots. Match the code at the write with the file through
`tools/research/exec-census/overlay_listing.py <dsun> 190 79C60 79D20`.
