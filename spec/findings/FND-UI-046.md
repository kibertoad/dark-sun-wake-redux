---
id: FND-UI-046
title: Overlay 201 +0073 is the overhead map, opened by Game Menu button 10305 and the O key, which draws the region's map picture with object marks and centres the view on a clicked point
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000897C0..0x00089827
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089833..0x00089BD0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089D3E..0x00089D95
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067EB0..0x00067F43
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068139..0x00068146
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068330..0x0006836D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000796DB..0x00079706
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079DA6..0x00079DE3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079F94..0x0007A024
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007A1C6..0x0007A20C
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py, trampoline_target.py, immediate_search.py and gff_tag_numbers.py at commit 97625dac; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. Overlay 181's code
starts at file 0x67EB0, overlay 190's at 0x78340 and overlay 201's at
0x897C0. FND-UI-044 reads overlay 201 +0073 (trampoline 5778:0020)
up to its party-slot loop: it opens `WIND/19502` with handler
5778:002A (+0000), draws `BMP ` 20120 and then `BMP ` number
`20100 + 25 * current_region + byte DS:4459`, centred in a 186 x 139
area whose corner is (67, 30). Its two direct callers are overlay 181
+028E and overlay 190 +1EBD.

### Caller in overlay 181

Overlay 181 +0480 opens `WIND/10500` (immediate 0x2904 at +04B2, the
only push of that number) with handler 56B9:0034, which is overlay
181 +0000. For event 6 that handler passes its arguments and the far
pointer 56B9:002A (+0612) to overlay 190's key routine +139B, except
for the key word 0x0F09. For event 2 it looks the event's word at
[BP+8] up in fourteen words at +0448 and jumps through the words at
+0464:

| Button | Target | Button | Target |
|---|---|---|---|
| 10300 | +02C0 | 10310 | +0296 |
| 10301 | +00D9 | 10311 | +029A |
| 10302 | +0199 | 10312 | +029E |
| 10303 | +025E | 10313 | +0093 |
| 10305 | +0289 | 11304 | +0302 |
| 10306 | +03F9 | 11305 | +031F |
| 10308 | +03E0 | 11306 | +037E |

+0289 calls +0612 and then overlay 201 +0073. The fourteen numbers are
the buttons of `WIND/10500`, where `BUTN/10305` is the overhead-map
symbol (FND-UI-028).

### Caller in overlay 190

Overlay 190 +139B compares the key word with 33 words at +1F44
(FND-COMBAT-025); none has the low byte 0x4F or 0x6F. An unmatched
key goes to +1A66, which takes the key's low byte ORed with 0x20,
subtracts 0x63 and, for 0 to 0x13, jumps through the words at +1F0C;
for `o` that is +1A82. +1A82 returns when 28C9:2522, which returns the
word at DS:0DAB, gives zero, and when turn_combatant is 4 or more
(signed). With no window in DS:1431 it jumps to +1CBE. With one, it
first runs the window comparisons from +1AA3 and then, at +1C54,
looks the folded key up among `c`, `e`, `i`, `o`, `u` and `v`; for `o`
(+1CA1) it calls 3EBE:071F with DS:1431, 0x2841 (10305) and 4, then
28C9:2A81 with 1. At +1CBE it far-calls the routine its caller passed
at [BP+1E] when that is not null, and jumps through the 20 words at
+1ECC by the same folded index; for `o` that is +1E86. +1E86 reads the
BIOS shift flags through 44B6:0011 (FND-INPUT-011). When bit 0 or 1
is set and byte DS:143C is nonzero it shows the text at DS:1F66
(`Subregions off`) and stores 0, 0, 0x800 and 0x620 at DS:14D7,
DS:14D9, DS:14DB and DS:14DD. Otherwise it calls overlay 201 +0073
(+1EBD).

### The map picture and marks

Before the loop, +0073 stores the picture's two dimension words at
DS:9B02 and DS:9B00, and the differences DS:14DB - DS:14D7 and
DS:14DD - DS:14D9 at DS:9AFE and DS:9AFC (+0165..+01A2). When the
picture request returns nonzero it skips the loop. The loop runs SI
from 0 to 319 over the handle table at 4F49:0C33 and picks a word in
DI, which is 0 before the loop and is not reset per handle:

| Handle | DI |
|---|---|
| kind 2, SI 0 to 3, SI equal to the leader word at 4C13:0369 | 1 |
| kind 2, SI 4 equal to the leader word | 16 |
| kind 2, SI above 4 | 16 |
| kind 2, SI 0 to 4, not the leader | 0 |
| any other kind, byte DS:143C zero | 0 |
| kind 1, DS:143C nonzero | 100 |
| kind 5, DS:143C nonzero | 11 |
| kind 0, 3, 4 or above 5, DS:143C nonzero | unchanged from the previous handle |

For a nonzero DI it takes the words at DS:67C5 and DS:67C7 plus 37
times SI, and goes on only when the first lies within DS:14D7..DS:14DB
and the second within DS:14D9..DS:14DD, both ends included, compared
unsigned. +057E computes `(c * a * 1000 / b) / 1000` from its words a,
b and c, sign-extended, the product taken in 32 bits and both
divisions unsigned, and gives 0 for b zero; the low word is returned.
The mark goes at x = DS:9B06 + 67 + f(DS:9B02, DS:9AFE, first word -
DS:14D7) and y = DS:9B04 + 30 + f(DS:9B00, DS:9AFC, second word -
DS:14D9), where f(a, b, c) is +057E, through 1BF3:2AF4 with the words
1, x, y and DI. After the loop (+02EB) it calls 28C9:2A81 with 1 and
overlay 182 +059C with 0x4A9D (19101) and 1, and releases the picture
buffer.

### The handler's routines

+03C9 does nothing when the dword at 5499:0000 is zero; otherwise it
clears DS:1431, passes that window pointer and 0 to overlay 182 +0128
(trampoline 56BD:0043) and clears 5499:0000. In both cases it then
calls overlay 182 +19F8 and 3D72:0D83. +0000 calls it for event 1,
then stores 0 at exit_control_byte and returns 0xFFFF, and for event
6 with the key words 0x011B, 0x1C0D or 0x3920.

+0317, which +0000 calls for event 2 from button 2099, reads the
pointer position through 45B9:0034 (mouse service 3, FND-INPUT-004)
into two words, the one it tests against x first, and calls +03C9.
When x lies within DS:9B06 + 67 and that plus DS:9B02 (signed low
test, unsigned high test) and y within DS:9B04 + 30 and that plus
DS:9B00, it computes X = f(DS:9AFE, DS:9B02, x - DS:9B06 - 67) +
DS:14D7 and Y = f(DS:9AFC, DS:9B00, y - DS:9B04 - 30) + DS:14D9 and
calls 2C5F:01F3 with X, Y and 1. It returns 0.

### Shipped pictures

`RESOURCE.GFF` holds `BMP ` 20120 and, from 20100 up, map numbers for
current_region 0 (20104 to 20107), 1 (20125 to 20134), 50, 51, 52, 54,
56, 57, 59, 60, 61, 62, 65, 66, 67, 68 and 74 (21350 to 21952), each
`20100 + 25 * region` plus 0 to 24.

## Interpretation

Overlay 201 +0073 is the overhead map (SCR-UI-022). The Game Menu's
button 10305 opens it after the menu's +0612 call, and the O key
(either case) opens it from overlay 190's key routine while the word
at DS:0DAB is nonzero and turn_combatant is below 4; with a Shift key
held and the byte at DS:143C nonzero, O instead turns region bounds
off and opens nothing. The map shows the region picture chosen by
current_region and the byte at DS:4459, with one mark per object
inside the region bounds: the leader, every kind-2 handle above slot
4, and, when DS:143C is nonzero, kind-1 and kind-5 handles. The other
party members are not marked. Escape, Enter and Space close the map;
a click on its button closes it and, inside the picture, scales the
point back to region coordinates and centres the view there through
2C5F:01F3 (FND-COMBAT-049).

## Alternatives

- Reading DI as reset for every handle fails: only the loop's
  initial `xor di, di` writes it before the loop, so a handle of
  kind 0, 3, 4 or above 5 with DS:143C set takes the previous mark
  value.
- Reading 1BF3:2AF4 as a pixel write and DI as its colour is the
  likely reading; the routine is not read here.
- The 25-per-region numbering of the shipped pictures agrees with the
  number formula but is circumstantial; the formula itself comes from
  the code at +0121..+0140.
- What DS:4459 holds while the map is open, what DS:0DAB and DS:143C
  stand for, and what overlay 182 +059C, +0128, +19F8 and overlay 181
  +0612 do are not read here.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the locked evidence Python. List
with overlay_listing.py overlay 201 file 897C0..89BD0 and
89D3E..89D95, overlay 181 67EB0..684C2 and 68340..68380, and overlay
190 796DB..7A20C. Read the words at overlay 181 file 682F8 (14 button
numbers, then 14 targets), at overlay 190 file 7A20C (20 targets),
7A24C (20 targets), 7A234 (6 keys, then 6 targets) and 7A284 (33
keys, then 33 targets). Run direct_callers.py for 201+0073, 201+03C9
and 201+0317, trampoline_target.py for 56B9:0020, 56B9:002A,
56B9:0034, 5778:0020, 5778:002A, 5778:002F, 5778:0039 and 571F:0020,
immediate_search.py for 2904, and gff_tag_numbers.py on the install
directory with tag `BMP `. List 1BF3:2AF4..1BF3:2B60 with
resident_listing.py. Retained listings are under
GAME_DIR/analysis/ui-window-19502-20261018/. No original function or
game process was run and no image was viewed.
