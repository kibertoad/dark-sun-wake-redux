---
id: FND-COMBAT-026
title: The overlay 182 routine behind 56BD:00CA offers GUARD, WAIT and END TURN under the title END name's MOVE, beside the character
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:00CA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:103A..57E0:1066
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

`56BD:00CA` is stub entry 34 of overlay 182 (bytes `CD 3F 22 1C 00`), whose target is the code
offset `0x1C22`, `DSUN.EXE+0x0006A472`. Its only caller found is the Q key handler of
FND-COMBAT-025. The routine takes x and y, and with `c` the word at `57E0:426D`:

1. It formats `END %Fs's MOVE` with the name at offset `0x21` of record `c` of the 49-byte
   records at `57E0:19C9` (FMT-COMBAT-001). When the result, measured through `1000:37D8`, is
   longer than 20 characters, it formats `END MOVE` in its place.
2. When `28C9:000A` returns 0 for `c`, it calls `2C5F:018C` with `c` and 1, and replaces x and y
   with the words at offsets 3 and 5 of record `c` of the 37-byte records at `57E0:67BB`, less the
   words at `57E0:1408` and `57E0:140A`.
3. It calls `566A:0025` with a left edge, a top edge, the title, 1, and the three items `GUARD`,
   `WAIT` and `END TURN`. The left edge comes from the routine at offset `0x07BC` with x, the byte
   at offset `0x13` of the 37-byte record and 146; the top edge from the routine at `0x07DD` with
   y, the byte at offset `0x14` and 68:

   | Routine | Result |
   |---|---|
   | `0x07BC` (x, w, 146) | x - 146 when x + w + 146 is more than 320, otherwise x + w |
   | `0x07DD` (y, h, 68) | 0 when y - 34 + h / 2 is negative; otherwise 199 - 68 when y + 34 + h / 2 is 199 or more; otherwise y + h / 2 - 34 |

   Each `/ 2` adds the sign bit before shifting, so it truncates toward zero.
4. When the result is 1 it calls `5671:0093` with `c` and 1, when 2 the same with `c` and 2, and
   when 3 `5671:0098` with `c` and 0. Any other result calls nothing.
5. In every case it then calls the routine at offset `0x19F8` (FND-COMBAT-008) and `3D72:0D83`.

## Interpretation

This is the end-of-move menu: a menu 146 pixels wide and 68 high placed to the right of the
character's figure, or to its left when it would pass the right edge of the screen, and centred
on the figure's height within the screen. GUARD and WAIT do what the G and W keys do, and END
TURN ends the character's turn. Cancelling the menu leaves the turn with the character.

## Alternatives

The meanings of the bytes `0x13` and `0x14` (the figure's width and height), of the routine at
`28C9:000A` (whether the figure is on screen) and of the results of `566A:0025` rest on the
arguments' use here; those routines were not read. That 146 and 68 are the menu's size is also
inferred from the placement arithmetic.

## How to reproduce

Read the stub entries of the header at `56BD:0020` onward, five bytes each, and disassemble
overlay 182 from its code at file offset `0x68850` at offsets `0x1C22`, `0x07BC` and `0x07DD`.
