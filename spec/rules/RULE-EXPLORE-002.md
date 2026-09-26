---
id: RULE-EXPLORE-002
title: Key 5 shows the whole party on the map and key 6, outside combat, shows only the leader
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-EXPLORE-001, FND-EXPLORE-002, FND-EXPLORE-003, FND-EXPLORE-004, FND-EXPLORE-005, FND-AI-004, FND-ACTOR-003, FND-ACTOR-005, FND-COMBAT-013, FND-COMBAT-022, FND-COMBAT-023, FND-PARTY-013, FND-SOUND-010, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-EXPLORE-003, RULE-COMBAT-001]
---

## Summary

Outside combat the party is shown as its leader alone unless the player asks for the whole party.
Key 5 brings the other members onto the map near the leader. Key 6 takes every member but the
leader off the map, freeing the cells they held, and does nothing during combat. The game starts
with the leader alone, unless it was started with the switch `-A`.

## When it runs

`show_whole_party` runs when the player presses 5, and `show_leader_only` when the player presses
6, each with `redraw` 1, through the key routine that also takes the keys 1 to 4 (FND-AI-004,
FND-EXPLORE-005).

## Parameters

`redraw`, a `UINT8`: when it is not 0 the map is redrawn afterwards.

## Inputs

`party_shown`, `leader`, `combat_state`, `g_57E0_1440`, `object_slots` and `object_entries`.

## Procedure

```text
define is_hideable_member(i: UINT16) -> UINT8:
    if i == leader or object_slots[i].entry_index == 0xFFFF:
        return 0
    return object_entries[object_slots[i].entry_index].flags & 8 != 0

define show_whole_party(redraw: UINT8):
    if party_shown == 1:
        return
    party_shown = 1
    for i in 0..4:
        if is_hideable_member(i):
            fn_28C9_2C9E(i, object_slots[leader])
    if redraw != 0:
        fn_2C5F_0271(g_57E0_1440 == 1)
        fn_2C5F_0139()
        fn_28C9_2A81(1)

define show_leader_only(redraw: UINT8):
    if combat_state != 0:
        return
    for i in 0..4:
        let slot = object_slots[i]
        if is_hideable_member(i) and slot.unk_00 & 0x80 == 0:
            slot.unk_00 = slot.unk_00 | 0x80
            remove_footprint(i, slot.position_x >> 4, slot.position_y >> 4)
            move_object(i, 0x840, 0x660)
    if redraw != 0:
        fn_2C5F_0139()
        fn_28C9_2A81(1)
    party_shown = 0
```

## Outputs

`show_whole_party` sets `party_shown` to 1 and shows the hidden members. `show_leader_only` sets
bit 7 of `unk_00` of each member it hides, frees its cells (RULE-EXPLORE-003), moves it to the
position `(2112,1632)`, whose cell is outside the map, and sets `party_shown` to 0. Neither returns
a value.

## Edge cases

`show_whole_party` does nothing when the whole party is already shown, and it passes every member
to `fn_28C9_2C9E` whether or not it was hidden. `show_leader_only` in combat returns before it
changes `party_shown`. A member whose entry does not have bit 3 of its flags set is neither hidden
nor shown by these keys.

## What the sources say

SRC-MANUAL-1994, page 4: by default only the leader appears on the map, the other three appear when
combat begins, and Collapse Party in the Game Menu shows all four at all times. Page 77: 5 shows
all the characters while moving and 6 the leader alone.

## Differences between builds

None known.

## Open questions

- What `fn_28C9_2C9E` does beyond clearing the member's bit 7, and so where the members appear; and
  what `fn_2C5F_0271`, `fn_2C5F_0139` and `fn_28C9_2A81` redraw (FND-EXPLORE-005, Q-EXPLORE-004).
- What `g_57E0_1440` stands for; the key routine's arrow keys set it to 1 (FND-COMBAT-013,
  Q-EXPLORE-004).
- What bit 3 of an entry's `flags` stands for, which is 0 in every shipped entity record, and
  which stores to `party_shown` belong to Collapse Party and to loading a game (FND-EXPLORE-005,
  Q-EXPLORE-004).
