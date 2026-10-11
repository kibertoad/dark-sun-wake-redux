---
id: FND-PARTY-166
title: The hit routine adds a spell's effect from DATA byte +0x19 only when the saving throw fails, CHAOS names effect 21 and has no case of its own, and the score table terms of the save modifier lie between -6 and 7
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000650F6..0x00065231
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000656B6..0x000656F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066A32..0x00066A5A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:02F2..27E5:033D
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py, immediate_search.py; a scratch script that prints words of the overlay 179 code block, bytes of the data segment and bytes of DATA resources read through the GFF directory as details_chunks.py reads a directory)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`, whose overlay 179 code block starts at file offset `0x642E0` and whose data segment
starts at `0x4D000`.

**The hit routine.** Overlay 179 `+0D2F` stores in `bp-0xA` the byte at `+0x19` of `DATA` `n`, `n`
being its item argument `si`, or 0 when `si` is -1 (`+0E16..+0E2F`), and a duration in `bp-0xC`
(`+0E32..+0E4D`). At `+0ED4..+0EE5` it calls `+2A46`, the saving throw (FND-PARTY-099), and keeps the
result in `bp-1`. When that is not 0 it sets the damage dword at `bp-6` to 0 when the word at
`+0x11` of `DATA` `n` has bit 0x8000 and halves it otherwise, and goes to `+0F42`
(`+0EE8..+0F1C`). When it is 0 and `bp-0xA` is not 0, it calls overlay 193 `+166F` with the
attacker, the target, `n`, `bp-0xA` and `bp-0xC`, and sets bit 2 of `bp-7` (`+0F1E..+0F3E`).
Both paths go on to overlay 197 `+0000` and overlay 179 `+1373` with `bp-1` (`+0F42`, `+1124`).

**Overlay 179 `+1373`** looks `n` up among the 81 words at `cs:28C1` and jumps through the word
0xA2 bytes on (`+13F0..+140C`); a number not found goes to `+2752`. There, a save that succeeded
goes to `+27E7`, and one that failed looks `n` up among the five words at `cs:2831` (3, 62, 204,
236 and 280) (`+2752..+2776`). Neither table holds 56. `immediate_search.py <dsun> 38` lists one
instruction that compares a word with 56, overlay 197 `+0689` (FND-PARTY-164).

**`DATA` 56**, `CHAOS`, holds 21 (0x15) at `+0x19`; `DATA` 60, `DISMISSAL`, holds 0.

**The score tables.** `27E5:02F2`, `27E5:030B` and `27E5:0324` call `27E5:04A0` with their slot
and 1, 2 and 4 and return the signed byte at `DS:0866`, `DS:0880` and `DS:089A` indexed by the
result. The 26 bytes of each are:

| Table | Bytes 0 to 25 |
|---|---|
| `DS:0866` | 0, 5, 5, 4, 3, 2, 1, 0 (7 to 14), -1, -2, -3, -4, -4, -4, -5, -5, -5, -6, -6 |
| `DS:0880` | 0, -2, -1, 0 (3 to 18), 1, 1, 2, 2, 3, 3, 4 |
| `DS:089A` | 0, -6, -4, -3, -2, -1, -1, -1, 0 (8 to 14), 1, 2, 3, 4 (18 to 25) |

## Interpretation

A spell without a case of its own adds its effect, the type at `+0x19`, only to a target that
fails the saving throw; a target that saves takes no effect and no damage or half. `CHAOS` is such
a spell, so the save that FND-PARTY-164 shows a non-warrior making on every roll but a 1 spares it
from effect 21.

The three tables are the dexterity, constitution and wisdom adjustments by score. Overlay 197
`+0206` subtracts the first and adds the other two (FND-PARTY-164), so each moves the modifier by
at most 6. Against `CHAOS`, whose word at `+0x1A` is 0 so that neither the roll doubling nor the
fire and lightning terms apply, the terms that raise the modifier add at most 39: 18 from
constants, 6 and 4 from these tables, 4 from the constitution table and 7 from twice the
constitution divided by 7 (`+0656..+0687`). A modifier with 100 taken off is then -61 or less, and
the byte sum with a roll of 2 to 19 at most 214, above every save.

## Alternatives

- The term at overlay 197 `+026F..+0286` subtracts 2 for each count up to the byte at
  `4E71:0255 + 10 * e`, `e` being the target's effect of type 0x3B; how large that byte gets was
  not read. Without it, the terms that can lower the modifier against `CHAOS` leave the sum at
  -115 or more for a roll of 2, and the sum falls below -256 plus the save, where the byte would
  be below the save, only when that byte is about 60 or more.
- What effect 21 does, and what overlay 193 `+166F` does with it, were not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `overlay_listing.py <dsun>` with 179 `6500F
65800`, 179 `65653 66000` and 179 `66A32 66B20`, `resident_listing.py <dsun> 27E5:02F2..27E5:0360`
and `immediate_search.py <dsun> 38`. Print the 81 words at file offset `0x66BA1` and the 81 after
them, the five words at `0x66B11` and the five after them, the 26 bytes at `0x4D866`, `0x4D880`
and `0x4D89A`, and the byte at `+0x19` of `DATA` 56 and 60 in `RESOURCE.GFF`.
