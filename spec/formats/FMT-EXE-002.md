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
evidence: [FND-EXE-002, FND-EXE-003, FND-EXE-561, FND-EXE-567, FND-EXE-571]
conflicting: []
split_with: []
related: []
---

## Layout

One record of the segment table that FMT-EXE-001's `segment_table_offset` points to, in the
resident load image. The table has `segment_count` records, counted from 0; the spec calls an
overlay by the index of its descriptor ("overlay 182"). The records are in load-image order:
the spans from `segment * 16 + start_offset` to `segment * 16 + end_offset` of the flags-0,
flags-1 and flags-3 descriptors cover the whole load image without overlapping, with at most 15
bytes of zero padding between them [FND-EXE-571].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `segment` | A paragraph number relative to the start of the load image, from 0 to 21,039. For an overlay descriptor, `segment + 0x1000` is the segment of its FMT-EXE-003 header. | supported | FND-EXE-002 |
| `0x02` | 2 | `UINT16LE` | `end_offset` | The offset in `segment` just past the last byte the segment occupies in the load image; equal to `start_offset` when it holds none, and `start_offset - 1` in every flags-4 descriptor. For an overlay descriptor this is the size in bytes of its resident header, `32 + 5 * trampoline_count`. At run time only whether it is 0 is read, by the overlay manager's descriptor walk. | supported | FND-EXE-002, FND-EXE-561, FND-EXE-567, FND-EXE-571 |
| `0x04` | 2 | `UINT16LE` | `flags` | 0, 1, 3 or 4; see below. Code reads only bit 1. | supported | FND-EXE-002, FND-EXE-567 |
| `0x06` | 2 | `UINT16LE` | `start_offset` | The offset in `segment` of the first byte the segment occupies in the load image; 0 for every overlay descriptor. No code reads it. | supported | FND-EXE-002, FND-EXE-567, FND-EXE-571 |
| `0x08` | | | | Total size 8 | | |

## Enumerations and flags

### `flags`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| `0` | `FBOV_SEGMENT_0` | Purpose unknown; 79 descriptors in the installed file, among them the data segment `57E0` (descriptor 218). | supported | FND-EXE-002, FND-EXE-571 |
| `1` | `FBOV_SEGMENT_1` | Purpose unknown; 87 descriptors. | supported | FND-EXE-002 |
| `3` | `FBOV_SEGMENT_OVERLAY` | An overlaid segment: `segment` locates its FMT-EXE-003 header. 49 descriptors, indexes 169 to 217. The installed file's overlay manager keeps a descriptor as an overlay when bit 1 of `flags` is set and `end_offset` is not 0; 3 is the only value with bit 1 set. | supported | FND-EXE-002, FND-EXE-003, FND-EXE-561 |
| `4` | `FBOV_SEGMENT_4` | Purpose unknown; 14 descriptors. | supported | FND-EXE-002 |

## Differences between builds

None known.

## Coverage

All 229 descriptors of the installed `DSUN.EXE` and all 229 of the disc's, which have the same
number of each `flags` value and the same overlay indexes [FND-EXE-002].

## Open questions

- What values 0, 1 and 4 of `flags` distinguish. The program does not read them [FND-EXE-567].
  The flags-1 segments hold 1,283 of the 1,485 resident function starts and 79 of 81 end with a
  return, and the flags-0 segments hold 3 and end mostly with `00` or `FF` bytes, which suggests
  code and data; the flags-4 descriptors hold no bytes and lie inside the data segment
  [FND-EXE-571]. Only the linker's description of the table can say (Q-EXE-024).
