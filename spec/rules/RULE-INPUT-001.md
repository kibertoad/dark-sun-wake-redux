---
id: RULE-INPUT-001
title: The right mouse button steps the pointer through the Walk, Attack and Look modes
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-INPUT-013, FND-PARTY-154, FND-UI-044, FND-COMBAT-013, FND-COMBAT-022, FND-COMBAT-023, FND-COMBAT-025, FND-EXPLORE-004, FND-EXPLORE-005, FND-EXPLORE-006, FND-PARTY-046, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-INPUT-002, RULE-EXPLORE-001]
---

## Summary

The game is played with the mouse. On the map the pointer is in one of three modes, Walk, Attack
and Look. Each press of the right button moves it to the next mode, and after Look it returns to
Walk. While `g_57E0_1A34` is not `0xFFFF`, Look steps to a fourth mode, 3, which steps back to
Walk. A left click acts in the current mode at the place the pointer's top-left corner is on:
Walk moves the party there, Attack attacks what is there, and Look examines it. Escape leaves any
menu [SRC-MANUAL-1994, FND-INPUT-013].

## When it runs

`next_pointer_mode` runs when the right button goes down while the pointer is over the map's
full-screen frame `APFM/19200`, and `release_right_button` when it comes up there (FND-INPUT-013,
FND-UI-044, FND-PARTY-154).

## Parameters

None.

## Inputs

`pointer_mode`, `right_button_held`, `combat_state`, `turn_combatant`, `g_57E0_0DAB`,
`g_57E0_44E6` and `g_57E0_1A34`.

## Procedure

```text
define next_pointer_mode():
    if combat_state != 0 and turn_combatant >= 4: return
    if g_57E0_0DAB != 1: return
    if right_button_held != 0: return
    right_button_held = 1
    if combat_state != 0 and combat_state != 1 and g_57E0_44E6 > 4: return
    if pointer_mode == 1:
        pointer_mode = 4
    else if pointer_mode == 4:
        pointer_mode = 2
    else if pointer_mode == 2 and g_57E0_1A34 != 0xFFFF:
        pointer_mode = 3
    else:
        pointer_mode = 1
    # visible: the pointer shows the image RULE-INPUT-002 picks for the new mode.

define release_right_button():
    if g_57E0_0DAB == 1: right_button_held = 0
```

## Outputs

`pointer_mode`, 1 for Walk, 4 for Attack, 2 for Look and 3 for the fourth mode, and
`right_button_held`.

## Edge cases

- Holding the right button down makes one step; the next step needs a release over the frame.
- In combat the press does nothing while `turn_combatant` is 4 or more, that is while the turn is
  not a party member's (FND-COMBAT-022).
- A press that passes the first checks but not the `g_57E0_44E6` check sets `right_button_held`
  without a step.
- Any value of `pointer_mode` other than 1, 4 and 2, including the 5 that overlay 208 stores and
  the 0 the pan routine stores, steps to Walk (FND-INPUT-013).
- In the fourth mode the pointer is an image taken from an object record; for mode 3 and mode 5
  the pointer image does not follow the pointer's movement (FND-INPUT-013).
- Right after the step from Attack to Look the pointer shows `ICON/19110`, because the Look image
  number is passed where a mode is expected; the next movement of the pointer over the frame shows
  the Look image (FND-INPUT-013).
- In combat, a left click with the Walk pointer on an enemy makes the active character walk to
  it and attack it (RULE-COMBAT-006). A left click with Look on something that offers one action
  takes that action without showing the Look panel [SRC-MANUAL-1994]. What each click does
  belongs to the rules of walking, combat and the Look panel.

## What the sources say

The manual says a mouse is required, that the right button cycles the three modes, that a left
click acts, and that Escape exits any menu even when playing with the mouse. It lists the modes
as Walk, Attack and Look (SRC-MANUAL-1994, pages 2 and 4), the order the code steps through them.

## Differences between builds

None known.

## Open questions

- What `g_57E0_1A34` holds, and so when and what for the fourth mode is reached, and what
  `g_57E0_44E6` holds (Q-INPUT-002).
- Which mode the pointer has when play begins; several routines store 1, 0 or a saved value in
  `pointer_mode` (Q-INPUT-001).
- What the routines at `362C:06B9` and overlay 190 `+2503`, which the step calls, do (Q-INPUT-002).
- What `g_57E0_0DAB` holds beyond being 1 whenever the right button steps the mode
  (Q-EXPLORE-001).
