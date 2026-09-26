---
id: FND-SAVE-005
title: Overlay 192 loads a game by reading PREF 100 and GREQ nn back into the same globals
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The code is in overlay 192, as FND-SAVE-004 describes, with the same conventions. The routine from
`DSUN.EXE+0x0007D8ED` to `DSUN.EXE+0x0007DA84` takes one 16-bit argument, a slot. It:

1. sets the word at `DS:459B` to 0 when it is 9;
2. shows `LOADING GAME` (`DS:20F3`);
3. calls a far routine with the far pointer held at `DS:6278` plus (the word at `4C4C:0002` plus
   the slot) * 125, the record FND-SAVE-004 fills;
4. reads `PREF` resource 100 into a local buffer through the read entry. When the read returns 0,
   it copies the buffer back to the globals the save routine took it from: the word at `DS:143A`,
   then the bytes at `DS:26B4`, `DS:26B5`, `DS:26B6`, `DS:1435`, `DS:1436`, `DS:1437` and
   `DS:1439`. Then:
   - it calls a far routine with the four words 4, 4, 16, 16 when the byte at `DS:1437` is not
     zero, and 16, 16, 16, 16 when it is;
   - it calls a far routine with the bytes at `DS:1436` and `DS:1435`;
   - when the byte at `DS:13F7` is not zero: if the byte at `DS:1436` is not zero, it calls a far
     routine with the byte at `DS:26B4` and, when the result is less than that byte, copies the
     byte at `DS:26B4` to `DS:26B6`; and if the byte at `DS:1435` is not zero, it calls a far
     routine with the byte at `DS:26B5` and 0;
5. reads `GREQ` resource (the word at `4C4C:0002` plus the slot plus 1) into a second local buffer
   of ten bytes. When the read returns 0, it copies the buffer to the words at `DS:14D7`,
   `DS:14D9`, `DS:14DB` and `DS:14DD` and the bytes at `DS:4459` and `DS:4458`.

The push of `PREF` is at `DSUN.EXE+0x0007D969` and of `GREQ` at `DSUN.EXE+0x0007DA3D`.

## Interpretation

This is the Load Game routine. A saved game's `GREQ` restores four words and a byte of game
state, and `PREF/100` restores the settings. The routine does not check the saved game number
against the number of `GREQ` resources. The tenth byte of the second buffer is never written by
the read of a 9-byte resource, so `DS:4458` receives whatever the stack held there.

## Alternatives

The segment `4C4C` rests on the same reading of the fixup words as FND-SAVE-004. The far
routines were not read; that the calls of step 4 put the loaded settings into effect is a
reading of their arguments. The read entry may read only as many bytes as
the resource holds, as the reading above assumes, or fail for a resource shorter than the buffer;
the entry was not read for this.

## How to reproduce

Place overlay 192 with `tools/ghidra/ReportFbovOverlayMap.ps1`, resolve each fixup word through
the segment table at file offset `0x4B080`, and disassemble from file offset `0x7D8ED` to `0x7DA85`.
