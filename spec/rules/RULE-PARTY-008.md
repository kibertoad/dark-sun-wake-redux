---
id: RULE-PARTY-008
title: The keys 1 to 4 choose the party leader
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994, FND-ACTOR-003, FND-COMBAT-008, FND-COMBAT-022, FND-COMBAT-023, FND-COMBAT-025, FND-EXPLORE-001, FND-EXPLORE-002, FND-PARTY-013, FND-PARTY-171, FND-PARTY-176, FND-PARTY-178, FND-PARTY-179]
conflicting: []
split_with: []
related: [SCR-UI-002]
---

## Summary

The party has a leader, who walks for the party and talks to other characters. Pressing 1, 2, 3
or 4 makes the character in that party slot the leader. The leader button beside each character
box of the View Character screen does the same.

## When it runs

`leader_for_key` runs for a key the resident key routine `28C9:0CFF` takes (FND-AI-004). The run
of FND-PARTY-178 reached it with the map shown, outside combat.

## Parameters

`key`, the character printed on the key the player pressed, as an ASCII code.

## Inputs

- `combatants`, the FMT-COMBAT-001 records, taken by the key's number.
- `object_slots` (FMT-ACTOR-004), for bit 5 of `unk_00` of the `turn_combatant` slot.
- `turn_combatant`, `combat_state` and `leader`.

## Procedure

```text
define leader_for_key(key):
    if key < 0x31 or key > 0x34:
        return
    slot = key - 0x31
    if combatants[slot].character_id == 0:                # no character: nothing
        return
    if object_slots[turn_combatant].unk_00 has bit 5 set:
        return
    turn_combatant = slot
    if combat_state != 0:                                        # in combat: no leader change
        return
    set_leader(slot)

define set_leader(slot):                                         # overlay 182 +1D3E
    if slot >= 4 or slot == leader:
        return
    if combatants[object_slot_combatants[slot]].combat_mark > 2:
        return
    leader = slot                                                # visible: the leader walks for the party
    turn_combatant = slot
    # When party_shown is 0, the new leader is shown at the old leader's place and the old
    # leader is hidden (FND-PARTY-179).
```

## Outputs

No return value. Sets `turn_combatant`, and outside combat `leader`, to the slot of the key when
the procedure gets that far; changes nothing for another key.

## Edge cases

A key whose slot holds no character changes nothing (FND-PARTY-176, FND-PARTY-178). In combat a
key changes `turn_combatant` and leaves `leader` alone (FND-PARTY-176). The party loader shows
the leader's slot and hides the others (FND-PARTY-177).

## What the sources say

SRC-MANUAL-1994, page 77, lists `1` to `4` as setting the corresponding character as leader.
Page 10 says the View Character screen has a leader button beside each character box, which
makes that character the one who leads when walking and talking to non-player characters, and
page 12 that the inventory screen has the same buttons. Keys 5 and 6 show all the characters or
the leader alone while moving (page 77).

## Differences between builds

None known.

## Open questions

- Whether a key tests the combatant record of its own number or the one `object_slot_combatants`
  names for its slot where the two differ: the code reads record `slot` (FND-PARTY-176), and the
  runs had them equal (No item: the parties the game builds put combatant `k` in slot `k`,
  FND-PARTY-013, so no party where they differ is known).
- Which screens pass keys to `28C9:0CFF`, so where the keys work (Q-PARTY-074).
- What a key's `set_leader` changes on screen beyond redrawing the view (`2C5F:0139`,
  FND-PARTY-130): it then runs `4328:0B4C` at once through `28C9:2A81(1)`, whose callees are not
  read (FND-PARTY-179, FND-PARTY-180, Q-PARTY-075).
