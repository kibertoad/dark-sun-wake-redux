---
id: FND-CONFIG-063
title: The two previously undecoded display-bound address hits are reads
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:01E0
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of a bounded resident control-flow window; raw address-word inspection
environment: null
---

## Observation

FND-CONFIG-036 left two raw loaded-image occurrences of the display-bound
addresses unclassified because the existing Ghidra import had no
containing instruction. In the resident path beginning at physical file
offset `0x0002F0E8`, bounded 16-bit disassembly shows:

| Raw address-word occurrence | Containing instruction and use |
|---|---|
| `0x0002F0F2`, the word `A039` | The instruction beginning at `0x0002F0F0` pushes the word read from `DS:A039`, followed by a push of `DS:A035` and a far helper call at `0x0002F0F8`. |
| `0x0002F102`, the word `A03B` | The instruction beginning at `0x0002F100` pushes the word read from `DS:A03B`, followed by a push of `DS:A037` and a far helper call at `0x0002F108`. |

The preceding path copies the double word at `DS:A145` to `DS:A14D`
when a local helper returns zero, then reaches these two read
instructions. The call targets' effects are not established here.

## Interpretation

Neither raw hit is a literal-address write to a display bound. Combined
with FND-CONFIG-036, every raw address-word occurrence in the surveyed
resident load image and declared overlay code now has a bounded
classification: the known graphics-initializer writes or reads.

## Alternatives

An indirect or block write, code loaded outside the surveyed ranges,
or a called helper that changes a bound through another path remains
possible. These two read instructions do not establish the bounds'
values at each message call.

## How to reproduce

Disassemble the approved `DSUN.EXE` from the branch target at
`0x0002F0E8` through `0x0002F110` in 16-bit mode. Classify the raw
little-endian address words at `0x0002F0F2` and `0x0002F102` by their
containing instructions, then compare the full raw-hit inventory in
FND-CONFIG-036.
