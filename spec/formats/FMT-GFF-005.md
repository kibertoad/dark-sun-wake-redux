---
id: FMT-GFF-005
title: GFF byte range
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["*.GFF"]
byte_order: little
size: 8
text: false
definition: fmt_gff_005.ksy
evidence: [FND-GFF-003, FND-GFF-004]
conflicting: []
split_with: []
related: []
---

## Layout

A range of bytes in a `.GFF` file: an entry of FMT-GFF-002's `gaps`, or of a `GFFI` resource's
`entries` (FMT-GFF-007). The installed `CHARSAVE.GFF`'s first gap is offset `0x120A`, size 224
[FND-GFF-004].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `data_offset` | File offset of the range's first byte. | supported | FND-GFF-003, FND-GFF-004 |
| `0x04` | 4 | `UINT32LE` | `data_size` | Size of the range in bytes. | supported | FND-GFF-003, FND-GFF-004 |
| `0x08` | | | | Total size 8 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Every gap and every `GFFI` range in the 26 installed and 25 disc `.GFF` files [FND-GFF-003,
FND-GFF-004].

## Open questions

None known.
