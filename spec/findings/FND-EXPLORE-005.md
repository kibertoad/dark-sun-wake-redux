---
id: FND-EXPLORE-005
title: Key 5 sets the byte at 57E0:13FA and brings back the party members key 6 hid, and key 6, outside combat, hides every member but the leader and clears it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0025..56BD:002F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:13FA..57E0:13FB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:006E..277B:0076
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; far calls in overlays found through their fixup words and the segment table at file offset 0x4B080; Python byte searches of the whole DSUN.EXE
environment: null
---

## Observation

`DS` is the data segment `57E0`. The key routine at `28C9:0CFF` calls `56BD:002A` for key 5 and
`56BD:0025` for key 6, each with 1 (FND-AI-004). They lead to the offsets `0x1E81`
(`DSUN.EXE+0x0006A6D1`) and `0x1F1D` (`DSUN.EXE+0x0006A76D`) of overlay 182, whose code starts at
file offset `0x68850`. In both, the leader is the word at `4C13:0369` (FND-COMBAT-023), and a
member counts when it is a party slot 0 to 3 other than the leader, the word at `0x01` of its
37-byte record (FMT-ACTOR-004) is not -1, and bit 3 of byte 5 of the 8-byte entry that word
indexes, in the table at the far pointer `DS:67B7`, is set.

- `0x1E81(redraw)` returns at once when the byte at `DS:13FA` is 1. It sets that byte to 1 and
  calls `28C9:2C9E` with each member and a far pointer to the leader's record. When `redraw` is
  not 0 it calls `2C5F:0271` with 1 when the word at `DS:1440` is 1 and 0 otherwise, then
  `2C5F:0139()` and `28C9:2A81(1)`.
- `0x1F1D(redraw)` returns at once when the word at `4C10:0019` is not 0. For each member whose
  record's first byte has bit 7 clear, it sets that bit, calls `2D40:3388` with the member and its
  words at `0x0A` and `0x0C` shifted right by 4 (FND-EXPLORE-002), and calls `31E0:4129` with the
  member, `0x840` and `0x660` (FND-EXPLORE-003). When `redraw` is not 0 it calls `2C5F:0139()` and
  `28C9:2A81(1)`. It then sets the byte at `DS:13FA` to 0.
- `28C9:2C9E(member, leader)` clears bit 7 of the member's first byte and goes on to routines that
  were not read.

The byte at `DS:13FA` holds 0 in the file. The command-line switch `-A`, entry 0 of the switch
table of FND-SOUND-010, sets it to 1 at `277B:006E`. The party loader clears bit 7 of every party
slot when the byte is not 0, and of the leader's slot alone otherwise (FND-PARTY-013), and
`25AF:0648`, `25AF:0692` and `2D40:32C5` pass over slots with bit 7 set (FND-EXPLORE-001,
FND-EXPLORE-002). The file holds five stores of a constant to the byte, at `277B:006E`, at the two
places above, and at offsets `0x0F35` (1) and `0x0F76` (0) of overlay 187, and no direct store
from a register.

## Interpretation

The byte at `DS:13FA` is not 0 while the whole party is shown, and bit 7 of a party slot's first
byte hides that member. Key 6 hides each shown member other than the leader, frees its cells and
moves it to `(2112,1632)`, whose cell `(132,102)` is the out-of-map cell `(0x84, 0x66)`. Key 5
shows the hidden members again through `28C9:2C9E`, which from its arguments places them near the
leader. The party starts with the leader alone, as the manual says (SRC-MANUAL-1994, page 4), and
key 6 does nothing in combat.

## Alternatives

`28C9:2C9E`, `2C5F:0271`, `2C5F:0139` and `28C9:2A81` were not read, so where key 5 puts the
members and what is redrawn are inferred. What bit 3 of the entry's byte 5 stands for, and which
of the two stores of overlay 187 belong to the Game Menu's Collapse Party, were not read. A store
through a register holding the address would not be found by the byte search.

## How to reproduce

Read the entries at `56BD:0020` onward, five bytes each, and disassemble overlay 182 from file
offset `0x6A6D1` to `0x6A82B`, `28C9:2C9E` to `28C9:2CE2` and `277B:0024` to `277B:0076` with the
relocations applied. Search the file for `C6 06 FA 13`, and for `A2 FA 13` and `88 xx FA 13`, which occur nowhere.
