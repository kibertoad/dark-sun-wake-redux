---
id: FND-CONFIG-069
title: The resident close-all wrapper has no literal direct caller
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47B9:0310
tool: Python 3.14.7 bounded raw-call inventory of the shipped executable; Capstone 5.0.7 16-bit disassembly of the resident wrapper
environment: null
---

## Observation

The resident routine at `47B9:0310` passes `0xFFFFFFFF` to the archive
close entry and handles a nonzero result (FND-CONFIG-041). It begins at
physical file offset `0x3D0A0` and returns with `RETF` at `0x3D0C3`.

A raw search of the complete shipped `DSUN.EXE` for the far-call prefix
and target offset `9A 10 03` finds one occurrence, at `0x3FC9F`. Its
segment word is `0x3A32`, rather than the wrapper's unrelocated
`0x37B9`; the call therefore targets another resident segment.
No literal far-call instruction in the shipped bytes points to this
wrapper, including the declared overlay code. A scan of candidate `E8`
near-call encodings in the resident MZ image finds none whose 16-bit
relative destination is offset `0x0310` in the wrapper's segment.

## Interpretation

The direct calls surveyed here do not provide another route to archive
close-all cleanup during ordinary messages. The exit-callback route to
overlay 180's separate close-all routine remains established by
FND-CONFIG-061. Neither route proves that an archive is removed before
a particular `WIND/10501` request.

## Alternatives

A call formed from a register, memory, a callback table or copied far
pointer, or code loaded outside the surveyed ranges, could still enter
`47B9:0310`. The absence of a direct call does not make the wrapper dead
code and does not establish the archive list at a message call.

## How to reproduce

Disassemble the approved `DSUN.EXE` at `0x0003D0A0..0x0003D0C4`.
Search the full file for `9A 10 03` and inspect the segment word at its
sole hit, `0x0003FC9F`. The wrapper segment `47B9` corresponds to
unrelocated `0x37B9` when the MZ image loads at `0x1000`.
For each `E8` candidate in the MZ image, calculate the signed 16-bit
relative destination within that segment and compare it with `0x0310`.
