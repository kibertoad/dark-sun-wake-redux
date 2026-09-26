---
id: RULE-TIME-001
title: The game waits a number of milliseconds by reading the timer chip until enough counts have passed
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-TIME-004]
conflicting: []
split_with: []
related: []
---

## Summary

When the game pauses for a fixed time, such as the pauses of 400 ms or 16 seconds some overlays
make, it waits in a busy loop that watches the timer chip count, so the pause lasts the same time
on any processor.

## When it runs

`calibrate_wait` runs once at startup. `wait_ms` runs when a routine of overlay 172, 187, 201,
204 or 206 calls it (FND-TIME-004).

## Parameters

`ms`, for `wait_ms`, a `UINT16`: the number of milliseconds to wait.

## Inputs

`pit_countdown`, `wait_units_per_ms`.

## Procedure

```text
clock pit_input: 1193182 Hz

define calibrate_wait():
    for i in 0..100:
        if pit_countdown & 1 == 0:
            wait_units_per_ms = 1193
            return

define wait_ms(ms: UINT16):
    let last: UINT16 = pit_countdown
    let target: UINT32 = UINT32(ms) * wait_units_per_ms + last
    while true:
        let now: UINT16 = pit_countdown
        if UINT32(now) >= target:
            return
        if now < last:
            if target < 0x10000:
                return
            target = target - 0x10000
        last = now
```

## Outputs

No return value. `calibrate_wait` may set `wait_units_per_ms` to 1,193. `wait_ms` changes nothing;
it returns once `ms` times `wait_units_per_ms` counts have passed.

## Edge cases

`wait_units_per_ms` starts at 2,386, the counts channel 0 of the timer chip makes per millisecond
in mode 3, where it counts down by 2 per cycle of `pit_input` and `pit_countdown` is always odd;
`calibrate_wait` changes it to 1,193 only when it reads an even value, as in a mode that counts
by 1. Either way a millisecond is 1,193 cycles of `pit_input`, 0.99985 ms.

`wait_ms(0)` returns at once. The loop only notices time passing when it reads the counter, so a
long interruption between two readings, longer than one full turn of the counter (65,536 counts),
makes the wait longer than asked. A reading below the one before means the counter has wrapped;
when the target is then below 65,536 it has been passed, and the wait ends.

## What the sources say

None known.

## Differences between builds

None known.

## Open questions

- What each caller waits for, and whether the word at `57E0:26B7`, a hundredth of the pause in
  overlay 172, is the Preferences message delay (Q-TIME-003).
- Whether the game ever reprograms channel 0 to a mode other than 3, which would make
  `calibrate_wait` choose 1,193 (Q-TIME-002).
