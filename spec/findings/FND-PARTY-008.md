---
id: FND-PARTY-008
title: DSUN.EXE names CHARSAVE.GFF twice, at 5000:9188 and in a message at 5000:9197, with no direct reference
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:9188..5000:9195
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:9197..5000:91A3
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The ASCII bytes of `CHARSAVE.GFF` occur twice in `DSUN.EXE`, both in the resident load image: a
NUL-terminated file name at `5000:9188` (file offset `0x4E388`), and at `5000:9197` inside the
message that starts at `5000:9195` and says the file was not found, with a `%Fs` conversion for
the path. After a full auto-analysis, Ghidra reports no reference to either address.

A query for functions whose scalar operands include all four of 29, 30, 31 and 32 finds none, and
the same query for 40, 41, 42 and 43 finds none. Each number on its own occurs in many unrelated
functions.

## Interpretation

The game opens `CHARSAVE.GFF` by name and reports when it is missing, but the code that does so
reaches the strings in a way Ghidra does not resolve, so these strings do not lead to it.

## Alternatives

The strings may be reached through a pointer built at run time, through a table of names, or
from overlay code, whose references Ghidra does not see in the load image (FND-EXE-006). The
overlay routines that read and write the archive's resources are in FND-PARTY-012. The negative
scalar queries rule out only a function that names all four numbers of either run as constants.

## How to reproduce

Run Ghidra's default auto-analysis on `DSUN.EXE`, then `ReportBytePattern` for the bytes of
`CHARSAVE.GFF`, `ReportReferences` on each match, and `ReportFunctionScalarIntersection` for the
sets 29 to 32 and 40 to 43.
