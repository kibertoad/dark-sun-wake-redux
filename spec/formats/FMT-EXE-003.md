---
id: FMT-EXE-003
title: FBOV overlay header in the resident image
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["DSUN.EXE", "CD:DSUN.EXE"]
byte_order: little
size: null
text: false
definition: fmt_exe_003.ksy
evidence: [FND-EXE-002, FND-EXE-003, FND-EXE-004]
conflicting: []
split_with: []
related: []
---

## Layout

The resident part of one overlaid segment, at the paragraph its FMT-EXE-002 descriptor's
`segment` names: `565C:0000` for overlay 169 to `57DD:0000` for overlay 217 in the installed
`DSUN.EXE` [FND-EXE-003]. The next overlay's header starts at the next paragraph boundary after
this one's trampolines.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `BYTE[2]` | `trap` | `CD 3F`, the instruction `INT 3Fh`, in every header. | supported | FND-EXE-003 |
| `0x02` | 2 | `UINT16LE` | `unk_02` | 0 in every header of both files. Purpose unknown. | supported | FND-EXE-003 |
| `0x04` | 4 | `UINT32LE` | `payload_offset` | Offset of this overlay's FMT-EXE-005 block from the start of FMT-EXE-001's `payload`; a multiple of 16. | supported | FND-EXE-003 |
| `0x08` | 2 | `UINT16LE` | `code_size` | Bytes of code in the block. | supported | FND-EXE-003 |
| `0x0A` | 2 | `UINT16LE` | `fixup_size` | Bytes of fixup list in the block after the code; always even. | supported | FND-EXE-003 |
| `0x0C` | 2 | `UINT16LE` | `trampoline_count` | Number of trampolines after the header, 1 to 53. | supported | FND-EXE-003, FND-EXE-004 |
| `0x0E` | 18 | `BYTE[18]` | `unk_0E` | All 0 in every header of both files. Purpose unknown. | supported | FND-EXE-003 |
| `0x20` | `trampoline_count * 5` | `FMT-EXE-004[trampoline_count]` | `trampolines` | The overlay's resident entry points. | supported | FND-EXE-004 |
| | | | | Total size `32 + trampoline_count * 5` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 49 overlay headers of the installed `DSUN.EXE` and all 49 of the disc's. In both, every block
the headers name lies inside the pack, the blocks do not overlap, and every trampoline offset lies
inside its overlay's code [FND-EXE-003, FND-EXE-004].

## Open questions

- Whether `unk_02` and `unk_0E` are filled in by the overlay manager at run time, for example with
  the segment an overlay is loaded at, and what `trap` does when it runs. No code that reads or
  writes a header has been located (FND-EXE-007, Q-EXE-001).
