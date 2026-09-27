---
id: SCR-UI-007
title: Preferences
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-033, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-CONFIG-001, RULE-CONFIG-002, RULE-CONFIG-003, SCR-UI-006, SCR-UI-023]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Thirteen buttons | the `ICON` of each `BUTN` child of `RESOURCE.GFF#WIND/16500` | None | each child's position from the window's corner, as FND-UI-033 lists | While the screen is shown | FND-UI-033 |

The window is `WIND/16500`, 210 x 116; where the game places it is not known.

## Mouse input

Rectangles are from the window's corner.

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Music on or off | (49, 24, 16, 16) | Not known | Turns music on or off (RULE-CONFIG-001). | FND-UI-033, SRC-MANUAL-1994 |
| Music volume down | (66, 29, 9, 8) | Not known | Lowers the music volume (RULE-CONFIG-003). | FND-UI-033, SRC-MANUAL-1994 |
| Music volume up | (159, 29, 9, 8) | Not known | Raises the music volume (RULE-CONFIG-003). | FND-UI-033, SRC-MANUAL-1994 |
| Sound effects on or off | (49, 43, 16, 16) | Not known | Turns sound effects on or off (RULE-CONFIG-001). | FND-UI-033, SRC-MANUAL-1994 |
| Sound volume down | (66, 48, 9, 8) | Not known | Lowers the sound effects volume (RULE-CONFIG-003). | FND-UI-033, SRC-MANUAL-1994 |
| Sound volume up | (159, 48, 9, 8) | Not known | Raises the sound effects volume (RULE-CONFIG-003). | FND-UI-033, SRC-MANUAL-1994 |
| Difficulty down | (56, 67, 9, 8) | Not known | Selects the next easier difficulty (RULE-CONFIG-002). | FND-UI-033, SRC-MANUAL-1994 |
| Difficulty up | (149, 67, 9, 8) | Not known | Selects the next harder difficulty (RULE-CONFIG-002). | FND-UI-033, SRC-MANUAL-1994 |
| Animations on or off | (49, 78, 16, 16) | Not known | Animation control with saved state at `PREF/100` offset `0x07`; its transition is unknown. | FND-UI-033, SRC-MANUAL-1994 |
| Unidentified control, About candidate | (67, 78, 16, 16) | Not known | Not established. | FND-UI-033 |
| Voice on or off | (85, 78, 16, 16) | Not known | Turns voice effects on or off (RULE-CONFIG-001). | FND-UI-033, SRC-MANUAL-1994 |
| Game Menu | (109, 78, 28, 16) | Not known | Returns to SCR-UI-006. | FND-UI-033, SRC-MANUAL-1994 |
| Return to game | (139, 78, 28, 16) | Not known | Closes the screen. | FND-UI-033, SRC-MANUAL-1994 |

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
  state; some saved fields remain unidentified (FMT-CONFIG-003, Q-CONFIG-001, Q-CONFIG-002).
- The manual calls the default difficulty "Average", a name the executable's list of difficulties
  does not hold (FMT-TEXT-004, Q-CONFIG-001).
- Whether `BUTN/16302` at (67, 78) opens About or has another effect; its four
  extracted icon frames each have one uniform colour (FND-UI-033, Q-CONFIG-001, Q-UI-002).
- No code that handles these buttons is known; no resident routine holds both numbers of either
  on-off or volume pair (FND-UI-012, FND-UI-013, Q-UI-002).
