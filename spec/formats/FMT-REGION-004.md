---
id: FMT-REGION-004
title: Region cell flags
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RGN*.GFF"]
byte_order: little
size: 1
text: false
definition: fmt_region_004.ksy
evidence: [FND-REGION-003, FND-REGION-005, SRC-DSUN-MUSIC-79B6927]
conflicting: []
split_with: []
related: []
---

## Layout

One cell of the cell flag map (FMT-REGION-003).

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x0` | 1 | `UINT8` | `flags` | The cell's flags: `0x00`, `0x40`, `0x80` or `0xC0` in every shipped cell. | supported | FND-REGION-003 |
| `0x0 bits 0..5` | | `bits[5]` | `unk_bits_0_4` | Purpose unknown. 0 in every shipped cell. | supported | FND-REGION-003 |
| `0x0 bits 5..6` | | `bits[1]` | `unk_bit_5` | Purpose unknown. 0 in every shipped cell, and the region loader clears it in columns 0 to 97 of every row after loading the map. | supported | FND-REGION-003, FND-REGION-005 |
| `0x0 bits 6..7` | | `bits[1]` | `unk_bit_6` | Purpose unknown here. Set in 113,126 shipped cells. | supported | FND-REGION-003 |
| `0x0 bits 7..8` | | `bits[1]` | `unk_bit_7` | Purpose unknown. Set in 85,818 shipped cells. | supported | FND-REGION-003 |
| `0x1` | | | | Total size 1 byte | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 250,880 cells of the 20 installed region files of BLD-GOG-EN-1.1 [FND-REGION-003].

## Open questions

- What each bit does. SRC-DSUN-MUSIC-79B6927 draws a wall image numbered by bits 0 to 4 when they
  are not 0, which no shipped cell exercises. Bit 5 is cleared on loading, which suggests the game
  sets it while the region runs (FND-REGION-005). The region loader does not read bits 6 and 7
  (Q-REGION-002).
