---
id: FND-COMBAT-022
title: The routine at 2C5F:03F1 draws BMP 19003 at (215, 4) and four centred lines, name, hit points, first effect and movement, for the character in 57E0:426D
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:03F1..2C5F:06CB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:0F4D..57E0:0F6A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 571F:0052
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The routine at `2C5F:03F1` takes a page number and a byte flag. It returns at once when the far
pointer at `4E28:00D7` is 0. When the far pointer at `57E0:0F3F` is 0 it requests
`RESOURCE.GFF#BMP/19003` into it through `38FF:04AB` and returns if that fails (FND-COMBAT-004).
It then sets the byte at `57E0:0F3A` to 1, draws the image with its top left corner at (215, 4)
on the given page through `2D40:3BEC`, and takes the image's width through `1BF3:76C2`.

It passes the word at `57E0:426D` to `2D40:3E64`, which fills two indexes, here called `b` and
`a`, and returns a byte. When the byte is 0 the routine stops after the image. Otherwise it draws
four lines of text through `2C5F:09F3`, with the two colours in the words at `57E0:2D14` and
`57E0:2D16`. Each line starts at x = 215 + (image width - text width) / 2, where the text width
is the sum of the glyph widths `2C5F:03B7` returns and the division shifts right after adding
the sign bit, so it truncates toward zero.

| y | Text |
|---|---|
| 6 | The `char[16]` at offset `0x21` of record `b` of the 49-byte records at the far pointer `57E0:19C9` (FMT-COMBAT-001) |
| 12 | When the word at `57E0:426D` is greater than 4 (signed), `???/???` from `57E0:0F4D`. Otherwise `sprintf` with `%d/%d` from `57E0:0F55`, the word at offset `0x00` of that 49-byte record and the word at offset `0x08` of record `a` of the 66-byte records at the far pointer `57E0:19C5` (FMT-COMBAT-002) |
| 18 | When byte `0x14` of the 49-byte record is 1, the routine calls `571F:0052` with the word at `57E0:426D` and keeps the result `w`; otherwise `w` is 0. When `w` is 0, `Okay` from `57E0:0F5B`. Otherwise the string at `4C87:0000 + w * 31` (FMT-COMBAT-003), copied through `1000:406D` |
| 24 | `sprintf` with `Move : %d` from `57E0:0F60` and the word at `4C4F:022E + b * 2` divided by 10 with `idiv`, which truncates toward zero |

Last, when the flag is 1 and the page is 0 or 1, it copies the rectangle from (215, 4) to
(315, 38) through a far call with those coordinates and the page.

`571F:0052` is the stub of entry 10 of overlay 190 (bytes `CD 3F 7C 21 00`), whose target is the
overlay's code offset `0x217C` (`DSUN.EXE+0x0007A4BC`). That routine calls the one at `0x2190`
with the character, 0 and 1. It collects up to 35 bytes through `573B:002A` (with `573B:0025`
first), sorts them with a bubble sort that swaps two neighbours only when the later one's key is
greater, and returns the byte at index 0, or 0 when there are none. The key, the routine at
`0x2267`, is 0 for the values 0, 12, 88 and 93 and for every value above 108, and 1 for every
other value, reached through a jump table of 109 words at offset `0x236E` whose cases all end in
one `inc dx`.

## Interpretation

This is the combat status panel of SCR-COMBAT-001: the name of the character whose turn it is,
the current and greatest hit points (or question marks when the character number is above 4), the
name of the character's first effect or `Okay`, and the movement left, stored in tenths. The
captures agree line for line (FND-COMBAT-018, FND-COMBAT-019).

## Alternatives

That the two words on the second line are the current and greatest hit points follows from
`50/72` in FND-COMBAT-018 and from the manual's hit points; the code does not name them. That
the 35 bytes are the character's active effects follows from the names in the table they index;
the routines behind `573B:0025` and `573B:002A` were not read. Why values above 4 show question
marks, and what `2D40:3E64` maps the character to, is open.

## How to reproduce

Disassemble `2C5F:03F1` to `2C5F:06CB` with the relocations applied. Read the five-byte stub
entries of the overlay header at `571F:0020` onward (`CD 3F` and a code offset), and disassemble
overlay 190 from its code at file offset `0x78340` at offsets `0x217C`, `0x2190` and `0x2267`,
following the jump table at `0x236E`.
