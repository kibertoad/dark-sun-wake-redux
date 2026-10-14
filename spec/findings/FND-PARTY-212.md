---
id: FND-PARTY-212
title: START GAME with the only party member at 0 hit points raises it to 1, shows YOUR PARTY HAS BEEN DEFEATED over the map until a click, then empties the slot and returns to the start window
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession, rust-dos v1.7.2 debug server (guarded memory writes, watchpoints, memory reads, screenshots, key and mouse input), tools/research/rust-dos-runs/run_plan.py with plans/nobody-standing.json and nobody-standing-watch.json
environment: Windows 11 Pro 10.0.26200; rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 5, DMA 1, HDMA 5, no Ultrasound
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E692..0x0005E6A2
---

## Observation

Run addresses are the static ones less `0x0A46`, so the run's `4D9A` is `DS`, `4503:0C33` the slot
kind bytes, `41CD:0369` the leader word, and `7662:0004` the combatant table the far pointer at
`DS:19C9` held; overlay 173 loaded at the run's `605A` (static `6AA0`).

`plans/nobody-standing.json` starts from the checkpoint `added` of `artifacts/runs/2026-10-14-cc7`,
the Create Characters screen with one character in the third box: slot 2 has kind byte 2, and its
combatant record at `7662:0066` has `hit_points` 54, `character_id` `0x8018` and `combat_mark` 1;
the other three records are zero. The plan writes 0 to that `hit_points` word (a guarded write of
the named field), presses Esc twice to reach the start window, clicks START GAME (157, 76), takes
screenshots and reads at 236, 256, 286 and 316 seconds, clicks (160, 100), and reads again at 326
and 356 seconds.

| Time (s) | Screen | `hit_points` | `character_id` | Slot 2 kind | `DS:13FB` | Leader |
|---|---|---|---|---|---|---|
| 236, 256 | the loading picture | 0 | `0x8018` | 2 | 0 | 0 |
| 286, 316 | the map, with a window holding a picture of a bloodied blade and `YOUR PARTY HAS BEEN DEFEATED` | 1 | `0x8018` | 2 | 1 | 0 |
| 326, 356 (after the click) | the start window | 1 | 0 | 0 | 0 | 0 |

`DS:4458` reads 0 at every stop.

`plans/nobody-standing-watch.json`, the same plan with watchpoints on `7662:0066` and `7662:006C`,
stops for the write of 1 to `hit_points` at overlay 173 `+3D80` (the instruction before it,
`+3D7B`, stores the word), in a frame that returns to overlay 173 `+07A4`; and, after the click,
for the write of 0 to `character_id` at `2D40:2539`, in frames returning to `2D40:2488`,
`2D40:244D`, `31E0:0554`, `31E0:0ACC` and `277B:0349`.

## Interpretation

The run shows FND-PARTY-185's reading of overlay 173 `+3CDA`: with no member standing it raises the
fallen member to 1 hit point, leaves `DS:13FB` at 1, sets no leader, and calls overlay 201 `+0410`,
whose window 19502 (FND-PARTY-194) is the defeat message. The window stays until a click. After it
closes, the game takes the character out of the party (its kind byte and `character_id` become 0)
and goes back to the start window, so the player starts again from there with an empty party.

## Alternatives

- The state was made by a memory write; whether a route without one reaches START GAME with no
  member standing is Q-PARTY-073's question.
- Whether the picture is `BMP` 0 or the fallback 10004 was not told apart.
- Only a party of one was tried; with several members, which slots are emptied after the window
  was not run.
- The routine at `2D40:2488` that clears `character_id`, and what it does to the details record,
  were not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `GAME_DIR` set to the
installed build and the checkpoint `added` in `artifacts/runs/2026-10-14-cc7`: run
`tools/research/rust-dos-runs/run_plan.py artifacts/runs/<dir>
tools/research/rust-dos-runs/plans/<plan>` for both plans, and read the `write`, `mem` lines and
the screenshots.
