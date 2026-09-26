---
id: FMT-REGION-001
title: Region name resource
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RGN*.GFF"]
byte_order: little
size: null
text: false
definition: fmt_region_001.ksy
evidence: [FND-REGION-001]
conflicting: []
split_with: []
related: []
---

## Layout

The layout of the `RNME` resource of a region file, whose number is the region's [FND-REGION-001].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | | `char[]` | `name` | The region's name, printable ASCII, 3 to 15 characters and a NUL in the shipped files. The resource ends with the NUL. | supported | FND-REGION-001 |
| | | | | Total size variable | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The 20 installed region files of BLD-GOG-EN-1.1 [FND-REGION-001].

## Open questions

- Whether and where the game uses the name. `DSUN.EXE` has no `RNME` tag bytes (FND-REGION-007,
  Q-REGION-001).
