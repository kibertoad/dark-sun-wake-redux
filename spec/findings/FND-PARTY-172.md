---
id: FND-PARTY-172
title: Releasing a placed object empties a character box record only through the kind 3 case of the switch at 2D40:248D, which runs for the entries 2D40:2196 builds from the object table
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:044C..31E0:0461
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2196..2D40:2410
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2410..2D40:248D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:248D..2D40:256E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00024DAD..0x00024DB7
    kind: file-data
tool: Capstone 5.0.9 (bounded listings)
environment: null
---

## Observation

`31E0:044C`, which `31E0:05BB` reaches when it removes a placed entry (FND-PARTY-118), returns at
once while the byte at `DS:265B` is 0 (`31E0:0457..045E`); start-up sets that byte before the start
window (FND-PARTY-047). Past that test it calls `2D40:2410` with the object.

**The list.** `2D40:2410` returns at once when the object is `270F` or more or its kind byte at
`4F49:0C33 + 3 * object` is 0. Otherwise it calls `2D40:2196` with the object and a 0x320-byte
local buffer, then `2D40:2452` with that buffer and 0. `2D40:2196` fills 10-byte entries, at most
79 (`cmp [bp-2], 4Fh` at `2D40:23CB`): for each object it visits, starting with the one it is
given, it stores a step kind (1 for the first entry, then 4, 3 or 2 by the byte at `DS:60ED` plus
the object and the word at `DS:60EB`) at `+0`, the object's kind byte at `+2`, the word at
`4F49:0C34 + 3 * object` at `+4`, the previous object at `+6` and a word from the table at
`DS:6167` indexed by the kind byte at `+8`. It takes the next object from `1AA0:0566` and
`1AA0:051C`. It ends with a step kind `FF` and puts the entry count in the first entry's `+1`.

**The switch.** `2D40:2452` calls `2D40:27B7` and then `2D40:248D` with its second argument as an
entry index. `2D40:248D` takes the entry at that index, subtracts 1 from the kind at `+2`, and when
the result is 0 to 4 jumps through the five words at `2D40:27AD`:

| Kind | Branch | What it does with the entry's `+4` |
| --- | --- | --- |
| 1 | `2D40:253B` | calls `2D40:0EB4` with it |
| 2 | `2D40:251B` | stores 0 to `+0x06` of that FMT-COMBAT-001 record at `DS:19C9` |
| 3 | `2D40:24B6` | when none of the 42 records at `DS:19C9` whose `+0x06` is nonzero has `+0x04` equal to it, stores 0 to `+0x0E` of that 66-byte record at `DS:19C5` (`2D40:2513`) |
| 4 | `2D40:27A9` | nothing; it returns |
| 5 | `2D40:2551` | stores 0 to the first word of that 23-byte record at `DS:19D1` |

The code after the switch, from `2D40:256E`, was not read.

## Interpretation

When `DROP` removes a party member's placed entry (FND-PARTY-171), the character box record at
`DS:19C5`, whose `+0x0E` the Create Characters screen tests for an empty box (FND-PARTY-046), is
emptied on this path only by an entry of kind 3 that names the box, and only once no live
combatant record names it. FND-PARTY-118 reads party objects below 4 with kind byte 2, whose case
clears the combatant record's `+0x06` and leaves the box record. Which entries `2D40:2196` builds
depends on the object table at the time of the drop, so a static reading leaves open whether the
box shows as empty after `DROP`.

## Alternatives

- The code after the switch, `2D40:27B7`, or `2D40:03C1`, which `31E0:044C` calls after
  `2D40:2410` (FND-PARTY-118), empties the box record another way: not read here. The scan of
  FND-PARTY-171 finds no other direct store to that record's `+0x0E` outside character generation.

## How to reproduce

From the commit that adds this finding, with Capstone 5.0.9 on the installed `DSUN.EXE`, list
`31E0:044C..31E0:0461`, `2D40:2196..2D40:2410`, `2D40:2410..2D40:248D` and `2D40:248D..2D40:2580`
from the load image (file `0x5200` plus 16 times the segment less `1000`, plus the offset), and read
the five words at `2D40:27AD`.
