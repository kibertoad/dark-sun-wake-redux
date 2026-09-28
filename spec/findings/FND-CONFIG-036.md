---
id: FND-CONFIG-036
title: Literal references to the display bounds outside initialization are reads
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0009
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:02B3
tool: Python 3.14.7 bounded raw-word search of the MZ load image and 49 FBOV code blocks; Ghidra 12.1.3 ReportReferences.java and ReportInstructionContext.java; Capstone 5.0.7 16-bit disassembly
environment: null
---

## Observation

The graphics initializer writes the display-bound words `DS:A039` and
`DS:A03B` on each of its three mode branches (FND-CONFIG-033). Searching the
raw loaded MZ bytes for their respective little-endian address words finds
12 and 11 occurrences. Six are those initializer assignments, three for
each bound. Ghidra decodes eight other `A039` occurrences and seven other
`A03B` occurrences as reads. They occur in the initializer, window
registration and other resident graphics routines. None of those decoded
instructions writes a bound.

One raw occurrence per word, at `DSUN.EXE+0x0002F0F2` and
`DSUN.EXE+0x0002F102`, has no containing instruction in the existing
Ghidra analysis. Bounded disassembly from the preceding branch target
classifies both as word reads passed by value to helpers (FND-CONFIG-063).
The resident inventory therefore has three initializer writes and nine
reads of `DS:A039`, and three initializer writes and eight reads of
`DS:A03B`. Neither address word occurs in the declared code bytes of
any of the 49 FBOV overlays.

Ghidra's direct-reference query reports the decoded reads but omits the
initializer's known writes; bounded physical disassembly at
`0x0002EF8A..0x0002EFB2` confirms those writes. The Ghidra reference list
alone is therefore insufficient for a negative writer finding.

## Interpretation

No literal-address write outside the initializer was found in
the surveyed code. With the initialized values, the `WIND/10501` bounds
test passes (FND-CONFIG-033). This does not establish that the bound words
retain those values at every message call.

## Alternatives

An indirect or block write, or code loaded outside the surveyed MZ and
FBOV ranges could change a bound.
The search does not settle those possibilities or the separate resource
acquisition gate (FND-CONFIG-034).

## How to reproduce

For the approved `DSUN.EXE`, scan the MZ load image and each code range
named by FMT-EXE-003 for the raw two-byte patterns `39 A0` and `3B A0`.
Inspect each resident hit as an instruction reference, using Ghidra's
instruction context where decoded and bounded 16-bit disassembly for the
initializer's three assignments at `0x0002EF8A`, `0x0002EF98` and
`0x0002EFA6`. Use FND-CONFIG-063 for the two hits that Ghidra left
undecoded. Query `57E0:A039` and `57E0:A03B` with ReportReferences.java
to see why recognized references alone are incomplete.
