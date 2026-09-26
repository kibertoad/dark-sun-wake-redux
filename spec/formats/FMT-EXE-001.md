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
evidence: [FND-EXE-001, FND-EXE-002, FND-EXE-003]
conflicting: []
split_with: []
related: []
---

## Layout

The pack starts at the end of the MZ image, the file offset the MZ header's page counts give
(`0x57570` in the installed `DSUN.EXE`, `0x57430` in the disc's), and runs to the end of the file
[FND-EXE-001]. Offsets below are from the start of the pack. The loader's own reading of these
fields has not been located, so each row states what the shipped files hold.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `char[4]` | `magic` | `FBOV`, ASCII, with no NUL. | supported | FND-EXE-001 |
| `0x04` | 4 | `UINT32LE` | `payload_size` | Bytes in `payload`: 276,656 in the installed file, 277,264 in the disc's. The pack ends exactly at the end of the file. | supported | FND-EXE-001 |
| `0x08` | 4 | `UINT32LE` | `segment_table_offset` | File offset of the segment table, `FMT-EXE-002[segment_count]`, which lies in the resident load image: `0x4B080` (`55E8:0000`) installed, `0x4AF90` (`55D9:0000`) on the disc. | supported | FND-EXE-001, FND-EXE-002 |
| `0x0C` | 4 | `UINT32LE` | `segment_count` | Number of segment-table descriptors, 229 in both files. | supported | FND-EXE-001, FND-EXE-002 |
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

- Which code reads the pack header, and whether it treats `segment_count` as signed (FND-EXE-007,
  Q-EXE-001).
