---
id: RULE-COMBAT-008
title: Saving, resting, adding a character and changing the leader are refused during combat
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-COMBAT-023, FND-COMBAT-024, FND-COMBAT-022, FND-COMBAT-025]
conflicting: []
split_with: []
related: [RULE-SAVE-001, RULE-PARTY-008]
---

## Summary

During combat the game will not save, rest or take a new character into the party, and the
leader can be changed only to the character whose turn it is. Each refusal shows a message.

## When it runs

`save_refused` runs when the player presses `F1`, `rest_refused` when the player rests,
`add_refused` when the player clicks the character box of an empty party slot, and
`leader_refused` when the player clicks a leader button, each before the action itself.

## Parameters

`slot`, for `leader_refused`, the party slot of the button clicked, 0 to 3.

## Inputs

`combat_state` and `turn_combatant`.

## Procedure

```text
define save_refused():
    if combat_state != 0:
        emit MessageShown(message_cant_save)
        return true
    return false

define rest_refused():
    if combat_state != 0:
        emit MessageShown(message_cant_rest)
        return true
    return false

define add_refused():
    if combat_state != 0:
        emit MessageShown(message_cant_add)
        return true
    return false

define leader_refused(slot):
    if combat_state != 0 and slot != turn_combatant:
        emit MessageShown(message_cant_change_leader)
        return true
    return false
```

## Outputs

True when the action is refused, after emitting `MessageShown` with the message. False otherwise,
and the action goes ahead.

## Edge cases

A click on the leader button of the character whose turn it is passes the test during combat, and
the leader then changes as it does outside combat (FND-COMBAT-023).

## What the sources say

SRC-MANUAL-1994 does not mention these refusals.

## Differences between builds

None known.

## Open questions

- Which values `combat_state` takes and when it is set (Q-COMBAT-003).
