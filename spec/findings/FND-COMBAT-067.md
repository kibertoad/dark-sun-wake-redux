---
id: FND-COMBAT-067
title: Overlay 173 +2560 swings up to a rate of half attacks per round, rolls the damage dice plus the type, magic, material and ammunition bonuses with a floor of 1, adds Strength in melee, multiplies a first-swing backstab by 2 to 5, and hands the hit routine the damage with twice it as the maximum
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005CE80..0x0005D37D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D37D..0x0005D3BA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D3BA..0x0005D4A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005DE0C..0x0005DE34
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00085899..0x000858FB
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7
environment: null
---

## Observation

The arguments of overlay 173 `+2560` are those FND-COMBAT-065 gives: list, needed base `b`,
target value `a`, attacker's combatant and details numbers, item `i`, type `t`, distance `r`,
attacker slot, attack number `k`, hit word `m`, backstab flag and rear flag. `direct_callers.py`
lists one caller, the near call at `+20E2`.

**Rate** (`+2573..+272F`). With `i` 0 or more the rate is type byte `+7` when `r` is above 1 and
the attacker's details byte `+0x24` otherwise; the dice, sides and bonus are type bytes `+9`,
`+8` and signed `+0x0A`. The bonus then adds the item's signed byte `+0x16` unless overlay 193
`+1C95` with 4 is nonzero for the attacker's combatant number or for the target `DS:4206`, and
adds `27E5:042B(i, t)`. With `i` below 0 the four values come from overlay 197 `+35A4` for
natural attack `k` (FND-PARTY-100). The rate is doubled when overlay 197 `+2499` of the attacker
returns 4, halved toward 0 when it returns 1 and divided by 9 when it returns 7. It then becomes
`(rate + bit 0 of (dword 4C13:035B / 60)) / 2`, signed. `+2A9A(list, i, t, 0)` gives an
ammunition count (999 for a weapon whose type byte `+0` lacks bit 8, as a mask) and sets `4C4F:0009` to the
ammunition's byte `+0x16`, which the bonus adds; the rate is cut to the count, and a count of 0
shows the text at `DS:05BA` ("Out of ammo"). The swing counter starts at the byte `4C4F:0184` plus
the attacker's combatant.

**Overlay 197 `+2499(slot)`** (`0x85899..0x858FB`) returns, from the slot's block: 0 for byte 8
bit 0x20; 5 for byte 8 bit 4; 6 for byte 7 bit 1; 1 for byte 11 bit 8 or byte 6 bit 4; 4 for byte
5 bit 4 or byte 6 bit 8; 1 for byte 6 bit 0x10; otherwise 2. It never returns 7.

**One swing** (`+2732..+29E2`). It draws `1000:0822`, takes the roll as the draw times 20 divided
by 32,768 plus 1, stores it at `DS:420E`, and uses one round of ammunition through `+2A9A` with
flag 1. A roll of 20 hits, 1 misses, and any other hits when `b - a` is at most the roll. On a hit,
unless `k` is not 0 and `r` is above 1, the damage `D` is `+2A5D(dice, sides, bonus)`, at least 1,
then for `r` of 1 or less overlay 197 `+2FB2(attacker, D)`; it adds `D` to the returned total and
sets `DS:054F` to 1. `+2A5D` draws once per die and sums `draw * sides / 32768 + 1` plus the bonus.
Unless `k` is not 0 and `r` above 1: when both flags are set and `4C4F:0184` plus the attacker's
combatant is 1 or less, `D` is multiplied by `+34EC(attacker)`. It calls overlay 206 `+0000`
with the attacker, the target, `DS:4208`, `r` and the negated item word `+0`, and, when no swing
of this call has set `DS:054F`, `2C5F:0A22` with 0x63. When `D` is above 0 it calls overlay
179 `+0C7F` with the target `DS:4206`, the attacker, -1, `m`, a far pointer to `D`, `2 * D`, 1 when
`r` is above 1, 4 when the rear flag is set, `r`, `k`, the attacker's details byte `+0x1E` and
combatant byte `+0x15`. When the target is not the attacker, `r` is 1 and overlay 193 `+1C95` of
the target and 0x26 is nonzero, it repeats the call with the attacker as target and 0x202 as `m`
(`+28B1..+28D4`). Then the item effect of FND-PARTY-257 runs.

**`+34EC(slot)`** returns `(27E5:00C1(slot, 0x11) - 1) / 4 + 2`, at most 5.

**The loop** (`+29D5..+2A5C`). Each swing adds 1 to the counter byte. Swings go on while the count
is below the rate and nothing has hit. When the count reaches the rate the byte becomes 0x63 and,
for an ammunition weapon whose count is at most the rate, the routine shows "Out of ammo". It
returns the total, or -1 when the count reached the rate with no hit.

## Interpretation

The rate counts half attacks per round: 2 is one swing a round, 3 alternates one and two by the
parity of the dword at `4C13:035B` divided by 60, to which each round step adds 60
(FND-COMBAT-060); a timer callback also writes that dword (FND-RNG-081). An effect that `+2499` reports as 4 doubles the
rate and one it reports as 1 halves it. A call of `+2560` swings until one swing hits or the
rate is used up, and the attack routine calls it again while the target stands, so one attack
command spends all the round's swings for that weapon.

Damage is the dice plus the type's bonus, the magic bonus (ignored when either side has effect
4), the material term and the ammunition's magic bonus, at least 1, then the Strength damage
entry for a melee hit. A thief's rear attack on the round's first or second swing multiplies the
damage by 2 at levels 1 to 4, 3 at 5 to 8, 4 at 9 to 12 and 5 from 13; the returned total keeps
the unmultiplied amount, and only the hit routine receives the product. The maximum passed is
twice the damage.

A ranged hit with attack number 1 or 2 does no damage; FND-COMBAT-065's loop never reaches those
numbers at range, so only a call entering with an attack number already above 0 would see it.

## Alternatives

- The `idiv` by 9 for a result of 7 cannot run, since `+2499` has no path to 7.
- What effect 0x26 is, and what the second call of `+0C7F` with `m` 0x202 does to the attacker,
  was not read past FND-COMBAT-069's gates.
- Overlay 206 `+0000` and `2C5F:0A22` were read only as calls; that they draw the swing and
  play a miss sound is a guess from their arguments.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 173 5CE80 5D4A8`, `173 5DE0C 5DE34` and `197 85899 858FB`;
`direct_callers.py <dsun> 173+2560`.
