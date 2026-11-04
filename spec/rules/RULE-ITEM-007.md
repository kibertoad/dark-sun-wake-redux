---
id: RULE-ITEM-007
title: A corroding hit destroys a picked held item when a d20 falls below 8 less the item's byte 0x16
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-035, FND-ITEM-010, FND-ITEM-009, FND-COMBAT-023, FND-CONFIG-143, FND-PARTY-051, FND-COMBAT-065, FND-EXE-600, FND-PARTY-248, FND-ITEM-013, FND-PARTY-214, FND-PARTY-218, FND-ITEM-014, FND-ITEM-017, FND-ITEM-018, FND-MAGIC-024, FND-SOUND-007, FND-EXE-602, FND-ITEM-028]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-ITEM-009]
---

## Summary

`weapon_corrodes` has overlay 173 `+2E1B` pick an entry of the target's held objects. With none
it returns 0 and makes no roll. Otherwise it rolls a d20; below 8 less the item's signed byte
`+0x16`, it shows that a weapon is corroded, removes the entry and returns 1, and otherwise it
returns 0.

## When it runs

The hit routine's effect switch runs it on the target for `DATA` 103, followed by
`armour_corrodes` when it returns 0, and for `DATA` 284 (FND-ITEM-010).

## Parameters

- `slot: INT16`: the target's object slot.

## Inputs

The held-object list `fn_2D40_2196` gives for the slot, and the signed byte `+0x16` of the item's
record in `item_records`.

For the recorded selector arguments -2 and 1, selection takes the first
qualifying kind-one entry: following its parent links to a kind-two entry
must give that ancestor's word +6 as 0x11, and its shared item-type byte +6
must equal one. The item-type record is selected by the attached item's
word +0x0A. This is the recorded local selection predicate, not an admission
proof for arbitrary lists or indices (FND-COMBAT-065, FND-EXE-600).

The signed byte +0x16 is an item bonus displayed after its name and used
by the recorded combat consumers. That does not establish every writer,
effect gate or use of the byte (FND-PARTY-248).

## Procedure

```text
define weapon_corrodes(slot):
    let list = fn_2D40_2196(slot)
    let e: INT16 = fn_5671_0084(-2, list, 1)
    if e == -1:
        return 0
    let b: INT16 = INT8(item_records[list[e].item][0x16])
    # roll_sum draws once (RULE-RNG-001); the total is compared as a signed word
    if INT16(roll_sum(1, 20)) >= 8 - b:
        return 0
    emit MessageShown("Weapon is corroded")
    fn_2D40_2452(list, e)
    return 1
```

## Outputs

1 when the entry was removed, 0 otherwise. The switch then sets bit 0x02 of the hit's flags byte
on a 1 (FND-ITEM-010).

When recursive deletion reaches its kind-one branch, it passes the attached
record index to a helper that links the record into the free list and clears
its identifier word. This is the helper's own local effect under stable,
disjoint storage (FND-ITEM-013). The deletion body's local order is kind
release, conditional parent-field request, map clear, conditional reparenting, then
recursive calls for retained children. Its reparenting failure branch can
be non-returning; admitted state and whole-callee effects remain open
(FND-ITEM-014, FND-PARTY-214, FND-PARTY-218).

Under admitted near-pointer, frame and record storage, the hit wrapper uses
result mask 0x02 set with damage mask 0x01 clear to select presentation
requests. Otherwise a zero combatant record word +0 also selects that path.
Mode gates control its subsequent requests, and the wrapper returns the flags
byte. These gates do not prove successful visible feedback or later caller
behavior (FND-ITEM-017, Q-ITEM-010).

Known hit-call continuations handle that byte differently: one wrapper saves
and forwards it across queue processing; the queue continuation does not
explicitly interpret it; casting zero-extends and aggregates it, tests only
mask 0x01 at its recorded follow-up gate, and returns separate bookkeeping.
These local paths do not establish complete caller coverage or whole-child
preservation (FND-ITEM-018). The known direct outer continuations forward,
replace or independently interpret the byte; the remaining resident kind
dispatcher reloads its own bookkeeping byte (FND-ITEM-028). This settles
local register handling, while other incoming routes and frame admission
remain Q-ITEM-010, Q-ITEM-011 and Q-ITEM-013.

The first request selects the sound effect numbered by DATA byte +0x16.
Its routine applies sound-setting and driver gates, preferring a BVOC resource
before the numbered loose SOUND file (FND-SOUND-007). The mode-one visual
helper places and draws an object subject to its view and creation gates;
the alternate helper requests frame 4 of BMP 19001 through its slot, resource
and layout gates and recorded counter wait (FND-EXE-602). The visual number
comes from DATA word +0x14 or the recorded fallback (FND-MAGIC-024). These
requests do not guarantee audible or visible output. Remaining feedback
admission/effects are Q-ITEM-012; lower-level audio is Q-SOUND-002.

## Edge cases

Each call that finds an entry draws once. A byte `+0x16` of 0 removes the entry on 1 to 7, 7 chances
in 20; 7 or more never removes it, and -13 or less always does.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Complete recursive removal by `fn_2D40_2452` and storage admission remain Q-ITEM-004.
- Independent selected-field writers and admitted bonus states remain
  Q-ITEM-006; `fn_5671_0084` list/index admission is Q-ITEM-008. The local selector
  criteria and bonus uses are recorded above.
- Near-pointer segment identity is Q-ITEM-011; frame/alias admission is
  Q-ITEM-013; feedback effects are Q-ITEM-012;
  other incoming routes remain Q-ITEM-010. Known direct local consumers are
  recorded by FND-ITEM-028.
- Which entries `fn_2D40_2196` lists for a slot beyond their kind and item words, and what the
  other fields of `item_records` hold (FND-CONFIG-143, Q-ITEM-004).
