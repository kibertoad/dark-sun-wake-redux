---
id: FND-TEXT-006
title: Ghidra finds no direct reference to the Preferences tables or their strings
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:A4B9..5000:A6CA
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

In the block of FND-TEXT-005, Ghidra records no direct reference to the difficulty pointer table
at `5000:A4B9`, to the first difficulty label at `5000:A4F5`, or to the first About line at
`5000:A5CC`. No decoded instruction has the scalar operand `0xA4B9` or `0xA4F5`, nor `0xA523`,
which lies inside the pattern at `5000:A521`.

A byte search of the resident load image finds 31 strings that start with `%C%C%C`, each right
after a NUL; the nine About lines are among them. A Ghidra search for the same prefix listed 30
strings, including all nine About lines, and none with a direct reference.

## Interpretation

No code that Ghidra recovers names these tables or strings by a literal address, so the code
that draws the Preferences screen is not found this way.

## Alternatives

The pointer tables are relocated far pointers, so code can reach them through a segment and an
offset built at run time, through a pointer stored elsewhere, or from overlay code, none of which
Ghidra records as a direct reference. Which of the 31 strings the Ghidra search left out was not
checked.

## How to reproduce

Import `DSUN.EXE` into Ghidra with the MZ loader at segment `0x1000` and run the full analysis.
Run `ReportReferences` on `5000:A4B9`, `5000:A4F5` and `5000:A5CC`, `ReportScalarConstants` for
`0xa4b9`, `0xa4f5` and `0xa523`, and `ReportBytePattern` for `25 43 25 43 25 43` with references.
Search the resident part of the file, `0x5200..0x57570`, for the same six bytes.
