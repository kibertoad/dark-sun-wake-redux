---
id: SCR-UI-007
title: Preferences
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-029, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-006]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Thirteen buttons | the `ICON` of each `BUTN` child of `RESOURCE.GFF#WIND/16500` | None | each child's position from the window's corner, as FND-UI-029 lists | While the screen is shown | FND-UI-029 |

The window is `WIND/16500`, 210 x 116; where the game places it is not known.

## Mouse input

Rectangles are from the window's corner.

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Music on or off | (49, 24, 16, 16) | Not known | Turns music on or off. | FND-UI-029, SRC-MANUAL-1994 |
| Music volume down | (66, 29, 9, 8) | Not known | Lowers the music volume. | FND-UI-029, SRC-MANUAL-1994 |
| Music volume up | (159, 29, 9, 8) | Not known | Raises the music volume. | FND-UI-029, SRC-MANUAL-1994 |
| Sound effects on or off | (49, 43, 16, 16) | Not known | Turns sound effects on or off. | FND-UI-029, SRC-MANUAL-1994 |
| Sound volume down | (66, 48, 9, 8) | Not known | Lowers the sound effects volume. | FND-UI-029, SRC-MANUAL-1994 |
| Sound volume up | (159, 48, 9, 8) | Not known | Raises the sound effects volume. | FND-UI-029, SRC-MANUAL-1994 |
| Difficulty down | (56, 67, 9, 8) | Not known | Selects the next easier difficulty. | FND-UI-029, SRC-MANUAL-1994 |
| Difficulty up | (149, 67, 9, 8) | Not known | Selects the next harder difficulty. | FND-UI-029, SRC-MANUAL-1994 |
| About | (49, 78, 16, 16) | Not known | Shows the game's version and copyright information. | FND-UI-029, SRC-MANUAL-1994 |
| Animations on or off | (67, 78, 16, 16) | Not known | Turns animations on or off. | FND-UI-029, SRC-MANUAL-1994 |
| Voice on or off | (85, 78, 16, 16) | Not known | Turns voice effects on or off. | FND-UI-029, SRC-MANUAL-1994 |
| Game Menu | (109, 78, 28, 16) | Not known | Returns to SCR-UI-006. | FND-UI-029, SRC-MANUAL-1994 |
| Return to game | (139, 78, 28, 16) | Not known | Closes the screen. | FND-UI-029, SRC-MANUAL-1994 |

## Keyboard input

None known.

## Other input

None known.

## Sounds

None known.

## States

None known.

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Where the window sits, whether the Game Menu's `BMP/10000` is drawn under it, and where the
  volume bars, the difficulty name and the descriptions of FMT-TEXT-004 are drawn (Q-UI-003,
  Q-CONFIG-001).
- The starting values, the volume steps and limits, and which frame each button shows in each
  state (Q-CONFIG-001).
- The manual calls the default difficulty "Average", a name the executable's list of difficulties
  does not hold (FMT-TEXT-004, Q-CONFIG-001).
- No code that handles these buttons is known; no routine holds both numbers of either on-off or
  volume pair (FND-UI-012, FND-UI-013, Q-UI-002).
