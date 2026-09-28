---
id: RULE-CONFIG-005
title: The Preferences message-delay adjustment
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-CONFIG-010, FND-UI-034, FND-TIME-004, FND-CONFIG-017, FND-CONFIG-018, FND-CONFIG-030, FND-CONFIG-031, FND-CONFIG-032, FND-CONFIG-033, FND-CONFIG-035]
conflicting: []
split_with: []
related: [SCR-UI-007]
---

## Summary

The first pair of Preferences arrows has the hover label `MESSAGE DELAY` and
changes a word in steps of eight. The left arrow stops at 20; the right branch
has no upper clamp of its own (FND-UI-034, FND-CONFIG-010).

## When it runs

The player clicks `BUTN/16305` to lower the value or `BUTN/16304` to raise it
on the Preferences screen (SCR-UI-007, FND-CONFIG-010).

## Parameters

`button`, one of those two control numbers.

## Inputs

`message_delay_value`, the unsigned word the game keeps at `DS:26B7` in
BLD-GOG-EN-1.1. It is 50 in the executable's loaded image, but a new game's
starting value has not been established. It is not one of the nine bytes of
`PREF/100` (FND-CONFIG-010).

## Procedure

```text
if button == 16305:
    if message_delay_value <= 28: message_delay_value = 20
    else: message_delay_value = message_delay_value - 8
else if button == 16304:
    message_delay_value = (message_delay_value + 8) modulo 65536
```

## Outputs

The screen redraws the upper bar from `message_delay_value - 20`. An overlay
172 routine multiplies the word by 100 with 16-bit arithmetic and passes the
result as milliseconds to the timer-chip wait. At the loaded-image value 50,
that call requests 5,000 ms. Several text-message paths call that routine;
the wait follows successful acquisition and setup of `WIND/10501`. Which
calls pass that gate in live states still needs a complete reading
(FND-CONFIG-010, FND-TIME-004, FND-CONFIG-017, FND-CONFIG-018).

## Edge cases

The lower boundary is 20 even when a loaded value is already below it. The
increase branch has no explicit upper clamp, so 16-bit wrap is possible
(FND-CONFIG-010).

## What the sources say

SRC-MANUAL-1994 calls the upper bar music volume, in conflict with the shipped
hover text and click branch (RULE-CONFIG-003).

## Differences between builds

None known.

## Open questions

- Whether another path imposes an upper limit on `message_delay_value`, and
  whether a new game replaces the loaded-image value 50 (Q-CONFIG-007).
  One reading is that 50 remains the starting value; another is that new-game
  setup writes a different value. A complete reading of initialization and
  indirect or block writers would distinguish them.
- Which text-message calls pass the overlay 172 wait gate (Q-CONFIG-008).
  The wait requires a nonzero far pointer at `0300:0007`, set by the
  `WIND/10501` acquisition and setup call. The shipped image fields bypass
  image registration failures, and all graphics initializer bounds admit the
  window; resource acquisition and possible later bounds writes
  remain open (FND-CONFIG-011, FND-CONFIG-017, FND-CONFIG-018,
  FND-CONFIG-030, FND-CONFIG-031, FND-CONFIG-032, FND-CONFIG-033,
  FND-CONFIG-034, FND-CONFIG-035, FND-CONFIG-036, FND-CONFIG-037,
  FND-CONFIG-038, FND-CONFIG-039, FND-CONFIG-040, FND-CONFIG-041,
  FND-CONFIG-042, FND-CONFIG-043, FND-CONFIG-044, FND-CONFIG-045,
  FND-CONFIG-046, FND-CONFIG-047, FND-CONFIG-048, FND-CONFIG-049,
  FND-CONFIG-050, FND-CONFIG-051, FND-CONFIG-052, FND-CONFIG-053,
  FND-CONFIG-054, FND-CONFIG-055, FND-CONFIG-056,
  FND-CONFIG-057,
  Q-TIME-003).
  One reading is that the present resource makes every message call pass the
  gate (FND-UI-001); another is that resource acquisition, bounds or child
  registration fails depending on state (FND-CONFIG-018, FND-CONFIG-030,
  FND-CONFIG-031, FND-CONFIG-032, FND-CONFIG-033, FND-CONFIG-034,
  FND-CONFIG-037, FND-CONFIG-038, FND-CONFIG-039,
  FND-CONFIG-040, FND-CONFIG-041, FND-CONFIG-042,
  FND-CONFIG-043, FND-CONFIG-044, FND-CONFIG-045,
  FND-CONFIG-046, FND-CONFIG-047, FND-CONFIG-048,
  FND-CONFIG-049, FND-CONFIG-050, FND-CONFIG-051,
  FND-CONFIG-052, FND-CONFIG-053, FND-CONFIG-054,
  FND-CONFIG-055, FND-CONFIG-056, FND-CONFIG-057).
  Reading resource acquisition, possible indirect or block bounds writes,
  and relevant callers, then checking original states if code alone does not
  decide, would separate them.
