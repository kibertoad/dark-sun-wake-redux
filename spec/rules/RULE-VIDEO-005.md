---
id: RULE-VIDEO-005
title: Script request 27 reseeds the generator and moves the display offset for a number of steps in one of four patterns, the fourth drawing one of eight directions
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-021, FND-RNG-022, FND-RNG-027, FND-TIME-004, FND-PARTY-122, FND-RNG-009, FND-RNG-014, FND-RNG-050, FND-RNG-053, FND-RNG-055]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-TIME-001]
---

## Summary

`offset_loop` first reseeds the generator from the clock, then, for each of `count` steps, sets a
horizontal and a vertical display offset by `mode`, hands both to the display and waits 50
milliseconds, and ends by setting both offsets to 0. Mode 1 moves the vertical offset to the
magnitude and back to 0, mode 2 the horizontal offset, mode 3 goes round eight directions in turn
and mode 4 draws one of the eight directions; modes 3 and 4 and the clear step of modes 1 and 2
return to 0 on every other step. A mode outside 1 to 4 hands 0 and 0 to the display at every
step. Mode 4 is the only mode that draws, once for each step that starts with both offsets 0.

## When it runs

`offset_request` runs for request 27 of script instruction `0x22`; 44 instructions in the
shipped `GPL` resources issue it, 42 of them with literal arguments (FND-RNG-027). The game also
calls `offset_loop` directly with magnitude 4, mode 4 and count 4 in the cast routine when the
`DATA` number is 293 and the cast routine's byte argument at `bp+0x1A` is not 0, which the cast
queue passes as 1 (FND-RNG-053); in the shipped data such a cast is the special attack of a
combatant whose special attack byte is 6, or its return by a target whose byte is 13
(FND-RNG-055), and with magnitude 2, mode 4 and count 1 at two places in overlay 201
(FND-RNG-022). Both are in overlay 201 `+05D5`, which overlay 206 `+0E1E` runs, in place of the
cry, for a removed combatant of state byte 2 whose word `+0x1B` has the absolute value 430; it has
no other caller. With `sound_on` and `digital_sound_on` not 0 and its first picture loaded, it plays
sound 230 and calls `offset_loop` each time `fn_4611_0051` reports the sample still playing,
testing before each call; with `sound_on` not 0 it plays sound 8 and calls `offset_loop` once at
frame 7 of its second picture, when that picture loads and has 8 frames or more (FND-RNG-050).

## Parameters

- `magnitude: INT16`: the size of the offset.
- `mode: INT16`: the pattern, 1 to 4.
- `count: INT16`: the number of steps.

## Inputs

`rng_state` (RULE-RNG-001), and the clock word the seed is taken from (`clock_word`).

## Procedure

```text
let offset_dx: INT8[8] = [0, 1, 1, 1, 0, -1, -1, -1]
let offset_dy: INT8[8] = [-1, -1, 0, 1, 1, 1, 0, -1]

define offset_request(packed: INT32, steps: INT32):
    # the quotient and remainder by ten and the steps each keep their low byte, read as signed
    offset_loop(INT8(packed / 10), INT8(packed % 10), INT8(steps))

define offset_loop(magnitude: INT16, mode: INT16, count: INT16):
    seed_random(clock_word())
    let x: INT16 = 0
    let y: INT16 = 0
    let pick: INT16 = 0
    let up = true
    emit DisplayGeometrySet(320, 200)
    let step: INT16 = 0
    while step < count:
        if mode == 1:
            if y != 0:
                y = 0
            else if up:
                y = magnitude
                up = false
            else:
                y = INT16(-magnitude)
                up = true
        else if mode == 2:
            if x != 0:
                x = 0
                y = 0
            else if up:
                x = magnitude
                up = false
            else:
                x = INT16(320 - magnitude)
                y = -1
                up = true
        else if mode == 3 or mode == 4:
            if x != 0 or y != 0:
                x = 0
                y = 0
            else:
                if mode == 3:
                    pick = pick + 1
                    if pick > 7:
                        pick = 0
                else:
                    # one draw of RULE-RNG-001's generator, scaled to 0..7
                    pick = (INT32(draw()) * 8) / 32768
                x = INT16(offset_dx[pick] * magnitude)
                y = INT16(offset_dy[pick] * magnitude)
                if x < 0:
                    x = INT16(320 - magnitude)
                    y = INT16(y - 1)
        if x < 0:
            x = 0
        if y < 0:
            y = 0
        emit DisplayOffsetSet(x, y)  # visible: the display takes the offsets
        wait_ms(50)
        step = step + 1
    emit DisplayOffsetSet(0, 0)  # visible: the display takes the offsets
```

## Outputs

The offsets handed to the display at each step and after the loop, and `rng_state`, which the
reseed sets and each mode-4 draw changes.

## Edge cases

A `count` of 0 or less still reseeds, sets the geometry and hands 0 and 0 to the display once.
The clamp at the end of each step keeps the offsets it changes, so a negative offset becomes 0
for the next step's test as well: with a positive magnitude, mode 1's second movement sets the
vertical offset to the negative magnitude, which is handed to the display as 0, so mode 1 moves
the display on the first step and every third step after it, while mode 2 alternates between the
magnitude, 0, `320 - magnitude` and 0. In mode 4 a direction whose offsets both end at 0 (direction 0,
or any direction with magnitude 0) leaves the next step to draw again; with magnitude 0 every
step draws. A negative horizontal offset is written as `320 - magnitude` with the vertical offset
one lower. `offset_request` keeps only the low byte of each value, so a packed value of 1284
gives magnitude -128 and a steps value of 256 gives a count of 0.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What the display does with the two offsets and the geometry call: the helper writes display
  registers after polling a status bit, so the visible movement and how long each step lasts
  depend on the hardware (FND-RNG-021). (No item: no run possible; a capture of the original
  during one of the scripts that issue request 27 would confirm it.)
- Which game events put a combatant whose word `+0x1B` is 430 on overlay 173's removal list, and
  so run the overlay 201 calls (FND-RNG-050, Q-RNG-014).
- How `clock_word` is formed from the date and time (FND-RNG-009, FND-RNG-014, Q-RNG-012).
- Which combatants have special attack 6 or 13, whose special attack casts `DATA` 293 with the
  cast routine's byte `bp+0x1A` set and so runs the loop from a cast, and whether the party's
  target cursor can pass 293 (FND-RNG-053, FND-RNG-055, Q-RNG-003).
