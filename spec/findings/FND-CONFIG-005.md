---
id: FND-CONFIG-005
title: DSUN.EXE reads sound.cfg through its sound library and warns when it cannot
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:0063..4734:0087
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:B2FF..5000:B30D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:87E7..5000:880A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

`DS` is the data segment `57E0`; `DS:x` is at `5000:x+0x7E00`. Among the data of the sound library,
before the text ` Mel Real Mode Version 2.2.7, 10/14/94 `, are `sound.cfg` at `DS:34FF` and `.ad`
at `DS:3509`. `DS:09E7` holds a message that there is no `sound.cfg` file and that `sound.bat`
should be run, and `DS:0A0B` a message that the library failed, asking whether `CD.DAT` is in the
directory.

- The resident routine at `4734:0063` passes the far pointer `DS:34FF` to the far routine at
  `47B9:0236` and returns what that returns in `DX:AX`. Nothing else pushes `DS:34FF`; `DS:3509` is
  pushed once, at `4734:04DA`.
- Overlay 180, whose resident header is at `56B2:0000`, has a routine from `DSUN.EXE+0x00067206`.
  When the byte at `DS:13F7` is 0 it skips its work. Otherwise it makes a far call to offset
  `0x0063` of the segment its fixup word `0x0240` names, descriptor 72, whose segment is `4734`
  (FMT-EXE-002, FMT-EXE-005), and when `DX:AX` is 0 passes `DS:09E7` to a routine that shows it
  (push at `DSUN.EXE+0x00067227`).

A search of the whole file, ignoring case, finds `sound.cfg` only at `DS:34FF` and inside the
message at `DS:09E7`, and no `sound.ini`. An earlier search in Ghidra for the upper-case
`SOUND.CFG` and `SOUND.INI` found neither.

## Interpretation

The game reads `SOUND.CFG` through its sound library, which returns a far
pointer. The game's zero-pointer branch tells the player to run setup; the
loader's error branch after a buffer was allocated returns an untraced
release routine's result (FND-CONFIG-019). `.ad` is likely the start of
the driver names' extension. The byte at `DS:13F7` turns sound on or off; the load routine tests it
too (FND-SAVE-005). The game does not read `SOUND.INI`. The earlier search missed the name because
it is in lower case.

## Alternatives

FND-CONFIG-019 reads `47B9:0236`: it returns a whole-file buffer on the
visible success path, without parsing or validating the 59-byte layout there.
FND-CONFIG-020 begins the field-consumer reading. The rest of sound-library
initialization and other consumers remain open.
That the loader puts the descriptor's segment in place of a fixup word is an assumption
(FMT-EXE-005); here it gives a call that fits the message after it. What sets `DS:13F7` is not
known; the command-line switches of `RAVAGER.BAT` may.

## How to reproduce

Search the file for `sound.cfg` ignoring case, disassemble `4734:0063` (file offset `0x3C5A3`), and
disassemble overlay 180 from file offset `0x67206`, resolving its fixup words through the segment
table at file offset `0x4B080`.
