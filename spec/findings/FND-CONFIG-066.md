---
id: FND-CONFIG-066
title: Literal uses of the startup resource-archive pointer stay in startup
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0000
tool: Python 3.14.7 bounded raw-word search of the shipped executable; Capstone 5.0.7 16-bit disassembly of bounded overlay windows
environment: null
---

## Observation

The startup resource-archive handle occupies a far pointer at `DS:1442`
(FND-CONFIG-039). A raw search of the complete shipped `DSUN.EXE` for the
little-endian address word `42 14` finds nine occurrences:

| File offset(s) | Classification |
|---|---|
| `0x4B4B9`, `0x4B4D1`, `0x4B4E1`, `0x4B4E9`, `0x4B4F1` | Resident data before the `FBOV` envelope, in a repeated eight-byte table; not decoded instructions. |
| `0x65AEA` | Bytes cross the displacement and immediate fields of a comparison against a different word; not a `DS:1442` reference. |
| `0x67600` | Overlay 180 startup clears the far pointer at `DS:1442`. |
| `0x6760B` | Startup passes the pointer's address to the archive-open wrapper as an output location. |
| `0x676DC` | Startup passes the returned pointer value to the graphics initializer. |

The three instruction references are in the same startup path. This raw
inventory finds no later literal-address read or write of `DS:1442` in
the resident image or declared overlay code.

## Interpretation

The direct startup handle is initialized and handed to graphics setup, but
the surveyed executable has no later literal use of that address to close
or replace the archive. This narrows the direct-handle path in
Q-CONFIG-008; it does not prove that the archive stays open or reachable
through the reader's active-list pointer at `DS:9D9F` (FND-CONFIG-037).

## Alternatives

A copied handle, computed pointer, block write, indirect close call or
close-all path may change the archive list without encoding `DS:1442` in
an instruction. This inventory also does not decide I/O outcomes during
resource acquisition.

## How to reproduce

Search the approved `DSUN.EXE` for the two-byte pattern `42 14` and classify
all nine hits. The MZ image ends at `0x57570`; inspect the repeated
eight-byte resident data records around the five `0x4B4xx` hits.
Disassemble `0x65AE7..0x65AF1` to see the
instruction-boundary coincidence, and `0x675FD..0x676E9` for the three
startup references. Compare the archive-open and graphics-handoff calls
with FND-CONFIG-039.
