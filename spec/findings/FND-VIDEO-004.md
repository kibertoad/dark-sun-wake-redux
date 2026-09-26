---
id: FND-VIDEO-004
title: The cinematic routine of overlay 187 copies n.FLI from the disc's CINE directory when it is not installed and plays it with song n + 35
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56EF:00FC..56EF:0100
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:18E9..57E0:1938
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4F48:0000..4F48:0005
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; far calls in overlays found through their fixup words and the segment table at file offset 0x4B080
environment: null
---

## Observation

`DS` is the data segment `57E0`. Overlay 187 has its header at segment `56EF` and its code from
file offset `0x6FE30`; the entry at `56EF:00FC` leads to its offset `0x2546`,
`DSUN.EXE+0x00072376`, a routine that takes a byte `n`. In order it:

1. keeps the byte at `DS:4263` (FND-SOUND-008);
2. goes to the fallback below when the byte at `DS:14E3` or at `DS:13F7` is 0;
3. calls offset `0x2B6C`, which stops the music (FND-SOUND-008), and sets the word at `DS:631B`
   to 1;
4. formats `%c:\CINE\%d.FLI` (`DS:18E9`) with `'A'` plus the byte at `4E71:0033` and `n`, the disc
   path, into a local buffer, and `%s%d.FLI` (`DS:18F9`) with the string at `DS:44F2` and `n`, the
   installed path, into `DS:43FB`;
5. goes to step 9 when `56BD:0034` finds the installed path;
6. goes to the fallback when `n` is 1, when the byte at `DS:13F6` is 0, or when `56BD:0034` does
   not find the disc path;
7. rebuilds the installed path in `DS:43FB` by copying `DS:44F2` and appending, through
   `2D40:3DC2` with a limit of 80, the string `1.FLI` at `4F48:0000` after setting its first byte
   to `'0'` plus `n`, and goes to step 9 when `56BD:0034` finds it;
8. calls offset `0x2802` with the disc path. When it returns 1, it rebuilds the installed path and
   calls `4544:0000` with the disc path, the installed path, 0 and 0, then goes to step 9. When
   it returns 0, it calls offset `0x2BAC` with `n`; when that returns 0, or offset `0x2802`
   returns 0 again, it goes to the fallback, and otherwise it goes back to the copy;
9. rebuilds the installed path and goes to the fallback when `56BD:0034` does not find it or when
   `DS:13F6` or `DS:13F7` is 0;
10. sets the byte at `DS:6298` to 1 when `n` is 2 and to 0 otherwise;
11. calls `5755:0043` (FND-VIDEO-002) with the installed path, `n + 35`, the byte at `DS:13F4` and
    a delay of 1,000 when `n` is 3, 4,000 when `n` is 5, and the word at `DS:13F2` otherwise;
12. calls `1BF3:2973` with `0x113` (FND-VIDEO-003), `1BF3:4723` with 1 and then with 0, and
    `3D72:0D83`;
13. when the kept byte is not 0, calls offset `0x2B91` (FND-SOUND-008) with 1 when the word at
    `4C10:0019` is 0 and 3 otherwise, and returns.

The fallback calls offset `0x28B3` with `n` (FND-VIDEO-006) and returns.

Offset `0x2802`, `DSUN.EXE+0x00072632`, opens its path through `44DE:0086`, takes the file's
length through `44DE:0127` and closes it, and asks `44DE:04A1` about drive 0; when that returns
`0xFFFF` it passes "Error reading drive information" (`DS:16E1`) to `56B2:0034`. It multiplies the
first two words the call filled in. When the length is below the product it returns 1. Otherwise
it shows "Could not copy game file." (`DS:1902`) and "Need an additional %lu bytes." (`DS:191C`)
with the length less the product through `566A:002A`, and returns 0.

Offset `0x2BAC`, `DSUN.EXE+0x000729DC`, counts the installed files `1.FLI` to `5.FLI`, leaving
out `n` and, when `n` is 5, `3.FLI`. When fewer than two are installed it returns 0. Otherwise,
going through the same files in order, it opens each installed one to take its length, deletes it
through `44DE:02A9`, sets the double word at `4E28:0005` plus 4 times its number to 0, and stops
after the first file whose length is not 0. It returns 1 when a delete succeeded.

In the file, the word at `DS:13F2` holds 2,800 and the byte at `DS:13F4` 107. The four bytes
`98 62` (`DS:6298`) occur in `DSUN.EXE` only in the two stores of step 10.

## Interpretation

This routine plays cinematic `n`. `DS:44F2` is the installation directory and `4E71:0033` the
disc drive (RULE-SOUND-002). The game plays an installed `n.FLI`. When the file is not installed
and `n` is not 1, it copies it from the disc's `CINE` directory, if the disk has room, deleting
another installed cinematic to make room if needed; `1.FLI` is never copied. Each FLI plays with
music song `n + 35`, disc track `n + 36` (FND-SOUND-013), one frame every 107 ms after a wait of
2.8 s, 1 s for `3.FLI` and 4 s for `5.FLI`. Without digital sound, with all sound off, with
cinematics off (`DS:13F6`, which `-I` clears, FND-SOUND-010) or when no file can be had, the
fallback slideshow plays instead. After the FLI the screen goes back to the game's mode and the
music mode is restored as after a spoken line. `DS:631B` set to 1 lets a key end the FLI at once.

## Alternatives

`1BF3:4723`, `3D72:0D83`, `4544:0000` and `566A:002A` were not read, so that they redraw, copy a
file and show a message is inferred. What `44DE:04A1` fills in, and so what the product measures,
was not read: a free-space check would multiply more than two words. What `DS:6298` is for is not
known, since no direct read of it was found.

## How to reproduce

Disassemble overlay 187 from file offset `0x72376` to `0x72631`, `0x72632` to `0x726E2` and
`0x729DC` to `0x72B8A`, resolving far calls through the segment table at `0x4B080`; read the
strings at file offsets `0x4E8E9` to `0x4E938`, `0x4E6E1` and `0x44680`, and the bytes at
`0x4E3F2` to `0x4E3F4`.
