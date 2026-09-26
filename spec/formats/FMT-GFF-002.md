---
id: FMT-GFF-002
title: GFF directory
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["*.GFF"]
byte_order: little
size: null
text: false
definition: fmt_gff_002.ksy
evidence: [FND-GFF-002, FND-GFF-003, FND-GFF-004]
conflicting: []
split_with: []
related: []
---

## Layout

The directory of FMT-GFF-001, at its `directory_offset` and `directory_size` bytes long:
`0x573CE9` in the installed `RESOURCE.GFF` [FND-GFF-002].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `tag_list_offset` | 8 in every shipped file, the offset of `tag_count` from the directory's start. | supported | FND-GFF-002 |
| `0x04` | 4 | `UINT32LE` | `tag_list_end` | Offset from the directory's start of the first byte after `tag_tables`, which is `gap_count`. | supported | FND-GFF-002 |
| `0x08` | 2 | `UINT16LE` | `tag_count` | Number of tag tables, 1 to 21 in the shipped files. | supported | FND-GFF-002 |
| `0x0A` | `tag_list_end - 10` | `FMT-GFF-003[tag_count]` | `tag_tables` | One table per tag, no tag twice. A file with indexed tables has a `GFFI` table first. | supported | FND-GFF-002, FND-GFF-003 |
| | 2 | `UINT16LE` | `gap_count` | Number of entries in `gaps`. | supported | FND-GFF-004 |
| | `gap_count * 8` | `FMT-GFF-005[gap_count]` | `gaps` | The ranges of FMT-GFF-001's `data` that no resource uses. Together they cover every such byte, and none overlaps a resource. | supported | FND-GFF-004 |
| | | | | Total size `tag_list_end + 2 + gap_count * 8` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The directories of all 26 installed and all 25 disc `.GFF` files: in each, `tag_list_offset` is 8,
walking the tables ends at `tag_list_end`, and the gap list ends at `directory_size`
[FND-GFF-002, FND-GFF-004]. Gap lists are empty apart from the installed `CHARSAVE.GFF`, both
copies of `GPLDATA.GFF` and both copies of `OBJEX.GFF`.

## Open questions

- Whether the game reads `tag_list_offset` and `tag_list_end` or walks the tables, and whether it
  reads or writes `gaps` (Q-GFF-002).
- That `gaps` lists free space a writer can reuse rests only on where the ranges fall
  (FND-GFF-004).
