---
id: FND-PARTY-177
title: The party loader in overlay 182 compares each slot with leader when it sets or clears bit 7 of the slot's object_slots entry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068FBF..0x00068FF7
tool: tools/research/exec-census/overlay_listing.py (Capstone 5.0.9)
environment: null
---

## Observation

Overlay 182's code starts at file offset `0x68850`; `+x` is file offset `0x68850 + x`.

In the loop of FND-PARTY-013's routine, whose slot is the byte at `[bp - 1]`, step 6 is
`+076F..+07A7`: when the byte at `DS:13FA` is not 0 it goes to `+0787`; otherwise it loads ES with
the segment word `0x02E8`, which the overlay's fixups resolve to segment `4C13`, and compares the
slot with the word at `ES:0369` (`cmp ax, es:[0369]` at `+0780`), going to `+0798` when they
differ. `+0787` clears bit 7 of byte 0 of the slot's `object_slots` entry (`and byte [bx + 67BB],
7F` at `+0791`, with `bx` the slot times `0x25`); `+0798` sets it (`or ..., 80` at `+07A2`).

The same comparison with `ES:0369` through segment word `0x02E8` appears at `+065D`, after the
same test of `DS:13FA` at `+064C`, where it chooses the position words of step 1.

## Interpretation

The word FND-PARTY-013 left unnamed is `leader` (`4C13:0369`, FND-PARTY-171). The loader shows the
leader's slot, and every slot when `DS:13FA` is not 0, and hides the others, and places the leader
by the position words at `DS:140E` and `DS:1410` and the others by other positions.

## Alternatives

FND-PARTY-013 described the compared word as unknown; this reading names it. Nothing else in that
finding changes.

## How to reproduce

From the commit that adds this finding, run
`tools/research/exec-census/overlay_listing.py <dsun> 182 68850 6AD08` on the installed
`DSUN.EXE` and read `+064C..+0686` and `+076F..+07A7`.
