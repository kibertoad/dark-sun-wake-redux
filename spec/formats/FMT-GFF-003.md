---
id: FMT-GFF-003
title: GFF tag table
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["*.GFF"]
byte_order: little
size: null
text: false
definition: fmt_gff_003.ksy
evidence: [FND-GFF-002, FND-GFF-003, FND-GFF-005]
conflicting: []
split_with: []
related: []
---

## Layout

One table in FMT-GFF-002's `tag_tables`, listing the resources of one tag. A plain table lists
each resource's number, offset and size. An indexed table gives its numbers as ranges and keeps
the offsets and sizes in a resource of the file's `GFFI` table, which is always a plain table
[FND-GFF-002, FND-GFF-003]. The installed `GPLDATA.GFF` has its `PORT` table at `0x21426B`.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `char[4]` | `tag` | Printable ASCII; a three-letter tag is padded with one space at the end. | supported | FND-GFF-002, FND-GFF-005 |
| `0x04` | 4 | `UINT32LE` | `count_word` | The number of resources and the kind of table. | supported | FND-GFF-002, FND-GFF-003 |
| `0x04 bits 0..31` | | `bits[31]` | `entry_count` | Number of resources under `tag`. | supported | FND-GFF-002, FND-GFF-003 |
| `0x04 bits 31..32` | | `bits[1]` | `is_indexed` | 1 for an indexed table, 0 for a plain one. | supported | FND-GFF-002, FND-GFF-003 |
| `0x08` | `entry_count * 12 if is_indexed == 0` | `FMT-GFF-004[entry_count]` | `entries` | The resources, their numbers rising. | supported | FND-GFF-002 |
| | `4 if is_indexed == 1` | `UINT32LE` | `indexed_count` | Equal to `entry_count`. | supported | FND-GFF-003 |
| | `4 if is_indexed == 1` | `UINT32LE` | `index_number` | Number of the resource in the file's `GFFI` table that holds this table's offsets and sizes (FMT-GFF-007). The `n`-th indexed table of a file, counted from 0, has `n`. | supported | FND-GFF-003 |
| | `4 if is_indexed == 1` | `UINT32LE` | `range_count` | Number of entries in `number_ranges`. | supported | FND-GFF-003 |
| | `range_count * 8 if is_indexed == 1` | `FMT-GFF-006[range_count]` | `number_ranges` | The resource numbers, rising. Expanded in order, they give `entry_count` numbers, and the `i`-th of them is the resource at position `i` of the `GFFI` resource. | supported | FND-GFF-003 |
| | | | | Total size `8 + entry_count * 12` for a plain table, `20 + range_count * 8` for an indexed one | | |

## Enumerations and flags

None.

## Differences between builds

None known. The installed `GPLDATA.GFF` has an indexed `TEXT` table that the disc's lacks, so the
disc's `index_number` values in that file are one lower [FND-GFF-003].

## Coverage

Every tag table of the 26 installed and 25 disc `.GFF` files. Indexed tables occur only in
`GPLDATA.GFF`, `OBJEX.GFF` and `RESOURCE.GFF`, where every table but `GFFI` is indexed
[FND-GFF-002, FND-GFF-003].

## Open questions

- How the game looks a resource up by tag and number: by a search of `entries` or of the expanded
  `number_ranges`, and whether it reads `indexed_count` (Q-GFF-002).
