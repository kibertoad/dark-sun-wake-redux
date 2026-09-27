---
id: RULE-CONFIG-004
title: The Preferences sound-effects volume adjustment
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-CONFIG-010, FND-UI-034, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [FMT-CONFIG-003, RULE-CONFIG-001, SCR-UI-007]
---

## Summary

The sound-effects volume arrows change the saved byte in steps of seven.
Ordinary values stay between 0 and 127. The left arrow reaches zero and the
right arrow stops at 127 (FND-CONFIG-010).

## When it runs

The player clicks `BUTN/16307` to lower the volume or `BUTN/16306` to raise it
on the Preferences screen (SCR-UI-007, FND-CONFIG-010).

## Parameters

`button`, one of those two control numbers.

## Inputs

`sound_effects_volume`, the unsigned byte at `PREF/100` offset `0x03`. The
sound-effects enable state at offset `0x05` also affects the associated button
and bar drawing (FMT-CONFIG-003).

## Procedure

```text
if button == 16307:
    if sound_effects_volume <= 7: sound_effects_volume = 0
    else: sound_effects_volume = sound_effects_volume - 7
else if button == 16306:
    sound_effects_volume = (sound_effects_volume + 7) modulo 256
    if sound_effects_volume > 127: sound_effects_volume = 127
```

After the decrease reaches zero, the click path asks the sound-state routine
to turn effects off. An increase while effects are off asks it to turn effects
on. Master sound can prevent that routine from honoring the requested state;
the value change above still occurs (FND-CONFIG-010).

## Outputs

The changed byte is saved with the game in `PREF/100`. The screen redraws the
sound-effects button and volume bar from the current values (FND-CONFIG-010,
FMT-CONFIG-003).

## Edge cases

The eight-bit addition precedes the cap. A malformed loaded byte of 249 to
255 can wrap to a small value on increase rather than saturating at 127. The
ordinary 0-to-127 values follow the seven-unit step and cap (FND-CONFIG-010).

## What the sources say

SRC-MANUAL-1994, page 15, describes a sound-effects volume bar adjusted by
buttons at either end, without giving the step or bounds.

## Differences between builds

None known.

## Open questions

- How the 0-to-127 byte maps to output loudness, and the exact bar and button
  frames, require the sound library reading or an owner capture
  (Q-CONFIG-002, Q-CONFIG-001).
