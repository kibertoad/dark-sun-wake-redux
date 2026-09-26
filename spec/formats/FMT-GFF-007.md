---
id: FMT-GFF-007
title: GFFI index resource
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["*.GFF"]
byte_order: little
size: null
text: false
definition: fmt_gff_007.ksy
evidence: [FND-GFF-003]
conflicting: []
split_with: []
related: []
---

## Layout

A resource under the tag `GFFI`, which holds the offsets and sizes for the indexed FMT-GFF-003
table whose `index_number` is its number. `GPLDATA.GFF#GFFI/1`, for the `GPLI` table, is the 12
bytes at `0x213E54` of the installed file [FND-GFF-003].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `entry_count` | Equal to the table's `entry_count`. | supported | FND-GFF-003 |
| `0x04` | `entry_count * 8` | `FMT-GFF-005[entry_count]` | `entries` | The bytes of each resource of the table, in the order of its expanded `number_ranges`. | supported | FND-GFF-003 |
| | | | | Total size `4 + entry_count * 8` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Every `GFFI` resource in the installed and disc copies of `GPLDATA.GFF`, `OBJEX.GFF` and
`RESOURCE.GFF`: each is exactly `4 + 8 * entry_count` bytes [FND-GFF-003].

## Open questions

None known.
