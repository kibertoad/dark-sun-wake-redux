---
id: RULE-COMBAT-004
title: G and W make the party member whose turn it is guard or wait, and Q opens the end-of-move menu
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-COMBAT-023, FND-COMBAT-024, FND-COMBAT-025, FND-COMBAT-022, FND-COMBAT-008, FND-ACTOR-003, FND-PARTY-013, FND-COMBAT-026, FND-AI-004, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-COMBAT-005, RULE-AI-001, FMT-COMBAT-001, FMT-ACTOR-004]
---

## Summary

During combat, when it is a party member's turn, `G` makes that member guard and `W` makes it
wait, with a message naming the member, and `Q` opens the menu that ends the member's move.
Outside combat these keys do nothing.

## When it runs

`combat_key` runs when the player presses a key while the map is shown, before the keys that
open screens.

## Parameters

`key_word`, the BIOS keyboard word of the key: the scan code in the high byte and the character
in the low byte, so `0x2247` for `G` and `0x2267` for `g`.

## Inputs

`combat_state`, `turn_combatant`, `g_57E0_143C`, `combatants`, `object_slots`, `view_left` and
`view_top`.

## Procedure

```text
define combat_key(key_word):
    let c = turn_combatant
    if key_word == 0x2247 or key_word == 0x2267:
        if combat_state != 0 and c < 4:
            emit NamedMessageShown(message_guards, combatants[c].name)
            fn_5671_0093(c, 1)
    else if key_word == 0x1157 or key_word == 0x1177:
        if combat_state != 0 and c < 4:
            emit NamedMessageShown(message_waits, combatants[c].name)
            fn_5671_0093(c, 2)
    else if key_word == 0x1051 or key_word == 0x1071:
        if key_word == 0x1071 and c >= 4:
            return
        if g_57E0_143C == 0 and c >= 4:
            return
        if combat_state != 0:
            let slot = object_slots[c]
            call RULE-COMBAT-005(slot.x - view_left, slot.y - view_top)
```

## Outputs

No return value. `G` emits `NamedMessageShown` with `message_guards` and the member's name and
then runs `fn_5671_0093` with 1; `W` does the same with `message_waits` and 2; `Q` or `q` runs
RULE-COMBAT-005 at the member's place on screen.

## Edge cases

`G`, `W` and `q` do nothing when `turn_combatant` is 4 or more, which is the case in an enemy's
turn. `Q` in upper case also works then when `g_57E0_143C` is not 0. Before the combat test, `Q`
and `q` run a callback the caller of the key routine may pass, which this rule does not cover.
`N`, `P` and Space do not reach this routine when the resident key routine of FND-AI-004 handles
the key first: it passes `N` and `P` to overlay 174 during a party member's turn in combat, and
Space runs RULE-AI-001. Only keys that routine does not handle come here, where `N` does nothing,
`P` does what `O` does, and Space posts an event for the selected character box (FND-COMBAT-025).

## What the sources say

SRC-MANUAL-1994, page 77, lists the combat hotkeys: `G` guard, `N` next target, `P` previous
target, `Q` quit turn, `W` wait, and Space to turn off computer control. The routine of
FND-COMBAT-025 handles `G`, `W` and `Q` as the manual says, with `Q` opening a menu rather than
ending the turn at once. The resident routine of FND-AI-004 handles `N`, `P` and Space before it.

## Differences between builds

None known.

## Open questions

- What `fn_5671_0093` does with 1 and 2, that is what guarding and waiting change (Q-COMBAT-006).
- What overlay 174 does with `N` and `P`, which screens pass keys to the resident routine of
  FND-AI-004 before this one, and what the callback of `Q` is (Q-COMBAT-004).
- What `g_57E0_143C` is (Q-COMBAT-004).
