---
id: FMT-EXE-002
title: FBOV segment-table descriptor
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["DSUN.EXE", "CD:DSUN.EXE"]
byte_order: little
size: 8
text: false
definition: fmt_exe_002.ksy
evidence: [FND-EXE-002, FND-EXE-003, FND-EXE-561]
conflicting: []
split_with: []
related: []
---

## Layout

One record of the segment table that FMT-EXE-001's `segment_table_offset` points to, in the
resident load image. The table has `segment_count` records, counted from 0; the spec calls an
overlay by the index of its descriptor ("overlay 182").

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `segment` | A paragraph number relative to the start of the load image, from 0 to 21,039. For an overlay descriptor, `segment + 0x1000` is the segment of its FMT-EXE-003 header. | supported | FND-EXE-002 |
| `0x02` | 2 | `UINT16LE` | `unk_02` | For an overlay descriptor, the size in bytes of its resident header, `32 + 5 * trampoline_count`. Purpose unknown for the others. | supported | FND-EXE-002 |
| `0x04` | 2 | `UINT16LE` | `flags` | 0, 1, 3 or 4; see below. | supported | FND-EXE-002 |
| `0x06` | 2 | `UINT16LE` | `unk_06` | 0 for every overlay descriptor. Purpose unknown. | supported | FND-EXE-002 |
| `0x08` | | | | Total size 8 | | |

## Enumerations and flags

### `flags`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| `0` | `FBOV_SEGMENT_0` | Purpose unknown; 79 descriptors in the installed file. | supported | FND-EXE-002 |
| `1` | `FBOV_SEGMENT_1` | Purpose unknown; 87 descriptors. | supported | FND-EXE-002 |
| `3` | `FBOV_SEGMENT_OVERLAY` | An overlaid segment: `segment` locates its FMT-EXE-003 header. 49 descriptors, indexes 169 to 217. The installed file's overlay manager keeps a descriptor as an overlay when bit 1 of `flags` is set and `unk_02` is not 0; 3 is the only value with bit 1 set. | supported | FND-EXE-002, FND-EXE-003, FND-EXE-561 |
| `4` | `FBOV_SEGMENT_4` | Purpose unknown; 14 descriptors. | supported | FND-EXE-002 |

## Differences between builds

None known.

## Coverage

All 229 descriptors of the installed `DSUN.EXE` and all 229 of the disc's, which have the same
number of each `flags` value and the same overlay indexes [FND-EXE-002].

## Open questions

- What `unk_02` and `unk_06` hold for the descriptors that are not overlays, and what values 0, 1
  and 4 of `flags` distinguish (Q-EXE-001).
