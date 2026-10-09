---
id: FMT-EXE-001
title: FBOV overlay pack at the end of DSUN.EXE
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["DSUN.EXE", "CD:DSUN.EXE"]
byte_order: little
size: null
text: false
definition: fmt_exe_001.ksy
evidence: [FND-EXE-001, FND-EXE-002, FND-EXE-003, FND-EXE-560, FND-EXE-561, FND-EXE-563]
conflicting: []
split_with: []
related: []
---

## Layout

The pack starts at the end of the MZ image, the file offset the MZ header's page counts give
(`0x57570` in the installed `DSUN.EXE`, `0x57430` in the disc's), and runs to the end of the file
[FND-EXE-001]. Offsets below are from the start of the pack. In the installed file the overlay
manager's startup reads `magic` and `payload_size` and no other field [FND-EXE-560]; each row
states what the shipped files hold.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `char[4]` | `magic` | `FBOV`, ASCII, with no NUL. The installed file's manager requires `FB` and skips a block whose next two bytes are not `OV` by that block's `payload_size`. | supported | FND-EXE-001, FND-EXE-560 |
| `0x04` | 4 | `UINT32LE` | `payload_size` | Bytes in `payload`: 276,656 in the installed file, 277,264 in the disc's. The pack ends exactly at the end of the file. The installed file's manager stores it in `57E0:356C` and caps an EMS or extended-memory overlay cache at it. | supported | FND-EXE-001, FND-EXE-560, FND-EXE-563 |
| `0x08` | 4 | `UINT32LE` | `segment_table_offset` | File offset of the segment table, `FMT-EXE-002[segment_count]`, which lies in the resident load image: `0x4B080` (`55E8:0000`) installed, `0x4AF90` (`55D9:0000`) on the disc. The installed file's manager does not read it and walks the table at `55E8:0000` directly. | supported | FND-EXE-001, FND-EXE-002, FND-EXE-560, FND-EXE-561 |
| `0x0C` | 4 | `UINT32LE` | `segment_count` | Number of segment-table descriptors, 229 in both files. The installed file's manager does not read it; its walk stops after 229 records at a fixed bound. | supported | FND-EXE-001, FND-EXE-002, FND-EXE-560, FND-EXE-561 |
| `0x10` | `payload_size` | `BYTE[payload_size]` | `payload` | One FMT-EXE-005 block for each overlaid segment, at the offset from this field's start that the segment's overlay header (FMT-EXE-003) gives, each padded with zeros to a multiple of 16 bytes, and zeros after the last block. | supported | FND-EXE-003 |
| | | | | Total size `16 + payload_size` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The installed `DSUN.EXE` and the disc's `DSUN.EXE` in BLD-GOG-EN-1.1, read in full: in each, the
pack header sits at the end of the MZ image and the pack ends at the end of the file, and the 49
overlay blocks and their padding account for every payload byte [FND-EXE-001, FND-EXE-003].

## Open questions

- Whether code other than the installed overlay manager's startup reads `segment_table_offset`
  or `segment_count`, and whether the disc's manager reads the pack header as the installed
  one does (Q-EXE-001).
