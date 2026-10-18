---
id: FND-SAVE-013
title: Script functions 33, 34 and 35 of opcode 0x22 set the subregion bounds, the map page and the defeat picture byte that a GREQ resource saves, and only the start of play and a load also set them
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF29..0x0008BF4E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C1F8..0x0008C203
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C322..0x0008C3F5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068D8F..0x00068DA3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007DA4F..0x0007DA78
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007986C..0x00079882
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007111B..0x00071133
tool: tools/research/exec-census/immediate_search.py and overlay_listing.py at commit 46933af5; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. FND-SAVE-004 and
FND-SAVE-005 show the Save and Load routines of overlay 192 copying the
words at DS:14D7, DS:14D9, DS:14DB and DS:14DD and the byte at DS:4459
to and from a `GREQ` resource, with the byte at DS:4458 kept beside
them. FND-COMBAT-049 shows 2C5F:01F3 keeping the view inside those
four words, and FND-UI-046 shows the overhead map using them as the
region bounds and DS:4459 as part of its picture number.

### Census of the operands

The immediate and displacement decode at every byte of the load image
and of every overlay's code finds these instructions with the operand
word 0x4459:

| Site | Instruction | Routine |
|---|---|---|
| overlay 182 +054A | store 0 | the end of overlay 182 +0365, the start-of-play routine of FND-PARTY-021 |
| overlay 190 +152C | read | the debug branch of FND-CONFIG-044, gated on byte DS:143C, which formats `20100 + 25 * current_region + byte` |
| overlay 192 +044D | read | the Save routine (FND-SAVE-004) |
| overlay 192 +076A | store | the Load routine (FND-SAVE-005), from byte 8 of the `GREQ` buffer |
| overlay 201 +011A, +0121, +012B | compare, store 0, read | the overhead map (FND-UI-046) |
| overlay 204 +0626 | store | case +0623 of overlay 204 +0169 |

and with 0x4458:

| Site | Instruction | Routine |
|---|---|---|
| overlay 192 +0453 | read | the Save routine |
| overlay 192 +0770 | store | the Load routine, from byte 9 of the buffer, which the 9-byte read does not fill |
| overlay 201 +044C | read | the defeat routine +0410, which requests `BMP ` by this signed byte (FND-PARTY-194) |
| overlay 204 +0440, +062F | store | cases +0438 and +062C of overlay 204 +0169 |

The other hits (overlay 204 file 8C3E5, 8C1FF and 8C3EE, overlay 192
file 7D74C, overlay 201 file 89C0D) start inside the instructions
listed. The 201 sites are positive controls located by FND-UI-046 and
FND-PARTY-194.

### Overlay 204's cases

Overlay 204 +0169, the function dispatcher of script opcode 0x22
(FND-PARTY-156), subtracts 1 from its first argument and jumps through
the 53 words at +06C7; its third and fourth arguments are the dwords at
[BP+0A] and [BP+0E]. Words 33, 34 and 35 are +0562, +0623 and +062C;
word 19 is +0438.

- Function 33 (+0562..+0620) divides the third argument by 1000
  (signed 32-bit) and stores the quotient's low word at DS:14D7 and the
  remainder's at DS:14D9, does the same with the fourth argument into
  DS:14DB and DS:14DD, adds DS:14D7 to DS:14DB and DS:14D9 to DS:14DD,
  multiplies each of the four words by 16 (16-bit, low word kept), and
  replaces DS:14D7 and DS:14DB by 2048 and DS:14D9 and DS:14DD by 1568
  where the word is at least that value (signed).
- Function 34 (+0623) stores the third argument's low byte at DS:4459.
- Function 35 (+062C) stores it at DS:4458.
- Function 19 (+0438) stores 1 at DS:13FB and the third argument's low
  byte at DS:4458 before its other work.

The same census for 0x14D7, 0x14D9, 0x14DB and 0x14DD finds stores
only in function 33, the Load routine (+0752..+0764), overlay 190's
`Subregions off` branch (FND-UI-046) and overlay 187 +12EB..+12FD.
The last is in overlay 187 +1132, which on one path shows `Fatal error:
Region change, Invalid save`, and like overlay 190 stores 0, 0, 2048 and
1568. No site outside overlay 182 +054A, the Load routine and function
34 stores to DS:4459.

## Interpretation

The four `GREQ` words are the current subregion: its left, top, right
and bottom edges in map pixels, the bounds the view is kept within and
the overhead map's extent. A script sets them through function 33 of
opcode 0x22, giving two packed numbers `x * 1000 + y`, the corner and
the size in 16-pixel cells, and they never pass 2048 by 1568, the
whole region; overlay 187 +1132, which the region-change text names,
resets them to the whole region. The `GREQ` byte at 0x08 is the overhead map's page,
which picks the map picture within the region; a script sets it
through function 34, the start of play sets it to 0 and the map resets
it to 0 when it is 25 or more. The tenth byte, DS:4458, is the
picture number the defeat window draws; scripts set it through
functions 19 and 35, the Save routine leaves it out of the `GREQ`
resource, and the Load routine fills it from stack bytes the read does
not write.

## Alternatives

- Reading the words as a screen or window rectangle fails: 2C5F:01F3
  compares them with the view's map position, and the map scales
  object positions by them.
- A block copy or a store through a computed pointer that covers
  DS:4458..DS:445A or DS:14D7..DS:14DF is outside the operand census;
  the Save and Load routines reach them only through direct operands.
- Which shipped scripts call functions 19, 33, 34 and 35, and with
  which values, needs the script instruction layouts (Q-SCRIPT-004).

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the locked evidence Python. Run
immediate_search.py for 4459 and 4458, and for 14d7, 14d9, 14db and
14dd, and classify each hit against
listings from instruction starts of its routine: overlay_listing.py
overlay 182 file 68D80..68DA3, 190 796DB..7A20C, 192 7D6FB..7DA85,
201 897C0..89C40, 187 710C0..71140 and 204 8BF29..8BF4E, 8C1E0..8C210 and 8C322..8C400.
Read the 53 words at overlay 204 file 8C487 (code base 8BDC0 plus
0x6C7). Retained listings are under
GAME_DIR/analysis/ui-map-page-20261018/. No original function or game
process was run.
