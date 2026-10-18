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

- Which effect IDs the rows hold when `fn_573B_0025` and `fn_573B_002A` collect them, and whether
  a stored ID can exceed 112 and index beyond `effect_names`. FND-COMBAT-029 reads the collection,
  FND-COMBAT-031 the insertion's field writes and FND-COMBAT-046 the shipped `DATA` effect bytes
  with their one code 127 (Q-COMBAT-009).
- The bound of the effect-row count over all its writers. FND-COMBAT-031, FND-COMBAT-032 and
  FND-COMBAT-033 read the insertion, removal and transfer writers locally, and FND-COMBAT-045
  shows resident backing for the boundary row at counts 0 to 192 (Q-COMBAT-017).
- What loading a saved game puts in the rows and count. FND-COMBAT-034, FND-COMBAT-035,
  FND-COMBAT-043 and FND-COMBAT-044 read the save sources and both restore callers' local routes
  (Q-COMBAT-018).
- Which rows and count a new game begins with. At program start the count is the load image's
  zero and nothing before START GAME's party load writes the count or a row; no writer sets the
  count to a constant (FND-COMBAT-053). With the count at 0 the removal and transfer entries
  change nothing, and every route a walk with 41 transfers unresolved read from the gate
  routine's later calls to an insertion passes the area-effect routine overlay 193 +12A8, which
  runs only for a slot made from object 147 or one it marked itself; only `RGN03B.GFF#ETAB/59`
  places object 147 (FND-COMBAT-054). With those transfers resolved, the insertion is also
  reached through the script dispatch, the window hook and an application frame callback
  (FND-EXE-592). A new game loads region 50, whose `ETAB` does not place object 147, and no
  literal placement on its first script's route names it (FND-UI-050); after startup the region
  word changes only through a saved-game load or script opcode 0x5E with a first operand of
  32766, which that route does not hold (FND-UI-051). No placement on that script route, whichever
  way its conditions go, names object 147, and the party's own objects are 300 to 313
  (FND-COMBAT-055). Whether a script registered by a trigger instruction places object 147, or a
  callback registered on the way inserts a row, remains open (FND-EXE-592, Q-COMBAT-019,
  Q-UI-020); which rows survive from
  an earlier game in the same run remain (Q-COMBAT-024).
- Which routines insert, remove and transfer rows, with which arguments. FND-COMBAT-039 reads one
  removal caller, the effect window's cache action (Q-COMBAT-020).
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
  FND-COMBAT-038 supplies the other direct collector caller's appends,
  late clamps and cache copy; admitted aggregate writes and lifetime remain.
- DS/SS identity, caller buffer lifetime and output/bitmap/table
  separation for `fn_573B_0025` and `fn_573B_002A` remain
  Q-COMBAT-012, with the local limits recorded by FND-COMBAT-029.
  FND-COMBAT-036 supplies the selector's exact frame and mixed-segment
  argument/access provenance. Its flag-one single collection fits the
  local pre-bitmap span under admitted matching DS/SS; complete native
  storage admission and the other exported caller path remain open.
  FND-COMBAT-038 adds the icon consumer's corresponding local frame
  and pointer/cache lifetimes; it does not establish all caller storage.
- (No item: the delivered resident-only emulated-call harness cannot reach
  the overlay collector, preparer or priority sorter.)
