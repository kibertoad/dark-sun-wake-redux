---
id: FND-COMBAT-072
title: Overlay 188 allocates 42 combatant records and 17 details records; 2D40:2C87 keeps an object of 4 or less on the records of its own number and gives a creature the first free combatant record from 5 and a details record from 5 to 16 shared by key; 2D40:3E64 reads the object's record number and that record's details_index; the panel's question marks test the object number, not the records
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00072FF7..0x00073098
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3E64..2D40:3EBF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2C87..2D40:2E0A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0002540A..0x00025414
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:248D..2D40:2588
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00024DAD..0x00024DB7
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0465..2C5F:052D
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py and direct_callers.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7
environment: null
---

## Observation

Overlay 188's code starts at file `0x72EA0`. An object is an entry of the 3-byte table at
`4F49:0C33`: a kind byte and, at `4F49:0C34`, a record number (FND-PARTY-051). Combatant records
are the 49-byte records under the far pointer `DS:19C9` (FMT-COMBAT-001) and details records the
66-byte records under `DS:19C5` (FMT-COMBAT-002).

**Allocation.** Overlay 188 `+0000`, after the `FNFO` requests (FND-CONFIG-150), calls
`444C:0008` three times with a byte count and 1 (`+0157..+01F8`): 0x23F0 bytes stored in
`DS:19C1` (`+0157..+016C`), 0x462 in `DS:19C5` (`+01AA..+01BF`) and 0x80A in `DS:19C9`
(`+01D1..+01E6`). A null result jumps to the failure exit `+03B1`. 0x23F0 is 400 records of 23
bytes, 0x462 is 17 of 66 and 0x80A is 42 of 49. Between the first two calls it stores `n + 1` in
word `+0x04` of each 23-byte record `n` from 0 to 398 and 9999 in that word of record 399
(`+0176..+0195`), and copies each far pointer to `DS:6153`, `DS:615B` and `DS:6157`.

**`2D40:3E64(h, &c, &d)`** (`2D40:3E64..+3EBF`) returns 0 when `h` is 9999 or more or the kind
byte at `4F49:0C33 + 3 * h` is not 2. Otherwise it stores the word at `4F49:0C34 + 3 * h` in
`c`, returns 0 when `c` is 42 or more, stores word `+0x04` of combatant record `c` in `d`, and
returns 1 only when `d` is below 17. The two bounds are the record counts above.
`direct_callers.py <dsun> 2D40:3E64` lists 33 call sites: three far calls in the load image, 27 in
overlays 173, 176, 179, 188, 195, 197, 198, 206 and 211, and three near calls in `2D40`.

**`2D40:2C87(object, tries, kind, data)`** picks the record for a chunk of `kind` (FND-PARTY-049,
FND-PARTY-051). It jumps through the five words at `cs:2E0A` (file `0x2540A`): kind 1 to
`+2D78`, 2 to `+2CAC`, 3 to `+2D1C`, 4 to `+2DDB` and 5 to `+2DB2`.

- Kind 2 (`+2CAC..+2D19`): an object of 4 or less (signed) gets the record of its own number.
  Otherwise the routine takes the lowest record from 5 to 41 whose word `+0x06` is 0. With a
  record it stores 9999 in its words `+0x08`, `+0x0A` and `+0x0C`.
- Kind 3 (`+2D1C..+2D76`): an object of 4 or less gets the record of its own number. Otherwise it
  walks records 5 to 16 and stops at the first whose word `+0x0E` equals word `+0x0E` of the
  chunk's data; while it walks it notes each record whose word `+0x0E` is 0, so without a match it
  takes the highest such record.
- Kind 4 returns -1. Kind 5 takes the lowest of the 20 records of 23 bytes under `DS:19D1` whose
  word `+0` is 0. Kind 1 takes a record from `2D40:0E80` and stores 9999 in its words `+0x04` and
  `+0x08`.

When no record is found (`+2DE0..+2E01`) it calls overlay 187 `+1B67` (trampoline `56EF:00BB`)
with the attempt number and the kind and tries again, while the attempt number is at most
`tries`. It returns the record number or -1.

**Release.** `2D40:248D` takes an entry of an object's graph (FND-PARTY-054) and jumps on the
entry's kind through the five words at `cs:27AD` (file `0x24DAD`). For kind 2 (`+251B..+2539`) it
stores 0 in word `+0x06` of the combatant record. For kind 3 (`+24B6..+2519`) it looks through
combatant records 0 to 41 for one whose word `+0x06` is not 0 and whose word `+0x04` equals the
details record's number, and stores 0 in the details record's word `+0x0E` only when there is
none. For kind 5 it clears word `+0` of the `DS:19D1` record; kind 1 goes to `2D40:0EB4`.

**The panel.** In `2C5F:03F1` (FND-COMBAT-022) the call `2D40:3E64(word 57E0:426D, &bp-2,
&bp-4)` at `2C5F:0473` fills the combatant record number at `bp-2` and the details record number
at `bp-4`. The hit point line compares the word `57E0:426D` itself with 4 (`2C5F:04E3`, signed,
`jle`); above 4 it prints `???/???`, otherwise word `+0` of combatant record `bp-2` and word
`+0x08` of details record `bp-4`. No other test chooses between the two texts.

## Interpretation

The combatant table has 42 records and the details table 17. A loaded object of number 0 to 4
uses combatant record and details record of the same number. Any other combatant object (5 to
47, FND-PARTY-119) gets its own combatant record from 5 to 41, the lowest free one, where free
means `character_id` 0. Its details record comes from 5 to 16 and is shared: every creature whose
details chunk carries the same `details_key` (word `+0x0E`) uses one record, which the last
release frees by setting the key to 0. So at most 37 creatures and 12 different details keys are
loaded at once. When a table is full, overlay 187 `+1B67` unloads objects (FND-PARTY-138) and the
search runs again.

`2D40:3E64` maps a combatant object to its two records in two reads: the object's record number
is the combatant record, and that record's `details_index` is the details record. It returns 0 for
an object that is not a combatant (kind other than 2), and for a record or details number past
the end of a table.

`turn_combatant` (`57E0:426D`) is an object number. The panel prints a combatant's hit points
only for objects 0 to 4, the numbers whose records are their own and which the allocators never
hand out (FND-PARTY-119); every creature loaded through the free lists is 5 or more and shows
`???/???`. The test does not look at the side, the records or `2D40:3E64`'s result. Since a
creature's details record is shared with every creature of its key, its `max_hit_points` belongs
to the key, while each creature has its own `hit_points` (FND-COMBAT-070).

## Alternatives

- What object 4 holds was not read. The allocators skip it, `2D40:2C87` treats it as party slots
  0 to 3 are treated, and FND-UI-047 marks a leader in it; whether any load or script puts a
  combatant there, and whether it is ever `turn_combatant`, is open.
- Whether the kind-3 release of `2D40:248D` runs before or after the kind-2 release of the
  combatant whose details it frees was not read; if after, a creature's own record would keep its
  details record from being freed until a later release.
- Overlay 187 `+1B67`, `2D40:0E80` and `2D40:0EB4` were not read.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 188 72F00 730A0`; `resident_listing.py <dsun>
2D40:3E64..2D40:3EC5 2D40:2C87..2D40:2E14 2D40:2410..2D40:25D0 2D40:27A9..2D40:27E0
2C5F:045A..2C5F:0476 2C5F:04C0..2C5F:0540`; `direct_callers.py <dsun> 2D40:3E64`. Read the five
words at file `0x2540A` and the five at `0x24DAD`.
