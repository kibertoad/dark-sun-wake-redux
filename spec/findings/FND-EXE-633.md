---
id: FND-EXE-633
title: Three overlay 204 routines run past their function inventory rows, leaving 217 bytes of their bodies in no row
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DDE9..0x0008DE17
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DE45..0x0008DF0E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DF0E..0x0008DFC4
tool: tools/research/exec-census/overlay_listing.py at the commit that adds this finding and a script that lists overlay inventory rows whose last byte is not a return or jump; Python 3.14.7, Capstone 5.0.9
environment: null
---

## Observation

Among the overlay rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`, 14 end on a byte that is not
a return or jump opcode. In 11 of them the next row starts at the row's end. The other three are
in overlay 204 (code from file offset `0x8BDC0`), and each is followed by bytes in no row:

| Row start | Row ends | Routine ends | Bytes in no row |
|---|---|---|---|
| `0x8DDE9` (`+2029`) | `0x8DDF3` | `0x8DE17`, after the `retf` at `+2056` | 36 |
| `0x8DE45` (`+2085`) | `0x8DF08` | `0x8DF0E`, after the `retf` at `+214D` | 6 |
| `0x8DF0E` (`+214E`) | `0x8DF15` | `0x8DFC4`, after the `retf` at `+2203` | 175 |

In each, the listing from the row start runs on without a jump out of the routine until the
`retf`, and the next row starts at that `retf`'s end (`0x8DE17`, `0x8DF0E` and `0x8DFC4`). The
bytes from the row end to the routine end are reached by fall-through and by branches from inside
the row: `+2038` and `+204C` jump to `+2052`, `+20AB` jumps to `+2148`, and `+21D0` jumps to
`+2201`.

## Interpretation

The inventory rows of `+2029`, `+2085` and `+214E` cut those routines short, so a coverage count
from the inventory treats 217 bytes of their bodies as part of no function. The routines
themselves are `+2029..+2057`, `+2085..+214E` and `+214E..+2204` (FND-EXE-626 gives the
inventory's ranges for the first two; FND-RNG-035 reads the third to `0x8DFC4`).

## Alternatives

A row could end early because the bytes after it belong to a separate routine entered by a far
call; none of overlay 204's 40 trampolines enters a byte from `+2033` to `+2057`, from `+2148` to
`+214E` or from `+2155` to `+2204`, and each region ends in
the epilogue that matches its row's prologue.

## How to reproduce

`overlay_listing.py <dsun> 204 8DDE9 8DE17`, `... 204 8DE45 8DF0E` and `... 204 8DF0E 8DFC4`,
and compare each with its row in the inventory file.
