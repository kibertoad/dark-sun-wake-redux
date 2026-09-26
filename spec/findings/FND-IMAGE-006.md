---
id: FND-IMAGE-006
title: The BMP tag is used by a 300-entry BMP or CBMP cache and by a window image request
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:3568
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:3388
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:03F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3CFA:0006
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:B116..5000:B121
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The four ASCII bytes `BMP ` occur six times in the resident load image, at `2000:CA16`,
`3000:5383`, `3000:CFC9`, `3000:D031`, `5000:B116` and `5000:B11D`, and 37 times in the `FBOV`
pack. Three of the resident ones are operands of decoded instructions:

- In `2C5F:03F1`, which has one direct caller, `2C5F:06CB`, and passes the tag to a resource
  request.
- In `31E0:3568`, which has one direct caller, `31E0:3388`. It picks the tag `BMP ` or `CBMP`
  from a flag its caller passes and keeps a cache of at most 300 entries of 16 bytes each, which
  it searches and fills before it requests the chosen tag. `31E0:3388` takes an index from 0 to
  320, reads resident tables at that index and calls `31E0:3568`. It is called from ten places in
  eight functions, among them `31BA:000E` and a guarded call from itself.
- In `3CFA:0006`, which requests `BMP ` for one of two nonzero resident values and then
  continues into a validation path. It has three direct callers, one of them the generic window
  registration routine `3A8E:02B3`.

The occurrences at `5000:B116` and `5000:B11D` (file offsets `0x50316` and `0x5031D`) are data
with no reference recorded by Ghidra. The query did not classify the remaining resident
occurrence or any in the pack. None of the three routines contains the number 11011 as an operand.

## Interpretation

The game loads `BMP ` and `CBMP` images through a cache that treats the two tags alike apart from
the tag, and loads the image a window names through a separate request. This matches the two
tags sharing one layout (FND-IMAGE-001).

## Alternatives

What the resident tables behind `31E0:3388` hold, what owns the cache, and which images or
windows reach either path are not shown. The query did not look at what the requests do with the
bytes they read.

## How to reproduce

Import `DSUN.EXE` into Ghidra with the MZ loader at segment `0x1000` and run the full analysis.
Run `ReportBytePattern` for `42 4D 50 20` over all loaded blocks, then
`ReportInstructionContext` on each decoded match and `ReportReferences` on the functions that
contain them, and read each function and `31E0:3388` in one decompile window of at most 100
lines.
