---
id: FND-PARTY-174
title: From the start window to 90 seconds after START GAME with characters added to boxes 1 and 3, the one write to leader comes from 2D40:3DB4, reached from overlay 173 +3DFA through overlay 182 +1D3E
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3DB4..2D40:3DB9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E71A..0x0005E71F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A5E4..0x0006A5E9
tool: dinorefurb-dosbox-session 0.9.0 RustDosSession, rust-dos v1.7.2 debug server (watchpoint, memory reads)
environment: Windows 11 Pro 10.0.26200; rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 7, DMA 1, HDMA 5
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. The run loads the program's first segment at `05BA`, so a segment `S` of the
repository's load image (based at `1000`) is `S - 0A46` in the run.

The run follows EXP-PARTY-002's run 1 (`tools/research/rust-dos-runs/plans/leader-writes-two.json`,
the plan `exp-add-two-start.json` with a watchpoint) from the start-window checkpoint: Clk'whrr
added to box 1 and Meggyn to box 3, Esc twice, START GAME, Esc 40,000 and 60,000 ms after it, to
90,000 ms after it. A watchpoint covers the two bytes of `leader` (`4C13:0369`, here `41CD:0369`)
for the whole run.

It stops once, 303,034 ms into the emulated run (about 55 s after START GAME), after the
instruction that ends at `2D40:3DB9`, which is `mov es:[0369], si` at `2D40:3DB4` (FND-PARTY-171).
`leader` is 1 from then on. The BP chain at the stop, each frame's word at `[bp+2]` taken as a far
return, gives these return addresses, innermost first. Overlay code was identified by matching the
bytes before each return in the run's memory with the file, leaving out relocated segment words:

| Return | In the file | Code before it |
| --- | --- | --- |
| `67E2:1D99` (here `5D9C:1D99`) | overlay 182 `+1D99` | the call to `2D40:3D4C` in `+1D3E` |
| `6C5E:3DFF` (here `6218:3DFF`) | overlay 173 `+3DFF` | the call at `+3DFA` to `+1D3E` through trampoline `56BD:0020` |
| `6C5E:07A4` (here `6218:07A4`) | overlay 173 `+07A4` | the near call at `+07A1` to `+3CDA` |
| `28C9:2158` | resident | |
| `28C9:242C` | resident, in `28C9:2322` (FND-PARTY-026) | |
| `277B:0574` | resident, the loop of `277D:0004` (FND-PARTY-026) | |
| `1000:015D` | resident, the entry point's call | |

The run made the same 8 draws as EXP-PARTY-002's run 1.

## Interpretation

When START GAME begins play with created characters in boxes 1 and 3, `leader` is set by overlay
173 `+3CDA` (FND-PARTY-185), which the main loop reaches through overlay 173 `+07A1`, and by no
other write in the first 90 seconds.

## Alternatives

- A write by a block copy or by a device would not stop a watchpoint the same way; rust-dos's
  watchpoints check the bytes after each instruction, which covers a `movs` or `rep stos` that
  changes them, but this was not tested here.
- A return address found by the BP chain can be wrong where a routine keeps BP for another use;
  each frame here lands just after a call instruction, which a wrong chain would be unlikely to.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `GAME_DIR` set to the
installed build and rust-dos installed by `tools/Install-RustDos.ps1`: run
`tools/research/rust-dos-runs/run_plan.py artifacts/runs/<new> tools/research/rust-dos-runs/plans/start.json`
into `artifacts/runs/2026-10-14-start5` if that checkpoint does not exist, then
`run_plan.py artifacts/runs/<new> tools/research/rust-dos-runs/plans/leader-writes-two.json`, and
read the `write` and `frame` lines it prints.
