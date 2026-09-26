---
id: FMT-REGION-003
title: Region cell flag map
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RGN*.GFF"]
byte_order: little
size: 12544
text: false
definition: fmt_region_003.ksy
evidence: [FND-REGION-003, FND-REGION-005]
conflicting: []
split_with: []
related: []
---

## Layout

The layout of the `GMAP` resource of a region file: one byte of flags for each cell of the terrain
map (FMT-REGION-002), in the same order, so the cell at column `c` and row `r` is
`cells[r * 128 + c]` [FND-REGION-003, FND-REGION-005]. The region loader loads it into the buffer
the far pointer at `DS:0538` names, and only when its second argument is not 0 [FND-REGION-005].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x0000` | 12,544 | `FMT-REGION-004[12544]` | `cells` | The flags of each cell. | supported | FND-REGION-003, FND-REGION-005 |
| | | | | Total size 12,544 bytes | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 20 installed region files of BLD-GOG-EN-1.1 [FND-REGION-003].

## Open questions

- When the loader is called with a second argument of 0, and what the game then uses for the
  cell flags (FND-REGION-005, Q-REGION-004).
