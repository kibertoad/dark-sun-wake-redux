---
id: FND-PARTY-208
title: A half-elf made with Druid first and Preserver second starts at level 6 in both with 40,000 experience while the screen gives 35,000 as the next level's, and its stored details record holds level 1 for the empty third class
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession, rust-dos v1.7.2 debug server (memory reads, screenshots, mouse input), tools/research/rust-dos-runs/run_plan.py with plans/druid-preserver.json
environment: Windows 11 Pro 10.0.26200; rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 5, DMA 1, HDMA 5, no Ultrasound
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C303..0x0006C504
---

## Observation

Run addresses are the static ones less `0x0A46`, so the run's `4D9A` is `DS`, and `4409:0029` is
the working details record `4E4F:0029` (FND-PARTY-073). Coordinates are in the 320 by 200 screen.

`plans/druid-preserver.json` starts from the checkpoint `start` of
`artifacts/runs/2026-10-14-start6`, clicks Create Characters (160, 93), right-clicks the first box
(70, 107) and clicks NEW (115, 117). It clicks the small portrait (160, 35) six times, which turns
the male human fighter the screen opens with into a male half-elf fighter, then clicks the class
list at (250, 29) to remove Fighter, (250, 21) for Druid and (250, 45) for Preserver, and then
DONE (270, 183). It reads 0x90 bytes at `4409:0029` and takes a screenshot after each step, and
after DONE also reads the far pointer at `DS:19C5`, the word at `DS:112E` and 264 bytes from the
address the pointer holds.

In the working record the byte at `0x12` is the origin counted from 1, the bytes `0x1B` to `0x1D`
the class bytes in the screen's codes 1 to 8, `0x1E` to `0x20` the level bytes, and the dword at
`0x00` the experience (FND-PARTY-065):

| Step | Origin | Class bytes | Level bytes | Experience | Screen |
|---|---|---|---|---|---|
| half-elf | 4 | 3, 0, 0 | 7, 0, 0 | 64,000 | MALE HALF-ELF, FIGHTER |
| Fighter removed | 4 | 0, 0, 0 | 0, 0, 0 | 0 | no class |
| Druid | 4 | 2, 0, 0 | 7, 0, 0 | 35,000 | DRUID, 7, `EXP:35000 (60000)` |
| Preserver | 4 | 2, 5, 0 | 6, 6, 1 | 40,000 | DRUID/PRESERVER, `6/6`, `EXP:40000 (35000)` |
| DONE | 4 | 2, 5, 0 | 6, 6, 1 | 40,000 | View Character: `Druid/Preserver`, `6/6`, `EXP:40000 (35000)` |

After DONE, `DS:19C5` holds `761B:0004` and `DS:112E` holds `0xFFFF`. Of the four 66-byte records
there, the first, second and fourth are zero from `0x00` to `0x21`; the third, the box the
character was made in, holds experience 40,000, origin 4, the class bytes 5, 11 and 0 and the
level bytes 6, 6 and 1 at the same offsets.

## Interpretation

The run shows BUG-PARTY-002 as FND-PARTY-065 reads it. With Druid first and Preserver second the
character starts at level 6 in both, with 40,000 experience, the Preserver's level-6 threshold,
and the Druid stays at level 6 although its next threshold, which the screen shows as 35,000, is
below that experience. The empty third position gets level 1, and storing the character keeps it
(FND-PARTY-073): the stored record holds class byte 0 with level byte 1. The stored class bytes
are the 17 codes overlay 184 `+1648` makes from the screen's codes through the table at
`4E4F:00C0` (FND-PARTY-063).

## Alternatives

- Only one pair was made, a male half-elf; the other pairs BUG-PARTY-002 names were not.
- Whether the Druid ever reaches level 7 later, when it gains experience in play, was not run.
- The stored `CHAR` resource in `CHARSAVE.GFF` was not read; the stored details record is the copy
  overlay 184 `+1648` makes in memory.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `GAME_DIR` set to the
installed build and the checkpoint `start` in `artifacts/runs/2026-10-14-start6`: run
`tools/research/rust-dos-runs/run_plan.py artifacts/runs/<dir>
tools/research/rust-dos-runs/plans/druid-preserver.json`, and read the `mem` lines of its output
and the screenshots.
