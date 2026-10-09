---
id: FND-EXE-571
title: Each FBOV segment descriptor's unk_06 and unk_02 are the start and end offsets of its segment's bytes, and the spans tile DSUN.EXE's load image
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004B080..0x0004B7A8
tool: Python 3.14.7 with xxhash 4.0.1 (tools/research/exec-census/segment_table_spans.py)
environment: null
---

## Observation

In the installed `DSUN.EXE`, the 229 segment descriptors (FMT-EXE-002) at file offset `0x4B080`
were read, and for each one the load-image span from `segment * 16 + unk_06` to
`segment * 16 + unk_02` was formed.

- `flags` is 0 in 79 descriptors, 1 in 87, 3 in 49 and 4 in 14. `unk_02` is 0 in four flags-0
  descriptors (86, 88, 160, 161) and one flags-1 descriptor (168), all with `unk_06` 0. In five
  more flags-1 descriptors (2, 15, 24, 26, 59) `unk_06` equals `unk_02`.
- In all 14 flags-4 descriptors (219 to 227), `unk_02` is `unk_06 - 1`: `0xFFFF` in the five
  whose `unk_06` is 0, and one less than `unk_06` in the other nine.
- Sorted by start, the spans with `unk_02` greater than `unk_06` and `flags` 0, 1 or 3 do not
  overlap. The first starts at load-image offset 0 (descriptor 0) and the last ends at
  `0x52370` (descriptor 228), the end of the load image. Of the 130 gaps between consecutive
  spans outside descriptor 218's span, every one is 1 to 15 bytes of zeros and ends on an even
  offset, and 117 end on a multiple of 16. Descriptor 218 (`flags` 0) spans `0x47E00` to
  `0x522EC`, load-image segment `57E0`; the flags-4 descriptors' positions fall inside it.
- Of the 81 flags-1 spans that are not empty, 79 end with a return byte (74 with `CB`, 5 with
  `C3`); the other two end with `FD` and `04`. The byte at the end offset is `55` for 57 of them.
  The spans hold 1,283 of the 1,485 resident starts in the function inventory
  `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`. The 75 non-empty flags-0 spans hold 3 of them; 46 end
  with `00` and 20 with `FF`.
- For the 49 flags-3 descriptors, `unk_06` is 0 and `unk_02` is the overlay header's size
  (FND-EXE-002), so their spans are the headers in the load image.
- The inventory's code regions (`DSUN.EXE.regions.tsv`, Ghidra's executable blocks from the plain
  MZ import) start exactly at 62 of the 81 non-empty flags-1 spans, so the regions were not
  formed from this table.

## Interpretation

`unk_06` is the offset of the first byte a segment occupies in the load image and `unk_02` the
offset just past its last byte, both in the segment `segment` names. The linker lays the segments
out in table order, aligned to 2 or 16 bytes and padded with zeros, and their spans cover the
whole load image. A descriptor whose two offsets are equal holds no bytes. Flags-1 segments hold
the resident code, ending with returns, and flags-0 segments hold data, among them the
program's data segment `57E0` (descriptor 218), which FND-EXE-568 reads through. The flags-4
descriptors also hold no bytes, but their end is one less than their start, and they lie inside
the data segment.

## Alternatives

That flags 1 marks code and flags 0 data is read from what the spans hold, not from a
description of the table. Two flags-1 spans that end on other bytes, and three inventory starts
in flags-0 spans, are not explained here. Why the flags-4 descriptors carry an end one less than
their start, rather than equal to it, is not known. The disc's `DSUN.EXE` is not covered
(docs/SOURCE-EDITIONS.md).

## How to reproduce

Run `python -I tools/research/exec-census/segment_table_spans.py <install dir>/DSUN.EXE` from the
commit that adds this finding, with the locked evidence Python. It checks the file, prints the
counts by `flags` and by kind of `unk_02`, and lists every overlap and every gap between the
sorted spans with the byte values in it. The return bytes, the inventory starts and the region
comparison were counted with the same span formula over the same table.
