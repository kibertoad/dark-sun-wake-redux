---
id: FND-SOUND-016
title: The installed STDPATCH.AD is a directory of 233 six-byte timbre entries ending in two 0xFF bytes and 233 fourteen-byte timbres, and the sound library opens it at setup and reads one timbre by bank and patch number on request
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: claude
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: STDPATCH.AD
    offset: 0x00..0x1236
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:04D9..4734:0583
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47B9:008B..47B9:0236
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 49DE:000F..49DE:00A7
tool: hex inspection with Python 3.14.7; tools/research/exec-census/immediate_search.py, resident_listing.py and direct_callers.py at 92e047c7 with Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

### The installed file

`STDPATCH.AD` is 4,662 bytes. From offset 0 it holds six-byte entries:
a patch byte, a bank byte and a little-endian 32-bit file offset. The
233 entries fill bytes 0 to 1,397 and are followed by the two bytes
`FF FF` at 1,398. The entries name bank 0 with patches 0 to 127, then
bank 127 with patches 35 to 87, then bank 1 with patches 0 to 51, each
patch once, and their offsets rise from 1,400 in steps of 14 to 4,648.
At each offset is a little-endian word 14 followed by 12 bytes, so the
last timbre ends at the end of the file and no byte lies outside the
directory, its terminator and the timbres.

### Where the game opens it

The bytes `stdpatch` are in DSUN.EXE's data segment at `DS:33F2`, with
`.ad` at `DS:3509`. Immediate searches for 33F2 and 3509 find pushes of
them only in 4734:0133, the sound library's setup routine. At
4734:04D9 it appends `.ad` to `stdpatch` through 4A68:0009 (a copy to
the end of the first string). When the word at +0x32 of `SOUND.CFG`
(the far pointer `DS:3411`) is 1 or 2, it builds a path in a local
buffer: the string at 4E71:0BBF followed by `stdpatch.ad` when
1000:40D7 finds that string not empty, and `stdpatch.ad` alone
otherwise. Then, when the word `DS:3444` is not 0 and +0x32 is still 1
or 2, it calls 44DE:0086 with that path and the mode 1, which opens it
read-only through int 21h function 3Dh and returns the handle or -1,
and stores the result in `DS:3415`; -1 calls 49DE:000F with 5. Earlier
in the routine, when the byte `DS:3417` is 1, +0x32 values 1 and 2 are
replaced with 4 (FND-CONFIG-020).

### Where it reads it

An immediate search for 3415 finds the store and test above and one
read, at 47B9:0205 in 47B9:01F5. 47B9:01F5 takes a bank and a patch
number, calls 47B9:008B with the handle, the bank and the patch, passes
the returned far pointer with the bank and patch to 4842:0CDC with the
driver word `DS:343E`, and then releases it through 4AB9:0043. Its
direct callers are far calls at files 3B7F4, 3B858 and 3C231 and a near
call at 3CDF9.

47B9:008B calls 49DE:000F with 0x1F when the handle is 0 or negative.
It seeks to offset 0 through 44DE:0064, then reads six bytes at a time
into `DS:A440` through 44DE:01AF. When the second byte is 0xFF it calls
49DE:000F with 4. When the second byte equals the bank and the first
the patch, it seeks to the entry's 32-bit offset, reads a word, requests
that many bytes through 4AB9:001C (49DE:000F with 0x20 on failure),
stores the word in the block's first two bytes, reads the word less 2
bytes after them, and returns the block. A failed seek or read calls
49DE:000F with 0x1C.

49DE:000F, called as all these calls call it, with the second argument
0x69, prints the message at `DS:350E` with the number, calls 4734:0591
and ends the program through 1000:03DF with the number.

### Other copies

The disc's `STDPATCH.AD` is 5,162 bytes and differs from the installed
file (FND-EXE-570). The bytes `stdpatch` also occur in `SOUND_DS.EXE`
at file 1D1B9; that program was not read.

## Interpretation

The installed `STDPATCH.AD` is a bank of 233 instruments, each found by
bank and patch number through the directory at the start of the file.
The game opens it once when the sound library is set up with word
+0x32 of `SOUND.CFG` at 1 or 2, and reads one instrument at a time when one is
asked for, giving the whole record, its length word included, to the
loaded music driver. A missing file, a missing instrument or a read
error ends the program with the library's error message.

## Alternatives

- What the 12 bytes after each length word mean is decided by the music
  driver that 4842:0CDC hands them to, which is not read here.
- When the four callers of 47B9:01F5 ask for an instrument, and with
  which numbers, is not read here.
- What the string at 4E71:0BBF holds, and what sets `DS:3444` and
  `DS:3417`, is not read here; with the shipped `SOUND.CFG` word +0x32 of
  1 the open is reached only when `DS:3444` is not 0 and `DS:3417` is
  not 1.
- The disc copy belongs to version 1.0 and is not studied.

## How to reproduce

Read `STDPATCH.AD` six bytes at a time from offset 0 until the second
byte is 0xFF, and at each entry's offset read the word and the bytes it
counts. In `tools/research/exec-census/`, with the locked evidence
Python and `<dsun>` the installed DSUN.EXE: `immediate_search.py
<dsun> 33F2`, `immediate_search.py <dsun> 3509` and
`immediate_search.py <dsun> 3415`; `resident_listing.py <dsun>
4734:0133..4734:0591 47B9:008B..47B9:0236 4A68:0009..4A68:0069
1000:40D7..1000:40F0 44DE:0086..44DE:0127 49DE:000F..49DE:00A7`; and
`direct_callers.py <dsun> 47B9:01F5`. Listings are retained under
GAME_DIR/analysis/sound-stdpatch-20261018/. No original function or
game process was run.
