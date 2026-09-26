---
id: RULE-COMBAT-009
title: The status panel names the character's first effect, putting the named effects before the others
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-COMBAT-022, FND-COMBAT-027]
conflicting: []
split_with: []
related: [FMT-COMBAT-003, SCR-COMBAT-001]
---

## Summary

The third line of the combat status panel names one effect the character is under, such as
`Blessed` or `Poisoned`, or reads `Okay` when there is none to name.

## When it runs

`shown_effect` runs each time the status panel is drawn for a character whose `combat_mark` is 1
(SCR-COMBAT-001).

## Parameters

`c`, the combatant, `turn_combatant` when the panel calls it.

## Inputs

What `fn_573B_002A` returns for `c`.

## Procedure

```text
define effect_key(effect):
    if effect == 0 or effect == 12 or effect == 88 or effect == 93 or effect > 108:
        return 0
    return 1

define shown_effect(c):
    let effects = fn_573B_002A(c)
    let n = min(count(effects), 35)
    if n == 0:
        return 0
    let swapped = true
    while swapped:
        swapped = false
        let i = 1
        while i < n:
            if effect_key(effects[i]) > effect_key(effects[i - 1]):
                let held = effects[i]
                effects[i] = effects[i - 1]
                effects[i - 1] = held
                swapped = true
            i = i + 1
    return effects[0]
```

## Outputs

The number of the effect to show, 0 to 255, where 0 means none. The panel shows `Okay` for 0 and
otherwise the `name` of that record of `effect_names`.

## Edge cases

The sort moves every effect with key 1 ahead of every effect with key 0 and keeps the order of
each group, so the result is the first effect with key 1, or when there is none the first effect
in the list. That can be `Draining Spells` (93), or record 12 or 88, whose names are empty, so the
line is then blank. Only the first 35 effects are sorted.

## What the sources say

SRC-MANUAL-1994, page 6, says Look in combat shows a monster's type and state, and does not
describe this panel.

## Differences between builds

None known.

## Open questions

- What list `fn_573B_002A` returns and in what order, and what `fn_573B_0025`, called just
  before it, prepares (Q-COMBAT-008).
- A number above 112 would index past the end of `effect_names`, into bytes that are 0 (Q-COMBAT-008).
