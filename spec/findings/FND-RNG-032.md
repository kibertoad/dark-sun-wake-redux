---
id: FND-RNG-032
title: Overlay 197's last unread generator call is a d20 roll against the caster's THAC0 less the target's Armor Class that five DATA numbers must pass before the hit routine applies them for a caster numbered 48 or more
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008428F..0x000842BC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000843CA..0x000843D2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008445F..0x00084502
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00084502..0x0008451A
tool: tools/research/exec-census/direct_callers.py, trampoline_target.py and overlay_listing.py at commit c40653e3; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 197's code starts at file `0x83400`.
Trampoline `575A:0093` enters `+0E8F`, the first of the three overlay 197 calls the hit routine
overlay 179 `+0D2F` makes before its saving throw (FND-PARTY-112). The direct-call query finds
one caller of `+0E8F`, the far call at overlay 179 `+0E68`, and no near caller. Decoding `+0E8F`
from its entry to its far return at `+1101` shows FND-RNG-028's overlay 197 site `+1082` as a far
call of `1000:0822` whose segment word is a descriptor-0 fixup.

`+0E8F` keeps its first argument `t`, the target, in `DI` and its fourth, the `DATA` number `i`,
in `SI`; its second argument `c` is the caster (FND-PARTY-112). A return of 1 ends the hit
routine, and 0 lets it go on. When none of the tests FND-PARTY-112 lists returns 1 and `i` is
not -1 (`+0FCA..+0FCF`), the routine compares `i` with the six words at `cs:1102`: 8, 18, 53,
121, 169 and 277 (`+105F..+1078`). A number not among them returns 0 (`+1075`). The six jump
words that follow, at `cs:110E`, send 8 to `+10D4` and the other five to `+107C`. The twelve words
are at file `0x84502..0x8451A`.

For 18, 53, 121, 169 and 277 (`+107C..+10D1`):

1. When `c` is below 48 (unsigned) it returns 0 with no draw.
2. Otherwise it draws at `+1082`, sign-extends the draw, multiplies it by 20 and divides by 32768
   as signed 32-bit values, and adds 1, giving `r` from 1 to 20.
3. When `r` is 20 it returns 0. When `r` is 1 it returns 1.
4. Otherwise it takes the signed byte `+0x16` of `c`'s combatant record at `DS:19C9 + 49 * c`,
   the THAC0 (FND-PARTY-091), and subtracts overlay 173 `+2C00` of `t`, `c` and 0, the value
   FND-PARTY-091 reads as the target's Armor Class. It returns 1 when that difference is above
   `r` (signed) and 0 otherwise.

For 8 (`+10D4..+10F9`) it makes no draw: when its sixth argument is 0 or 1 and bit 0x02 of byte
`+0x0B` of `t`'s block is set, it calls overlay 179 `+12B1` with `t`, `t`, 0x133 and 6 and returns
1; otherwise 0.

So a call makes one draw exactly when `i` is one of the five numbers, `c` is 48 or more and every
earlier test let the call reach the list.

## Interpretation

The draw is a to-hit roll for the five `DATA` numbers when the caster is numbered 48 or more:
the hit is applied on a roll of 20, never on a roll of 1, and otherwise when the roll is at least
the caster's THAC0 less the target's Armor Class. This is the comparison of RULE-COMBAT-002 and of
overlay 173 `+2560` (FND-PARTY-091), with no bonus taken off the THAC0 here. A caster numbered
below 48 makes no roll and the hit goes on. No rule entry describes this test yet.

## Alternatives

- Which spells, powers or items the `DATA` numbers 8, 18, 53, 121, 169 and 277 are, and which
  combatants are numbered 48 or more, were not read here.
- Overlay 173 `+2C00` is read as the target's Armor Class from FND-PARTY-091, which did not read
  its body; the third argument 0 here and its value in the attack routine were not compared.
- What overlay 179 `+12B1` does with its arguments for `DATA` 8 was not read.
- The overlay is outside the resident harness; no emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: `trampoline_target.py <dsun>
575A:0093`; `direct_callers.py <dsun> 197+0E8F` (one far caller, overlay 179 `+0E68`);
`overlay_listing.py <dsun> 197 83400 84520` and read the ranges above, the table as twelve
little-endian words at file `0x84502`. Output is kept under
`GAME_DIR/analysis/rng-callers-20261020/`. Nothing was executed.
