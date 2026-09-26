---
id: FND-EXE-007
title: The only resident routine that calls both the DOS seek and read wrappers is a signature-and-length record reader
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47B9:008B
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The resident image has one wrapper that sets up DOS function `42h` (move file pointer) and one
that sets up DOS function `3Fh` (read from file) before `INT 21h`. The seek wrapper takes a
caller's handle, origin and 32-bit offset; the read wrapper reads a caller's buffer in bounded
chunks. The only function that calls both directly is `47B9:008B`. It seeks to offset 0, reads a
six-byte header into resident scratch storage again and again until two 16-bit values in it match
two values its caller passed, seeks to a 32-bit offset taken from that header, reads a 16-bit
length, allocates that many bytes and reads the rest into them. Its seeks use 0 or resident
values. None of its direct callers passes the MZ image end, the `FBOV` marker, the pack length,
the segment-table offset or a segment-table value. It has one direct caller, which passes on two
of its caller's words and a resident file handle. That caller has one caller in the same module
and one outside it, which takes each word from a byte of a loaded structure.

## Interpretation

The routine reads records by signature and length from a file the caller has open. It is not
shown to read the FBOV pack, so the code that loads overlays, reads the segment table and applies
the fixups is still unlocated.

## Alternatives

The overlay manager may read the file through `INT 21h` calls whose function numbers are not set
by a literal near the call, through a runtime library routine outside these two wrappers, or
through code reached only indirectly. None of that was searched.

## How to reproduce

Import `DSUN.EXE` into Ghidra with the MZ loader at segment `0x1000`. Run `ReportDosInt21Services`
to list `INT 21h` calls with a literal `AH=42h` or `AH=3Fh` at most twelve instructions before
them, then `ReportReferences` on the two wrappers and on `47B9:008B`, and read `47B9:008B` in one
decompile window.
