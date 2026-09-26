---
id: FMT-REGION-005
title: Region entity table
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RGN*.GFF"]
byte_order: little
size: null
text: false
definition: fmt_region_005.ksy
evidence: [FND-REGION-004]
conflicting: []
split_with: []
related: []
---

## Layout

The layout of the `ETAB` resource of a region file: the objects placed on the region, with no
header or count [FND-REGION-004]. `size` is the resource's size from the GFF directory, a multiple
of 8.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | `size` | `FMT-REGION-006[size / 8]` | `entities` | The entity records. | supported | FND-REGION-004 |
| | | | | Total size `size` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 20 installed region files of BLD-GOG-EN-1.1, 13,559 records [FND-REGION-004].

## Open questions

- Which code reads the table. `DSUN.EXE` names the tag only in the code of overlays 187 and 200
  (FND-REGION-007, Q-REGION-003).
