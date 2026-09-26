---
id: FND-UI-015
title: DSUN.EXE does not contain the name shown in the first hostile Look panel as ASCII bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..6237:0000
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The Look panel captured in FND-UI-018 shows a six-letter name for its target. A byte search of
all memory Ghidra loads from `DSUN.EXE` found neither the name's six ASCII bytes nor the same
bytes followed by a NUL. A byte scan of the whole 634,416-byte file, which covers the 276,672
bytes after the 357,744-byte load image that Ghidra does not load, found neither pattern. A
search for the NUL-terminated form in the copy of `DSUN.EXE` that loads the overlay code
(FMT-EXE-001) found none either. The name itself is not copied here.

## Interpretation

The game does not take this name from a string in its executable, in this spelling and encoding.

## Alternatives

The name may come from a data file, from a script, or from text built at run time, or be stored
in another case or encoding.

## How to reproduce

Search the loaded memory and the whole file of `DSUN.EXE` for the name as captured, with and
without a trailing NUL.
