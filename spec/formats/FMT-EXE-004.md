---
id: FMT-EXE-004
title: FBOV overlay trampoline
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["DSUN.EXE", "CD:DSUN.EXE"]
byte_order: little
size: 5
text: false
definition: fmt_exe_004.ksy
evidence: [FND-EXE-004, FND-EXE-562]
conflicting: []
split_with: []
related: []
---

## Layout

One resident entry point of an overlay, in the `trampolines` of its FMT-EXE-003 header. Its
address in the resident image is the header's segment and offset `0x20 + 5 * i`, counted from
`i = 0`.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `BYTE[2]` | `trap` | `CD 3F`, the instruction `INT 3Fh`, in every trampoline. In the installed file the handler loads the overlay, the manager rewrites every trampoline of it as `EA`, `target` and the load segment, a far jump into the loaded code, and returns to the trampoline; unloading writes `CD 3F`, `target` and 0 back. | supported | FND-EXE-004, FND-EXE-562 |
| `0x02` | 2 | `UINT16LE` | `target` | An offset in the overlay's code; less than the header's `code_size` in every trampoline. | supported | FND-EXE-004 |
| `0x04` | 1 | `UINT8` | `unk_04` | 0 in every trampoline of both files. In the far-jump form it is the high byte of the load segment. | supported | FND-EXE-004, FND-EXE-562 |
| `0x05` | | | | Total size 5 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 854 trampolines of the installed `DSUN.EXE` and all 863 of the disc's [FND-EXE-004].

## Open questions

- Whether the disc's overlay manager handles trampolines as the installed one does
  (Q-EXE-001).
