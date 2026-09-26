---
id: FND-EXE-005
title: Each overlay's fixup list names words in its code that hold a segment-table index times eight
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
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The fixup list of each overlay (FND-EXE-003) is its fixup size divided by 2 little-endian 16-bit
values, 8,262 in all, stored in the payload right after the overlay's code, in the file range
`DSUN.EXE+0x00057580..0x0009AE30`. Every value `v` satisfies `v + 2 <= code size`. The 16-bit word
at offset `v` in the overlay's code has its low three bits 0 in all 8,262 cases, and shifted right
by three it is less than 229, an index into the segment table (FND-EXE-002). Of the descriptors
those indexes name, 2,984 fixups point at one with word 4 equal to 0, 3,196 at one with 1 and
2,082 at one with 3.

The fixup lists sit in overlay code, which has no `segment:offset` address, so this finding is
located at the resident overlay headers that give each list's position and size.

## Interpretation

Each fixup names a place in the overlay's code that holds a segment reference, written as a
segment-table index shifted left by three. Before the code runs, the loader replaces the word with
the segment the descriptor names. Low bits of 0 in every word mean no fixup uses a flag in those
bits.

## Alternatives

The replacement is the reading that makes far calls from overlay code land in the right segment,
and it is what a relocation list in Borland's overlay scheme does, but the code that applies the
fixups has not been located (FND-EXE-007). Whether a segment reference to an overlay resolves to
its resident header or to its loaded code is not shown.

## How to reproduce

For each overlay, read the fixup list after its code, check each value against the code size,
read the 16-bit word at that offset in the code, and tally its low three bits and its value
shifted right by three. `tools/ghidra/New-FbovMappedImage.ps1` applies the same replacement in a
local-only copy and reports how many words had bit 0 set, 0.
