---
id: FMT-REGION-002
title: Region terrain map
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RGN*.GFF"]
byte_order: little
size: 12544
text: false
definition: fmt_region_002.ksy
evidence: [FND-REGION-002, FND-REGION-005, FND-REGION-006, FND-IMAGE-010]
conflicting: []
split_with: []
related: [RULE-REGION-001]
---

## Layout

The layout of the `MAP ` resource of a region file. The map is a grid of 98 rows of 128 cells,
stored row by row from the top, each row from the left; the cell at column `c` and row `r` is
`cells[r * 128 + c]` and covers the 16x16 world pixels whose top-left is `(c * 16, r * 16)`
[FND-REGION-006]. The region loader loads it into a buffer of 12,544 bytes, and prefers an `RMAP`
resource of the same number when the files hold one [FND-REGION-005].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x0000` | 12,544 | `UINT8[12544]` | `cells` | For each cell, the number of the `TILE` resource of the same file that RULE-REGION-001 draws there. | supported | FND-REGION-002, FND-REGION-006, FND-IMAGE-010 |
| | | | | Total size 12,544 bytes | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 20 installed region files of BLD-GOG-EN-1.1: every byte names a tile of the same file
[FND-REGION-002]. The first gameplay frame of the opening region matches the tiles its map
names [FND-IMAGE-010].

## Open questions

- Where an `RMAP` resource would come from; no shipped file has one (FND-REGION-005,
  Q-REGION-004).
