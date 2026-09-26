---
id: RULE-MAGIC-003
title: Activating and maintaining a psionic power
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: []
---

## Summary

Every psionic power names an ability and an amount added to or taken from the character's score
in it; the result is the power score. Activating a power always needs a power check, which
succeeds with a chance of 5 percent per point of power score. A power that activates costs its
initial cost in psionic strength points (PSPs), and one that fails costs half of it. A power with a
maintenance cost can be kept running from round to round, paying that cost each round with no
new check.

## When it runs

When a character activates a psionic power from the Cast Spells / Use PSI screen (SCR-UI-009), and
each round after that while the power is maintained.

## Parameters

`attribute`, the character's score in the ability the power names. `modifier`, the amount the
power's description adds to that score, for example -3 for "Con -3". `initial_cost`, the power's
initial cost in PSPs. `maintenance_cost`, the power's maintenance cost in PSPs per round, 0 when
the description gives none.

## Inputs

None beyond the parameters.

## Procedure

```text
define power_score(attribute, modifier):
    return attribute + modifier

define power_check_percent(attribute, modifier):
    return power_score(attribute, modifier) * 5

define power_cost(initial_cost, succeeded):
    if succeeded:
        return initial_cost
    return initial_cost / 2

define can_maintain(maintenance_cost):
    return maintenance_cost > 0
```

## Outputs

`power_check_percent` returns the chance, in percent, that the power check succeeds.
`power_cost` returns the PSPs the activation takes from the character. `can_maintain` returns true
when the power can be kept running, at `maintenance_cost` PSPs a round. None of them changes
state; the caller makes the check and takes the PSPs.

## Edge cases

A power score of 20 or more gives a chance of 100 percent or more, and one of 0 or less a chance
of 0 or less. Half an odd initial cost is not a whole number; the manual does not say how it is
rounded.

Cell Adjustment and Mind Blank print a maintenance cost of zero rather than NA, and the Mind Blank
description says it costs nothing to maintain. The procedure's `can_maintain` treats 0 as no
maintenance cost, so for these two a zero cost would need to count as maintainable.

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

## Differences between builds

None known.

## Open questions

- How the game makes the power check, how it rounds half an odd initial cost, and what it does
  when the character has fewer PSPs than the cost (Q-MAGIC-004).
- How many PSPs a character has at each level, and which powers a character who is not a
  psionicist can use (Q-MAGIC-004).
- Whether the game keeps the manual's costs, how it prices the powers the manual gives as Varies,
  and whether a zero maintenance cost lets a power be maintained (Q-MAGIC-004).
