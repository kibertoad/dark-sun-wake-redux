---
id: FMT-GFF-004
title: GFF resource entry in a plain tag table
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["*.GFF"]
byte_order: little
size: 12
text: false
definition: fmt_gff_004.ksy
evidence: [FND-GFF-002, FND-GFF-005]
conflicting: []
split_with: []
related: []
---

## Layout

One entry of a plain FMT-GFF-003 table. `RGN001.GFF#RNME/1` is the entry number 1, offset 28,
size 7 [FND-GFF-002].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `number` | The resource's number, 0 to 32,003 in the shipped files. | supported | FND-GFF-002, FND-GFF-005 |
| `0x04` | 4 | `UINT32LE` | `data_offset` | File offset of the resource's bytes, in FMT-GFF-001's `data`. | supported | FND-GFF-002 |
| `0x08` | 4 | `UINT32LE` | `data_size` | Size of the resource's bytes, never 0 in the shipped files. | supported | FND-GFF-002, FND-GFF-005 |
| `0x0C` | | | | Total size 12 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Every entry of every plain table in the 26 installed and 25 disc `.GFF` files: no two resources of
a file share a byte [FND-GFF-002, FND-GFF-005].

## Open questions

None known.
