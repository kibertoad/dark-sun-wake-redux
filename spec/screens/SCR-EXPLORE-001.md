---
id: SCR-EXPLORE-001
title: Exploration view
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-EXPLORE-001, RULE-EXPLORE-002, SCR-COMBAT-001, SCR-UI-006, SCR-UI-011, SCR-UI-012, SCR-UI-022]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Region view | Not known | The party and nearby people, objects and terrain | Not known | During exploration | SRC-MANUAL-1994 |
| Pointer mode icons | Not known | Walk, Look or Attack according to the selected mode | At the pointer | During exploration | SRC-MANUAL-1994 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Region view in Walk mode | Not known | During exploration | Directs the party toward the chosen location. | SRC-MANUAL-1994 |
| Region view in Look mode | Not known | During exploration | Examines the chosen person or object and may open SCR-UI-011. | SRC-MANUAL-1994 |
| Region view in Attack mode | Not known | During exploration or combat | Chooses a target to attack. | SRC-MANUAL-1994 |
| Screen edge | Not known | During exploration | Scrolls the region view toward that edge (RULE-EXPLORE-001). | SRC-MANUAL-1994 |

## Keyboard input

None known.

## Other input

None known.

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Leader only | The adventure begins or the party is collapsed | The party display changes | SRC-MANUAL-1994 |
| Whole party shown | The player expands the party display | The party display changes | SRC-MANUAL-1994 |

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Which resources compose the exploration view, where each layer is drawn, and how the pointer and camera state change are not established by the manual (Q-EXPLORE-007).
