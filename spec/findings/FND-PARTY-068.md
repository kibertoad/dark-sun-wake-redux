---
id: FND-PARTY-068
title: The generation screen enables the class buttons from an 8-word origin table at 4E68:0000 for the first class and from DATA 1001, by origin, first class and second class, for the second and third, and enables none after three
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006B8FC..0x0006B989
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006BACB..0x0006BBBE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00043880..0x00043890
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x186CB..0x1890B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000958D4..0x00095C7D
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py), and a Python 3.14.7 reading of RESOURCE.GFF through its directory (FMT-GFF-002, FMT-GFF-003, FMT-GFF-007)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Segment `4E68` (segment word `0x0368`, descriptor 109) starts at file `0x43880`.

**Setting the buttons.** Overlay 183 `+091C` (trampoline `56CC:005C`) takes a word. For `k` from 0
to 7 it sets the state of button `0x7D2` plus `k` in the window at `DS:1431` to 1 when bit
`0x80 >> k` of the word is clear and to 0 when it is set, sets the button's state of kind 3, and
draws it (`+0920..+09A4`). Its only calls are in overlay 183 `+0A3E` (FND-PARTY-063), on the
branches for the mask of the class bytes `+08DB` returns (FND-PARTY-065):

- mask 0, no class (`+0AEB..+0B04`): the word at `4E68:0000` plus 2 times the origin byte at
  `+0x12`, less 2;
- mask 4, one class (`+0B0B..+0B2E`), and after moving a lone class forward: the result of
  overlay 185 `+0000` for `DATA` 1001 with the origin byte less 1, the first class byte less 1
  and 0;
- mask 6, two classes (`+0B7B..+0BA3`), and after moving two classes forward: the same with the
  second class byte as the last argument;
- mask 7, three classes (`+0BAF..+0BB2`): 0.

It then calls `+0A0E` with the three class bytes, which calls `+09A9` for each nonzero one; `+09A9`
sets the state of button `0x7D1` plus the code to 0 and its state of kind 2 to 2, the state
`+0E5E` reports as on (`+0BB6..+0BD8`, `+0A13..+0A36`, `+09AD..+0A03`). Overlay 185 `+0000` returns
the signed byte at `DATA` 1001 plus 72 times its second argument plus 9 times its third plus its
fourth (FND-PARTY-058).

**Who reads `DATA` 1001.** Of the twelve calls of overlay 185 `+0000` (FND-PARTY-058), the two in
`+0A3E` push 1001 (`+0B21`, `+0B96`) and the other ten push 1000, the six in overlay 210 among
them (`+0974`, `+09C2`, `+0A0B`, `+0C13`, `+0C66`, `+0D15`). A search of the whole file for 1001
as the immediate of `push`, of `mov` into each of the eight word registers, and of `mov` of a word
to `[bp + disp8]`, `[bp + disp16]` or a direct address finds only those two pushes. Values
computed into 1001 were not searched.

**The origin table**, 8 words at `4E68:0000` (file `0x43880`), one per origin from human; each bit
`0x80 >> k` stands for class `k + 1` in the order of `DS:1164` (Cleric 0x80 to Thief 0x01):

| Origin | Word | Classes |
| --- | --- | --- |
| human | 0x00FF | all eight |
| dwarf | 0x00B5 | Cleric, Fighter, Gladiator, Psionicist, Thief |
| elf | 0x00BF | Cleric, Fighter, Gladiator, Preserver, Psionicist, Ranger, Thief |
| half-elf | 0x00FF | all eight |
| half-giant | 0x00B6 | Cleric, Fighter, Gladiator, Psionicist, Ranger |
| halfling | 0x00F7 | Cleric, Druid, Fighter, Gladiator, Psionicist, Ranger, Thief |
| mul | 0x00F5 | Cleric, Druid, Fighter, Gladiator, Psionicist, Thief |
| thri-kreen | 0x00F6 | Cleric, Druid, Fighter, Gladiator, Psionicist, Ranger |

**`DATA` 1001**, the only resource of that number, 576 bytes at `0x186CB` of `RESOURCE.GFF`: 8
blocks of 72 bytes by origin, each 8 rows of 9 bytes by first class, each byte a mask in the bits
of the origin table. The 37 rows that are not all 0 (the other 27 rows, all eight of human among
them, are 0):

| Origin | First class | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| dwarf | Cleric | 0x24 | 0x00 | 0x00 | 0x04 | 0x00 | 0x00 | 0x20 | 0x00 | 0x00 |
| dwarf | Fighter | 0x85 | 0x04 | 0x00 | 0x00 | 0x00 | 0x00 | 0x81 | 0x00 | 0x04 |
| dwarf | Psionicist | 0xA1 | 0x20 | 0x00 | 0x81 | 0x00 | 0x00 | 0x00 | 0x00 | 0x20 |
| dwarf | Thief | 0x24 | 0x00 | 0x00 | 0x04 | 0x00 | 0x00 | 0x20 | 0x00 | 0x00 |
| elf | Cleric | 0x2F | 0x00 | 0x00 | 0x0D | 0x00 | 0x27 | 0x2B | 0x0D | 0x2E |
| elf | Fighter | 0x8D | 0x0D | 0x00 | 0x00 | 0x00 | 0x85 | 0x89 | 0x00 | 0x8C |
| elf | Preserver | 0xA7 | 0x27 | 0x00 | 0x85 | 0x00 | 0x00 | 0xA3 | 0x85 | 0xA6 |
| elf | Psionicist | 0xAB | 0x2B | 0x00 | 0x89 | 0x00 | 0xA3 | 0x00 | 0x89 | 0xAA |
| elf | Ranger | 0x8D | 0x0D | 0x00 | 0x00 | 0x00 | 0x85 | 0x89 | 0x00 | 0x8C |
| elf | Thief | 0xAE | 0x2E | 0x00 | 0x8C | 0x00 | 0xA6 | 0xAA | 0x8C | 0x00 |
| half-elf | Cleric | 0x2F | 0x00 | 0x00 | 0x0D | 0x00 | 0x27 | 0x2B | 0x0D | 0x2E |
| half-elf | Druid | 0x2D | 0x00 | 0x00 | 0x0D | 0x00 | 0x25 | 0x29 | 0x00 | 0x2C |
| half-elf | Fighter | 0xCD | 0x0D | 0x0D | 0x00 | 0x00 | 0xC5 | 0xC9 | 0x00 | 0xCC |
| half-elf | Preserver | 0xE7 | 0x27 | 0x25 | 0xC5 | 0x00 | 0x00 | 0xE3 | 0x85 | 0xE6 |
| half-elf | Psionicist | 0xEB | 0x2B | 0x29 | 0xC9 | 0x00 | 0xE3 | 0x00 | 0x89 | 0xEA |
| half-elf | Ranger | 0x8D | 0x0D | 0x00 | 0x00 | 0x00 | 0x85 | 0x89 | 0x00 | 0x8C |
| half-elf | Thief | 0xEE | 0x2E | 0x2C | 0xCC | 0x00 | 0xE6 | 0xEA | 0x8C | 0x00 |
| half-giant | Cleric | 0x26 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 |
| half-giant | Fighter | 0x84 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 |
| half-giant | Psionicist | 0xA2 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 |
| half-giant | Ranger | 0x84 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 |
| halfling | Cleric | 0x27 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 |
| halfling | Druid | 0x25 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 |
| halfling | Fighter | 0xC5 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x01 | 0x00 | 0x04 |
| halfling | Psionicist | 0xE3 | 0x00 | 0x00 | 0x01 | 0x00 | 0x00 | 0x00 | 0x01 | 0x22 |
| halfling | Ranger | 0x85 | 0x00 | 0x00 | 0x00 | 0x00 | 0x00 | 0x01 | 0x00 | 0x04 |
| halfling | Thief | 0xE6 | 0x00 | 0x00 | 0x04 | 0x00 | 0x00 | 0x22 | 0x04 | 0x00 |
| mul | Cleric | 0x25 | 0x00 | 0x00 | 0x01 | 0x00 | 0x00 | 0x00 | 0x00 | 0x20 |
| mul | Druid | 0x25 | 0x00 | 0x00 | 0x01 | 0x00 | 0x00 | 0x00 | 0x00 | 0x20 |
| mul | Fighter | 0xC5 | 0x01 | 0x01 | 0x00 | 0x00 | 0x00 | 0x01 | 0x00 | 0xC4 |
| mul | Psionicist | 0xE1 | 0x00 | 0x00 | 0x01 | 0x00 | 0x00 | 0x00 | 0x00 | 0x20 |
| mul | Thief | 0xE4 | 0x20 | 0x20 | 0xC4 | 0x00 | 0x00 | 0x20 | 0x00 | 0x00 |
| thri-kreen | Cleric | 0x26 | 0x00 | 0x00 | 0x04 | 0x00 | 0x00 | 0x22 | 0x04 | 0x00 |
| thri-kreen | Druid | 0x24 | 0x00 | 0x00 | 0x04 | 0x00 | 0x00 | 0x20 | 0x00 | 0x00 |
| thri-kreen | Fighter | 0xC4 | 0x04 | 0x04 | 0x00 | 0x00 | 0x00 | 0xC0 | 0x00 | 0x00 |
| thri-kreen | Psionicist | 0xE2 | 0x22 | 0x20 | 0xC0 | 0x00 | 0x00 | 0x00 | 0x80 | 0x00 |
| thri-kreen | Ranger | 0x84 | 0x04 | 0x00 | 0x00 | 0x00 | 0x00 | 0x80 | 0x00 | 0x00 |

No byte has a bit set for the row's first class or for its own column's class, and no byte of a
row or column for Cleric has the Druid bit, or of one for Druid the Cleric bit. 69 bytes are 0x80
or above.

## Interpretation

On the generation screen the class buttons a character can press next are the classes its origin
may take while it has none, the classes `DATA` 1001 lists for its origin and first class in
column 0 while it has one, and those in the column of its second class while it has two; with
three it can press only its own classes, to remove them. The chosen classes stay pressable. A
human can take any one class and no second, since every human row is 0. The table bounds which
classes may go together far more than any rule of the sources: no Gladiator row has a bit set,
so a Gladiator never has a second class and is offered as a second class by none, and no Fighter
row offers a Ranger. The signed read does not matter, since `+091C` tests only bits 0x80 to 0x01.

Taking classes off and moving the rest forward never leaves an order the tables would not have
offered: going over every order of classes reachable from none by adding an offered class or
removing any one, each origin separately, gives 488 orders, and in each one every class is offered
given the classes before it. Those orders make 9 sets of classes for a human, 13 for a dwarf, 38
for an elf, 49 for a half-elf, 11 for a half-giant, 22 for a halfling, 19 for a mul and 17 for a
thri-kreen; 2, 16, 22, 0, 2, 3 and 3 of the non-human ones, in that order, have three classes.

## Alternatives

- Writes to the words at `4E68:0000` were not searched, and the reading assumes the screen uses
  the shipped values. `DATA` 1001 is requested afresh and freed on each read (FND-PARTY-058), so
  only a change to the resource file would change it.
- This reading covers only what the buttons offer. Whether DONE or another path checks the
  classes again, and the class minimum scores, is not read here.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 183 6B8FC 6B989`,
`183 6B989 6BA1E`, `183 6BA1E 6BC40` and `210 95880 95CA0`; `direct_callers.py <dsun> 183+091C
183+0A0E 183+09A9 185+0000`. Search the file for the bytes `68 E9 03`, `B8`/`B9`/`BA`/`BB`/`BC`/
`BD`/`BE`/`BF` then `E9 03`, `C7 46 xx E9 03`, `C7 86 xx xx E9 03` and `C7 06 xx xx E9 03`.
Read the 16 bytes at file `0x43880`. Read `DATA` 1001 of the installed `RESOURCE.GFF` through
the `GFFI` index of the `DATA` table (FMT-GFF-007) and print each byte as a class mask.
