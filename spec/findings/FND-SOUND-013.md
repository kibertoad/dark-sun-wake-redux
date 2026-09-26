---
id: FND-SOUND-013
title: The music routine plays song n as audio track n + 1 of the disc when SOUND.CFG asks for disc music
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4A32:0011..4A32:00E2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:01C4..2660:020B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:0250..2660:02B6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2660:052D..2660:06F8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1E8C..1000:1EF2
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; searches of the whole file for CD 2F and for stores to the word at 57E0:348E
environment: null
---

## Observation

`DS` is the data segment `57E0`; `cfg` below is the record at the far pointer `DS:3411`, whose
word at offset 8 and word at offset `0x14` are `unk_08` and `unk_14` of `SOUND.CFG`
(FMT-CONFIG-001). The installed `SOUND.CFG` has 122 and 11 there.

The far routine at `4A32:0011` takes a word, `n`, and returns nothing:

1. It returns when `cfg` word 8 is `0x71` (113) and bit 1 (value 2) of `cfg` word `0x14` is 0.
2. It returns when the word at `4E71:0B51` is 3 or 5.
3. When bit 1 of `cfg` word `0x14` is set: when the far pointer at `DS:348E` is not 0 it calls it
   with `n + 1`. Then it returns.
4. Otherwise it returns when bit 2 (value 4) of the byte at `DS:217A + n` is 0. Else it clears the
   byte at `4E71:0C1F`, calls `4ABF:006B` with 1 when the byte at `4E71:0C22` is 1, clears the word
   at `4E71:0B56`, calls `47E5:01C4` with `n`, returns when `4E71:0B56` is then 1, and otherwise
   calls `4A32:0310`, calls `4A6E:0064` with the byte at `4E71:0C14`, stores `n` in the byte at
   `DS:3468` and sets the byte at `4E71:0C28` to 1.

Its far callers are at load-image addresses `0x2663A`, `0x266A1`, `0x27A8C`, `0x289AB` and
`0x4A995`, and in overlays at offset `0x0109` of overlay 180 (FND-SOUND-011) and through
`2834:0665` (FND-SOUND-012).

The only stores to `DS:348E` are in `2660:01C4`, which the startup of overlay 180 calls when
`DS:14E3` is not 0. It stores the far pointers `2660:052D`, `2660:05CA`, `2660:0250` and
`2660:0447` at `DS:3486`, `DS:348A`, `DS:348E` and `DS:3492`. It first computes `!w & 2` from `cfg`
word `0x14`, which is always 0, so it stores them whatever the word holds; `2660:052D`,
`2660:0614` and `2660:066C` start with the same test.

- `2660:0250` takes a track number `t`. It returns 0 when the byte at `4E71:0C3F` is 0 or when `t`
  is above the byte at `4E71:0C43`. Otherwise it reads the 32-bit values at offset 2 of entries
  `t` and `t + 1` of a table of 7-byte entries at the far pointer `DS:3482`, and goes on to play
  from them.
- `2660:052D` calls `2660:0614`, sets `4E71:0C3F` to 1, calls `2660:066C`, clears `4E71:0C3F` when
  that fails, and otherwise fills the table at `DS:3482` for each track from the byte at
  `4E71:0C42` to the byte at `4E71:0C43`, and stores the 32-bit value at `4E71:0C44` in the entry
  after the last.
- `2660:0614` passes `0x2F` and a register block with `AX` = `0x1500` to `1000:1E8C`, stores the
  returned `CX` at `4E71:0C35` and `BX` at `4E71:0C39`, and succeeds only when `BX` is 1.
- `2660:066C` sets the byte at `4E71:0C41` to `0x0A` and passes it, with 7 and 3, to `2660:035E`,
  which builds a 13-byte request at `4E71:0C53`.

`1000:1E8C` writes the bytes `55 CD`, the interrupt number, `5D CB` on the stack and calls them.
The file's only `CD 2F` bytes in code are at `15F3:00CC`, `15F3:00D8`, `4AE5:0F40` and `4AE5:0F49`,
each after `MOV AX, 4300h` or `4310h`; the fifth match, at file offset `0x5ECB2`, is inside a table
of words.

## Interpretation

This is the music player. With bit 1 of `unk_14` set, as in the installed `SOUND.CFG`, the game
plays music as compact-disc audio through the CD-ROM extensions: `1000:1E8C` is the C library's
`int86`, `INT 2Fh` with `AX` 1500h asks whether the extensions are loaded and how many CD drives
there are, and `2660:066C` sends the driver the command that reads the disc's first and last
audio tracks and the lead-out address. Song `n` is disc track `n + 1`, so `DJ.DAT`'s songs 1 to 35
are tracks 2 to 36, the Ogg files `Track02.ogg` to `Track36.ogg` (FND-SOUND-002), and the song 2
of the startup is `Track03.ogg`. Without that bit the game plays songs through its FM or MIDI
driver, only those whose byte at `DS:217A` has bit 2 set. The four literal `INT 2Fh` calls are the
extended-memory driver checks.

## Alternatives

How `2660:0250` starts play and how the requests reach the driver were not read. That the table
entries hold each track's start address, and so that a song plays to the start of the next track,
is read from their use. Whether the CD handlers are installed at all when `DS:14E3` is 0, and so
whether music plays then, was not checked: `DS:348E` is 0 in the file. What `4E71:0B51` holds was
not read.

## How to reproduce

Disassemble `4A32:0011`, `2660:01C4`, `2660:0250`, `2660:052D` to `2660:06F8` and `1000:1E8C`,
and search the file for `CD 2F` and for `C7 06 8E 34`.
