---
id: FND-CONFIG-004
title: SOUND_DS.EXE reads sound.ini and writes the 59 bytes of sound.cfg, run by SOUND.BAT
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-CONFIG-213]
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0001..1AF6:0170
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1BD4:0003..1BD4:00A8
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1E36:D630..1E36:D9F8
  - build: BLD-GOG-EN-1.1
    file: SOUND.BAT
    offset: 0x00..0x71
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

`SOUND_DS.EXE` is 204,593 bytes (XXH3-128 `236c2dc23c071eca421eb5b427caee57`), an MZ file whose
load image starts at file offset `0x1400`; its data segment is `1E36`. The disc's copy is the same
file. Its data segment holds, among others, these NUL-terminated strings: `sound.ini` at
`1E36:D630`, after a message about reinstalling, and again at `1E36:D904`, followed by `rb` at
`1E36:D90E`; `sound.cfg` at `1E36:D9EC`, followed by `wb` at `1E36:D9F6`; and ` Version RM 5.0 `.

- The routine at `1AF6:0001` sets the word at `1E36:ECCC` to 1, opens `sound.ini` (`1E36:D904`)
  with mode `rb` through the far routine at `1000:2C11`, keeps the far file pointer at
  `1E36:1888`, returns 0 when the open fails, and otherwise calls the routine at `1AF6:0BF9` and
  goes on to parse.
- The routine at `1BD4:0003` opens `sound.cfg` (`1E36:D9EC`) with mode `wb` through the same far
  routine. When the open fails it prints a message and calls `1000:0357` with 1. Otherwise it calls
  the far routine at `1000:2E28` with the far pointer `1E36:E01B`, 59 (`0x3B`), 1 and the file.
  When that returns 0 it prints a message, closes the file and calls `1000:0357` with 1; otherwise
  it closes the file and returns 1.

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

## How to reproduce

List the strings of the data segment, which starts at file offset `0xF760`, and disassemble
`1AF6:0001` (file offset `0xC361`) and `1BD4:0003` (file offset `0xD143`). Read `SOUND.BAT` as
text.
