---
id: FND-RNG-076
title: Overlay 173 +059A, given combat word 0 and 9999 at its pointer, passes 9999 to no callee; with no slot to keep it runs the round step +0C65, which adds 60 to the dword 4C13:035B, and then ends combat
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005AEBA..0x0005AEFC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005AFE4..0x0005B11C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B585..0x0005B5D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B98A..0x0005B9A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B9B8..0x0005BB58
tool: tools/research/exec-census/overlay_listing.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Overlay 173 code starts at file offset `0x5A920`. `+059A(a, p)` loads `p` into `DI` and the word at
`p` into `SI`, and reads the slot's entry `4F49:0C33 + 3 × SI` into `[bp-4]` only when `SI` is below
9999 (`+05A2..+05C0`). It stores 9999 in `DS:44E6` (`+05C3`). For `a` of 0, `a - 1` is above 3, so it
skips the jump table at `+0855` and goes to `+06C4` (`+05C9..+05D2`); with `SI` at 9999 that goes on
to `+06E0`.

`+06E0` calls `+1098` with the address of `[bp-4]`. `+1098` returns 9999 at once when `+3304`
returns a zero byte; otherwise it goes through the slots 0 to `0x2F` (FND-RNG-042) and, for a slot
it keeps, stores into that slot's records only, below `0x30` (`+1155..+11C0`), and returns it.
When it returns 9999, `+071B` calls `+0C65` with no argument. `+0C65`:

- when the byte `DS:054E` is not 0, stores 0 there and skips the next step;
- otherwise stores `0xFF` at `4C4F:0006`, adds `0x3C` to the dword `4C13:035B` (`+0C90`) and calls
  `371D:0334` and the overlay 195 trampolines `574E:003E` (`+0AAD`) and `574E:0039` (`+0A98`), none
  with an argument (`+0C80..+0CA6`);
- then goes through the slots `DI` from 0 below `0x30` (`+0CB1`, `+106A..+1070`), stores 9999 at
  `4C4F:022C` and returns what `+3304` returns (`+1073..+1087`).

A nonzero byte from `+0C65` leads through `+07F1` back to `+06E0`; zero leads to the end of combat
of FND-RNG-070, whose calls take no slot. A slot kept by `+1098` goes to `2D40:2F34` at `+07D5`.

## Interpretation

On this path 9999 is used only as the "no slot" value: no callee receives it or an address formed
from it, and no store is indexed by it. Each call that finds no slot to keep runs one round step,
which advances `4C13:035B` by 60, and then the end-of-combat sequence.

## Alternatives

The loop body of `+0C65` from `+0CB6`, `+3304`, `371D:0334`, overlay 195 `+0AAD` and `+0A98`, and
the callees of the end-of-combat sequence were not read here; they take no slot argument from this
path.

## How to reproduce

`overlay_listing.py <dsun> 173 <start> <end>` for each offset range above.
