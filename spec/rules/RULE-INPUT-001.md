---
id: RULE-INPUT-001
title: The right mouse button steps the pointer through the Walk, Attack and Look modes
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-INPUT-002]
---

## Summary

The game is played with the mouse. On the map the pointer is in one of three modes, Walk, Attack
and Look. A right click moves it to the next mode, and after Look it returns to Walk. A left click
acts in the current mode at the place the pointer's top-left corner is on: Walk moves the party
there, Attack attacks what is there, and Look examines it. Escape leaves any menu.

## When it runs

`next_pointer_mode` runs when the player presses the right mouse button over the map, outside any
menu or screen [SRC-MANUAL-1994].

## Parameters

`mode`, the current pointer mode: 0 for Walk, 1 for Attack and 2 for Look, numbered in the order
the right button steps through them.

## Inputs

None beyond the parameters.

## Procedure

```text
define next_pointer_mode(mode):
    return (mode + 1) % 3
```

## Outputs

The new pointer mode. The pointer then shows the image RULE-INPUT-002 picks for it.

## Edge cases

In combat, a left click with the Walk pointer on an enemy makes the active character walk to it
and attack it (RULE-COMBAT-006). A left click with Look on something that offers one action takes that action
without showing the Look panel [SRC-MANUAL-1994]. What each click does belongs to the rules of
walking, combat and the Look panel.

## What the sources say

The manual says a mouse is required, that the right button cycles the three modes, that a left
click acts, and that Escape exits any menu even when playing with the mouse. It gives no order of
the modes other than listing them as Walk, Attack and Look (SRC-MANUAL-1994, pages 2 and 4).

## Differences between builds

None known.

## Open questions

- The order and starting mode are from the manual's list; no capture or code shows them. The
  pointer images of FND-INPUT-002 were taken one mode at a time (Q-INPUT-001).
- Where the game keeps the mode, and whether the right button does anything else on other
  screens (Q-INPUT-001).
- No code that handles a right click is known. The mouse driver's events reach the game through
  the wrappers of FND-INPUT-010 and the handler of FND-INPUT-005, which queues each event as a
  packet; the code that reads the queue is not recovered (Q-INPUT-002).
