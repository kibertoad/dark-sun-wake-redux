---
id: SCR-UI-017
title: Quick Cast panel
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-009]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Recent spell box | Not known | Up to five icons for recently used spells or psionic powers | Not known | After a right click on the Dark Sun icon when the queue is populated | SRC-MANUAL-1994 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Recent icon | Not known | The icon is listed | Chooses the spell or psionic power for use. | SRC-MANUAL-1994 |

## Keyboard input

None known.

## Other input

None known.

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Recent icons available | The Dark Sun icon is right-clicked with recent choices in the queue | An icon is chosen or the panel closes | SRC-MANUAL-1994 |
| No recent icons | The Dark Sun icon is right-clicked with an empty queue | SCR-UI-009 opens instead | SRC-MANUAL-1994 |

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Which control graph draws the box, its placement, and the code that maintains and chooses from the queue (Q-UI-005).
