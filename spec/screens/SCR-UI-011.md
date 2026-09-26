---
id: SCR-UI-011
title: Look panel
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-018, FND-UI-031, FND-UI-015, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-UI-001, SCR-UI-002, SCR-UI-008, SCR-UI-009, SCR-UI-010]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Panel outline | Not known | None | left column at x 67 | While the panel is shown | FND-UI-018 |
| Target's name and level | Not known | The looked-at character's name and level, `Draxan` and `LEVEL: 10` in FND-UI-018's capture | inside the panel | While the panel is shown | FND-UI-018, FND-UI-015 |
| Talk button | frame 2 of `RESOURCE.GFF#ICON/15105` of `BUTN/15306` | None | (70, 103, 16, 15) | The action is not available | FND-UI-018, FND-UI-031 |
| Pick up button | frame 2 of `RESOURCE.GFF#ICON/15107` of `BUTN/15308` | None | (90, 103, 16, 15) | The action is not available | FND-UI-018, FND-UI-031 |
| Use button | frame 2 of `RESOURCE.GFF#ICON/15106` of `BUTN/15307` | None | (110, 103, 16, 15) | The action is not available | FND-UI-018, FND-UI-031 |
| Close button | frame 0 of `RESOURCE.GFF#ICON/15109` of `BUTN/15309` | None | (128, 104, 28, 11) | While the panel is shown | FND-UI-018, FND-UI-031 |

The panel is `RESOURCE.GFF#WIND/3020`, 92 x 77, with its origin at (67, 44) in the one capture,
taken for a hostile character.

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Talk | (70, 103, 15, 15) | Not known; shown unavailable for a hostile character | Starts a conversation with the target. | FND-UI-018, FND-UI-031, SRC-MANUAL-1994 |
| Pick up | (90, 103, 15, 15) | Not known; shown unavailable for a hostile character | Picks up the target. | FND-UI-018, FND-UI-031, SRC-MANUAL-1994 |
| Use | (110, 103, 15, 15) | Not known; shown unavailable for a hostile character | Uses the target. | FND-UI-018, FND-UI-031, SRC-MANUAL-1994 |
| Close | (128, 104, 27, 11) | Never by the pointer search: its mask is 84 (RULE-UI-001) | Closes the panel. | FND-UI-018, FND-UI-031 |

## Keyboard input

None known.

## Other input

None known.

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Panel with its actions | The player uses Look on something with more than one option | The player chooses an action or closes the panel | SRC-MANUAL-1994 |
| No panel, sole option taken | The player uses Look on something with one option: the game takes it without showing the panel | At once | SRC-MANUAL-1994 |
| Combat panel | The player uses Look in combat: the panel shows the target's type and state | Not known | SRC-MANUAL-1994 |
| Party member looked at | The player uses Look on a party member: the last chosen of SCR-UI-002, SCR-UI-008, SCR-UI-009 and SCR-UI-010 opens | At once | SRC-MANUAL-1994 |

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Where the panel is placed for other targets, and which frames show the available and pressed
  looks of the action buttons (FND-UI-018, Q-UI-003, Q-UI-004).
- How the close button is chosen when the pointer search passes over it, and what the 145 x 87
  frame `APFM/15200` and `BMP/15002` are for (FND-UI-031, Q-UI-001).
- Where the name and level come from; the executable holds no label for the panel that code names
  (FND-UI-015, Q-UI-002).
