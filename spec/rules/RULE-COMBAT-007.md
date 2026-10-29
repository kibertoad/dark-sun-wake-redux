---
id: RULE-COMBAT-007
title: What the difficulty setting changes in combat
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-COMBAT-065, FND-COMBAT-075]
conflicting: []
split_with: []
related: [RULE-CONFIG-002, RULE-COMBAT-002]
---

## Summary

The difficulty setting (RULE-CONFIG-002) gives every combatant outside the party, slot 4 or more,
`difficulty - 1` to hit: -1 on Easy, 0 on Balanced, +1 on Hard and +2 on Hideous
(FND-COMBAT-065). A creature whose details record is above 4 has its greatest hit points set to
50, 100, 150 or 200 percent of its resource's value, rounded down, when it is loaded, and the load
then gives every slot from 4 up current hit points equal to its greatest value (FND-COMBAT-075).
Scripts also read the setting, through function 21 of instruction 0x22.

## When it runs

`enemy_to_hit` runs in each attack of overlay 173 `+1CC1` (RULE-COMBAT-002).
`creature_max_hit_points` runs when overlay 187 `+039A` copies a details chunk of a `CHAR` load,
and `appearing_hit_points` at the end of that load (`2D40:000A`) for each slot (FND-COMBAT-075).

## Parameters

- `slot: INT16`: the attacker's slot.
- `difficulty`: 0 to 3.
- `record: INT16`: the details record number the load fills.
- `resource_max: UINT16`: the resource's details word `+0x08`, the greatest hit points.
- `max_hit_points: UINT16` and `hit_points: INT16`: the slot's greatest and current hit points
  after the load.

## Inputs

None beyond the parameters.

## Procedure

```text
define enemy_to_hit(slot, difficulty):
    if slot >= 4:
        return difficulty - 1
    return 0

define creature_max_hit_points(record, resource_max, difficulty):
    if record <= 4:
        return resource_max
    let p: INT32 = (difficulty - 1) * 50 + 100
    return UINT16((INT32(resource_max) * p) / 100)

define appearing_hit_points(slot, max_hit_points, hit_points):
    if slot >= 4:
        return max_hit_points
    return hit_points
```

## Outputs

The amount added to the attacker's to-hit bonus in RULE-COMBAT-002; the greatest hit points
stored in the details record; the slot's current hit points.

## Edge cases

Party slots 0 to 3 use records 0 to 3 and are never scaled; slot 4 uses record 4, is not scaled,
and still has its hit points set to the greatest. The scale keys on the record number, so allies
outside the party are scaled too. A creature already loaded keeps its values when the setting
changes, and each load scales the resource's value again, so the scale does not compound. A
resource value of 1 gives 0 on Easy, so the creature appears with 0 hit points at `combat_mark`
1. A scaled value above 65,535 keeps only its low 16 bits.

## What the sources say

SRC-MANUAL-1994, page 15: difficulty controls the level of difficulty in combat. SRC-GAMEFAQS-81038,
section 2.8: a new game begins on Balanced; Easy seems to give hostile creatures about half the hit
points they have on Balanced and Hideous about twice as many, and the change applies when a
creature appears, not to creatures already present. It says nothing of Hard, of rounding, or of the
party. The executable gives 50 and 200 percent, matching the FAQ, and 150 on Hard.

## Differences between builds

None known.

## Open questions

- What the 47 script calls of function 21 of instruction 0x22 do with the setting,
  including preservation of MAS/67's local counter through child calls
  (FND-COMBAT-075, FND-COMBAT-076, Q-COMBAT-007).
- Whether saved creatures' current and maximum hit points are scaled or reset again
  on loading a numbered saved game (FND-COMBAT-075, FND-SAVE-022, Q-COMBAT-044).
- Whether a numbered save can contain a populated temporary presentation slot 319 and
  what its restoration changes (FND-COMBAT-078, Q-COMBAT-045).

- Which event and field producers admit the numbered-load caller and reader paths
  (FND-COMBAT-081, FND-COMBAT-098, Q-COMBAT-046).
- Which script states and other incoming routes admit the independent numbered
  reader, beyond its argument-width and current-region gates
  (FND-COMBAT-099, Q-COMBAT-055).
- Which capacities, resource lengths and pointer lifetimes govern saved combatant and
  details restoration (FND-SAVE-015, FND-SAVE-021, Q-COMBAT-047).
- Which callbacks reached during loading can affect health or continuation selection
  (FND-COMBAT-080, FND-CONFIG-189, Q-COMBAT-048).
- Whether APFM callback fields can be nonzero on an admitted load path, and which
  concrete targets that permits (FND-COMBAT-087, Q-COMBAT-049).
- Which archive, selected resource and transfer outcomes admit the APFM acquisition
  call during loading (FND-COMBAT-089, Q-COMBAT-050).
- Which live records, storage lifetimes and remaining writes supply the callback
  fields through the selector and service pointers (FND-COMBAT-087,
  FND-COMBAT-088, FND-COMBAT-090, FND-COMBAT-091, FND-COMBAT-092, FND-COMBAT-093,
  FND-COMBAT-094,
  FND-UI-068, Q-COMBAT-051).
- Which producer states and service order admit the modes that dispatch the two
  callback fields during loading (FND-CONFIG-189, Q-COMBAT-052).
- Which source bytes, allocated extent and complete or failed transfer outcomes
  supply the window passed into registration (FND-COMBAT-040, FND-COMBAT-093,
  FND-COMBAT-094, FND-COMBAT-095, FND-COMBAT-096, Q-COMBAT-053).
- Which actual caller and producer paths admit the archive/directory state used
  by the window request (FND-COMBAT-096, FND-COMBAT-097, Q-COMBAT-054).
