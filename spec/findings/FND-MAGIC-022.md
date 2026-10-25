---
id: FND-MAGIC-022
title: Details byte +0x12 is the origin, 1 to 8 for party members and 9 in the details chunks of nine RDFF creatures named as undead; byte +0x1E is the level of the first class position; 2D40:06E1 returns at once, so a failed DATA 306 test changes nothing
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:06E1..2D40:06E6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097CE4..0x00097D1F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006B082..0x0006B0AD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D868..0x0006D8B3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E70B..0x0006E729
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E760..0x0006E77D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000789F4..0x00078A16
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x004934D9..0x00493525
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x005A7E8A..0x005A7ED6
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x005A7F1B..0x005A7F67
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x005A7FAC..0x005A7FF8
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x005AF35B..0x005AF3A7
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x005AF42E..0x005AF47A
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00611A1E..0x00611A6A
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00614FD1..0x0061501D
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0065F382..0x0065F3CE
tool: tools/research/exec-census/resident_listing.py, overlay_listing.py, direct_callers.py, record_base_accesses.py, field_stores.py, block_copies.py, details_chunks.py and gff_tag_numbers.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x6AFE0 + x` for overlay 183, `0x6D090 + x` for 184,
`0x78340 + x` for 190 and `0x95E10 + x` for 211. The details table is the 66-byte records under
`DS:19C5` (FMT-COMBAT-002); during generation `DS:1429` names a details record and `DS:142D` a
combatant record (overlay 184 `+07D8` points them at `4E4F:0029` and `4E4F:006B`, `+07E9..+0801`).

**`2D40:06E1`** (`2D40:06E1..2D40:06E6`) is `push bp`, `mov bp, sp`, `pop bp`, `retf`: it reads no
argument, writes nothing and returns. `direct_callers.py <dsun> 2D40:06E1` finds far calls at
overlay 187 `+0137`, `+0147`, overlay 188 `+04DE`, `+04EE`, `+0982`, `+14D5`, `+156A`, `+170F`,
`+17BE`, `+17D9`, overlay 197 `+351E`, overlay 204 `+0F7B`, overlay 211 `+1F05`, and a near call at
load image `0x26636`. In the `DATA` 306 test (overlay 211 `+1E40`, FND-RNG-035) the call at
`+1F05` pushes the caster (`bp+8`), and the routine then pops it and returns (`+1F02..+1F0F`).

**Stores at `+0x12` and `+0x1E`.** `record_base_accesses.py <dsun> <inventory> 12 1E` and
`field_stores.py <dsun> <inventory> --es 12 1E` list the same writes. Through `DS:19C5` with a
stride of 0x42 there are only stores at `+0x1E` (plus a class position index): overlay 209 `+1831`
and `+1890`, overlay 210 `+07CB` and `+0D46` (raise) and `+0C86` (lower). The others:

| Site | Byte | Base | Value |
|---|---|---|---|
| overlay 183 `+00B5`, `+00C8` | `+0x12` | `DS:1429` | `(word +0x10 of DS:142D AND 0xFFFE) / 2 + 1`, then 8 when that word is 13 (`+00A2..+00CD`) |
| overlay 184 `+081E` | `+0x12` | `DS:1429` | 1 (`+0810..+0823`) |
| overlay 183 `+136A`, `+13C4`, `+13D2`, `+14BB` | `+0x1E` (plus an index) | `DS:1429` | 0, 7, 6, raise |
| overlay 179 `+25E7` | `+0x12` | the target's details record | 1d8, `DATA` 77 (FND-RNG-044) |
| overlay 184 `+10C3` | `+0x12` | `DS:142D` | a combatant record |
| overlay 173 `+3F13` | `+0x12` | `DS:19C1` | a 23-byte record |
| overlay 190 `+06D1` | `+0x1E` | its argument `bp+0xC` | 0 after `1000:40D7` of the same pointer returns above 30 (`+06B4..+06D6`) |
| `2D40:0B8A`, `2D40:0C25` | `+0x12` | `4F49:08A3 + 0x13 * n` | 19-byte records |
| `31E0:0EE5`, `31E0:22D2`, `31E0:22DB` | `+0x12` | `DS:67BB + 0x25 * n` (`31E0:2195..+21A4`; FND-ACTOR-003) | 37-byte records |

The word stores at `+0x12` (`2D40:1452`, `2D40:1853`, `3BA6:00D1`, `3EBE:0008`, `3F96:000B`) go
through argument or local pointers and write words.

`block_copies.py <dsun> <inventory> 24` adds three copies of 0x42 bytes into the details table:
the load's (overlay 187, FND-PARTY-051) and overlay 184's two copies from `4E4F:0029` into the
record of `DS:112E` or of slot `si` (`+167B..+1699`, `+16D0..+16ED`).

**The loaded records.** `gff_tag_numbers.py <install> CHAR` finds `CHAR` resources only in
`CHARSAVE.GFF`. `details_chunks.py <gff> --byte 12` over every `.GFF` of the installation finds
details chunks in `CHARSAVE.GFF` (22) and `OBJEX.GFF` (375, 353 inside `RDFF` resources) only.
Byte `+0x12` of the data is 1, 3, 4, 5, 6, 7 or 8 in the 19 `CHAR` records. In the `RDFF`
resources it takes the values 0 to 9, 12 and 14 to 16; 9 occurs in nine, whose first class code,
level `+0x1E` and name (from the type-1 chunk) are:

| `RDFF` | Header | Class code | `+0x1E` | Name |
|---|---|---|---|---|
| 482 | `0x4934D9` | 9 | 14 | Soulshard |
| 418 | `0x5A7E8A` | 0 | 4 | Giant Skeleton |
| 419 | `0x5A7F1B` | 0 | 4 | Giant Skeleton |
| 549 | `0x5A7FAC` | 0 | 4 | Giant Skeleton |
| 407 | `0x5AF35B` | 0 | 9 | SkeletonWarrior |
| 548 | `0x5AF42E` | 0 | 9 | SkeletonWarrior |
| 556 | `0x611A1E` | 9 | 8 | Revenant |
| 567 | `0x614FD1` | 9 | 3 | Shadow |
| 406 | `0x65F382` | 0 | 9 | SkeletonWarrior |

Value 14 occurs in ten, among them `Water Elemental`, `Grtr Water Elem` and `Grt Earth Elem`.

## Interpretation

Details byte `+0x12` is the origin (FMT-PARTY-001 `origin`, copied from `CHAR` byte `0x57`) for
party members and a creature kind for other combatants. Every writer that can reach a party
member's record gives 1 to 8: the `CHAR` load (1 to 8 in the shipped records), generation (1 to 7
from the origin choice, 8 for choice 13), and `DATA` 77's 1d8. The value 9 enters only from
creature data, and the nine creatures that carry it are skeletons, a revenant, a shadow and a
soulshard: 9 is the undead kind, and the `DATA` 306 test destroys undead. Kind 14 is the
elemental kind that `DATA` 60 and 165 destroy (FND-RNG-044).

Details byte `+0x1E` is the level in the first class position (FMT-PARTY-001 `levels`). For the
undead it is 3 to 14, so the test's `h` is the creature's level.

A failed `DATA` 306 test has no effect: `2D40:06E1` is an empty routine. With the test, the switch
case of `DATA` 306 either deals 1,000 damage to an undead target on a d20 at least the target
number or does nothing.

## Alternatives

- `1AA0:012A` writes a record field at an offset from the `FNFO` tables (FND-PARTY-051); a script
  or `CHAR` chunk storing a kind-3 field that maps to `+0x12` would be a writer the census cannot
  see. Which kind-3 field number maps to offset 0x12 was not read.
- The `SAVE` resource restores the tables as saved (FND-SAVE-021), so a saved game keeps the values
  above; a changed save could hold any value.
- Reading 9 as undead rests on the names of the nine creatures, which are data; no code compares
  `+0x12` with 9 for any other reason than the ones FND-RNG-044 and FND-MAGIC-018 list.
- `details_chunks.py` matches the header pattern; the nine headers were checked to be followed by
  a value of 9 at `+0x12` and to lie after a type-1 chunk with a name, so they are in chunk chains.
- Overlay 184 `+1DE3` copies 0x42 bytes from an argument pointer into `4E4F:0029`; its callers
  were not read, so a copy of an existing record into the generation's staging record could carry
  another value into generation.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python, `<dsun>` the installed `DSUN.EXE` and
`<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: `resident_listing.py <dsun>
2D40:06C4..2D40:0780 2D40:0B54..2D40:0B90 2D40:0BF0..2D40:0C2A 31E0:2180..31E0:21A4`;
`direct_callers.py <dsun> 2D40:06E1 211+1E40`; `overlay_listing.py <dsun> 211 97CE0 97D1F`,
`183 6AFF3 6B0D8`, `184 6D868 6D8C0` and `190 789F4 78A30`; `record_base_accesses.py <dsun>
<inventory> 12 1E` and the same with `--source 19C5`; `field_stores.py <dsun> <inventory> --es 12
1E`; `block_copies.py <dsun> <inventory> 24`, keeping the sites that push 0x42. With `<install>`
the installation folder: `gff_tag_numbers.py <install> CHAR`, and `details_chunks.py <gff> --byte
12` for each `.GFF` there. Nothing was executed.
