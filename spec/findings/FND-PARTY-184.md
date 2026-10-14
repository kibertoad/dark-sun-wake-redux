---
id: FND-PARTY-184
title: Adding Clk'whrr and Meggyn, both stored with combat_mark 0, leaves their combatant records at 1, which they keep through START GAME
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070311..0x00070316
tool: dinorefurb-dosbox-session 0.9.0 RustDosSession, rust-dos v1.7.2 debug server (watchpoints, memory reads)
environment: Windows 11 Pro 10.0.26200; rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 7, DMA 1, HDMA 5
---

## Observation

A segment `S` of the repository's load image is `S - 0A46` in the run; overlay code runs where the
overlay manager loads it and is identified by matching its bytes with the file.

The run repeats EXP-PARTY-002's run 1 (`tools/research/rust-dos-runs/plans/marks-two.json`): Clk'whrr
added to box 1 and Meggyn to box 3, START GAME, to 90,000 ms after it. The far pointer `DS:19C9`
holds `7662:0004` at the start window and after the second ADD, so `combat_mark`, byte `+0x14` of
49-byte record `k`, is at `7662:0018 + 0x31 * k`. Watchpoints cover those four bytes, and the run
prints each write that changes one.

In `CHARSAVE.GFF`, the type-1 chunks of the `CHAR` records 29 to 43 and 50 to 53 hold
`combat_mark` 0 in records 34, 36, 37, 38, 39, 40 and 42 and 1 in the others.

| Point | Records 0 to 3 |
| --- | --- |
| start window | 0, 0, 0, 0 |
| after the second ADD, after Esc twice, and 10,000 and 30,000 ms after START GAME | 0, 1, 0, 1 |
| 60,000 and 90,000 ms after START GAME | 0, 1, 0, 1 |

While the ADD list for box 1 is open, record 1's byte goes from 0 to 1 and back several times: each
time the byte is stored as 1 by the instruction ending at overlay 187 `+04E6` (`mov byte
es:[bx + 0x14], 1` at `+04E1`, FND-PARTY-183) and set back to 0 by a copy through `1000:3F6F`
called from overlay 187 `+047B`. After the second ADD's list, record 3's byte is stored as 1 by
the same instruction. No write changes the bytes from the second ADD to the end of the run.
`leader` goes from 0 to 1 between the reads 30,000 and 60,000 ms after START GAME, as in
EXP-PARTY-002.

## Interpretation

The characters this run adds arrive with `combat_mark` 1 through FND-PARTY-183's store, and nothing
before START GAME's party restore changes it, so that restore finds both slots marked 1 and sets
the leader (FND-PARTY-175).

## Alternatives

- The run does not say which `CHAR` record each added character is. The store at `+04E1` runs only
  after the copy left 0 in the byte, so the data copied last into records 1 and 3 held 0, which
  fits the records stored with 0.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `GAME_DIR` set to the
installed build and the start checkpoint of `plans/start.json` in `artifacts/runs/2026-10-14-start5`:
run `tools/research/rust-dos-runs/run_plan.py artifacts/runs/<new>
tools/research/rust-dos-runs/plans/marks-two.json` and read its `mem` and `write` lines; walk the
`CHAR` records of `CHARSAVE.GFF` as FND-PARTY-181 does and read byte `0x14` of each type-1 chunk's
data.
