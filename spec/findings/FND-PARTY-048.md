---
id: FND-PARTY-048
title: For ADD's placement, 31E0:0EFF skips the RDFF load and passes its slot-record test, leaving the OJFF request for 300 plus the party record's word at 0x10 as its only failure
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058E06..0x00058E3A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:015C..31E0:0176
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0F20..31E0:0F51
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0FE6..31E0:1065
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`.

**The arguments.** Overlay 171 `+0AA5` pushes, in order, 0, 7, the byte at `DS:044A`, two
position words, the slot number from `4E71:0B44`, and 0x12C plus the word at `+0x10` of the
slot's 49-byte record at `DS:19C9` (FMT-COMBAT-001), then calls `31E0:0121` (`+0BF6..+0C25`).
`31E0:0121` calls `31E0:0EFF` with the byte at its `+0x12` (the 0 pushed first), `(7 & 7) +
0x48` masked to 7, its `+0x08` (the slot number), the count at `DS:264E` and its `+0x06` (the
object number) (`31E0:015C..+0170`). So `31E0:0EFF` receives the object number 300 plus that
word, the count as the entry index, the slot number, 7 and flags 0, in the argument order
FND-ACTOR-003 gives.

**The helper's tests for that call**, after the test on `DS:265B` (FND-PARTY-047):

1. `31E0:0F35` requests `OJFF` with the object number through `38FF:04AB`; a nonzero result
   takes the path to `31E0:1262`, which returns -1.
2. `31E0:1000` tests bit 2 of the flags. With flags 0 it jumps to `31E0:1040`, so the `RDFF`
   load at `31E0:1028` and its failure branch are skipped.
3. `31E0:1045` tests the word at `0x67BC` plus 0x25 times the slot, which is the slot record's
   `+0x01`. `31E0:0E1B`, called at `31E0:0FFA`, has just written the entry index there
   (FND-ACTOR-003), which is the count. While the count is 0 the test passes.
4. With a slot below 319, `31E0:1065` jumps to `31E0:1112`; nothing from there to the return at
   `31E0:125E` returns -1.

`OBJEX.GFF` has `OJFF` resources for the object numbers 300 to 313 and 320 to 326 and none for
314 to 319 (FMT-ACTOR-001's value file).

## Interpretation

With `DS:265B` at 1 and the count at 0, `ADD`'s placement fails only when `OBJEX.GFF` has no
`OJFF` resource for 300 plus the slot record's word at `0x10`, or the request fails for another
reason. The supplied-party loader computes the same number (FND-PARTY-013), so the question of
which values that word takes after a `CHAR` load is the one Q-PARTY-003 asks about the record
layout.

## Alternatives

- `38FF:04AB` returns nonzero on success and 0 on failure, reversing test 1: not read here.
  FND-ACTOR-003 reads a nonzero result as failure.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 171
0x58CB5 0x58E60` and `resident_listing.py <dsun> 31E0:0121..31E0:01F0 31E0:0EFF..31E0:12A0`.
Read the `object` column of `spec/formats/FMT-ACTOR-001.objects.csv` for 290 to 340.
