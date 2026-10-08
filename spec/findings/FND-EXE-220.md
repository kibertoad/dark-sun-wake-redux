---
id: FND-EXE-220
title: Gap and reader addresses have no exact four-byte absolute representation in the shipped executable
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600A80..0x00600A8D
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F50A0..0x005F5156
tool: Whole-file physical address-word search
environment: null
---

## Observation

The shipped executable contains no exact four-byte little-endian word
representing any of the 28 preferred addresses from `0x00600A71` through
`0x00600A8C`, inclusive. This covers every byte address in the gap preceding
FND-EXE-166's reader and in its direct body, rather than only its entry.
Each address was searched separately at every possible shipped-file offset,
including unaligned and overlapping positions, without function ownership
or section filtering.

The independent positive control is FND-EXE-165's callback address
`0x005F50A0`, whose stored-target writer is at `0x005F50B8`.
Its exact four-byte word occurs 748 times. No search reaches its 16384-match
limit. The file has 3,802,624 bytes and XXH3-128 fingerprint
`09861838aa3018346f9f15c9a4f5925c`.

## Interpretation

The shipped file does not contain an unchanged absolute four-byte word for
any address in these two intervals. This closes that particular physical
representation search for their interiors. It does not establish that the
reader has no other caller or that FND-EXE-219's gap is unreachable.

The search excludes relative displacements, split or differently encoded
pointers, computed targets, runtime-written pointers, relocation-dependent
representations, alternate instruction streams and external target producers.
An instruction's decoded target can name the reader even when its physical
operand is a relative displacement, as FND-EXE-166 already demonstrates.

## Alternatives

Searching only the reader entry would leave stored interior targets and
gap addresses unchecked. This search covers those values too, but absence
of this one representation cannot eliminate the excluded target forms.

## How to reproduce

Use `tools/ghidra/ReportPhysicalBytePattern.ps1` at revision `44c6502` with
SourcePath naming the licensed build's `DOSBOX/DOSBox.exe` and
MaximumMatches 16384. For each integer from `0x00600A71` through
`0x00600A8C`, inclusive, supply Pattern as its four-byte little-endian
representation. Run the same query for control `0x005F50A0`. Confirm the
source length and fingerprint above before accepting the results. Retain
the complete per-value results locally; neither source nor output is
committed.
