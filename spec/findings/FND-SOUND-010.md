---
id: FND-SOUND-010
title: Command-line switches turn sound, digital sound and speech on or off and set the install type
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0024..277B:023F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:05A1..277B:05D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:13F6..57E0:14E8
  - build: BLD-GOG-EN-1.1
    file: RAVAGER.BAT
    offset: 0x00..0x0C
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

`DS` is the data segment `57E0`. The far routine at `277B:0024` takes the argument count and the
argument vector. For each argument from the second on it lowers the second character, subtracts
`'a'`, and when the result is 0 to 25 jumps through the table of 26 words at `277B:05A1`. The
switches that change the bytes this area reads are:

| Switch | Effect |
|---|---|
| `-H` | sets the bytes at `DS:14E8`, `DS:14E3`, `DS:13F7` and `DS:14E4` to 1 |
| `-L` | sets `DS:14E3`, `DS:13F7` and `DS:14E4` to 1 (it enters the `-H` code after its first store) |
| `-M` | sets `DS:13F7` to 0 |
| `-P` | sets `DS:14E4` to 0 |
| `-I` | sets `DS:13F6` to 0 |
| `-W` | converts the rest of the argument to a number and stores it in the byte at `DS:55BC`; when it is below 0 or above 7 it passes `DS:06C1`, "Error: invalid install type, reinstall game to specify correct type.", to the far routine at `56B2:0034` |

`-K` compares the rest of the argument with `911` at `DS:06BD` and, when they match, sets the byte
at `DS:143C` to 1. Several switches do something only when `DS:143C` is not 0; among them `-R`
stores a number in the word at `DS:140C`, and `-X` and `-Y` store 16 times a number plus 8 in the
words at `DS:140E` and `DS:1410`.

In the file, `DS:13F7`, `DS:13F6`, `DS:1435`, `DS:1436` and `DS:1439` hold 1; `DS:14E3`,
`DS:14E4`, `DS:14E8` and `DS:143C` hold 0; the word at `DS:140C` holds 50. `RAVAGER.BAT` runs
`DSUN -W0 -L`.

## Interpretation

`DS:13F7` turns all sound on; `-M` turns it off. `DS:14E3` turns digital sound on, the sound
effects and speech that the routines of FND-SOUND-007 and FND-SOUND-008 play and wait for, and
`DS:14E4` turns speech on; `-P` turns speech off. `-H` and `-L` differ only in `DS:14E8`, which
makes the speech routine touch the disc after each line. The GOG build runs with sound, digital
sound and speech on, and install type 0. The install type decides which files the game deletes
from the installation; with bit 2 set it deletes the speech files (FND-SOUND-006). `-K 911` unlocks switches for testing, among them a start region (50 is
`RGN032.GFF`) and a start position.

## Alternatives

The names "high" and "low" for `-H` and `-L` are guesses. What `DS:13F6`, which `-I` clears, does
is only partly read: the cinematic player tests it before it copies a file from the disc. The
switches not listed here were not examined.

## How to reproduce

Disassemble `277B:0024` to `277B:023F`, read the 26 words at `277B:05A1`, and read the bytes above
at file offset `0x4D000` plus the address.
