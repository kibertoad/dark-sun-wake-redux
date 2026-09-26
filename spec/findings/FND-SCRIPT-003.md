---
id: FND-SCRIPT-003
title: Overlay 180 opens GPLDATA.GFF through the name at 57E0:0AF1 and reports it missing through the pattern after it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:0AF1..57E0:0B20
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The only occurrence of the bytes `GPLDATA.GFF` in `DSUN.EXE` is at file offset `0x4DAF1`, in
the resident image at `57E0:0AF1` (Ghidra's mapped address `5000:88F1`), followed by a NUL. The
next string, at `57E0:0AFD`, is a message pattern with two `%Fs` conversions, a file and a path, that says the file was
not found.

The routine of overlay 180 (resident header `56B2:0000`, code from file offset `0x671E0`) that
holds these references:

1. stores 0 in the double word at `57E0:1446`;
2. at `DSUN.EXE+0x000674A6` pushes `0x0AF1` with `DS` and the address `57E0:1446`, and far-calls
   the routine of fixup word `0x05B0` (descriptor 182, overlay 182) at offset `0x002F`;
3. when that returns 0 in `AL`, formats the pattern at `57E0:0AFD` with the string at `57E0:44F2`
   and `57E0:0AF1` (the push of `0x0AF1` is at `DSUN.EXE+0x000674BA`) into a local buffer, and
   passes it to a routine of its own overlay.

`57E0` is the program's data segment: the entry at `1000:0000` loads it into `DS`.

An earlier Ghidra reading found no direct reference to the name and no far pointer
`F1 88 00 50` to it in the loaded image.

## Interpretation

Overlay 180 opens `GPLDATA.GFF` in the game's directory (`57E0:44F2`, FND-SAVE-004) and keeps
what the open returns at `57E0:1446`; when the file cannot be opened it reports the missing file.
The script resources the interpreter loads come from this archive. The earlier reading missed the
references because they are in overlay code, which it did not cover.

## Alternatives

The routine at overlay 182 offset `0x002F` was not read; that it opens a GFF archive rests on the
call's arguments and on the message that follows a failure. What the routine of step 3 does after
formatting the message was not read.

## How to reproduce

Search `DSUN.EXE` for `GPLDATA.GFF`, search overlay code for `push 0x0AF1` (`68 F1 0A`), place
the hits with `tools/ghidra/ReportFbovOverlayMap.ps1`, and disassemble overlay 180 from file
offset `0x6748E` to `0x674DA`.
