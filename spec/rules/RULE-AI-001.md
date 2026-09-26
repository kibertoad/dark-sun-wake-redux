---
id: RULE-AI-001
title: The computer-control button toggles computer control of a party member unless it is locked, and Space turns it off for every unlocked member
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-AI-002, FND-AI-003, FND-AI-004, FND-COMBAT-008, FND-COMBAT-022, FND-COMBAT-023, FND-COMBAT-024, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-AI-002, RULE-COMBAT-004, FMT-COMBAT-001, SCR-UI-002, SCR-UI-008]
---

## Summary

Each party member has a computer-control setting. The small button beside the member's character
box turns it on or off, with a message saying which, unless the setting is locked, when the
button only says so. Space turns the setting off for every member whose setting is not locked.

## When it runs

`toggle_computer_control` runs when the player clicks one of the buttons `BUTN/11313` to
`/11316`. `release_computer_control` runs when the player presses Space and the resident key
routine of FND-AI-004 receives the key.

## Parameters

`slot`, for `toggle_computer_control`, the party slot of the button clicked, 0 to 3: the button
number less 11313.

## Inputs

`combatants` and `combatant_details`.

## Procedure

```text
define toggle_computer_control(slot):
    if combatant_details[slot].unk_0E == 0:
        return
    let member = combatants[slot]
    if member.control_locked == 1:
        emit MessageShown(message_control_locked)
    else if member.computer_control == 1:
        member.computer_control = 0
        emit MessageShown(message_control_off)
    else:
        member.computer_control = 1
        emit MessageShown(message_control_on)

define release_computer_control():
    for slot in 0..4:
        let index = fn_2D40_3E64(slot)
        if index >= 0 and combatants[index].control_locked == 0:
            combatants[index].computer_control = 0
```

## Outputs

No return value. `toggle_computer_control` changes `computer_control` of the member's record
and emits `MessageShown`; it also gives the button frame 5 when it turns the setting off and
frame 4 when it turns it on. `release_computer_control` changes `computer_control` in the
records and emits nothing.

## Edge cases

A button whose slot has `unk_0E` equal to 0 does nothing. The button does not look at
`combat_state`, so the setting can be changed outside combat. `release_computer_control` does not
redraw any button and does not look at `combat_state` either. The hover text of the buttons
(regions 11209 to 11212) reads `COMPUTER CONTROL IS ` followed by `LOCKED ` when the setting is
locked and then `ON` or `OFF`, and `INACTIVE CHARACTER` for a slot whose `unk_0E` is 0
(FND-AI-003).

## What the sources say

SRC-MANUAL-1994, page 12: beside each character box are two small buttons, one for computer
control and one for the leader; clicking computer control places the character's actions under
the computer's control during combat. Page 13 says the same of the inventory screen, and page
77 gives Space as turning off computer control in combat. The manual does not mention the lock.

## Differences between builds

None known.

## Open questions

- Where the game sets `control_locked`; no instruction found sets the bit on its own (Q-AI-002).
- Which screens pass Space to the resident key routine, and so whether Space works outside
  combat (Q-AI-002).
- What `unk_0E` of `combatant_details` is (Q-COMBAT-002).
- What `fn_2D40_3E64` returns for a slot with no combatant (Q-COMBAT-002).
