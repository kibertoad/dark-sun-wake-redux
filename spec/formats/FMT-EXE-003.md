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
evidence: [FND-EXE-002, FND-EXE-003, FND-EXE-004, FND-EXE-520, FND-EXE-561, FND-EXE-562]
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
| `0x00` | 2 | `BYTE[2]` | `trap` | `CD 3F`, the instruction `INT 3Fh`, in every header. In the installed file, when the manager unloads the overlay, the innermost frame that would return into its code returns here instead, which runs the `INT 3Fh` handler. | supported | FND-EXE-003, FND-EXE-562 |
| `0x02` | 2 | `UINT16LE` | `unk_02` | 0 in every header of both files. In the installed file the manager stores there, when it unloads the overlay, the return offset it took from that frame, and gives it back when it loads the overlay again. | supported | FND-EXE-003, FND-EXE-562 |
| `0x04` | 4 | `UINT32LE` | `payload_offset` | Offset of this overlay's FMT-EXE-005 block from the start of FMT-EXE-001's `payload`; a multiple of 16. | supported | FND-EXE-003 |
| `0x08` | 2 | `UINT16LE` | `code_size` | Bytes of code in the block. | supported | FND-EXE-003 |
| `0x0A` | 2 | `UINT16LE` | `fixup_size` | Bytes of fixup list in the block after the code; always even. | supported | FND-EXE-003 |
| `0x0C` | 2 | `UINT16LE` | `trampoline_count` | Number of trampolines after the header, 1 to 53. | supported | FND-EXE-003, FND-EXE-004 |
| `0x0E` | 18 | `BYTE[18]` | `unk_0E` | All 0 in every header of both files. At run time the installed file's manager keeps the load segment in the word at header offset `0x10`, the next kept header's segment at `0x12`, the near address of the loader at `0x18`, state bits at byte `0x1A` (`0xFF` leaves the header out) and a count at byte `0x1B`. | supported | FND-EXE-003, FND-EXE-520, FND-EXE-561, FND-EXE-562 |
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

- What the manager keeps in header words `0x0E`, `0x14`, `0x16`, `0x1C` and `0x1E`. The
  placement code uses `0x1C`, and manager code after `4AE5:0900`, not read, uses offsets
  `0x0E`, `0x14` and `0x16` of a segment it loads into ES. Whether the disc's manager uses
  the header as the installed one does is also open (Q-EXE-001).
