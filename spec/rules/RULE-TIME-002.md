---
id: RULE-TIME-002
title: The timer interrupt runs at the shortest period any of 17 timer slots asks for, given in microseconds
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-TIME-007, FND-TIME-012, FND-TIME-013, FND-TIME-014]
conflicting: []
split_with: []
related: []
---

## Summary

A resident library keeps up to 17 timers, each with a period in microseconds, and sets the
hardware timer interrupt to fire at the shortest period among the timers in use, or at the BIOS
rate of 18.2 times a second when the period is too long for the chip. Each interrupt adds the
period to every running slot's `timer_accumulators` entry and calls the slot's routine whenever
that entry reaches the slot's period, taking the period off, so each slot runs at its own average
rate (FND-TIME-012).

With the installed sound set-up the slots during play are the library's own call of the BIOS
timer handler every 54,925 µs, a sound counter at 100 Hz and the game tick at 96 Hz, which adds 1
to `game_tick`; an FLI adds a slot of 1,000 µs while it plays (FND-TIME-012, FND-VIDEO-008). The
installed launch never installs the FM music driver, whose service would run at 120 Hz
(FND-TIME-013). The interrupt runs at 10,000 µs until the intro FLI and at 1,000 µs from then on,
since giving the FLI's slot back does not choose the period again (FND-TIME-014).

## When it runs

`choose_timer_period` runs whenever a slot's period is set, from the period setter `4842:093D`
(FND-TIME-012). `timer_reload_for` runs inside it. Giving a slot back does not run it.

## Parameters

`period_us`, for `timer_reload_for`, a `UINT16`: a period in microseconds.

## Inputs

`timer_slot_used`, `timer_periods`, `timer_period`, `timer_accumulators`.

## Procedure

```text
clock timer_interrupt: 1193182 / 65536 Hz

define timer_reload_for(period_us: UINT16) -> UINT16:
    if period_us >= 54925:
        return 0
    return UINT16((UINT32(period_us) * 10000) / 8380)

define choose_timer_period():
    timer_shortest = 0xFFFFFFFF
    for slot in 0..17:
        if timer_slot_used[slot] != 0 and timer_periods[slot] < timer_shortest:
            timer_shortest = timer_periods[slot]
    if timer_shortest != timer_period:
        timer_scan_slot = 0xFFFF
        timer_period = timer_shortest
        timer_reload = timer_reload_for(UINT16(timer_shortest))
        for slot in 0..17:
            timer_accumulators[slot] = 0
```

## Outputs

`timer_reload_for` returns the reload value. `choose_timer_period` returns nothing; when the
shortest period changes it sets `timer_scan_slot`, `timer_period` and `timer_reload`, clears
`timer_accumulators`, and writes the
reload value to channel 0 of the timer chip in mode 3, which changes the rate of
`timer_interrupt` to 1,193,182 divided by the reload value, with 0 meaning 65,536.

## Edge cases

With no slot in use the shortest period stays `0xFFFFFFFF`, whose low word is at least 54,925, so
the reload is 0 and the interrupt runs at 18.2 Hz. Only the low 16 bits of the chosen period
reach `timer_reload_for`, so a period of 65,536 µs or more is taken modulo 65,536. The product
`period_us * 10000` fits in 32 bits for every `UINT16`, and the quotient fits in 16 bits for every
period below 54,925.

`timer_scan_slot` is the slot the interrupt handler is serving, so a period change made inside a
slot's routine makes the handler start its scan again from slot 0. A slot given back leaves the
rate as it was, so after an FLI the interrupt runs at 1,000 Hz until the next period change
(FND-TIME-012).

## What the sources say

None known.

## Differences between builds

None known.

## Open questions

- What a music driver's function `0x67` does on each of its 120 Hz calls, in a launch that installs
  one, which the installed files never make (FND-TIME-012, FND-TIME-013; Q-TIME-002).
