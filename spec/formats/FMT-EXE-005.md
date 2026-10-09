---
id: FMT-EXE-005
title: FBOV overlay code block with its fixup list
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["DSUN.EXE", "CD:DSUN.EXE"]
byte_order: little
size: null
text: false
definition: fmt_exe_005.ksy
evidence: [FND-EXE-003, FND-EXE-005]
conflicting: []
split_with: []
related: []
---

## Layout

One overlay's block in FMT-EXE-001's `payload`, at the `payload_offset` its FMT-EXE-003 header
gives, with that header's `code_size` and `fixup_size`. Overlay code is located in the spec by its
file offset: overlay 169's code starts at `DSUN.EXE+0x00057580` [FND-EXE-003].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | `code_size` | `BYTE[code_size]` | `code` | The overlay's code and data. A trampoline's `target` is an offset in it. | supported | FND-EXE-003 |
| | `fixup_size` | `UINT16LE[fixup_size / 2]` | `fixups` | Offsets in `code` of 16-bit words that each hold a segment-table index shifted left by three, with the low three bits 0. Every offset leaves room for its word inside `code`. | supported | FND-EXE-005 |
| | | | | Total size `code_size + fixup_size` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 49 blocks of the installed `DSUN.EXE`, with 8,262 fixups, and the sizes of all 49 of the
disc's [FND-EXE-003, FND-EXE-005]. FND-EXE-173 additionally validates both sources'
fixup-table dimensions, operand bounds and descriptor-index admission through the
bounded source reader; this does not establish native loader behavior.

## Open questions

- Does any native transfer reach the fixup-table and zero-padding bytes the
  analyzer assigns to overlay bodies (Q-EXE-022)? FND-EXE-173 and FND-EXE-174
  show that analysis, including after relocation-pair repair, assigns them to
  bodies. Accounting for every analyzer reference into them, as a table read
  at the wrong base or data decoded as code, settles it.
