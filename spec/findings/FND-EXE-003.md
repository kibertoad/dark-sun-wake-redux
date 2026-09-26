---
id: FND-EXE-003
title: 49 overlay headers in the resident image locate the code and fixup blocks of the FBOV payload
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 565C:0000..57DF:0005
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 564D:0000..57D6:0005
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The 49 segment-table descriptors with word 4 equal to 3 (FND-EXE-002) point at 49 headers in the
resident image, from segment `565C` (descriptor 169) to `57DD` (descriptor 217), in descriptor
order. Each header is 32 bytes:

- bytes `CD 3F`, then a 16-bit 0;
- at `0x04`, a 32-bit offset counted from the first byte after the pack header, file offset
  `0x57580`;
- at `0x08`, a 16-bit code size; at `0x0A`, a 16-bit fixup size, always even; at `0x0C`, a 16-bit
  trampoline count, from 1 to 53;
- 18 bytes of 0 at `0x0E`.

The trampolines follow the header (FND-EXE-004), and the next header starts at the next paragraph
boundary after them. The last header, at `57DD:0000` with one trampoline, ends at `57DF:0005`.

Each header's offset is a multiple of 16 and names a block of the payload that holds the code
bytes and then the fixup list (FND-EXE-005). The blocks lie in descriptor order and do not
overlap. The first, for descriptor 169, starts at `DSUN.EXE+0x00057580`, and the last, for
descriptor 217, is 92 bytes of code at `DSUN.EXE+0x0009ADC0`. The 49 blocks hold 258,376 bytes of
code and 16,524 bytes of fixups. The remaining 1,756 bytes of the 276,656-byte payload are 1,736
bytes between blocks, which pad each block to a multiple of 16, and 20 bytes after the last block;
all of them are 0.

The disc's `DSUN.EXE` has 49 headers from `564D:0000`, in the same form, with 258,921 bytes of
code and 16,560 bytes of fixups. Its blocks also lie in descriptor order without overlap, and the
1,763 bytes between them and the 20 after the last are 0.

## Interpretation

Each overlaid segment keeps a small resident header, and its code lives in the pack. The header
gives the file position and size of the code and of the fixup list that goes with it.

## Alternatives

`CD 3F` is the x86 instruction `INT 3Fh`. Reading the header's first two bytes as a trap into an
overlay manager, and the 18 zero bytes and the 16-bit 0 as fields that manager fills at run time,
fits Borland's overlay scheme, but no code that reads or writes them has been located
(FND-EXE-007).

## How to reproduce

For each overlay descriptor, read 32 bytes at file offset `0x5200 + word0 * 16`, then check that
`0x57580 + offset + code size + fixup size` stays within the file and that the blocks, sorted by
offset, do not overlap. `tools/ghidra/ReportFbovOverlayMap.ps1 -SourcePath <DSUN.EXE>` prints each
overlay's `HeaderSegment`, `TrampolineCount`, `CodeFileOffset`, `CodeBytes` and `FixupBytes`.
