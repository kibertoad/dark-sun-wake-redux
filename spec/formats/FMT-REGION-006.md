---
id: FMT-REGION-006
title: Region entity record
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RGN*.GFF"]
byte_order: little
size: 8
text: false
definition: fmt_region_006.ksy
evidence: [FND-REGION-004, FND-IMAGE-010, FND-ACTOR-001, FND-ACTOR-003, SRC-DSUN-MUSIC-79B6927]
conflicting: []
split_with: []
related: []
---

## Layout

One record of a region's entity table (FMT-REGION-005): an object placed on the region
[FND-REGION-004]. In the first gameplay frame of the opening region, the first frame of each
record's object image is drawn with its top-left at `x` and `y` less the offsets of the object's
`OJFF` definition, and less `vertical_offset` on `y` [FND-IMAGE-010].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x0` | 2 | `INT16LE` | `x` | World x in pixels, -129 to 3,559 in the shipped records. | supported | FND-REGION-004, FND-IMAGE-010 |
| `0x2` | 2 | `INT16LE` | `y` | World y in pixels, 0 to 2,382 in the shipped records. | supported | FND-REGION-004, FND-IMAGE-010 |
| `0x4` | 1 | `INT8` | `vertical_offset` | Pixels the object is drawn above `y`: 0, 10, 24, 30, 32, 64 or 85 in the shipped records, and in every one equal to the `vertical_offset` of the object's FMT-ACTOR-001, which is the copy the drawing position is computed from. | supported | FND-REGION-004, FND-IMAGE-010, FND-ACTOR-001, FND-ACTOR-003 |
| `0x5` | 1 | `UINT8` | `flags` | Flags; 26 values in the shipped records. | supported | FND-REGION-004 |
| `0x5 bits 0..3` | | `bits[3]` | `unk_flags_bits_0_2` | Purpose unknown. Every value from 0 to 7 occurs. | supported | FND-REGION-004 |
| `0x5 bits 3..5` | | `bits[2]` | `unk_flags_bits_3_4` | Purpose unknown. 0 in every shipped record. | supported | FND-REGION-004 |
| `0x5 bits 5..6` | | `bits[1]` | `unk_flags_bit_5` | Purpose unknown. Set in 8,128 shipped records. | supported | FND-REGION-004 |
| `0x5 bits 6..7` | | `bits[1]` | `unk_flags_bit_6` | Purpose unknown. 0 in every shipped record. | supported | FND-REGION-004 |
| `0x5 bits 7..8` | | `bits[1]` | `unk_flags_bit_7` | Purpose unknown. Set in 213 shipped records. | supported | FND-REGION-004 |
| `0x6` | 2 | `INT16LE` | `object` | Number of the object's `OJFF` resource in `OBJEX.GFF`, 1 or more in every shipped record. | supported | FND-REGION-004 |
| `0x8` | | | | Total size 8 bytes | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 13,559 records of the 20 installed region files of BLD-GOG-EN-1.1 [FND-REGION-004]. The
placement of 22 records, none with `unk_flags_bit_7` set, matches the first gameplay frame of the
opening region [FND-IMAGE-010].

## Open questions

- What the flag bits do. SRC-DSUN-MUSIC-79B6927 draws the object mirrored left to right when
  `unk_flags_bit_7` is set; no capture shows such a record (Q-REGION-003).
- Whether the game treats `object` as signed, and what it does with the 154 records that lie
  outside the map's 2,048 by 1,568 pixels (FND-REGION-004, Q-REGION-003). The routine at
  `2D40:0589` compares the object word of the in-memory table of placed objects with a negative
  number and requests the negated number (FND-ACTOR-004); that table has this record's layout,
  but nothing yet shows it is filled from `ETAB`.
