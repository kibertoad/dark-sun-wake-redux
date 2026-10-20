---
id: FND-RNG-062
title: A monster's combatant byte +0x0E is byte 0x0E of its RDFF object's 49-byte combatant chunk, copied whole by the load that copies a CHAR record; of the 352 shipped chunks only RDFF 428 holds 6 and only RDFF 430 holds 13
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
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x24274..0x66105E
tool: tools/research/exec-census/rdff_chunks.py (its --combatant-byte option), gff_tag_numbers.py and overlay_listing.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 187's code starts at file `0x6FE30` of
the installed `DSUN.EXE`. `OBJEX.GFF` is the installed one (6,816,516 bytes, XXH3-128
`e08d3cac153c547f6804ca27b155915d`).

**The copy.** The object load requests an object's `RDFF` resource through `2D40:000A`, which
walks its chunks exactly as it walks a `CHAR` record's (FND-ACTOR-015, FND-PARTY-051). A type-1
chunk of kind 2 reaches overlay 187 `+039A`'s kind-2 case `+045E`, which copies 0x31 bytes from
the chunk's data to the record of the returned number in the table of FMT-COMBAT-001 records at
the far pointer `DS:19C9` through `1000:3F5A` (`+045E..+047D`). It then stores 9,999 in the
record's words `+0x04`, `+0x0A`, `+0x08` and `+0x0C`, 1 in byte `+0x14` when that byte is 0, and
the record number in the handle's entry at `4F49:0C34`, and leaves through `+04F5`
(`+0480..+04F8`). No instruction of the case writes byte `+0x0E`. This is
the copy at overlay 187 `+0478` that FND-PARTY-115 counts as the `CHAR` load's.

**The chunks.** `rdff_chunks.py OBJEX.GFF --combatant-byte 0E` reads byte `0x0E` of the data of
the 352 type-1 kind-2 chunks of the 1,643 `RDFF` resources, with the record's name at `+0x21`:

| Byte `0x0E` | Chunks | Resources |
|---|---|---|
| 0 | 275 | |
| 1 | 2 | 415, 561 (`Air Drake`) |
| 2 | 1 | 541 (`Earth Drake`) |
| 3 | 1 | 547 (`Fire Drake`) |
| 6 | 1 | 428 (`Grt Earth Elem`) |
| 7 | 2 | 429 (`Grtr Water Elem`), 523 (`Shambling Mound`) |
| 8 | 4 | 401, 494, 545 (Feyr), 569 (`Dust devil`) |
| 9 | 1 | 540 (`Obsidian Golem`) |
| 10 | 1 | 414 (`Magma Golem`) |
| 11 | 1 | 3 (`Lord Warrior`) |
| 12 | 13 | 55, 416, 454, 527, 528, 533, 534, 535, 537, 538, 542, 543, 544 (mind flayers and `Prim. Marakian`) |
| 13 | 1 | 430 (`Tarrasque`) |
| 14 | 10 | 5, 400, 433, 434, 446, 447, 461, 467, 550, 551 (snakes, yuan-ti, `Traxiss`, `Kartang`) |
| 15 | 31 | 53, 54, 62 to 71, 73, 124 to 130, 156, 157, 162, 177, 178, 179, 183, 220, 221, 222, 286 (townsfolk and guards, among them FND-PARTY-116's `Nortemus`, `Fori` and `Zydeko`) |
| 17 | 3 | 418, 419, 549 (`Giant Skeleton`) |
| 19 | 3 | 403, 404, 405 (`Umber Hulk`) |
| 20 | 2 | 409, 427 (fire elementals) |

**Saved tables.** The numbered save writes the table at `DS:19C9` whole as source entry 4 and the
numbered restore reads that resource back into it (FND-PARTY-236). `gff_tag_numbers.py <install>
SAVE` finds no `SAVE` table in any of the 26 installed archives, and `RDFF` only in `OBJEX.GFF`
(1,643 resources).

## Interpretation

A monster's combatant record is the 49-byte chunk of its `RDFF` object copied unchanged at byte
`+0x0E`, so its special attack byte is the chunk's byte `0x0E`. With FND-PARTY-113's stores
(overlay 198 `+0549`, which stores 0x12 or 0, and overlay 204 `+259D`, which has no caller the
searches find), FND-PARTY-115's block copies (this load and character generation's, whose record
holds 0) and a restore that reads back only what a save wrote from the table, the only shipped
sources of 6 and 13 in that byte are `RDFF` 428 and `RDFF` 430. So in the shipped data the
special attack that casts `DATA` 293 through overlay 174 `+084D` is the great earth elemental's,
and the target that casts a 293 back one time in six is the tarrasque (FND-RNG-055).

## Alternatives

- Which regions and scripts create objects 428 and 430, and so whether the game ever loads them,
  was not read.
- A pointer to a combatant record formed outside the straight-line runs FND-PARTY-115 follows,
  or a write the inventory-based searches of FND-PARTY-113 miss, could store another value; the
  inventory lacks some walked code (FND-EXE-593).
- The kind-2 path of `2D40:2C87` for a record number above 4 was not read; it decides which record
  receives the copy, not its bytes.
- No emulated call or run was made; overlay 187 is outside the resident harness.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, `<dsun>` the installed
`DSUN.EXE` and `<install>` the installation directory: `rdff_chunks.py <install>/OBJEX.GFF
--combatant-byte 0E`; `gff_tag_numbers.py <install> SAVE` and `gff_tag_numbers.py <install>
RDFF`; `overlay_listing.py <dsun> 187 7028E 70328`. Outputs are kept under
`GAME_DIR/analysis/rng-0E-20261021/`. Nothing was executed.
