---
id: FND-PARTY-074
title: The generation screen keeps a new character's greatest hit points between a low bound of one per level and a high bound of the class's hit die per level, each averaged over the classes, doubled for a half-giant and raised by a constitution bonus, and it rolls each level's die into the details record's word at 0x0A
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D1ED..0x0006D3CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006BBC7..0x0006BC24
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006B6BA..0x0006B708
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00094F60..0x00095091
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00095261..0x000952C5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000953A9..0x000954A5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004380E..0x00043816
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004ADE0..0x0004AE6C
    kind: file-data
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py, trampoline_target.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Segment `4E4F` starts at file `0x436F0` and segment `55BE` at `0x4ADE0`. The details
records of the party slots are 66 bytes each in the table at `DS:19C5` and the combatant records
49 bytes each in the table at `DS:19C9`; `4E71:0B44` holds the slot being made, `DS:1429` and
`DS:142D` the generation records (FND-PARTY-073).

**Overlay 184 `+015D`** (trampoline `56DD:0066`) takes a far pointer, two near pointers (a low and
a high bound) and a flag (`+0165..+0168`). It sets the two bounds and a count to 0, calls `+1648`
with the slot, which copies the generation records into the slot's and turns the class codes into
the stored ones (FND-PARTY-073), and stores 0 in the word at `+0x0A` of the slot's details record
(`+016B..+019A`). For each class position 0 to 2 it copies the level byte at `+0x1E` to `+0x39` of
the same record and, when the level is not 0, adds 1 to the count and for each level `k` from 1 to
the level adds 1 to the low bound and the byte at `4E4F:011D` plus the generation record's class
byte to the high bound, and when the flag is nonzero calls overlay 210 `+0000` (trampoline
`57B9:0020`) with the slot, the stored class byte and `k` (`+01A0..+026A`). When the slot's origin
byte at `+0x12` is 5 it doubles both bounds (`+026D..+0296`). When the count is not 0 it sets each
bound to itself divided by the count (signed) plus the result of overlay 210 `+0301` (trampoline
`57B9:0034`) for the slot, and stores the slot's word at `+0x08` through the far pointer; otherwise
it stores 0 in both bounds and through the pointer (`+0298..+02FF`). It ends by calling `+1DCE`,
which copies the slot's records back to the generation records (FND-PARTY-080), and returns
(`+0304..+0337`).

The eight bytes at `4E4F:011E` (file `0x4380E`), for generation codes 1 to 8: 8, 8, 10, 10, 4, 6,
10, 6.

**Overlay 210 `+0000`** takes a slot, a stored class code and a level `k`. When the slot's origin
byte is 1 and its level byte at `+0x1F` or `+0x20` is not below `k` it returns; when the sum of the
three bytes at `+0x39` is above the sum of the three level bytes at `+0x1E` it returns
(`+0027..+007F`). It takes the byte at `55BE:0010` plus the class code as a row and reads the row's
four bytes at `55BE:0000` plus 4 times it. When `k` is above the row's byte 1 the gain is the row's
byte 2; otherwise it is the result of `28C9:391D` (`roll_sum`) with 1 and the row's byte 0, raised
to the byte at `55BE:0038` plus the details record's byte at `+0x17` when that is greater, and
doubled when the origin byte is 5 (`+0082..+0107`). It adds the gain to the word at `+0x0A`
(`+0109..+010C`).

The tables in `55BE` (file `0x4ADE0`): rows 0 to 3 are 8, 9, 2; 10, 9, 3; 4, 10, 1; and 6, 9, 2
(byte 3 is not read here). The row bytes at `55BE:0010` for stored codes 0 to 17 are 0, 0, 0, 0,
0, 0, 0, 0, 0, 1, 1, 2, 3, 1, 1, 1, 1, 3. The signed bytes at `55BE:0038` plus a score are 1 for
scores 0 to 19, 2 for 20, 3 for 21 and 22, and 4 for 23 to 25. Those at `55BE:0052` plus a score
are -3, -3, -2, -2, -1, -1, -1 for 0 to 6, 0 for 7 to 14, and 1, 2, 3, 4, 5, 5, 6, 6, 6, 7, 7
for 15 to 25.

**Overlay 210 `+0449`** takes a slot, a near pointer to four pairs of bytes and a flag. It zeroes
the pairs, and for each class position with a nonzero class byte (skipping, for an origin byte of
1, a position after the first whose level is not below the first's, unless the flag is set) takes
the class's row from `55BE:0010`; when the position's level is above the pair's byte 0 it stores
the level there and in byte 1, lowers byte 1 to the row's byte 1 when it is above it, and keeps the
greatest byte 1 over the positions, which it returns (`+0451..+053E`).

**Overlay 210 `+0301`** takes a slot, calls `+0449` with it and 1, and takes the signed byte at
`55BE:0052` plus the byte at `+0x1B` of the slot's combatant record, its constitution. It returns
that byte times the byte 1 of pair 1, plus the byte capped at 2 times the result of `+0449` less
the byte 1 of pair 1 (`+0304..+0362`).

**The callers.** `direct_callers.py` finds `+015D` called from overlay 184 `+055B` in the roll
routine (FND-PARTY-071), with that routine's flag and `DS:42D8` and `DS:42D6` as the bounds, and
from overlay 183 `+0BFF` and `+06E7`. Overlay 183 `+0A3E`, when its third argument is nonzero,
calls `+1323` (FND-PARTY-065) and then `+015D` with the word at `+0x08` of the generation details
record, `DS:42D8`, `DS:42D6` and 1; it then sets that word to `DS:42D6` when it is not below it
and to `DS:42D8` when it is not above it (unsigned), and copies it to `+0x00` of the combatant
record (`+0BE7..+0C41`). Overlay 183 `+06BE`, the constitution score's button (FND-PARTY-071),
calls `+015D` with 0 and then holds the word at `+0x08` between the same two bounds the same way
(`+06DA..+0725`). The hit point button holds the same word between them with wrapping
(FND-PARTY-070). Overlay 210 `+0000` is also called from overlay 210 `+07EB`, and `+0301` from
overlay 210 `+02A4` and `+054E`.

## Interpretation

The player sets a new character's greatest hit points with the hit point button, between a low
bound of one per level and a high bound of the class's hit die per level (d8 for a cleric or druid,
d10 for a fighter, gladiator or ranger, d4 for a preserver, d6 for a psionicist or thief), each
summed over the classes and divided by their number, doubled for a half-giant, and raised by a
constitution bonus. The bonus is the constitution table's figure for each level of the character's
highest warrior class, and at most 2 for each further level of its highest class, counting levels
up to 9 (10 for a preserver). Choosing a class sets the greatest hit points to the low bound when
they were below it, so a new character starts there, and a change of constitution moves them into
the new bounds; the current hit points follow.

When a class is chosen and on each roll, the screen also rolls each level's hit die, at least the
constitution floor and doubled for a half-giant, into the word at `0x0A` of the details record.
Nothing on the generation screen reads that word; overlay 210 `+07EB` calls the same roll, which
suggests it serves level gains in play.

## Alternatives

- The roll's floor reads the details record's constitution at `+0x17`, which the finish fills from
  the combatant record only at DONE (FND-PARTY-070), so the floor may use an older score; which
  value it holds when the screen rolls was not traced.
- What the word at `0x0A` is used for, and how overlay 210 `+07EB` uses the roll, was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 184 6D1ED 6D3CE`,
`183 6BBC7 6BC24`, `183 6B6BA 6B708` and `210 94F60 954A5`; `direct_callers.py <dsun> 184+015D
210+0000 210+0301`; `trampoline_target.py <dsun> 56DD:0066 57B9:0020 57B9:0034`. Read the eight
bytes at file `0x4380E`, the 16 bytes at file `0x4ADE0`, the 18 bytes at `0x4ADF0`, and the signed
bytes at `0x4AE18` and `0x4AE32`, 26 each.
