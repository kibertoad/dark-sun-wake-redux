---
id: FMT-ACTOR-003
title: MONR resource
status: unknown
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RESOURCE.GFF"]
byte_order: little
size: 1134
text: false
definition: null
evidence: []
conflicting: []
split_with: []
related: []
---

## Layout

The one `MONR` resource of `RESOURCE.GFF`, 1,134 bytes. Nothing is claimed about its layout yet.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x000` | 1134 | `BYTE[1134]` | `unk_000` | Purpose unknown. | unknown | None |
| `0x46E` | | | | Total size 1,134 bytes | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

None.

## Open questions

- Its layout. Split into 27 units of 42 bytes, the same bytes of each unit are 0 or small, and
  rows of 42 bytes repeat far more than rows of any other width that divides the size
  (FND-ACTOR-008), while at 81 bytes no column is constant (FND-ACTOR-013). These are patterns
  in the data.
- Which code reads it. The tag occurs only in the code of overlay 204 (FND-ACTOR-006), which has
  not been read.
