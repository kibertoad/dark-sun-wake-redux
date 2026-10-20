---
id: RULE-VIDEO-006
title: A removed combatant in view is unloaded and its rectangle is copied back from the second screen buffer pixel by pixel at random, two draws per copy and a wait every ten copies
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-043, FND-RNG-041, FND-COMBAT-049, FND-PARTY-140, FND-ACTOR-003, FND-COMBAT-026, FND-EXPLORE-001, FND-EXPLORE-002, FND-COMBAT-025, FND-ACTOR-005, FND-PARTY-013, FND-EXPLORE-005]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-SOUND-004, RULE-COMBAT-016]
---

## Summary

When overlay 206 `+0E1E` removes a combatant of slot 4 or more whose figure overlaps the view,
it unloads the slot and calls `dissolve` with the figure's rectangle on the screen, mode 2 and a
delay of 1. Mode 2 copies `w * h` pixels from buffer 1 to buffer 0, each at a place drawn at
random in the rectangle, x first, and waits after every tenth copy. Modes 0 and 1 copy the whole
rectangle row by row or column by column with a wait after each row or column and make no draw.

## When it runs

In `+0E1E`, after the cry and overlay 173 `+3136`, for a slot of 4 or more whose state byte is 2
and whose figure passes the view test; a slot that fails it, or of state byte 1, is only unloaded
(FND-RNG-043).

## Parameters

- `x0: INT16`, `y0: INT16`, `x1: INT16`, `y1: INT16`: the rectangle, the end excluded.
- `mode: UINT8`: 0, 1 or 2.
- `delay: UINT8`: the wait in milliseconds.

## Inputs

The two screen buffers through `fn_1BF3_2C18` and `fn_1BF3_2AF4`.

## Procedure

```text
define dissolve(x0: INT16, y0: INT16, x1: INT16, y1: INT16, mode: UINT8, delay: UINT8):
    if x0 < 0: x0 = 0
    if y0 < 0: y0 = 0
    if x1 > 320: x1 = 320
    if y1 > 200: y1 = 200
    if mode == 0:
        for y in y0..y1-1:
            for x in x0..x1-1:
                fn_1BF3_2AF4(0, x, y, fn_1BF3_2C18(1, x, y))
            fn_1000_12FA(delay)
    else if mode == 1:
        for x in x0..x1-1:
            for y in y0..y1-1:
                fn_1BF3_2AF4(0, x, y, fn_1BF3_2C18(1, x, y))
            fn_1000_12FA(delay)
    else if mode == 2:
        let w: UINT16 = UINT16(x1 - x0)
        let h: UINT16 = UINT16(y1 - y0)
        let n: UINT16 = UINT16(INT16(w) * INT16(h))
        let k: UINT16 = 0
        while k < n:
            # two draws of RULE-RNG-001's generator, x first
            let x: INT16 = x0 + (INT32(draw()) * INT32(w)) / 32768
            let y: INT16 = y0 + (INT32(draw()) * INT32(h)) / 32768
            fn_1BF3_2AF4(0, x, y, fn_1BF3_2C18(1, x, y))
            if k % 10 == 0:
                fn_1000_12FA(delay)
            k = k + 1
        fn_28C9_2A81(1)

define remove_dissolve(slot):
    let s = object_slots[slot]
    if s.x < view_left + 320 and s.x + s.width > view_left
            and s.y < view_top + 200 and s.y + s.width > view_top:
        fn_31E0_05BB(s.entry_index, 1, 0, 1)
        fn_2C5F_0139()
        let x = s.x - view_left
        let y = s.y - view_top
        dissolve(x, y, x + s.width, y + s.height, 2, 1)
    else:
        fn_31E0_05BB(s.entry_index, 1, 1, 1)
```

## Outputs

The copied pixels in buffer 0, and the call of `fn_28C9_2A81` after mode 2.

## Edge cases

The vertical test adds the figure's `width`, not its `height`. The pixels of mode 2 are drawn
with repetition, so some are copied more than once and others not at all. `w` and `h` are taken
as unsigned in the scaling: a rectangle clipped to a negative height makes `n` the low word of a
negative product and `y` scaled by the zero-extended `h`; a height of 0 makes no draw. The first
copy is followed by a wait, as `k` 0 is a multiple of 10. Other modes do nothing.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_1BF3_2C18`, `fn_1BF3_2AF4`, `fn_1000_12FA`, `fn_2C5F_0139`, `fn_28C9_2A81` and
  `fn_31E0_05BB`'s arguments do, taken here from their arguments only (FND-RNG-043, Q-RNG-014).
