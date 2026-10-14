---
id: FND-PARTY-183
title: Overlay 187's chunk copy gives a loaded combatant record combat_mark 1 when its data holds 0, and keeps any other value
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007028E..0x00070328
tool: tools/research/exec-census/overlay_listing.py (Capstone 5.0.9)
environment: null
---

## Observation

Overlay 187's code starts at file offset `0x6FE30`; `+x` is file offset `0x6FE30 + x`. Segment word
`0x0388` resolves through the overlay's fixups to `4F49`.

FND-PARTY-051 reads overlay 187 `+039A`, the copy of one chunk's data into the record
`2D40:2C87` returns. It dispatches through the five-word table at `+05FB` (`0x04F8`, `0x045E`,
`0x054C`, `0x05CE`, `0x05B0`) on the `object_slot_kinds` byte of the handle `di`, less 1. Kind 2,
the combatant (`+045E..+04F8`), with `si` the record number:

1. stores `si` to the word at `DS:55B0` and copies 0x31 bytes of the data to record `si` of the
   table at the far pointer `DS:19C9` through `1000:3F5A` (`+045E..+047D`);
2. stores 9,999 to the record's words at `+0x04`, `+0x0A`, `+0x08` and `+0x0C` (`+0480..+04BE`);
3. when the record's byte at `+0x14`, `combat_mark`, is 0, stores 1 there (`+04C4..+04E1`);
4. stores `si` to the handle's word at `4F49:0C34 + 3 * di`, its `object_slot_combatants` entry
   (`+04E6..+04F0`), and goes on at `+05CE` (`+04F5`), which FND-PARTY-051 reads.

## Interpretation

A character loaded from a `CHAR` record takes the `combat_mark` its type-1 chunk holds, except
that 0 becomes 1. A stored 2 or 3 is kept.

## Alternatives

None found; the store at `+04E1` is a plain byte store after a test of the same byte.

## How to reproduce

From the commit that adds this finding, run
`tools/research/exec-census/overlay_listing.py <dsun> 187 6FE30 70360` on the installed
`DSUN.EXE` and read `+045E..+04F8`, and read the five words at file offset `0x7042B`.
