---
id: RULE-CONFIG-003
title: Setting the music and sound effects volumes
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: []
---

## Summary

The music and sound effects volumes are each a slide bar on the Preferences screen, with a button
at each end that lowers or raises the volume.

## When it runs

When the player clicks a button at either end of a volume bar on the Preferences screen
(SCR-UI-007, SRC-MANUAL-1994, page 15).

## Parameters

`volume`, the current volume of the bar. `step`, the amount one click changes it by, negative
for the lower button. `highest`, the largest volume the bar allows.

## Inputs

None beyond the parameters.

## Procedure

```text
define change_volume(volume, step, highest):
    return min(max(volume + step, 0), highest)
```

## Outputs

The new volume.

## Edge cases

None known.

## What the sources say

SRC-MANUAL-1994, page 15: music volume is a slide bar the player can adjust to control music
volume by clicking the buttons on either end of the bar; sound effects volume is a slide bar that
works the same way.

## Differences between builds

None known.

## Open questions

- The step, the highest volume, the starting volumes, and where the game keeps them. The saved
  settings hold two bytes, 255 and 63 in the shipped resource, that the load routine treats in the
  way volumes would be (FMT-CONFIG-003, Q-CONFIG-001, Q-CONFIG-002).
