---
id: RULE-ITEM-009
title: A corroding hit destroys the first held item at position 0, 6 or 9 at once when its word 0x10 is 0, and otherwise on a d20 below 10 less that word
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-035, FND-ITEM-010, FND-ITEM-009, FND-COMBAT-023, FND-CONFIG-143, FND-PARTY-051, FND-PARTY-217, FND-PARTY-221, FND-ITEM-013, FND-PARTY-214, FND-PARTY-218, FND-ITEM-014, FND-ITEM-017, FND-ITEM-018, FND-MAGIC-024, FND-SOUND-007, FND-EXE-602, FND-ITEM-028, FND-ITEM-031, FND-ITEM-032, FND-COMBAT-032, FND-COMBAT-033]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-ITEM-007]
---

## Summary

`armour_corrodes` goes through the target's held-object entries of kind 1 in order. For each whose
item has signed byte `+0x13` of 0, 6 or 9 it takes the item's word `+0x10`: 0 removes the entry
with no roll; any other value rolls a d20, which removes it when below 10 less that word. A removal
shows that armour is corroded and returns 1; a roll that does not remove goes on to the next entry.
After the last entry it returns 0.

## When it runs

The hit routine's effect switch runs it on the target for `DATA` 275 and 283, and for `DATA` 103
when `weapon_corrodes` returned 0 (FND-ITEM-010).

## Parameters

- `slot: INT16`: the target's object slot.

## Inputs

The held-object list `fn_2D40_2196` gives for the slot, and byte `+0x13` and word `+0x10`, both
signed, of each listed item's record in `item_records`.
Byte +0x13 is the signed inventory position, as the inventory projection
reads it and the conditional transfer writes it (FND-PARTY-217, FND-PARTY-221). The numeric condition below tests position codes
0, 6 and 9; it does not test the shared item-type index at word +0x0A.
The individual positions' complete placement domains remain open.

## Procedure

```text
define armour_corrodes(slot):
    let list = fn_2D40_2196(slot)
    for e in 0..count(list):
        if list[e].kind != 1:
            continue
        let r = item_records[list[e].item]
        let position: INT16 = INT8(r[0x13])
        if position != 0 and position != 6 and position != 9:
            continue
        let w: INT16 = INT16(r.word(0x10))
        if w != 0:
            # roll_sum draws once (RULE-RNG-001); the total is compared as a signed word
            if INT16(roll_sum(1, 20)) >= 10 - w:
                continue
        emit MessageShown("Armor is corroded")
        fn_2D40_2452(list, e)
        return 1
    return 0
```

## Outputs

1 when an entry was removed, 0 otherwise. The switch then sets bit 0x02 of the hit's flags byte
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

For admitted aligned effect rows, valid counts/indices, complete source
spans and preserved copy storage, the known row moves do not introduce new
active code values. Insertion retains the old active codes and adds its
supplied code (FND-ITEM-032). Removal's extra boundary-row copy lands outside
the reduced active count (FND-COMBAT-032). Owner transfer preserves the
saved row's code and replaces only its owner (FND-COMBAT-033). Thus these
moves preserve an existing active-code bound under those assumptions; they
do not establish that bound initially or validate restored rows. The known
insertion route's conditional 1..127 bound still permits bitmap offset 15,
so it does not establish a fifteen-byte output span (FND-ITEM-031/032).
Other producers, copy admission and frame aliases remain Q-ITEM-013.

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

A call draws once for each entry it reaches whose position matches and whose word is not 0, so it can
draw several times and still return 0. An item whose word `+0x10` is 0 is removed at once, before
any later entry. A word of 9 or more is never removed by the roll, and -11 or less always is.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Complete recursive removal by `fn_2D40_2452` and storage admission remain Q-ITEM-004.
- Which admitted items have position values zero, six and nine and the
  writer/consumer contract of word +0x10 remain Q-ITEM-006 (FND-RNG-035).
- Near-pointer segment identity is Q-ITEM-011; frame/alias admission is
  Q-ITEM-013; feedback effects are Q-ITEM-012;
  other incoming routes remain Q-ITEM-010. Known direct local consumers are
  recorded by FND-ITEM-028.
- Which entries `fn_2D40_2196` lists for a slot beyond their kind and item words, and what the
  other fields of `item_records` hold (FND-CONFIG-143, Q-ITEM-004).
