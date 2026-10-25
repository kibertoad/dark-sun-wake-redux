---
id: FND-COMBAT-069
title: A weapon hit enters the hit routine with DATA number -1, passes gates on the target's type word, effects and range, takes twice the damage against a target at combat_mark 3, is scaled by overlay 197 +1C52's percentage of the hit word, and reaches overlay 173 +22C4, which leaves hit points at 0 at worst and kills only on one hit of at least the hit points plus 10
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00064F5F..0x0006500F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006500F..0x0006508B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006508E..0x000654BC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008428F..0x00084502
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008451A..0x000846AC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00085052..0x000851C6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084B60..0x00084E94
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087310..0x00087459
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005CBE4..0x0005CCCA
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7
environment: null
---

## Observation

Overlay 179's code starts at file `0x642E0`, 197's at `0x83400` and 198's at `0x87310`. The
arguments are FND-COMBAT-067's call: target, attacker, `n` = -1, `m`, total pointer, maximum,
ranged flag, rear value, `r`, `k`, `L` and `g`.

**Overlay 179 `+0C7F`** (`0x64F5F..0x6500F`) treats `n` -1 as kind 4. For it, when the total
pointer's offset word is above 0, as it always is for the swing's local, it removes effects 0x35
and 0x36 from the target through overlay 193 `+1AA4`; it then calls `+0D2F` with its arguments
and `+11DC` after. Bits are named by their mask.

**Overlay 179 `+0D2F`** for `n` -1 (`+0D2F..+11DB`): it ends at once when the word at `DS:2588`
indexed by the target's combatant byte `+0x0F` is -1. A target in state 1 takes `+22C4` of the
total directly. Otherwise it fills the attacker's and the target's blocks and sets no level
(`+0E52`). Then:

1. **Overlay 197 `+0E8F`** (`0x8428F..0x84502`) ends the hit when `2D40:3E64` does not find the
   target, the `DS:2588` word is -1, overlay 208 `+0F07` (trampoline `57A6:0061`) returns 0, or the
   target's block byte 7 has bit 1 and the target is not the attacker. With the ranged flag set
   it ends the hit when block byte 5 has bit 0x80 (after queuing `DATA` 0x133 on the target) or
   `+314E` returns nonzero. It ends the hit when the target's word at `4C4F:00DC` is -1 and block
   byte 1 has bit 8. Otherwise, for `n` -1, it lets the hit go on (`+0FCA..+0FCF`).
2. **Overlay 197 `+1760`** (`0x84B60..0x84E94`) removes effect 5 from the target for block byte 0
   bit 0x20, and for an attacker below 48 with the target's block byte 4 bit 0x10 queues `DATA`
   0x7C on the attacker and removes effect 0x24. For `n` -1 it skips to `+18D6`: when the
   attacker's combatant byte `+0x0E` is not 0 it calls overlay 198 `+0000` with the target, the
   attacker, the total, `k`, `L`, the attacker's last natural attack with a rate, and whether
   `2D40:35EF` of the two is 1 or less. When the attacker's block byte 3 has bit 0x10 and the
   target's details byte `+0x12` is 0x0E, it calls `+2A05` with 1000 damage. It returns 1.
3. **Overlay 197 `+111A`** (`0x8451A..0x846AC`), when `m` is not 0 and block byte 12 has bit 1,
   removes effect 0x60 and ends the hit when `m` has a bit of 0x38; it ends the hit, removing
   effect 0x39, when `m` has a bit of 0x38 and byte 7 has bit 2; and when `m` has bit 2 and
   overlay 176 `+076C` (trampoline `5691:0061`, FND-PARTY-258's class level) of the target and
   3 is 5 or more. It stores `+1C52(target, block, m)` as the percentage, ends the hit when it is
   0, and for `n` -1 returns at `+12A9`.
4. **Overlay 197 `+1C52`** (`0x85052..0x851C6`): with `w` the `DS:2588` word for a type below
   0x12, when `w & m` has a bit of 0x38 the percentage is 0 if `w & 0xB000` exceeds `m & 0xB000`
   (unsigned) and 100 otherwise; when it has none, 0 if `w & m` is not 0 and 100 otherwise. For a
   type below 7 it halves when the word at `DS:25AC` shares a bit with `m`. From the block it forms
   4 (byte 9 bit 2), 2 (byte 9 bit 0x20), 0x80 (byte 9 bit 0x40) and 6 (byte 4 bit 0x40), and
   halves when `m` shares a bit with them and has a bit outside them and 0x200. It is 0 when `m`
   has no bit of 0xFD79 and byte 9 has bit 4. FND-PARTY-112 gives the two tables.
5. Overlay 197 `+0000` turns the total into the damage, taking the maximum when the target's block
   byte 8 has bit 0x20 or its `combat_mark` is 3 (FND-MAGIC-018); the hit routine stores `total *
   percentage / 100` (`+0F42..+0F8A`).
6. When that is above 0 (`+0F8D..+107E`): for an attacker whose details byte `+0x12` is 8, when
   `k` is the attacker's last natural attack with a nonzero rate and `27E5:0154` of the attacker
   is 5 or more, it queues `DATA` 0x131 on the target through `+12B1`; it plays the target's
   details word `+0x3C` as a sound, or 0x62; it sets bit 0 of the result and calls `+2A05` with
   the target and the damage, and returns. When it is 0 it plays 0x64 and returns.

**Overlay 198 `+0000`** (`0x87310..0x87459`), only when its near flag is set, queues through
`+12B1` on the target the `DATA` number its `+0149` gives for the attacker's byte `+0x0E` and a
code of 1 (`k` 0), 2 (`k` the last natural attack) or 3, with special handling for the numbers
0x11F and 0x120; and, when the target's byte `+0x0E` is not 0, the number `+0149` gives for the
attacker's byte and 4, on the attacker. FND-RNG-055 reads the byte's values 6 and 13.

**Overlay 173 `+22C4(slot, d)`** (`0x5CBE4..0x5CCCA`) does nothing for a state-0 slot or `d` of 0
or less. For state 2, with `h` the combatant's word `+0`: when `h + 10` is at most `d` (signed),
`combat_mark` becomes 8 when `d` is 10,000 or more and 7 otherwise, and `h` 0; else when `d` is
at least `h`, `combat_mark` becomes 3 and `h` 0; else `h` drops by `d` (FND-PARTY-086).

## Interpretation

A weapon hit carries no `DATA` number; the gates that apply to it are the target's type word,
some effect bits and, at range, two missile protections. The hit word `m` decides immunity:
when the type word and `m` share a bit of 0x38, a target whose type word holds 0x1000, 0x3000 or
0xB000 needs a weapon of magic level 1, 2 or 3 to take any damage (types 9, 10 and 12 have 0x1000,
0x3000 and 0x3000); when they share only other bits the target takes none. A natural attack's `m`
is 8 with its level bits, so a monster of more than 4 levels counts as a magic weapon against
these types, and type 1's `DS:25AC` word 0x0A, which has 8, halves natural attacks.

Hit points never go below 0 through `+22C4`. A blow that leaves 0 or less knocks the combatant
out (`combat_mark` 3), and a single blow at least 10 more than the hit points left kills it (7),
or removes it (8) at 10,000 or more. A combatant at 0 is killed only by one blow of 10 or more;
smaller blows leave it at 0 and 3. A weapon hit on a combatant at mark 3 deals twice its damage.
The order of FND-COMBAT-028's names, Out Cold at 3, Dead at 7 and Gone at 8, matches these marks,
but no reader of that list was found.

A thri-kreen of level 5 or more casts `DATA` 305 with its last natural attack, which fits the
paralysing bite of the AD&D thri-kreen (circumstantial).

## Alternatives

- The rest of `+111A` for `n` -1, `+314E`, overlay 208's and overlay 176's gate routines, and the
  meaning of the block bits were not read here; FND-PARTY-112 and RULE-MAGIC-010 read parts for
  spells.
- Whether a combatant at `combat_mark` 3 can be chosen as a target was not read; the doubled
  damage needs it.
- `+11DC`, run after every hit, was not read.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 179 64F5F 6508B`, `179 6508E 654BC`, `197 8428F 8451A`,
`197 8451A 84AB5`, `197 85052 851C6`, `197 84B60 84E94`, `198 87310 87459` and `173 5CBE4 5CDB5`;
`direct_callers.py <dsun> 173+22C4`.
