---
id: RULE-COMBAT-010
title: Five DATA numbers used by a combatant numbered 48 or more take effect only on a to-hit roll against its THAC0 less the target's Armor Class
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-032, FND-PARTY-112, FND-PARTY-091]
conflicting: []
split_with: []
related: [RULE-COMBAT-002, RULE-RNG-001]
---

## Summary

When the hit routine applies `DATA` 18, 53, 121, 169 or 277 from a caster numbered 48 or more,
the caster rolls 1 to 20 first: a 20 lets the hit go on, a 1 stops it, and any other roll lets it
go on when the roll is at least the caster's THAC0 less the target's Armor Class. A caster
numbered below 48 makes no roll.

## When it runs

`data_hit_roll_passes` runs in the hit routine's first gate, after that gate's earlier tests have
let the hit go on (FND-PARTY-112), for every target the hit routine is given; when it returns
false the hit routine ends for that target before its saving throw (FND-RNG-032).

## Parameters

`data_number`, the `DATA` number the hit routine applies. `caster`, the caster's combatant number.
`caster_thac0`, the signed byte `+0x16` of the caster's combatant record. `armour_class`, the
target's Armor Class as overlay 173 `+2C00` gives it for the target and the caster.

## Inputs

None beyond the parameters.

## Procedure

```text
define data_hit_roll_passes(data_number, caster, caster_thac0, armour_class):
    if data_number != 18 and data_number != 53 and data_number != 121 and data_number != 169 and data_number != 277:
        return true
    if caster < 48:
        return true
    # one draw of RULE-RNG-001's generator, scaled to 1..20
    let roll = (INT32(draw()) * 20) / 32768 + 1
    if roll == 20:
        return true
    if roll == 1:
        return false
    return caster_thac0 - armour_class <= roll
```

## Outputs

True when the hit goes on to the next gate, false when it ends for this target.

## Edge cases

Each value from 1 to 20 comes from 1638 or 1639 of the 32768 draw values. No bonus or penalty
changes the THAC0 here, unlike the attack routine (FND-PARTY-091). A target whose Armor Class is
at least the caster's THAC0 less 1 is hit on every roll but 1.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Which spells, powers or items the five `DATA` numbers are, and which combatants are numbered 48
  or more (Q-COMBAT-025).
- Whether overlay 173 `+2C00` gives the target's Armor Class for the arguments the gate passes
  (Q-COMBAT-025).
