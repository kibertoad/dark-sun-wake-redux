---
id: FND-EXPLORE-004
title: The keypad direction keys step the chosen character one cell in eight directions, or in combat act on the object whose area holds a blocked cell
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:10B6..28C9:1131
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:01CB..28C9:0353
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2535..28C9:254C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:29F7..57E0:2A08
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; far calls in overlays found through their fixup words and the segment table at file offset 0x4B080; Python byte searches of the whole DSUN.EXE
environment: null
---

## Observation

`DS` is the data segment `57E0`. The key routine at `28C9:0CFF` sends Up, PgUp, Right, PgDn, Down,
End, Left and Home to the handlers at `0x10B6`, `0x10BA`, `0x10BF`, `0x10C4`, `0x10C9`,
`0x10CE`, `0x10D3` and `0x10D8` (FND-AI-004), which set a direction `d` of 0 to 7 in that order
and go on at `28C9:10DB`:

1. When the word at `DS:1440` is not 1, it calls `28C9:2535(1)`, which stores 1 in the words at
   `DS:4596` and `DS:1440` and calls `2C5F:0C8D(1)`.
2. With `c` the word at `DS:426D`, it takes the words at `0x0A` and `0x0C` of slot `c`'s record,
   each shifted right by 4, and adds the signed bytes `DS:29F7 + d` and `DS:2A00 + d`, which hold
   0, 1, 1, 1, 0, -1, -1, -1 and -1, -1, 0, 1, 1, 1, 0, -1.
3. It calls `28C9:01CB(c, tx, ty)` and then `2C5F:0182()`.

`28C9:01CB(c, tx, ty)`:

1. When `c` is below 4 it calls `25AF:0648`, `25AF:00E1(tx, ty)` and `25AF:0692`; otherwise
   `25AF:00E1(tx, ty)` alone (FND-EXPLORE-001).
2. When the cell is blocked and the word at `4C10:0019` is 0, it returns 0. When it is blocked and
   that word is not 0, it visits the slots 5 to 47 whose byte at `4F49:0C33 + slot * 3` is 2,
   calls `2D40:3440` for each (FND-EXPLORE-002), and for the first slot whose words at `0x0A` and
   `0x0C`, shifted right by 4, are within `size` columns and `cut` rows of `(tx, ty)`, calls
   `28C9:305F(c, slot, 1)` and returns 1; when none is, it returns 0.
3. When the cell is open, it stores `tx`, `ty` and 15 in the words at `0x0`, `0x2` and `0x4` of
   the 19-byte record at `4F49:08A3 + c * 0x13`. When the word at `4C10:0019` is 0 and `c` is below
   4 it calls `25AF:0648`, `2D40:10AE(c, 1)` and `25AF:0692`; otherwise `2D40:10AE(c, 1)` alone.
   It sets the byte at `4E71:0000` to 1 and returns 1.

## Interpretation

The eight keys of the numeric keypad move the character in `DS:426D` one cell up, up and right,
right, down and right, down, down and left, left, and up and left, as the manual says the keypad
moves the characters (SRC-MANUAL-1994, page 4). Outside combat the party's own cells are taken off
the map while the target is tested and while the move starts, so members do not block each other.
In combat a blocked step toward another object's area goes to `28C9:305F`, which reads as the
attack the Walk pointer makes on a click (RULE-COMBAT-006). The key also sets the input mode in
`DS:1440` to 1 (FND-COMBAT-013).

## Alternatives

`2D40:10AE` beyond its start, `28C9:305F`, `2C5F:0182`, `2C5F:0C8D` and the use of the 19-byte
records and of `4E71:0000` were not read, so that the move and the attack happen is inferred.
Which BIOS key service delivers the key words, and so whether the grey arrow keys of an enhanced
keyboard match the keypad words, was not checked. The area test uses the low four bits of the
details byte for columns and the high four for rows, while the footprint routine uses them as a
size and a cut.

## How to reproduce

Disassemble `28C9:10B6` to `28C9:1139`, `28C9:01CB` to `28C9:0353` and `28C9:2522` to `28C9:254C`
with the relocations applied, and read the 16 bytes at file offsets `0x4F9F7` to `0x4FA07`.
