---
id: FMT-GFF-006
title: GFF numbering range in an indexed tag table
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["*.GFF"]
byte_order: little
size: 8
text: false
definition: fmt_gff_006.ksy
evidence: [FND-GFF-003]
conflicting: []
split_with: []
related: []
---

## Layout

One entry of an indexed FMT-GFF-003 table's `number_ranges`. The installed `GPLDATA.GFF`'s `PORT`
table has the ranges first 1, count 175 and first 177, count 3, for the numbers 1 to 175 and 177
to 179 [FND-GFF-003].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `first_number` | The first resource number in the range. | supported | FND-GFF-003 |
| `0x04` | 4 | `UINT32LE` | `number_count` | How many consecutive numbers the range holds. | supported | FND-GFF-003 |
| `0x08` | | | | Total size 8 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Every range of every indexed table in the installed and disc copies of `GPLDATA.GFF`, `OBJEX.GFF`
and `RESOURCE.GFF` [FND-GFF-003].

## Open questions

None known.
