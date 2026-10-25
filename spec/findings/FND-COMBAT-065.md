---
id: FND-COMBAT-065
title: The attack routine overlay 173 +1CC1 takes the attacker, the target, an item list and the distance 2D40:35EF gives, picks a melee weapon at distance 1 or less and a missile weapon in range beyond it, falls back to a natural attack only in melee, and lowers the needed roll for a rear attack and a thief's backstab
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005C5E1..0x0005CBE4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005CE64..0x0005CE80
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D6F0..0x0005D73B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D73B..0x0005D7E7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005BDB9..0x0005BDC8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005C0D7..0x0005C0E9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:13CF..2D40:13DE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:1C55..2D40:1C66
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:30FA..28C9:310C
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py and immediate_search.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 173's code starts at file `0x5A920`. A
slot's state byte is at `4F49:0C33 + 3 * slot` and its combatant number at `4F49:0C34 + 3 *
slot`; combatant records are the 49-byte records under `DS:19C9`, details records the 66-byte
records under `DS:19C5`, item records the 23-byte records under `DS:19C1` and item type records
the 15-byte records under `DS:19CD`. An item list is FND-EXE-600's list of 10-byte entries.
Bits are named by their mask.

**Callers and arguments.** `direct_callers.py <dsun> 173+1CC1` lists a near call at overlay 173
`+17C6` and far calls through trampoline `5671:0057` at `28C9:3107`, `2D40:13D9` and `2D40:1C61`.
The arguments are the attacker's slot, the target's slot, a far item list and a word `r`. The
three resident calls pass `r` = 1, the latter two with the list at `4F49:000E` filled by
`2D40:2196` (`2D40:13CF..+13DE`, `2D40:1C55..+1C66`); `28C9:3107` passes its own argument
(`28C9:30FA..+310C`). The call at `+17C6` passes the word at `bp-0xA` (`+17B7..+17C9`), which
`+1238` sets to 1 at `+1838` or to the result of `2D40:35EF` of the two slots at `+14A5`, `+16A4`
and `+1A75` (`+1499..+14A8`). FND-RNG-041 reads `2D40:35EF` as a distance.

**Set-up** (`+1CC1..+1DBA`). It stores the attacker in `DS:444B`. It returns -1 when `2D40:3E64`
does not find the attacker or the target slot is 9999 or more. When the fight word at
`4C10:0019` is 0 it replaces the target by `+085D`'s result for it and stores 2 in the attacker's
word at `4C4F:01D8`. It notes whether the target's state byte is 1 (an object, `bp-0x1A`), takes
the attack number from the byte at `4C4F:01AE` plus the attacker's combatant (`bp-0x20`), calls
overlay 195 `+09A7` with the attacker and -1, and calls `+2E1B` attack-number-plus-one times
from the index -2 to reach the weapon entry for this attack number (`bp-0x18`).

**Weapon choice, `+2E1B`** (`0x5D73B..0x5D7E7`). It takes a current index, the list, `r` and a
near pointer to a count. It counts, and returns the first after the current index of, the
entries of kind 1 for which `+2DD0` returns 0x11 and whose type byte `+6` is 1 when `r` is 1 or
less, or above 1 when `r` is above 1; 0xFFFF when there is none, or when the current index is
0xFFFF. `+2DD0` (`0x5D6F0..0x5D73B`) follows an entry's byte `+1` up through the list to the
first entry of kind 2 and returns that entry's word `+6`.

**Kind and range** (`+1DBD..+1F94`). The base is the attacker's combatant byte `+0x16`, signed
(`bp-0x1E`). With a weapon entry (index 0 or more): the item is the entry's word `+4`, its type
the item's word `+0x0A`, the hit word `m` (`bp-0x14`) the type byte `+1`, and the magic level
(`bp-0xC`) the item's signed byte `+0x16`. The to-hit bonus (`bp-8`) is the item's byte `+0x16`,
or 0 when overlay 193 `+1C95` of the target and 4 is nonzero, plus `27E5:03B1` of the item and
type (`+1E25..+1E5A`). When `r` is 1 or less it adds `+3514` of the weapon count, the attacker
and the item (`+1E5D..+1E75`); otherwise it calls `+2A9A` of the list, item, type and 0 and
raises the magic level to the byte `4C4F:0009` that `+2A9A` sets when that is higher
(`+1E78..+1EB4`). Without a weapon entry the item is -1 and `m` is 8; when the attacker's
details byte `+0x12` is 0 or 9 or more, the magic level is 4, 3, 2 or 1 as details byte `+0x1E`
is above 10, 8, 6 or 4 (`+1EB7..+1F48`). A magic level above 0, 1 and 2 ORs 0x1000, 0x2000 and
0x8000 into `m` (`+1F4D..+1F69`). When `r` is above 1, the routine returns -1 unless there is a
weapon entry whose type byte `+6` is at least `r` (`+1F6E..+1F94`).

**Facing, rear and backstab** (`+1F97..+207F`). `1BDF:009B` of the target's and the attacker's
positions (words `+0x0A` and `+0x0C` of their 37-byte records at `DS:67BB`, shifted right 4)
gives a direction `d`; the attacker's combatant byte `+0x1F` and `DS:4208` become `d + 4` brought
into 0 to 7. For a target that is not an object, the byte at `4C4F:005E` plus the target's
combatant holds a direction: when it is negative the routine stores `d` there; otherwise, when it
equals `+2544(d, 4)`, the attack is from the rear (`bp-0x19` = 1) and the base drops by 2, and
when the attacker's details word `+0x10` has bit 0x400, `r` is 1 and the type byte `+2` is 0x28
or less, the base drops by 2 more and the backstab flag `bp-0x21` is set. `+2544`
(`0x5CE64..0x5CE80`) returns the sum of its two arguments brought into 0 to 7 by one add or
subtract of 8. FND-COMBAT-060 has every round step store 0xFF in `4C4F:005E` for the slots it sets up.

**Terms and the swing** (`+207F..+2138`). The target value is `+2C00(target, attacker, rear)`
(`bp-6`). The base drops by overlay 197 `+259E(target, attacker, list, r)`. When the attacker's
slot is 4 or more, the bonus rises by the word `DS:143A` less 1. `DS:4206` takes the target. It
then calls `+2560` with the list, the base less the bonus, the target value, the attacker's
combatant and details numbers, item, type, `r`, the attacker, the attack number, `m`, the
backstab flag and the rear flag (`+20B4..+20E5`), and calls it again while it returns 0 or more,
the target is an object whose item type word `+0x0A` is not 0x15 or a combatant whose
`combat_mark` is 1, and the attacker's word at `4C4F:01D8` is not 0 (`+20E8..+2135`).

**After the swings** (`+2138..+22C3`). With a weapon and a positive result it calls `+3A9F`
with the item and type. The attack number rises, the next weapon comes from `+2E1B` and the swing
count byte at `4C4F:0184` is cleared, when the target is an object or still has `combat_mark` 1,
or the swing count is 0x63 or more. It goes back to `+1DBD` for the next attack number while the
number is below 3, the attacker's word at `4C4F:01D8` is not 0 and has bit 2, the target's
`combat_mark` is 1 and either `r` is 1 or less or the number is 0 (`+21B3..+21FD`). It stores the
number at `4C4F:01AE`, calls `+306B(attacker, 0)` in the cases at `+2210..+2277`, then
`1695:02AD(attacker, target)` and `+24C7`, and stores 1 in the fight word when it was 0, the
target is not an object and `+3304` returns nonzero (`+2293..+22BE`). It returns 0.

`immediate_search.py <dsun> 143A` lists `+20A9` and, besides the writes of FND-CONFIG-010 and
FND-CONFIG-120, reads at overlay 187 file `0x703B1`, overlay 192 `0x7D705`, overlay 203
`0x8B633` to `0x8B916` and overlay 204 `0x8C226`.

## Interpretation

`r` is the distance between the attacker and the target. An attack at distance 1 or less is a
melee attack with the first melee weapon held at location 0x11, or a natural attack when there
is none; beyond 1 it is a ranged attack with a missile weapon whose range byte `+6` reaches the
target, and with none the routine refuses the attack. A character next to an enemy with only a
missile weapon fights with a natural attack. The attack numbers 0, 1 and 2 step through the held
weapons and then the natural attacks, so a second melee weapon is attack number 1.

Before the roll, the needed number is the THAC0 (combatant byte `+0x16`), less 2 for an attack
from the side opposite the first attacker of the round, less 2 more for a thief's backstab in
melee with a small weapon, less overlay 197 `+259E`'s term, less the weapon's magic bonus, a
material term, a two-weapon term and, for a non-party attacker, the difficulty less 1, less the
target's armour class. The difficulty is RULE-CONFIG-002's index 0 to 3, so Easy gives enemies
-1, Balanced 0, Hard +1 and Hideous +2 to hit. `m` carries the weapon's type byte `+1` and its
magic level; FND-COMBAT-069 shows what reads it.

## Alternatives

- That `r` is a distance in grid steps rests on FND-RNG-041's reading of `2D40:35EF` and on the
  constant 1 the movement callers pass; `28C9:3107`'s argument was not followed.
- That location 0x11 is the hands, bit 0x400 the thief (FMT-PARTY-001 `class_flags`), type byte
  `+6` the range and type byte `+2` a size rests on how they are tested; no writer was read.
- `+085D`'s replacement target, overlay 195 `+09A7`, `+306B`, `+3A9F`, `1695:02AD`, `+24C7` and
  the meaning of bit 2 of `4C4F:01D8` (Q-COMBAT-006) were not read.
- The natural-attack path writes no type into the local `bp-4`, which keeps what an earlier
  attack number or the stack left there, and the backstab test reads type byte `+2` through it;
  a thief's unarmed rear attack therefore tests an undefined record.
- The other readers of `DS:143A` were found by a search and not read.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 173 5C5E1 5CBE4`, `173 5CE64 5CE80`, `173 5D6F0 5D73B`,
`173 5D73B 5D7E7` and `173 5BB58 5C3ED`; `resident_listing.py <dsun> 28C9:30D0..28C9:3110
2D40:13A0..2D40:13E2 2D40:1C20..2D40:1C6A`; `direct_callers.py <dsun> 173+1CC1`;
`immediate_search.py <dsun> 143A`.
