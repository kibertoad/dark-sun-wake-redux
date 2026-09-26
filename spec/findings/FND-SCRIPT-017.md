---
id: FND-SCRIPT-017
title: Overlay 187 converts script entry points in the trigger records to GPLI entry numbers and back
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56EF:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:5AF5..57E0:5AF7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4F49:08A3
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

Overlay 187 has its resident header at `56EF:0000` and its code from file offset `0x6FE30`. Its
segment fixup words resolve through the segment table at file offset `0x4B080`: `0x0128` to
`38FF`, `0x0388` to `4F49`, `0x06D0` to `57E0`, `0x01D0` to `444C` and `0x05A0` to overlay 180.
The two routines below hold the four `GPLI` tag pushes of FND-EXE-006.

The far routine at `DSUN.EXE+0x00071445` takes one byte argument. It:

1. calls `38FF:05B5` with tag `GPLI`, number 1 and a local size; on failure, or when the size is
   0, it far-calls overlay 180's offset `0x0034` with `0x180B` or `0x181B` and returns;
2. reads the resource through `38FF:04AB` into a buffer, and counts its records as size / 6;
3. walks the list of 19-byte records from the head `57E0:5AF5`, each at `4F49:08A3 + 19 * index`,
   through the signed byte at offset 18, until `-1`. For each record and each of its two pairs j
   (offset word at `8 + 2j`, script word at `12 + 2j`), it takes the first `GPLI` record, in file
   order, whose second word equals the offset and whose third word equals the script, and stores
   that record's first word in the offset word;
4. when its argument is not 0, does the same for each of the 200 13-byte records at the far
   pointer `57E0:40C0` whose word 2 is not 0, comparing word 0 with the second word and word 2
   with the third, and storing the first word in word 0;
5. frees the buffer through `444C:0092`.

The far routine at `DSUN.EXE+0x00071659` loads the resource the same way and does the reverse:
for a 19-byte pair whose offset word equals a `GPLI` record's first word, it stores the record's
second word in the offset word and its third in the script word; for a 13-byte record with word 2
not 0 whose word 0 equals a first word, it stores the second word in word 0 and the third in word
2.

## Interpretation

The trigger records hold script entry points as an offset in a `GPL ` resource and its number
(FND-SCRIPT-014, FND-SCRIPT-016). The first routine replaces them with the stable entry numbers of
`GPLDATA.GFF#GPLI/1` (FND-SCRIPT-002), and the second restores them. An entry point that no `GPLI`
record names keeps its offset. That the game does this around saving and loading, so that a saved game does not depend
on script offsets, is the likely purpose; the callers were not traced.

## Alternatives

In the reverse routine a converted 13-byte record keeps a script word that is not 0, so the test
on word 2 does not tell a converted record from an unconverted one; an offset that equals some
entry number would be converted again. What the byte argument selects was not traced beyond the
13-byte records.

## How to reproduce

Place overlay 187 with `tools/ghidra/ReportFbovOverlayMap.ps1`, resolve its fixup words through
the segment table, and disassemble from file offset `0x71445` to `0x71659` and from `0x71659` to
the next `retf`.
