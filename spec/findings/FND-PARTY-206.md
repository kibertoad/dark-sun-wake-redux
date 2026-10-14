---
id: FND-PARTY-206
title: With left Ctrl held, the generation screen's alignment button steps a fighter from true neutral through neutral evil, chaotic good and chaotic neutral to chaotic evil, DONE stores chaotic evil, and EDIT opens the character at lawful good
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession, rust-dos v1.7.2 debug server (memory reads, screenshots, batched key and mouse input), tools/research/rust-dos-runs/run_plan.py with plans/alignment-ctrl.json
environment: Windows 11 Pro 10.0.26200; rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 5, DMA 1, HDMA 5, no Ultrasound
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C5CE..0x0006C611
---

## Observation

Run addresses are the static ones less `0x0A46`, so the run's `4409:0029` is the working details
record `4E4F:0029` (FND-PARTY-187); its byte at `0x14` is the alignment counted from 1, the bytes
`0x5B` to `0x60` the six scores and the word at `0x08` the hit points (FND-PARTY-204). Coordinates
are in the 320 by 200 screen.

`plans/alignment-ctrl.json` starts from the checkpoint `start` of
`artifacts/runs/2026-10-14-start6`, clicks Create Characters (160, 93), right-clicks the first box
(70, 107) and clicks NEW (115, 117), which opens the generation screen on a male human fighter of
level 7, true neutral (byte 5). It then sends four times, three seconds apart, one batch of input:
left Ctrl down, 200 ms, a left click on the alignment button (81, 146), 300 ms, left Ctrl up. It
then clicks DONE (270, 183), right-clicks the box (70, 107), clicks EDIT (115, 117), and makes one
plain left click on the alignment button. It reads 0x90 bytes at `4409:0029` and takes a
screenshot after each step.

| Step | Alignment byte | Screen |
|---|---|---|
| generation screen | 5 | TRUE NEUTRAL |
| Ctrl and click | 6 | NEUTRAL EVIL |
| Ctrl and click | 7 | CHAOTIC GOOD |
| Ctrl and click | 8 | CHAOTIC NEUTRAL |
| Ctrl and click | 9 | CHAOTIC EVIL |
| DONE | 9 | View Character: DEESTAN, MALE HUMAN, CHAOTIC EVIL, Fighter 7 |
| EDIT | 1 | LAWFUL GOOD |
| click | 2 | LAWFUL NEUTRAL |

The scores (17, 17, 16, 15, 16, 16) and the hit points (7) stay the same at every step.

## Interpretation

Holding Ctrl skips the classes' check (FND-PARTY-207): a fighter steps onto neutral evil and
chaotic evil, which a fighter's row forbids, and DONE stores chaotic evil. Opening the stored
character with EDIT checks the alignment and moves it forward from chaotic evil, past the wrap,
to lawful good, the first one a fighter allows. That fits the static readings: opening the screen
on a stored character calls overlay 183 `+0A3E` with 0, 0 and 0 (FND-PARTY-063), which ends by
calling the roll routine overlay 184 `+033E`, and that calls the check `+1631` with 1
(FND-PARTY-071).

## Alternatives

- Only left Ctrl was held; the BIOS sets the same flag bit for either Ctrl key.
- Only a fighter was stepped, and only forward with Ctrl held.
- The value the stored `CHAR` record holds was read from the screens and the working record, not
  from the archive.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `GAME_DIR` set to the
installed build and the checkpoint `start` in `artifacts/runs/2026-10-14-start6`: run
`tools/research/rust-dos-runs/run_plan.py artifacts/runs/<dir>
tools/research/rust-dos-runs/plans/alignment-ctrl.json`, and read byte `0x14` of each
`mem 4409:0029` line of its output and the screenshots.
