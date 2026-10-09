---
id: FND-CONFIG-213
title: SOUND_DS.EXE reads sound.ini and writes the 59 bytes of sound.cfg, run by SOUND.BAT
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0001..1AF6:0171
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1BD4:0003..1BD4:00A9
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1E36:D630..1E36:D9F9
  - build: BLD-GOG-EN-1.1
    file: SOUND.BAT
    offset: 0x00..0x72
tool: Capstone 5.0.7 16-bit disassembly and xxhash 4.0.1
environment: null
---

## Observation

`SOUND_DS.EXE` is 204,593 bytes (XXH3-128 `236c2dc23c071eca421eb5b427caee57`), an MZ file whose
load image starts at file offset `0x1400`; its data segment is `1E36`. The disc's copy is the same
file. Its data segment holds, among others, these NUL-terminated strings: `sound.ini` at
`1E36:D630`, after a message about reinstalling, and again at `1E36:D904`, followed by `rb` at
`1E36:D90E`; `sound.cfg` at `1E36:D9EC`, followed by `wb` at `1E36:D9F6`, whose NUL is at
`1E36:D9F8`; and ` Version RM 5.0 `.

- The routine at `1AF6:0001..1AF6:0171` (file offsets `0xC361..0xC4D1`, from `push bp` to the
  `retf` at `1AF6:0170`) sets the word at `1E36:ECCC` to 1, opens `sound.ini` (`1E36:D904`)
  with mode `rb` through the far routine at `1000:2C11`, keeps the far file pointer at
  `1E36:1888`, returns 0 when the open fails, and otherwise calls the routine at `1AF6:0BF9` and
  goes on to parse.
- The routine at `1BD4:0003..1BD4:00A9` (file offsets `0xD143..0xD1E9`, from `push bp` to the
  `retf` at `1BD4:00A8`) opens `sound.cfg` (`1E36:D9EC`) with mode `wb` through the same far
  routine. When the open fails it calls `1000:14BB`, prints a message and calls `1000:0357` with 1.
  Otherwise it calls the far routine at `1000:2E28` with the far pointer `1E36:E01B`, 59 (`0x3B`),
  1 and the file. When that returns 0 it calls `1000:14BB`, prints a message, closes the file and
  calls `1000:0357` with 1; otherwise it closes the file and returns 1.

`SOUND.BAT` (114 bytes, XXH3-128 `7f4161a0e9226e5fb6228ee55b0b0c0f`) turns echo off, prints a line,
copies `D:\*.ADV` into the current directory, runs `SOUND_DS`, and deletes `*.ADV`. The disc's
`SOUND.BAT` is a different file of 4,873 bytes.

An earlier search of the whole file for the upper-case `SOUND.CFG` and `SOUND.INI` found neither.

## Interpretation

`SOUND_DS.EXE` is the sound setup program. It reads the card list from `SOUND.INI` and writes the
choice as the 59-byte `SOUND.CFG` (FND-CONFIG-003), in one `fwrite` of 59 bytes from `1E36:E01B`;
`1000:2C11` is `fopen`, `1000:2E28` `fwrite` and `1000:0357` `exit`. `SOUND.BAT` runs it with the
drivers copied from the disc in drive D. The earlier search missed the names because they are in
lower case.

## Alternatives

The C library routines are named from their arguments and the calls round them; their bodies were
not read. How the parser at `1AF6:0BF9` treats the file is described from the file alone
(FND-CONFIG-007).

This replaces FND-CONFIG-004, whose locations ended on the last item they covered instead of the
byte after it: `1AF6:0170` and `1BD4:00A8` are the routines' closing `retf` instructions,
`1E36:D9F8` is the NUL after `wb`, and `0x71` is the last byte of the 114-byte `SOUND.BAT`. The
documentation check found the second when a valid SOUND_DS function inventory placed a function
ending at `1BD4:00A9`; decoding each range's end found the others. FND-CONFIG-004 also left out the
calls to `1000:14BB` on the two failure paths of the `sound.cfg` routine. Its other observations
are unchanged.

## How to reproduce

Check the size and XXH3-128 of `SOUND_DS.EXE` and `SOUND.BAT`. List the strings of the data
segment, which starts at file offset `0xF760`, and disassemble with Capstone 5.0.7 in 16-bit mode
`1AF6:0001..1AF6:0171` (file offsets `0xC361..0xC4D1`) and `1BD4:0003..1BD4:00A9` (file offsets
`0xD143..0xD1E9`). Read `SOUND.BAT` as text.
