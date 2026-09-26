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
evidence: [FND-REGION-003, FND-REGION-005, FND-EXPLORE-001, SRC-DSUN-MUSIC-79B6927]
conflicting: []
split_with: []
related: []
---

## Layout

One cell of the cell flag map (FMT-REGION-003).

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x0` | 1 | `UINT8` | `flags` | The cell's flags: `0x00`, `0x40`, `0x80` or `0xC0` in every shipped cell. | supported | FND-REGION-003 |
| `0x0 bits 0..5` | | `bits[5]` | `unk_bits_0_4` | Purpose unknown. 0 in every shipped cell. The cell test for movement passes bits 0 to 2 to a routine that decides for one object slot. | supported | FND-REGION-003, FND-EXPLORE-001 |
| `0x0 bits 5..6` | | `bits[1]` | `occupied` | Set together with `blocked` when an object occupies the cell, and cleared with it when the object leaves; `blocked` is cleared only where this is set. 0 in every shipped cell, and the region loader clears it in columns 0 to 97 of every row after loading the map. | supported | FND-REGION-003, FND-REGION-005, FND-EXPLORE-001 |
| `0x0 bits 6..7` | | `bits[1]` | `blocked` | The cell blocks movement: the cell test returns it, and an object cannot occupy a cell that has it. Set in 113,126 shipped cells, and by an object that occupies the cell. | supported | FND-REGION-003, FND-EXPLORE-001 |
| `0x0 bits 7..8` | | `bits[1]` | `unk_bit_7` | Purpose unknown. Set in 85,818 shipped cells. | supported | FND-REGION-003 |
| `0x1` | | | | Total size 1 byte | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 250,880 cells of the 20 installed region files of BLD-GOG-EN-1.1 [FND-REGION-003].

## Open questions

- What bits 0 to 4 and bit 7 do. SRC-DSUN-MUSIC-79B6927 draws a wall image numbered by bits 0 to
  4 when they are not 0, which no shipped cell exercises, and the executable's cell test hands bits
  0 to 2 to a routine that was not read (FND-EXPLORE-001). One routine tests bit 7 of a cell, for a
  caller that was not read (FND-EXPLORE-001, Q-REGION-002).
- Why the region loader clears `occupied` only in columns 0 to 97 of each row (FND-REGION-005,
  Q-REGION-002).
