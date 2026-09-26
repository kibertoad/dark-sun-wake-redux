---
id: RULE-TIME-002
title: The timer interrupt runs at the shortest period any of 17 timer slots asks for, given in microseconds
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-TIME-005]
conflicting: []
split_with: []
related: []
---

## Summary

A resident library keeps up to 17 timers, each with a period in microseconds, and sets the
hardware timer interrupt to fire at the shortest period among the timers in use, or at the BIOS
rate of 18.2 times a second when the period is too long for the chip.

## When it runs

`choose_timer_period` runs when the library's timer slots change; its callers were not located.
`timer_reload_for` runs inside it.

## Parameters

`period_us`, for `timer_reload_for`, a `UINT16`: a period in microseconds.

## Inputs

`timer_slot_used`, `timer_periods`, `timer_period`.

## Procedure

```text
clock timer_interrupt: 1193182 / 65536 Hz

define timer_reload_for(period_us: UINT16) -> UINT16:
    if period_us >= 54925:
        return 0
    return UINT16((UINT32(period_us) * 10000) / 8380)

define choose_timer_period():
    g_4868_0120 = 0xFFFFFFFF
    for slot in 0..17:
        if timer_slot_used[slot] != 0 and timer_periods[slot] < g_4868_0120:
            g_4868_0120 = timer_periods[slot]
    if g_4868_0120 != timer_period:
        g_4868_011E = 0xFFFF
        timer_period = g_4868_0120
        timer_reload = timer_reload_for(UINT16(g_4868_0120))
```

## Outputs

`timer_reload_for` returns the reload value. `choose_timer_period` returns nothing; when the
shortest period changes it sets `g_4868_011E`, `timer_period` and `timer_reload`, and writes the
reload value to channel 0 of the timer chip in mode 3, which changes the rate of
`timer_interrupt` to 1,193,182 divided by the reload value, with 0 meaning 65,536.

## Edge cases

With no slot in use the shortest period stays `0xFFFFFFFF`, whose low word is at least 54,925, so
the reload is 0 and the interrupt runs at 18.2 Hz. Only the low 16 bits of the chosen period
reach `timer_reload_for`, so a period of 65,536 µs or more is taken modulo 65,536. The product
`period_us * 10000` fits in 32 bits for every `UINT16`, and the quotient fits in 16 bits for every
period below 54,925.

## What the sources say

None known.

## Differences between builds

None known.

## Open questions

- Which routines register timer slots, with which periods, and what runs on each; so the rate of
  `timer_interrupt` while the game runs is not known (Q-TIME-002).
- What `g_4868_011E` is for, and whether anything but `choose_timer_period` reads
  `g_4868_0120`, where it keeps the shortest period while it searches (Q-TIME-002).
