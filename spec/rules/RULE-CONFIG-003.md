---
id: RULE-CONFIG-003
title: Setting the music and sound effects volumes
status: disputed
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: [FND-UI-034, FND-CONFIG-010]
split_with: []
related: []
---

## Summary

The manual describes music and sound-effects volume bars, each with two arrow buttons. The
shipped screen's top arrows instead have the hover label `MESSAGE DELAY` and change `DS:26B7`,
while the second pair changes the sound-effects volume byte (FND-UI-034, FND-CONFIG-010).

## When it runs

The manual's proposed volume adjustment would run when an arrow beside either of its named
bars is clicked (SRC-MANUAL-1994, page 15).

## Parameters

The manual does not give a step or upper bound.

## Inputs

None established for a music-volume arrow on this screen.

## Procedure

```text
# The manual's music-volume arrow has no established executable procedure.
# See RULE-CONFIG-005 and RULE-CONFIG-004 for the shipped arrow paths.
```

## Outputs

Not established for music volume on this screen.

## Edge cases

The apparent music-volume arrows are dispatched as message-delay controls in
this build (FND-UI-034, FND-CONFIG-010).

## What the sources say

SRC-MANUAL-1994, page 15: music volume is a slide bar the player can adjust to control music
volume by clicking the buttons on either end of the bar; sound effects volume is a slide bar that
works the same way.

## Differences between builds

None known.

## Open questions

- Whether music volume can be adjusted through another control or key remains open. The
  executable's first arrow pair changes message delay, and the second changes sound-effects
  volume; the manual says the first is music volume (FND-UI-034, FND-CONFIG-010,
  SRC-MANUAL-1994, Q-CONFIG-002).
