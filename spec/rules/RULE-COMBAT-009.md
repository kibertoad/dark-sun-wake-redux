---
id: RULE-COMBAT-009
title: The status panel names the character's first effect, putting the named effects before the others
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-COMBAT-022, FND-COMBAT-027, FND-COMBAT-029]
conflicting: []
split_with: []
related: [FMT-COMBAT-003, SCR-COMBAT-001]
---

## Summary

The third line of the combat status panel names one category-zero effect
the character is under, or reads `Okay` when there is none to name.

## When it runs

`shown_effect` runs each time the status panel is drawn for a character whose `combat_mark` is 1
(SCR-COMBAT-001).

## Parameters

`c`, the combatant, `turn_combatant` when the panel calls it.

## Inputs

The prepared effect membership bitmap and category-zero list from
`fn_573B_0025` and `fn_573B_002A` for `c` (FND-COMBAT-029). Matching rows
retain their table order and repeated IDs. The prepared bitmap and table
must have admitted independent storage; their complete producer/lifetime
contract remains open.

## Procedure

```text
define effect_key(effect):
    if effect == 0 or effect == 12 or effect == 88 or effect == 93 or effect > 108:
        return 0
    return 1

define shown_effect(c):
    let effects = fn_573B_002A(c) # category zero, using the preceding prepared bitmap
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
in the list. Records 12 and 88 have empty names, so the line can be blank.
With the shipped classification bitmap, effect 93 is category one and
does not enter this caller's list, even though its generic sort key is
zero. Only the first 35 collected effects are sorted; the collector's
separate append cap is 113 (FND-COMBAT-029).

## What the sources say

SRC-MANUAL-1994, page 6, says Look in combat shows a monster's type and state, and does not
describe this panel.

## Differences between builds

None known.

## Open questions

- Complete runtime inputs of `fn_573B_0025` and `fn_573B_002A`, including
  effect-row/count storage, sorted-object admission,
  ID bounds and admitted removal/transfer callers remain
  Q-COMBAT-009. FND-COMBAT-029 answers the local preparation/collection
  branches, but not all admitted producers or other callers.
  FND-COMBAT-031 supplies the common insertion's conditional prepend order
  within an owner group and field/count writes; other mutation paths and
  the full count/ID invariant remain part of that item.
  FND-COMBAT-032 supplies local removal compaction/order and its full
  boundary-row backing requirement; handler effects and admitted removal
  callers remain Q-COMBAT-009.
  FND-COMBAT-033 supplies the saved-row relocation count pair and
  conditional threshold-selection/order; its admitted inputs remain
  Q-COMBAT-009.
  FND-COMBAT-034 supplies the explicit effect-row/count save-source
  pointers and shipped byte lengths, with runtime preparation and
  restoration still open under Q-COMBAT-009.
  FND-COMBAT-035 identifies the local full-resource restore requests and
  effect-storage-before-count ordering, retaining admitted resources,
  backing and postprocessing under Q-COMBAT-009.
- Category bitmap producers, initialization and lifetime
  remain Q-COMBAT-011; FND-COMBAT-029 supplies only their local uses and
  preparation.
  FND-COMBAT-034 separates two neighboring-table writers and that
  table's save source from the classifier prefix under admitted counts;
  it does not close bitmap writers or restoration.
  FND-COMBAT-035 supplies that numbered source's local restore request;
  complete bitmap restoration coverage remains Q-COMBAT-011.
  FND-COMBAT-037 accounts for the explicit relocated segment-reference
  candidates and separates the neighboring records conditionally;
  runtime count admission and nonliteral/bulk writer coverage remain.
- Prepared membership bitmap producers and content lifetime through
  collection remain Q-COMBAT-013, split from the category-bitmap question.
  FND-COMBAT-029 and FND-COMBAT-036 supply local preparation/collection
  operations and selector buffer arguments, not every intervening writer.
- DS/SS identity, caller buffer lifetime and output/bitmap/table
  separation for `fn_573B_0025` and `fn_573B_002A` remain
  Q-COMBAT-012, with the local limits recorded by FND-COMBAT-029.
  FND-COMBAT-036 supplies the selector's exact frame and mixed-segment
  argument/access provenance. Its flag-one single collection fits the
  local pre-bitmap span under admitted matching DS/SS; complete native
  storage admission and the other exported caller path remain open.
- A collected number above 112 would index beyond `effect_names`; the
  admitted ID domain remains Q-COMBAT-009.
- (No item: the delivered resident-only emulated-call harness cannot reach
  the overlay collector, preparer or priority sorter.)
