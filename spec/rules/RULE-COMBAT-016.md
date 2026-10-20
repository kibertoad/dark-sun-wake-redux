---
id: RULE-COMBAT-016
title: A removed combatant of slot 4 or above that holds objects of kind 1 drops them into object 2200, placed at a random offset of -7 to 7 in each position word
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-041, FND-RNG-040, FND-PARTY-013, FND-ACTOR-005, FND-CONFIG-143, FND-RNG-035, FND-PARTY-051, FND-PARTY-133, FND-ACTOR-003, FND-EXPLORE-001, FND-EXPLORE-002]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-MAGIC-012]
---

## Summary

`drop_held_objects` is overlay 173 `+3136`. For a slot of 4 or more, at the first held object of
kind 1 it places object 2200 at the slot's position plus two draws of -7 to 7, and moves every
held object of kind 1 to the placed slot one at a time. When the list runs out it sets the
greatest byte `+0x12` of the moved items into the low three bits of byte `+5` of the placed
slot's entry in the table at `57E0:67B7`.

## When it runs

Overlay 206 `+0E1E` calls it at `+0FA4`, its only direct
caller (FND-RNG-041).

## Parameters

- `slot: INT16`: the removed combatant's object slot.

## Inputs

`object_slots` (`position_x`, `position_y`, `entry_index`); `object_slot_kinds`; the held-object
entries `fn_2D40_2196` lists, with their `kind` and `item`; byte `+0x12` of `item_records`; the
8-byte entries at the far pointer `57E0:67B7`.

## Procedure

```text
define drop_held_objects(slot):
    if slot < 4:
        return
    let greatest: INT16 = 0
    let placed = false
    let p: INT16
    repeat:
        let held = fn_2D40_2196(slot)
        let moved = false
        for entry in held while not moved:
            if entry.kind != 1:
                continue
            greatest = max(greatest, item_records[entry.item].unk_12)
            if not placed:
                placed = true
                # two draws of RULE-RNG-001's generator, scaled to 0..14; the first gives the row
                let ry: INT16 = (INT32(draw()) * 15) / 32768
                let y = object_slots[slot].position_y - 8 + ry + 1
                let rx: INT16 = (INT32(draw()) * 15) / 32768
                let x = object_slots[slot].position_x - 8 + rx + 1
                p = fn_31E0_0121(2200, -1, x, y, 0, UINT8(greatest), 7)
                if p == -1:
                    return
                if object_slot_kinds[p] == 0:
                    fn_31E0_05BB(object_slots[p].entry_index, 1, 0, 0)
                    return
            if fn_2D40_288C(held, entry, -1, fn_2D40_2196(p), 0, 4) == 0:
                return
            moved = true
    until not moved
    if placed:
        g_57E0_67B7[object_slots[p].entry_index].byte_5 = (byte_5 & 0xF8) | UINT8(greatest)
```

## Outputs

The placed object, the objects moved to it and its entry's byte `+5`.

## Edge cases

A slot with no held object of kind 1 makes no draw and places nothing. A call makes 0 or 2
draws. The offset is applied to the position words, not to cells. A failed placement, a placed
slot of kind 0, or a failed move ends the call before the store to byte `+5`. The greatest byte
passed to the placement is the greatest seen up to the first object of kind 1; the stored one is
the greatest over all the passes. The byte is OR-ed into the low three bits after they are
cleared, so a byte above 7 also sets higher bits.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What object 2200 is, what byte `+0x12` of an item and byte `+5` of the entry hold, what kind 1
  stands for, and what `fn_31E0_0121`'s other arguments, `fn_2D40_288C` and `fn_31E0_05BB` do
  (FND-RNG-041, Q-COMBAT-030).
- What the held-object entries of `fn_2D40_2196` hold beyond `kind` and `item`, what the other
  bytes of `item_records` hold, and what reads byte `+5` of `g_57E0_67B7`'s entries (FND-RNG-041,
  Q-COMBAT-030).
