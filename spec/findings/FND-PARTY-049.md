---
id: FND-PARTY-049
title: The CHAR load copies bytes 0x0A to 0x3A of a kind-2 record into the slot's party record, so ADD's object number is 300 plus the CHAR word at 0x1A, 300 to 313 for every stored character
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:000A..2D40:012F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000229B9..0x000229C1
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FE30..0x0006FE70
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000701CA..0x00070328
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007042B..0x00070435
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2C87..2D40:2CE6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2DE0..2D40:2E0A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0002540A..0x00025414
    kind: file-data
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py, trampoline_target.py, field_stores.py); a Python 3.14.7 reading of CHARSAVE.GFF's directory (FMT-GFF-001, FMT-GFF-002)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed files.

**The load.** `2D40:000A` takes a slot, a resource number, a tag and a byte. For a number outside
9,000 to 13,998 it requests the resource through `38FF:05B5` (its size) and `38FF:04AB` (its
bytes) into a buffer from `444C:00FA`; a nonzero result from either request returns -1. It then
reads the resource's byte 0, less 1, as an index from 0 to 3 into the four words at `2D40:03B9`
(`2D40:00E9`): `2D40:00EE`, `+012F`, `+018C` and `+0200`. Every `CHAR` record of the installed
`CHARSAVE.GFF` has 1 in byte 0 (FMT-PARTY-001), so a `CHAR` load takes `2D40:00EE`, which calls
overlay 187 `+0000` (trampoline `56EF:0025`) with the resource's address plus 0x0A, the slot,
the resource's address and a pointer to a local word (`2D40:00FB`).

**Overlay 187 `+0000`** stores the resource's byte 2 to the byte at `4F49:0C33` plus 3 times the
slot and calls `+039A` with the address plus 0x0A, the resource's address and the slot twice
(`+0030`).

**Overlay 187 `+039A`** calls `2D40:2C87` with the slot, the word at `DS:55B2`, the kind byte at
`4F49:0C33` plus 3 times the slot, and the address plus 0x0A (`+0419`). It then reads that kind,
less 1, as an index from 0 to 4 into the five words at `+05FB` (`+0459`): `+04F8`, `+045E`,
`+054C`, `+05CE` and `+05B0`. Kind 2 goes to `+045E`, which stores the slot `2D40:2C87` returned
to `DS:55B0` and copies 0x31 bytes from the address plus 0x0A to the record of that number in
the table of 49-byte records at `DS:19C9` (FMT-COMBAT-001), through `1000:3F5A` (`+0478`). It
then stores 9,999 to the record's words at `+0x04`, `+0x0A`, `+0x08` and `+0x0C`, 1 to its byte
at `+0x14` when that byte is 0, and the slot to the word at `4F49:0C34` plus 3 times the slot.
Kind 1 (`+04F8`) copies 0x17 bytes into the table at `DS:19C1` instead.

**`2D40:2C87`** reads its kind argument, less 1, as an index from 0 to 4 into the five words at
`2D40:2E0A`. Kind 2 goes to `2D40:2CAC`: with a slot of 4 or less it keeps the slot, sets the
record's words at `+0x08`, `+0x0A` and `+0x0C` to 9,999, and returns the slot through
`2D40:2DE0` and `2D40:2E04`.

**The stored characters.** All 19 `CHAR` records of the installed `CHARSAVE.GFF` (29 to 43 and
50 to 53) have 2 in byte 2. Their words at `0x1A` are:

| Word at `0x1A` | Characters |
| --- | --- |
| 0 | 31, 41, 53 |
| 5 | 37, 40 |
| 7 | 30, 50 |
| 8 | 33, 43 |
| 11 | 29, 38 |
| 12 | 39, 52 |
| 13 | 32, 34, 35, 36, 42, 51 |

The copy puts the `CHAR` record's `name` at `0x2B` (FMT-PARTY-001) at the party record's `0x21`,
the offset FND-COMBAT-022 reads a character's name from.

## Interpretation

A `CHAR` load into a box's slot copies the record's bytes `0x0A` to `0x3A` into that box's
49-byte party record, so the word at `0x10` that `ADD` adds to 300 (FND-PARTY-048) is the `CHAR`
record's word at `0x1A`. For every stored character that number is 300, 305, 307, 308, 311, 312
or 313, all of which have an `OJFF` resource in `OBJEX.GFF` (FND-PARTY-048). With `DS:265B` at 1
(FND-PARTY-047), `31E0:0EFF` therefore finds the definition and returns the slot, and
`31E0:0121` raises the placed-object count, unless a file request fails at run time.

## Alternatives

- The party record is written again between the load and the placement: the code between
  `2D40:000A`'s return and the call at overlay 171 `+0C25` writes only the locals that hold the
  position (FND-PARTY-046); the routines it calls there (`56E9:0025`, overlay 171 `+0938` and
  `56BD:00A2`) were not read for stores to the record.
- `38FF:04AB` returns 0 on failure: ruled against by `2D40:000A` itself, which treats a nonzero
  result as failure (`2D40:00B1`), as FND-ACTOR-003 reads it for `31E0:0EFF`.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `resident_listing.py <dsun>
2D40:000A..2D40:0200 2D40:2C87..2D40:2D60 2D40:2DE0..2D40:2E0A`, `overlay_listing.py <dsun> 187
0x6FE30 0x6FE70` and `187 0x701CA 0x70435`, and `trampoline_target.py <dsun> 56EF:0025`. Read
the four words at file `0x229B9`, the five at `0x7042B` and the five at `0x2540A`. Read each
`CHAR` resource of `CHARSAVE.GFF` through the archive's directory (FMT-GFF-002) and print its
byte 2 and its word at `0x1A`.
