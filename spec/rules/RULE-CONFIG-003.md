---
id: RULE-CONFIG-003
title: Music-level readback on the Preferences screen
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-CONFIG-235, FND-CONFIG-010, FND-CONFIG-236, FND-CONFIG-029, FND-UI-034, FND-UI-065, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-CONFIG-004, RULE-CONFIG-005, SCR-UI-007]
---

## Summary

On entry with sound and music enabled, Preferences stores the sound-library
getter's returned byte as `music_level`. The inspected button branches contain
no direct store to that byte. The arrows beside the top bar adjust message delay
(RULE-CONFIG-005), and those beside the second bar adjust effects volume
(RULE-CONFIG-004). This does not exclude changes through callees, copied state
or controls elsewhere [FND-CONFIG-235, FND-CONFIG-029].

## When it runs

When the Preferences screen opens (SCR-UI-007) [FND-CONFIG-235].

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

`music_level` receives the getter result only when both gates pass; otherwise
this entry path makes no such direct store. A music-toggle redraw passes this byte
when music is enabled, or zero otherwise, to the top bar. Full redraw uses
that position for message delay instead (FND-UI-065). Effects through the
getter and subsequent callees remain unresolved.

## Edge cases

- With either entry gate off, this entry path does not store a getter result.
  A music-toggle redraw with music off uses zero (FND-CONFIG-235, FND-UI-065).
- The driver's answer can be 255 for card ID 113 and for some unavailable drivers
  (FND-CONFIG-236).

## What the sources say

SRC-MANUAL-1994, page 15: music volume is a slide bar the player can adjust to control music
volume by clicking the buttons on either end of the bar; sound effects volume is a slide bar that
works the same way. The first half does not hold for this build: the top arrows are the
message-delay buttons (FND-CONFIG-235).

## Differences between builds

None known.

## Open questions

- Which admitted driver states and callee effects determine `fn_4A6E_011F`'s result,
  whether the button paths can change `music_level` through indirect or copied
  writes, and whether another control changes saved or audible music level.
  The direct-store census does not exclude these mechanisms
  (FND-CONFIG-236, FND-CONFIG-029, FND-CONFIG-235, Q-CONFIG-002).
