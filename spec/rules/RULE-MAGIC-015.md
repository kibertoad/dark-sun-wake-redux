---
id: RULE-MAGIC-015
title: A target ignores an effect of type word 0x400 when two rolls of 1 to its psionic defence score each beat the last power check's die
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-046, FND-RNG-025, FND-PARTY-112, FND-PARTY-104, FND-PARTY-210, FND-COMBAT-022, FND-AI-004, FND-PARTY-081, FND-EXPLORE-002, FND-COMBAT-052, FND-COMBAT-023, FND-MAGIC-007, FND-MAGIC-008, FND-COMBAT-072]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-MAGIC-014]
---

## Summary

`psionic_defence` decides, when overlay 197 `+1C52` tests a target against a type word of 0x400,
whether the target ignores the effect. Its defence score U starts as byte `+0x1D` of its combatant
record less 7. A party member raises it with the best defence mode it has in effect among powers
24, 27, 28, 30 and 33 that its points can pay for, the mode's rank plus half its cost, and pays
that cost. Another creature takes the byte `+0x1E` of its details record when that is higher. U is
at least 1. The target then rolls 1 to U twice, the second only when the first beat
`psionic_check_result`, the die of the last power check (RULE-MAGIC-014); when both beat it, the
target ignores the effect and `+1C52` returns 0.

## When it runs

Overlay 197 `+1C52` calls it last, when its type word argument is 0x400 (FND-RNG-046). `+1C52` is
called only from overlay 197 `+111A`, the hit routine's gate of FND-PARTY-112.

## Parameters

- `slot: INT16`: the target's object slot.
- `type: UINT16`: the type word `+1C52` passes, always 0x400.

## Inputs

`combatant_found`, `fn_2D40_3E64` (the combatant number `c`) and `combatant_details_of` (the
details number `e`) of the slot; `combatants` record `c`: byte `+0x1D` (`wisdom` of FMT-COMBAT-001) and the word
`+0x02` (`psionic_points`); `combatant_details` record `e`: byte `+0x1E` (`levels[0]` of
FMT-COMBAT-002); `psionic_ranks`, `psionic_powers`, `psionic_check_result`.

## Procedure

```text
define psionic_defence(slot, type) -> UINT8:
    if not combatant_found(slot):
        return 0
    let c = fn_2D40_3E64(slot)
    let e = combatant_details_of(slot)
    if type != 0x400:
        return 0
    let u: INT16 = INT16(combatants[c].wisdom) - 7
    if slot < 4:
        let kept: UINT16 = 0
        for p in [24, 27, 28, 30, 33]:
            if psionic_ranks[slot][p] & 1 != 0:
                let candidate: INT16 = psionic_rank(slot, p) + psionic_powers[p].cost / 2
                if candidate > u and psionic_powers[p].cost <= UINT16(combatants[c].psionic_points):
                    u = candidate
                    kept = psionic_powers[p].cost
        combatants[c].psionic_points = UINT16(combatants[c].psionic_points) - kept
    else:
        if INT16(combatant_details[e].levels[0]) > u:
            u = combatant_details[e].levels[0]
    if u < 1:
        u = 1
    # random_between draws once when u > 1 (RULE-RNG-001)
    if INT16(psionic_check_result) >= random_between(1, u):
        return 0
    # random_between draws once when u > 1 (RULE-RNG-001)
    if INT16(psionic_check_result) >= random_between(1, u):
        return 0
    return 1
```

The scan visits the powers in rising order and replaces the kept mode only with a higher
candidate, so of equal candidates the first is kept.

## Outputs

1 when the target ignores the effect, and `+1C52` then returns 0 in place of its percentage;
0 otherwise. A party member's points word loses the kept mode's cost whatever the rolls give.

## Edge cases

With U of 1 neither call draws, and both return 1, so the target ignores the effect only when the
die is 0 or less, that is after a failed or disrupted check. A die of U or more is never beaten.
The cost is taken before the rolls, even when the first roll ends the test. `psionic_check_result`
is whatever the last use of any power left, including a use by another slot.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Which `DATA` records reach `+1C52` with the type word 0x400, and so which effects the defence
  applies to (FND-RNG-046, Q-MAGIC-014).
- What byte `+0x1D` of the combatant record and byte `+0x1E` of the details record hold, and how
  `fn_2D40_3E64` maps a slot to its records (FND-RNG-046, FND-COMBAT-022, Q-MAGIC-014).
