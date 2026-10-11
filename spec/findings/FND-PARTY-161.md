---
id: FND-PARTY-161
title: The DUAL window offers a class only to a human of level 2 or more with fewer than three classes, a current-class prime requisite of 15, a new-class prime requisite of 17, no class of the same group, and a good alignment for a ranger or true neutral for a druid
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009439E..0x000946B7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000947FC..0x000948C9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00094B07..0x00094C7C
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py; scratch scripts that find the trampolines entering overlay 209 and read the tables at 4E4F:009C, 4E4F:0153 and 55BE:0024 from the file)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 209's trampolines `57B0:0048` and `57B0:003E` enter `+169C` and `+17DC`.
A details record is 0x42 bytes at the far pointer `DS:19C5`, a combatant record 0x31 bytes at
`DS:19C9`.

**The window.** Overlay 209 `+123E` takes a slot `s`. For each class code `c` from 1 to 17 it
calls `+19A7` with `s` and `c`, and stores the result in one of eight flags: index 0 for codes 1
to 4, 1 for 5 to 8, 2 to 5 for codes 9 to 12, 6 for 13 to 16 and 7 for 17, through the jump table
at `cs:1682` (`+1246..+128B`). When no call returns true it shows `NO OTHER CLASSES AVAILABLE`
through overlay 172 `+002A` and opens no window (`+128D..+12A2`). Otherwise it registers `+169C`
as the event handler, builds the window, and for each flag `k` from 0 to 7 that is 0 calls
`3EBE:071F` with button `0x4397 + k` and code 1, which closes the button to presses
(FND-PARTY-077) (`+1526..+1555`).

**The test.** Overlay 209 `+19A7` (`+19A7..+1B1B`) returns 0 unless every step passes, and 1
after the last:

1. The origin byte at `+0x12` of `s`'s details record is 1, a human.
2. With `k` the byte at `4E4F:009D + 2 * x` for the class byte `x` at `+0x1B`, the current class,
   the byte at `+0x19` plus the word at `4E4F:0153 + 3 * k` of `s`'s combatant record is at
   least 15. The same with `x` replaced by `c` gives at least 17.
3. The class byte at `+0x1D` is 0 and the level byte at `+0x1E` is at least 2 (unsigned).
4. The OR of the bytes at `55BE:0024` plus each of the three class bytes shares no bit with the
   byte at `55BE:0024 + c`.
5. For `c` from 13 to 16 the alignment byte at `+0x14` is 1, 4 or 7; for `c` from 5 to 8 it is 5.

`4E4F:009D + 2 * x` is the class's position from 1 (Cleric) to 8 (Thief) (FND-PARTY-083), and
the word at `4E4F:0153 + 3 * k` its prime requisite's position among the six scores
(FND-PARTY-071): Cleric, Druid, Psionicist and Ranger 4, Fighter and Gladiator 0, Preserver 3,
Thief 1. The combatant record keeps the current scores, CHAR bytes `0x23` to `0x28`, at `+0x19` to
`+0x1E` in that order (FND-PARTY-050, FMT-PARTY-001). The 18
bytes at `55BE:0024` (file `0x4AE04`) are 0 for code 0, 1 for codes 1 to 8, 2 for 9 and 10, 4
for 11, 8 for 12, 2 for 13 to 16 and 0x10 for 17. The alignment byte counts from 1 in the order
lawful good, lawful neutral, lawful evil, neutral good, true neutral (glossary `alignment`).

**The handler.** Overlay 209 `+169C` takes an event kind and a button. For kind 2 and a button
from `0x4396` to `0x439E` it jumps through the table at `cs:1769` (`+16CD..+16DB`): `0x4396`
closes the window through `+177B` with no change; `0x4397 + k` gives class code 1 (`k` 0), 5 (1),
9 to 12 (2 to 5), 13 (6) or 17 (7). For codes 1, 5 and 13 it calls overlay 172 `+0025` with the
title `SPHERE: AIR`, 1 and the items `EARTH`, `FIRE` and `WATER`, and adds the result to the code
when it is 1, 2 or 3 (`+1700..+1735`). It then calls `+17DC` with the slot in `4E71:0B44` and the
code, and closes the window (`+1737..+174A`). Kind 6 with `0x11B` closes the window
(`+16BA..+16CA`).

## Interpretation

The DUAL list holds the classes a human may add: the character must be at least level 2 in its
current class and hold fewer than three classes, have 15 or more in the current class's prime
requisite and 17 or more in the new class's, and hold no class of the new class's group (priest:
cleric and druid; warrior: fighter, gladiator and ranger; preserver; psionicist; thief). A ranger
needs a good alignment and a druid true neutral. A class the character holds, now or before, is
always in its own group, so neither the current class nor a former one is offered. The prime
requisite counts as one score per class. The minimum scores of RULE-PARTY-002 other than the prime
requisite are not tested. A cleric, druid or ranger gets its sphere from a menu, air when the
menu is closed without a choice.

## Alternatives

- Overlay 172 `+0025` was read only through its other callers (FND-COMBAT-026), which take its
  results 1 to 3 as the three items; what it returns when closed without a choice was not read.
- Whether the View Character screen offers DUAL to a character that fails every test was not
  read; `+123E` then shows only the message.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `overlay_listing.py <dsun> 209 9439E 946B7`,
`overlay_listing.py <dsun> 209 947FC 948C9` and `overlay_listing.py <dsun> 209 94B07 94C7C`.
Read the 13 words at file `0x947E2` and the 9 at `0x948C9` (the jump tables at `cs:1682` and
`cs:1769`), the 18 bytes at `0x4AE04`, the 36 bytes at `0x4378C` and the 27 bytes at `0x43843`.
