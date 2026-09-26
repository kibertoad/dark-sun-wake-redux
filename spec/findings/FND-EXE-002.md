---
id: FND-EXE-002
title: The FBOV segment table has 229 eight-byte descriptors, 49 of them with flag value 3
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 55E8:0000..55E8:0728
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 55D9:0000..55D9:0728
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The table at `55E8:0000` (file offset `0x4B080`) that the pack header points to (FND-EXE-001) is
229 descriptors of four little-endian 16-bit words each, counted from index 0.

- Word 0 ranges from 0 to 21,039.
- Word 4 takes four values: 0 in 79 descriptors, 1 in 87, 3 in 49 and 4 in 14.
- The 49 descriptors with word 4 equal to 3 are indexes 169 to 217, with no other descriptor
  among them. In each, word 0 plus `0x1000` is the segment of an overlay header in the resident
  image (FND-EXE-003), word 2 is `32 + 5 * n`, where `n` is that header's trampoline count, and
  word 6 is 0.
- Across all 229 descriptors, word 2 is at least word 6 in 220 and smaller in 9. The sum of
  `word2 - word6 + 1` over all of them is 663,607, more than the 276,656 bytes of the pack's
  payload.

The disc's `DSUN.EXE` has its table at `55D9:0000`, with the same counts of each word 4 value and
its 49 value-3 descriptors at the same indexes, 169 to 217, with word 2 and word 6 related to
their headers the same way.

## Interpretation

Each descriptor names a segment of the program. Value 3 in word 4 marks an overlaid segment: its
word 0 is the paragraph of the segment's resident header, relative to the start of the load
image, and word 2 is the size of that resident header in bytes. The other values mark segments
that are not overlaid.

## Alternatives

What words 2 and 6 hold for the 180 descriptors that are not overlays, and what values 0, 1 and 4
of word 4 distinguish, are not known. The sum of `word2 - word6 + 1` rules out reading words 2 and
6 as the ends of ranges in the payload. Bit 1 (`0x0002`) of word 4 is set exactly in the value-3
descriptors, so the test could be on that bit or on the whole value; nothing here tells which.

## How to reproduce

Read 229 records of 8 bytes at file offset `0x4B080` and tally word 4. For each record whose word 4
is 3, go to file offset `0x5200 + word0 * 16` and check the header described in FND-EXE-003.
`tools/ghidra/ReportFbovOverlayMap.ps1 -SourcePath <DSUN.EXE>` lists the 49 overlay descriptors
by index with their header segments.
