---
id: RULE-CONFIG-005
title: The Preferences message-delay adjustment
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-CONFIG-010, FND-UI-034]
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

The screen redraws the upper bar from `message_delay_value - 20`. A separate
routine multiplies the word by 100 with 16-bit arithmetic and passes it to a
far routine; the latter's timing contract has not been read (FND-CONFIG-010).

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

- What timing unit `message_delay_value` represents, whether another caller imposes an
  upper limit, and whether a new game replaces the loaded-image value 50
  (Q-CONFIG-007).
