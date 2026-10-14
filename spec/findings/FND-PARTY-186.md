---
id: FND-PARTY-186
title: START GAME chosen first from the start window fills the four details records with the experience and identifiers of CHAR records 40, 41, 42 and 43
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x00..0x2DD7
tool: dinorefurb-dosbox-session 0.9.0 RustDosSession, rust-dos v1.7.2 debug server (memory reads)
environment: Windows 11 Pro 10.0.26200; rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 7, DMA 1, HDMA 5
---

## Observation

The run is EXP-PARTY-001's run 2 (`tools/research/rust-dos-runs/plans/exp-start-control.json`):
from the start-window checkpoint, START GAME and nothing else. It reads the far pointer
`DS:19C5`, which holds `761B:0004` (run addresses; a segment `S` of the repository's load image is
`S - 0A46`), and the 264 bytes there, the four 66-byte FMT-COMBAT-002 details records.

At the start window the 264 bytes are 0 and `object_slot_kinds` is 0 for the four slots. 10,000 ms
after START GAME and at every later read (30,000, 60,000 and 90,000 ms), `object_slot_kinds` is 2
for the four slots and the records hold:

| Record | Experience (`UINT32` at `+0x00`) | Word at `+0x0E` |
| --- | --- | --- |
| 0 | 40000 | `0x8019` |
| 1 | 55000 | `0x8018` |
| 2 | 32000 | `0x8017` |
| 3 | 75000 | `0x8016` |

In `CHARSAVE.GFF`, walked as FND-PARTY-181 describes, the type-3 chunk of each `CHAR` record holds
these pairs: 40 holds 40000 and `0x8019`, 41 55000 and `0x8018`, 42 32000 and `0x8017`, 43 75000
and `0x8016`. No other record holds any of these four pairs: records 35 to 38 hold the same words
with 30000, 30000, 40000 and 36000; record 39 holds 75000 with `0x8015`; record 53 holds
2475000 with `0xE482` and record 33 365393 with `0xFFF2`.

## Interpretation

Chosen first from the start window, START GAME loads `CHAR` records 40, 41, 42 and 43 into the
party's slots 0 to 3, in that order, and neither 53 nor 33.

## Alternatives

- The run is under rust-dos's DOS, not the DOSBox GOG ships; the paths that rest on what DOS
  reports (FND-PARTY-034, FND-PARTY-038) took the branch that finds `CHARSAVE.GFF` there.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `GAME_DIR` set to the
installed build and the start checkpoint of `plans/start.json` in `artifacts/runs/2026-10-14-start5`:
run `tools/research/rust-dos-runs/run_plan.py artifacts/runs/<new>
tools/research/rust-dos-runs/plans/exp-start-control.json`, decode the `761B:0004` reads as four
66-byte records, and walk `CHARSAVE.GFF`'s `CHAR` records printing each type-3 chunk's `UINT32`
at `0x00` and word at `0x0E`.
