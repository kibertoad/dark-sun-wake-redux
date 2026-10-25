---
id: RULE-MAGIC-003
title: Keeping a psionic power in effect, a character's psionic strength points and the powers it starts with
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-MAGIC-007, FND-MAGIC-008, FND-MAGIC-009, FND-MAGIC-010, FND-RNG-046, FND-PARTY-081, FND-PARTY-104, FND-PARTY-169, FND-PARTY-210, FND-COMBAT-060, FND-MAGIC-005, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-MAGIC-014, RULE-MAGIC-015, RULE-MAGIC-004]
---

## Summary

Using a power is RULE-MAGIC-014's. A power a party member has in effect is kept at every round
step of a fight: when its upkeep, byte 2 of `psionic_powers`, is not above the member's psionic
strength points (PSPs), it is applied again with no check and the upkeep is paid; otherwise it
ends. An upkeep of 0 keeps a power for nothing, even at 0 PSPs, and 99 means the power is never
kept. A member who falls loses every power in effect, and the end of a fight and a rest end them
all. A psionicist's greatest PSPs grow by 10 per Psionicist level above the first plus a Wisdom
term, a character of any other class has 4 per level, and both add bonuses for Constitution,
Intelligence and Wisdom of 15 or more. A new psionicist knows 18 fixed powers at rank 1 and any
other new character the five powers of its one chosen discipline; any member can use a power whose
rank is above 0.

## When it runs

`keep_powers` runs for every slot at each round step of a fight (overlay 173 `+0C65`), and again
at a slot's turn when byte 12 of its flag block has bit 0x80 (FND-MAGIC-008). `greatest_psp`
runs at generation and, for every standing member, after every fight and every experience award
(FND-MAGIC-010). `starting_powers` runs at generation (FND-MAGIC-009).

## Parameters

`member`, a party slot 0 to 3, with `psionic_points` its current PSPs, `in_effect[p]` whether
power `p` is in effect for it, `standing` whether its slot state is not 0 and its `combat_mark`
below 3. `psionicist_level`, the member's Psionicist level, 0 when it has none. `level`, its level
as `fn_27E5_0154` gives it. `con`, `int` and `wis`, its Constitution, Intelligence and Wisdom.
`is_psionicist` and `discipline` (0 psychokinesis, 1 psychometabolism, 2 telepathy) for
`starting_powers`.

## Inputs

`psionic_powers`.

## Procedure

```text
# Bonuses for scores 15 to 25 (FND-PARTY-081, FND-MAGIC-010); 0 below 15.
let con_bonus: INT8[11] = [0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2]
let int_bonus: INT8[11] = [0, 1, 2, 3, 4, 5, 6, 0, 20, 22, 24]
let wis_bonus: INT8[11] = [20, 22, 24, 26, 28, 30, 32, 34, 0, 1, 2]
let wis_per_level: INT8[11] = [0, 1, 2, 3, 4, 5, 6, 7, 8, 8, 10]

define keep_powers(member):
    for p in 0..34:
        if member.in_effect[p]:
            let upkeep = psionic_powers[p].byte_2
            if member.standing and upkeep <= member.psionic_points:
                # the power's DATA number p + 235 is applied again at psionicist_level, or 1
                member.psionic_points = member.psionic_points - upkeep
            else:
                member.in_effect[p] = false
                # the power's effect entries caused by member end

define score_bonus(table, score):
    if score < 15:
        return 0
    return table[score - 15]

define greatest_psp(psionicist_level, level, con, int, wis):
    let b = score_bonus(con_bonus, con) + score_bonus(int_bonus, int) + score_bonus(wis_bonus, wis)
    if psionicist_level > 0:
        return b + 10 * (psionicist_level - 1) + score_bonus(wis_per_level, wis) * (psionicist_level - 1)
    return b + 4 * level

define starting_powers(is_psionicist, discipline):
    if is_psionicist:
        return [0, 1, 4, 5, 7, 8, 11, 13, 14, 16, 23, 24, 25, 27, 31, 30, 32, 33]
    if discipline == 0:
        return [0, 1, 3, 5, 30]
    if discipline == 1:
        return [8, 10, 11, 18, 0]
    return [23, 22, 25, 31, 30]
```

## Outputs

`keep_powers` changes the member's PSPs and the powers in effect. `greatest_psp` gives the
greatest PSPs; the game stores it only when it is above the stored value, and adds the rise to the
current PSPs. `starting_powers` lists the powers a new character knows at rank 1; all others are
rank 0.

## Edge cases

- At generation a character with no Psionicist class gets the score bonuses alone; the first run
  after a fight or an experience award raises it to `b + 4 * level` (FND-MAGIC-010).
- The greatest PSPs are never lowered.
- An upkeep of 0 (Mind Blank and the four defence modes) keeps the power with 0 PSPs.
- Share Strength (19) is kept on the member it was used on; every power after it in the same call
  is applied again on that member too (FND-MAGIC-008).
- A character who is not a psionicist uses its powers at level 1, and their ranks never rise with
  its levels; an item can teach or raise any power for any member (FND-MAGIC-009).
- The bonus tables give 0 for Constitution 23 and Intelligence 22, as the game reads them.

## What the sources say

SRC-MANUAL-1994, page 69, defines the power score as the ability score plus or minus a set amount,
gives the chance of activating a power as the power score times 5 percent, and says a character
always makes a power check when activating a power. It defines the initial cost as the PSPs spent
when the power is first used, of which a character failing the check spends half, and the
maintenance cost as the PSPs spent to keep a power running from the previous round, with no new
check; a power with no maintenance cost cannot be maintained. The power descriptions (pages 70
to 74) give each power's ability, amount and costs. The value file `RULE-MAGIC-003.powers.csv`
gives them for all 34 powers, one row per power in the printed order, with the columns `power`
(the name as printed), `discipline`, `kind` (`science` or `devotion`), `ability`, `modifier`,
`initial_cost` and `maintenance_cost` (PSPs per round). A cost the manual prints as NA is `NA`,
one it prints as a slashed zero is `0`, and one it prints as Varies is `varies`. The Enhanced
Strength description sets its initial cost at twice the Strength points added and its maintenance
at the number of points added per round; the costs of Domination and Mass Domination depend on
the subject, and Superior Invisibility's initial cost on how many creatures it protects. Page 13
says all characters begin as first-level psionicists, and page 22 that all intelligent creatures
on Athas have some psionic ability.

The game differs (FND-MAGIC-007, FND-MAGIC-008, FND-MAGIC-009): the power check is a d20 and the
activation cost is RULE-MAGIC-014's; Control Body, Life Draining and Cell Adjustment cannot be
maintained; Tower of Iron Will, Intellect Fortress, Mental Barrier and Thought Shield are
maintained at 0; Enhanced Strength's maintenance is a flat 8; Domination and Mass Domination cannot
be maintained; Superior Invisibility costs a flat 5. Every character starts with powers, as page
13 says, but a character who is not a psionicist gets the five powers of its one chosen
discipline, and they never rise with its levels.

## Differences between builds

None known.

## Open questions

- What overlay 193 `+0000` does when a kept power is applied again, and what its byte argument 1
  selects (FND-MAGIC-008, Q-MAGIC-013).
- Which effect sets bit 0x80 of byte 12 of the flag block, which keeps a member's powers a second
  time at its turn (FND-MAGIC-008, Q-MAGIC-023).
- When the stored time `4F49:0002` can be more than 60 ahead of the clock, the only way
  `2D40:37C1` ends the paid powers outside a fight (FND-MAGIC-008, Q-MAGIC-025).
- Whether a run shows a kept power's upkeep taken each round and a 0 upkeep power kept for nothing,
  and a new non-psionicist's greatest PSPs rising after its first fight (FND-MAGIC-008,
  FND-MAGIC-010, Q-MAGIC-026).
