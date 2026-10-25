---
id: FND-EXPLORE-010
title: DS:0DA8 is 1 in the load image and the only instruction that stores to it writes 0, when the edge-scroll pass finds the pointer outside the band, so scrolling waits only for the pointer's first time out of the band
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:23B4..28C9:23CC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:23F1..28C9:23F6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004DDA7..0x0004DDAD
    kind: file-data
tool: Capstone 5.0.9; tools/research/exec-census/immediate_search.py and resident_listing.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Static `DS` is `57E0`, so `DS:0DA8` is at file offset `0x4DDA8`. The bytes at `0x4DDA7..0x4DDAD`
are `01 01 FF FF 00 00`: `DS:0DA7` and `DS:0DA8` hold 1, the word `DS:0DA9` holds `0xFFFF` and the
word `DS:0DAB` holds 0.

`immediate_search.py` with `0DA7`, `0DA8`, `0DA9`, `0DAA` and `0DAB` finds two instructions with
the displacement `0x0DA8`, both in the edge-scroll pass `28C9:2322`: the test of the byte at
`28C9:23C2` and the store of 0 at `28C9:23F1`. The instructions with the displacements `0x0DA7` and
`0x0DA9` store bytes into `DS:0DA7` and words into `DS:0DA9`, neither of which covers `DS:0DA8`.
The positive controls are the same search's finds of the test at `28C9:23C2` and the store at
`28C9:23F1`.

In `28C9:2322`, `28C9:23A8..23C0` sends the pass to `28C9:23F1` when the column and row the mouse
driver gives are both nonzero, the column is below 318 and the row below 199, and to `28C9:23C2`
otherwise. `28C9:23C2` ends the pass when `DS:0DA8` is not 0 and goes on to the step test when it
is 0. `28C9:23F1` stores 0 in `DS:0DA8` and ends the pass.

## Interpretation

No instruction stores a value other than 0 in `DS:0DA8`; its 1 comes from the load image. From
program start, the view does not scroll until a pass finds the pointer outside the band once,
and after that the byte stays 0 unless something not covered here, such as a block copy or a
file read over the data segment, writes it. This fits FND-EXPLORE-007, which reads it as 0
throughout its runs from `play-ready`.

## Alternatives

Writes through a pointer or a block copy into the data segment were not searched; Q-RNG-037 and
Q-RNG-038 cover those for the data segment as a whole.

## How to reproduce

`immediate_search.py <dsun> 0DA7 0DA8 0DA9 0DAA 0DAB`; `resident_listing.py <dsun>
28C9:2322..28C9:23F6`; the bytes of `DSUN.EXE` at `0x4DDA7..0x4DDAD`.
