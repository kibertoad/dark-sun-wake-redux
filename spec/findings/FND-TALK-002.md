---
id: FND-TALK-002
title: GPL 135 opens with portrait 18 and a menu of eight entries titled by global string 4, which MAS 99 assigns with strings 5 and 6
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x7C345..0x7D361
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x1FEAA5..0x1FEC40
tool: hex inspection with Python 3.14.7, instructions and expressions read as RULE-SCRIPT-002 and RULE-SCRIPT-004 describe
environment: null
---

## Observation

`GPLDATA.GFF#GPL/135` (4,124 bytes) and `#MAS/99` (411 bytes), read from their start with the
instruction and expression encoding of FMT-SCRIPT-001 and RULE-SCRIPT-004. Offsets are from the
start of each resource.

`GPL/135`:

- Offset 16 is instruction `0x54` with the byte 18. Offset 19 loads the accumulator with global
  number 22 compared equal (`0xD7`) to 1, and the `if` at 24 calls the local subroutine at 52 when
  it holds and the one at 610 when it does not.
- Instruction `0x4F` at 118 and at 199 each print a string, under conditions read before them.
- Offset 253 is instruction `0x48`. Its title is global string 4. Its eight entries have strings
  of kind 5 as labels, except entry 7, whose label is global string 5. Their targets are
  literals, and their conditions are: local flags 0, 1, 2 and 3 for entries 0 to 3; local number
  0 equal to 2 (`E2 82 00 D7 8F 02 E1`) for entry 4; local flags 9 and 5 for entries 5 and 6; the
  literal 1 for entry 7. The entries end with `0x4A` at 546. Entry 7's target is 2905.
- Offset 750 is a menu of seven entries and offset 2616 one of seven more, both titled by global
  string 4. Each has local flags as the conditions of its first six entries and the literal 1 as
  the last one's. The last entry's label is global string 5 at 750 and global string 6 at 2616.
- Global flag 357 (`CD 01 65`) is named at 611, 711 and 1798 in the parameter of instruction
  `0x18` at 610, 710 and 1797, and at 1821 as the variable of the assignment `0x16` at 1818,
  which stores 1.

`MAS/99` has instruction `0x0A` at offsets 0, 20 and 66. Each names global string 4, 5 or 6 in
that order as the variable (`86 04`, `86 05`, `86 06`) and then gives a string of kind 5. The
string at offset 3 decodes to 16 characters.

## Interpretation

`GPL/135` shows portrait 18, prints one of two speeches and offers the menu at 253. Entry 7 has
a constant condition, so it is always offered, and its label comes from a global string that
`MAS/99` sets. The same holds for the last entries of the two later menus. All three menus share
the title that `MAS/99` puts in global string 4. Instruction `0x0A`, whose handler this spec does
not describe yet (Q-SCRIPT-004), sets a string variable.

## Alternatives

What instruction `0x0A` does is read here from its operands only.

## How to reproduce

Find the two resources through the directory of `GPLDATA.GFF` (FMT-GFF-001), and read the menu at
offset 253 of `GPL/135` as FND-TALK-001 describes the instruction: after `0x48`, one expression,
then three per entry until byte `0x4A`.
