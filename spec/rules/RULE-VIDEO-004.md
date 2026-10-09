---
id: RULE-VIDEO-004
title: While an FLI plays, a timer slot with a period of 1,000 microseconds counts milliseconds
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-VIDEO-008, FND-TIME-007]
conflicting: []
split_with: []
related: [RULE-TIME-002]
---

## Summary

The FLI player registers a slot of the timer library with a period of 1,000 microseconds, and the
routine the slot runs adds 1 to a counter each time, so the player can wait in milliseconds.

## When it runs

Once per tick of `fli_tick`, from the start of `play_fli` (RULE-VIDEO-002) to its end. The
player's setup takes a free slot of the library RULE-TIME-002 describes, sets its period to
1,000,000 divided by 1,000 microseconds and starts it; the library then sets the timer chip to the
shortest period of the slots in use (FND-VIDEO-008, FND-TIME-007).

## Parameters

None.

## Inputs

`fli_ticks`.

## Procedure

```text
clock fli_tick: 1193182 / 1193 Hz

fli_ticks = fli_ticks + 1
```

## Outputs

`fli_ticks` goes up by 1, wrapping from 65,535 to 0.

## Edge cases

With the FLI slot the shortest in use, `timer_reload_for(1000)` gives 1,193, so the interrupt and
the tick run at 1,193,182 / 1,193 Hz, 0.99985 ms a tick.

## What the sources say

None known.

## Differences between builds

None known.

## Open questions

- Whether another slot with a shorter period is in use while an FLI plays, and how the library
  then decides when to run each slot's routine: the interrupt handler that calls the slots was not
  read, so the rate of `fli_tick` is shown only for the FLI slot alone (FND-TIME-007, Q-TIME-002).
