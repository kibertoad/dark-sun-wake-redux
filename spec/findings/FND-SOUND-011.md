---
id: FND-SOUND-011
title: At startup the game reads SOUND.CFG and DJ.DAT, a table of 38 six-byte music records
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
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:043B..2834:0508
  - build: BLD-GOG-EN-1.1
    file: DJ.DAT
    offset: 0x00..0xE7
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; far calls in overlays resolved through their fixup words and the segment table at file offset 0x4B080; hex inspection of DJ.DAT
environment: null
---

## Observation

`DS` is the data segment `57E0`. The routine of overlay 180 at `DSUN.EXE+0x00067206`, whose
resident header is at `56B2:0000`, does nothing when the byte at `DS:13F7` is 0. Otherwise, in order, it:

1. far-calls `4734:0063`, which returns the far pointer of the `SOUND.CFG` data or 0
   (FND-CONFIG-005), and shows the message at `DS:09E7` when it is 0;
2. calls `4AAB:0008` with the 32-bit values 25,000 and 12,000 when `DS:14E3` is 0, and with
   25,000 and 0 when it is not; then calls `49D2:0006` and `49DE:00A7` with `0x48` and `0x5A0`;
3. when `DS:14E3` is not 0, calls `2660:01C4`, then `4734:0128` with 0; otherwise calls
   `4734:0128` with 1. `4734:0128` stores its argument in the word at `DS:3444`;
4. calls `2834:043B`, and when that returns 0 shows the message at `DS:0A0B`, "Wowowowowowow...
   Mel DJ failed in the most atomic way. Is CD.DAT in you dir?";
5. passes the far pointer from step 1 to `4734:00B0`, and calls `4602:000F` when `DS:14E3` is 0;
6. when `DS:13F7` and `DS:1435` are not 0, calls `45E1:0097` with 0 and the byte at `DS:26B5`,
   then calls `49E9:0142` when `49E9:00FD` returns a non-zero byte;
7. when `DS:14E3` is 0, calls `4A32:0011` with 2.

`2834:043B` opens `dj.dat` (`DS:08C2`) through `44DE:0086` with mode 1 and returns 0 when the
handle is -1. It reads 3 bytes to `DS:4252`: a count byte, then a word at `DS:4253`. When all 3
arrive it allocates count times 6 bytes through `444C:00FA`, keeps the far pointer at `DS:424E`,
reads count times 6 bytes there, closes the file and returns 1 when all arrive. On a short read it
closes the file, frees the block when there is one, and returns 0.

The installed `DJ.DAT` is 231 bytes: the count 38, the word 1,000, and 38 records of 6 bytes. In
each record the first byte is 255 in 28 records, 1 in 4, 56 in 3 and 66 in 3; the second is 10,
8, 5 or 3; the third is 1, 2 or 3; the word at offset 3 is 1 in 10 records, 2 in 18 and 3 in 10;
the last byte runs from 1 to 35, each value once except 29, 32 and 34, twice each:

| Records | Word at 3 | Last byte | First byte | Second byte |
|---|---|---|---|---|
| 0 to 9 | 3 | 1 to 10 | 255 | 10 |
| 10 to 19 | 1 | 11 to 20 | 255, and 1 in record 17 | 10 |
| 20 to 37 | 2 | 21 to 35 | 255, 56, 66 or 1 | 3, 5 or 8 |

## Interpretation

This is the sound setup at startup. `DJ.DAT` is the music table of the sound library's music
selector, "Mel DJ" (FMT-SOUND-002, FND-SOUND-012); the message's `CD.DAT` names a file that does
not exist in this build. Without digital sound (`DS:14E3` at 0) the game starts music track 2 at
once. `DS:26B5`, `unk_03` of `PREF`, is passed to the library right after the sound-effects
setting `DS:1435` is checked, so it reads as the effects volume (FMT-CONFIG-003).

## Alternatives

The routines of steps 2, 3, 5 and 6 were not read; what the two pairs of numbers and `DS:3444`
mean is not known.

## How to reproduce

Disassemble overlay 180 from file offset `0x67206` (its code starts at `0x671E0`), resolving each
fixup word through the segment table, disassemble `2834:043B` to `2834:0508`, and read `DJ.DAT`.
