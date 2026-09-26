---
id: RULE-UI-001
title: Which controls the window code offers an event or the pointer to, by their event masks
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-UI-006, FND-UI-007]
conflicting: []
split_with: []
related: [FMT-UI-003, FMT-UI-004, FMT-UI-005]
---

## Summary

Each button, frame and edit box carries a mask of event bits. A frame is handed an event only when
its mask and the event have a bit in common. When the game looks for the control under the
pointer, it passes over a button whose mask has either of two bits set, and it considers an edit
box only when a third bit is set.

## When it runs

`event_matches` runs when the frame dispatcher hands an event to an FMT-UI-004 frame, with the
event bits its caller passes. `button_can_be_hit` and `edit_box_can_be_hit` run when the child
dispatcher looks for the control under the pointer, for each FMT-UI-003 button and FMT-UI-005
edit box child of a window it visits [FND-UI-006].

## Parameters

`event_matches(event_mask, requested)`: `event_mask`, the `event_mask` of the frame as the game
holds it at that moment, and `requested`, the event bits being offered.
`button_can_be_hit(event_mask)`: the `event_mask` of the button.
`edit_box_can_be_hit(event_mask)`: the `event_mask` of the edit box.

## Inputs

None beyond the parameters.

## Procedure

```text
define event_matches(event_mask: UINT16, requested: UINT16):
    let both: INT16 = event_mask & requested
    return both > 0

define button_can_be_hit(event_mask: UINT16):
    return (event_mask & 6) == 0

define edit_box_can_be_hit(event_mask: UINT16):
    return (event_mask & 2) != 0
```

## Outputs

Each function returns true or false and changes no state.

## Edge cases

A frame whose mask is 0 takes no event. The frame test reads the result as a signed 16-bit value,
so a common bit that is bit 15 alone counts as no match; no shipped mask has bit 15 set
[FND-UI-006]. The game can set bits, clear bits or zero a frame's mask while it runs
[FND-UI-007], so the mask tested is not always the one in the file. A button with a mask of 0 can
always be hit, and an edit box with a mask of 0 never can.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What event each bit stands for. The shipped masks use the values 2, 4, 8, 16, 32, 64, 128 and
  256 (FND-UI-004, FND-UI-005, Q-UI-001).
- Which callers pass which `requested` bits to the frame dispatcher, and when the game changes
  a mask (FND-UI-006, FND-UI-007, Q-UI-001).
