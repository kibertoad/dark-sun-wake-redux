---
id: FND-PARTY-195
title: When a leader key runs 4328:0B4C past its gates, it makes the other VGA page the shown one, copies the shown page onto the hidden one and calls the pointer services, and changes no party state
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession, rust-dos v1.7.2 debug server (breakpoints, memory reads, key input), tools/research/rust-dos-runs/run_plan.py with plans/leader-page-flip.json; Python 3.14.7 with Capstone 5.0.7 for the listings
environment: Windows 11 Pro 10.0.26200; rust-dos v1.7.2 (executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4), deterministic at 15000 instructions a millisecond from 1995-04-11 12:34:56, host sound muted, Sound Blaster 16 at 220, IRQ 7, DMA 1, HDMA 5
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4328:0B4C..4328:0BD5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:5814..1BF3:586B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:42FB..1BF3:4310
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:2A74..1BF3:2A8C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4400:000E..4400:0062
---

## Observation

**Static.** Addresses are in the load image based at segment 1000.

`4328:0B4C` goes past `0B71` only when the word at `DS:2FB8` is not 0, the words at `DS:33B6` and
`DS:33B8` differ and the word at `DS:33BA` is not 0 (FND-CONFIG-171). Past them it calls
`409B:1675` with the far pointer at `DS:A17F`; `3D72:0DE5` with `DS:33B8` when `DS:3330` is not 0,
else `3D72:0B84`; `1BF3:5814` with `0xFFFF` (`0B96`); `4400:000E` with `DS:33B6` and `DS:33B8` when
`DS:A2B2` is not 0; stores 0 at `DS:2FB8` (`0BB5`); and calls `3D72:0E10` with `DS:33B8` when
`DS:3330` is not 0, else `3D72:0942`.

`1BF3:5814` sets DS to CS. When its argument equals the word at `CS:118E` it returns that word.
Otherwise it flips bit 0 of `CS:118E`. For an argument of `0xFFFF` it stores `0xA000` at `CS:0004`
and `0xA400` at `CS:0006` when `CS:118E` is now 0, and the other way round when it is 1. It then
writes the high byte of the start address, register `0x0C` at port `0x3D4`, from `0xA000` when
`CS:118E` is 0 and `0xA400` when it is 1, so the segment stored at `CS:0004` is always the one shown.
It calls `1BF3:2A74`, which reads the input status port (the CRT port from the word at `0040:0063`
plus 6) until bit 3 is clear and then until it is set, and returns `CS:118E`.

`1BF3:42FB` takes a page number `n` and loads the segment from the word at `CS:0004 + 2n` (and
further words of tables at `CS:0204 + 2n` onward). `4400:000E` gets the segment of page `[bp + 8]`
(its second argument) into ES and of page `[bp + 6]` into DS through `1BF3:42FB`, sets bit 0 of
graphics register 5 (write mode 1), enables all four planes through sequencer register 2, copies
`0x3E80` (16,000) bytes from `DS:0` to `ES:0`, and clears bit 0 of register 5 again.

**Run.** A segment `S` of the load image is `S - 0A46` in the run, so `4328` is `38E2`, `1BF3` is
`11AD` and DS is `4D9A`. From the checkpoint `play` of FND-PARTY-178, the run presses 4, which makes
slot 3 the leader (FND-PARTY-178), with breakpoints at `38E2:0B71`, `38E2:0B96` and `38E2:0BB5`, and reads memory before the
key and at 342,000 ms.

| Read | `leader` | `DS:2FB8` | `DS:33B6`, `33B8`, `33BA` | `DS:3330` | `DS:A155`, `A141`, `A033` | `CS:118E` | `CS:0004`, `CS:0006` |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Before the key | 1 | 0 | 0, 1, 1 | 1 | 0, 0, 0 | 1 | `0xA400`, `0xA000` |
| At 342,000 ms | 3 | 0 | 0, 1, 1 | 1 | 0, 0, 0 | 0 | `0xA000`, `0xA400` |

At both reads `DS:A17F` holds 0, `DS:A2B2` 1 and `DS:332E` 0. Each breakpoint stops once, at
338,197.6, 338,198.2 and 338,202.0 ms of emulated time, in that order, with the same caller frame.
The same plan with a breakpoint on `38E2:0B4C` instead stopped there 1,157 times between 338,000
and 340,160 ms before the session failed, so the routine is called many times a second, and the
breakpoints past its gates stop only once from the checkpoint to 342,000 ms.

## Interpretation

On the key's call, `4328:0B4C` runs past its gates once. With the values read, it calls
`409B:1675` with a null pointer, which returns at once (FND-PARTY-035); the pointer services
`3D72:0DE5` and `3D72:0E10` with page 1 (FND-CONFIG-197, FND-CONFIG-189); makes the page the game
had drawn on (`0xA000`) the shown one, waiting for the vertical retrace; and copies the shown page,
page 0, onto the hidden page 1. It writes `DS:2FB8`, `CS:118E`, `CS:0004`, `CS:0006` and video
memory, and nothing of the party, the combatant records or `leader`. So what a key's `set_leader`
changes on screen beyond the view redraw is that the redrawn picture becomes visible at once,
rather than at a later pass of the routine.

## Alternatives

- The values read are those of play outside combat with the map shown. With `DS:3330` 0, or
  `DS:A2B2` 0, the routine takes the other calls or leaves out the copy; neither was run.
- What the pointer services do with `DS:A155` 0 is FND-CONFIG-189's reading and was not traced
  here.
- A copy to video memory shows bytes written, not pixels; the screenshots were not compared.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `GAME_DIR` set to the
installed build and the checkpoint `play` of FND-PARTY-178 in
`artifacts/runs/2026-10-14-twoplay2`: run `tools/research/rust-dos-runs/run_plan.py
artifacts/runs/<dir> tools/research/rust-dos-runs/plans/leader-page-flip.json` and read the `mem`
and `break` lines. Disassemble the load image (file offset of `S:O` is `(S - 0x1000) * 16 + O +
0x5200`) at `4328:0B4C`, `1BF3:5814`, `1BF3:42FB`, `1BF3:2A74` and `4400:000E`.
