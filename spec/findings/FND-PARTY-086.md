---
id: FND-PARTY-086
title: Experience is given through one overlay 188 routine, by the script instruction 0x21 to one party member or all, and by the damage routine for each enemy killed, which shares the dead combatant's details dword at 0x04 among the party
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000744A4..0x00074591
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0EE9..172C:0F69
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0000D429..0x0000D435
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005CBE4..0x0005CD3A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D9E7..0x0005DA56
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:0283..27E5:02A7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B0C4..0x0005B0D9
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Records, slots, the slot state byte at `4F49:0C33` plus 3 times the slot and the
combatant number at `4F49:0C34` are as FND-PARTY-081 gives.

**Overlay 188 `+1604`** (trampoline `5702:005C`) takes a slot and a signed word, the amount. It
returns at once unless the slot is below 4 (unsigned) and `2D40:3E64` succeeds for it, giving a
combatant number and a details index (`+160B..+1630`); unless the combatant record's byte at
`+0x14` is at most 2 (`+1630..+1646`); and unless bit `n` of `0x71` is set, `n` being the
combatant record's byte at `+0x15` (`+1646..+1660`). It then divides the amount (signed) by the
result of overlay 210 `+02BA` for the slot, the number of classes counted (FND-PARTY-081), extends
the quotient's sign and adds it to the details record's dword at `+0x00`, and takes 2,000,000,000
when the sum is greater, signed (`+1660..+169C`). It stores the sum in the details record's dword
at `+0x04` when that dword is below it, unsigned, and then in the dword at `+0x00`
(`+169C..+16DC`). On every path, rejected or not, it ends by calling overlay 210 `+0B44`, the
level gain of FND-PARTY-081, when the word at `4C10:0019` is 0 (`+16DC..+16F1`).

**The script instruction 0x21.** FND-SCRIPT-005 gives `172C:0EE9` as the handler of instruction
0x21. It reads `script_parameters` 0, the character, and compares it with the two dwords at
`172C:0F69` (file `0xD429`): 32,766 and 32,767, whose targets are the words that follow, `0F3C`
and `0F65` (`172C:0EFA..172C:0F27`). For 32,766 it calls `2D40:0776` from 9,999 for each party slot in
turn until it returns 9,999, and calls overlay 188 `+1604` with each and the low word of
`script_parameters` 1 (`172C:0F29..172C:0F4A`); for 32,767 it does nothing; for any other value it calls
`+1604` with the low words of `script_parameters` 0 and 1 (`172C:0F4C..172C:0F65`).

**Overlay 173 `+22C4`** (trampoline `5671:005C`) takes a combat slot and an amount of damage. When
the slot's state byte is 2 and the amount is above 0, it reads the combatant record's word at
`+0x00`, the hit points `h` (`+22D6..+232F`):

1. When the amount is at least `h + 10`, it stores 8 in the combatant byte at `+0x14` for an amount
   of 10,000 or more and 7 otherwise, and 0 at `+0x00` (`+232F..+2376`).
2. Otherwise, when the amount is at least `h`, it stores 3 at `+0x14` and 0 at `+0x00`
   (`+2376..+239C`).
3. Otherwise it subtracts the amount from `+0x00` (`+239C..+23AA`).

Then, unless the byte at `+0x14` is 1, it calls `+306B`, and when the byte is above 2 it calls
`+30C7` with the combatant number (`+23EF..+2419`). `direct_callers.py` finds `+22C4` called from
overlays 179 and 195, among others.

**Overlay 173 `+30C7`** takes a combatant number. Unless bit `n` of `0xF80` is set, `n` being the
combatant record's byte at `+0x15`, it returns (`+30CE..+30EA`). It divides the dword at `+0x04`
of the details record that the combatant's word at `+0x04` numbers, unsigned, by the result of
`27E5:0283`, keeps the low word of the quotient, and calls overlay 188 `+1604` with each slot from
0 to 3 and that word (`+30EA..+3133`). **`27E5:0283`** returns the number of slots from 0 to 3
whose state byte is not 0.

**Overlay 173 `+07A4`** calls overlay 210 `+0B44` when the byte at `DS:13FB` is 0, and overlay 188
`+0101` after it (`+07A4..+07B9`).

## Interpretation

Every experience award read here goes through overlay 188 `+1604`: it gives a party member who is
not out of the fight (`+0x14` at most 2) and whose combatant kind is 0, 4, 5 or 6 the amount
divided among the classes counted, raises the details dword at `+0x04` to the new experience
when it is below it, and runs the level gain unless the word at `4C10:0019` defers it. A script
gives experience with instruction 0x21, to one party slot, to every member for the character
32,766, or to nobody for 32,767. In a fight, a combatant of kind 7 to 11 brought to 0 hit points
gives its details dword at `+0x04` divided by the number of filled party slots to each of the four
slots; a member out of the fight gets nothing, and its share is not passed on.

The details dword at `+0x04` is the FMT-PARTY-001 field at `0x49`, which a load copies there
(FND-PARTY-058): for an enemy it is the experience its death gives, and for a party member it
follows the greatest experience reached. The share passes to `+1604` as a 16-bit word that
`+1604` divides as a signed number, so a share of 32,768 or more would arrive negative and lower
the experience of each member; whether any enemy's dword reaches that was not read.

## Alternatives

- The level gain that runs from overlay 173 `+07A4` when the word at `4C10:0019` is not 0 is the
  one that follows a fight: what sets and clears that word, and the byte at `DS:13FB`, was not
  read here.
- The kinds 7 to 11 of the combatant byte at `+0x15` as enemies, and 0, 4, 5 and 6 as the party
  and its allies, rest on the two masks; no reader of the byte that names its values was traced.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 188 744A4 74591`,
`173 5CBE4 5CD3A`, `173 5D9E7 5DA56` and `173 5B0C4 5B0D9`; `resident_listing.py <dsun>
172C:0EE9..172C:0F69` and `27E5:0283..27E5:02A7`; and `direct_callers.py <dsun> 173+22C4 173+30C7
27E5:0283`. Read the 12 bytes at file `0xD429`.
