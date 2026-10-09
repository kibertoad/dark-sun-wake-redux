---
id: FND-CONFIG-063
title: Display-bound reads feed mouse range services
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
instructions. The far calls target `45B9:00BB` and `45B9:00D3`, the
mouse-driver wrappers identified in FND-INPUT-010. The first places 7
in `AX` and the two by-value arguments in `CX` and `DX` before `INT 33h`;
the second does the same with service 8. Neither wrapper writes to a
display-bound word or receives its address.

## Interpretation

Neither raw hit is a literal-address write to a display bound. Combined
with FND-CONFIG-036, every raw address-word occurrence in the surveyed
resident load image and declared overlay code now has a bounded
classification: the known graphics-initializer writes or reads. These two
reads pass the initialized ranges to the mouse driver; the intervening
executable code does not change either bound.

## Alternatives

An indirect or block write elsewhere, code loaded outside the surveyed
ranges, or an effect inside the mouse driver remains possible. The static
reading of the `INT 33h` wrappers does not establish the bounds' values
at each message call or the driver's behavior.

## How to reproduce

Disassemble the approved `DSUN.EXE` from the branch target at
`0x0002F0E8` through `0x0002F110` in 16-bit mode. Classify the raw
little-endian address words at `0x0002F0F2` and `0x0002F102` by their
containing instructions. Apply the MZ relocation to the two far calls,
then disassemble the wrappers at physical file offsets
`0x0003AE4B..0x0003AE7B`; compare their services with FND-INPUT-010 and
the full raw-hit inventory in FND-CONFIG-036.
