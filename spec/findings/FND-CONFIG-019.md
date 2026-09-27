---
id: FND-CONFIG-019
title: The sound-library file entry loads SOUND.CFG whole without field parsing
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47B9:0236
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of a bounded resident-image range
environment: null
---

## Observation

The routine at `47B9:0236` (`DSUN.EXE+0x0003CFC6`) receives a far
filename pointer. It opens that name in mode 1, obtains the file length,
passes the length to an allocation routine, and, when the returned far
pointer is nonzero, asks the file routine to read that length into the
buffer. It closes the file and returns the buffer pointer in `DX:AX`.
The caller at `4734:0063` supplies the resident `sound.cfg` name
(FND-CONFIG-005).

The routine checks for `0xFFFF` from open and read and for `0xFFFFFFFF`
from length. On those results it passes a nonzero buffer to `4AB9:0043`
and returns that routine's `DX:AX`; when no buffer was acquired it returns
zero. The callee's return value has not been read.
It does not compare a non-error read count with the requested length, check
for 59 bytes, or inspect any `SOUND.CFG` field in this routine. Overlay 180
passes the returned pointer to `4734:00B0` after the missing-file message
path (`DSUN.EXE+0x0006729C`).

## Interpretation

This entry loads a whole file buffer; field interpretation begins in its
consumers, including the initialization path in FND-CONFIG-020. The 59-byte
layout is not validated by this entry itself.

## Alternatives

The called open, length, allocation, read and release routines were identified
from their call pattern and earlier file uses, not completely read here.
Other consumers may validate the length or fields. A short read that does
not return `0xFFFF` is not rejected by this routine's visible branches.

## How to reproduce

Use the approved `DSUN.EXE` and disassemble the resident range
`0x0003CFC6..0x0003D05F` in 16-bit mode. Inspect the caller at
`0x0003C5A3` and overlay 180's pointer handoff at
`0x00067218..0x000672A8`. Do not infer file validation from the callee
names alone; follow this entry's result checks.
