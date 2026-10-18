---
id: RULE-CONFIG-003
title: The Preferences screen shows the music volume and has no control that changes it
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-CONFIG-219, FND-CONFIG-010, FND-CONFIG-012, FND-CONFIG-029, FND-UI-034, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-CONFIG-004, RULE-CONFIG-005, SCR-UI-007]
---

## Summary

The Preferences screen draws the music volume as a bar, but none of its buttons changes it. When
the screen opens with sound and music on, it takes the volume from the sound driver. The arrows
beside the top bar change the message delay (RULE-CONFIG-005) and those beside the second the
sound-effects volume (RULE-CONFIG-004). In this build no control of the game sets the music
volume: it comes from the sound driver and from saved games [FND-CONFIG-219, FND-CONFIG-029].

## When it runs

When the Preferences screen opens (SCR-UI-007) [FND-CONFIG-219].

## Parameters

None.

## Inputs

`master_sound_on` and `music_on`.

## Procedure

```text
if master_sound_on and music_on:
    music_level = fn_4A6E_011F()
```

## Outputs

`music_level`, which the music bar shows while `music_on` is true; no button of the screen
changes it afterwards.

## Edge cases

- With music off the bar shows 0, and `music_level` keeps its value (FND-CONFIG-219,
  FND-CONFIG-012).
- The driver's answer can be 255 for card ID 113 and for some unavailable drivers
  (FND-CONFIG-012).

## What the sources say

SRC-MANUAL-1994, page 15: music volume is a slide bar the player can adjust to control music
volume by clicking the buttons on either end of the bar; sound effects volume is a slide bar that
works the same way. The first half does not hold for this build: the top arrows are the
message-delay buttons (FND-CONFIG-219).

## Differences between builds

None known.

## Open questions

- Whether the sound library changes the audible music level by a route that does not write
  `music_level`, and what `fn_4A6E_011F` gets from the driver on entry (Q-CONFIG-002).
