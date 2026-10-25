---
id: SCR-UI-007
title: Preferences
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-033, FND-UI-034, FND-UI-058, FND-UI-064, FND-UI-065, FND-UI-066, FND-CONFIG-218, FND-INPUT-012, FND-CONFIG-010, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-CONFIG-001, RULE-CONFIG-002, RULE-CONFIG-003, RULE-CONFIG-004, RULE-CONFIG-005, SCR-UI-006, SCR-UI-023]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Thirteen buttons | the `ICON` of each `BUTN` child of `RESOURCE.GFF#WIND/16500` | None | each child's position from the window's corner, as FND-UI-033 lists | While the screen is shown | FND-UI-033 |
| Top bar | `BMP/20106`, filled with `BMP/20107` from (76, 27) | the message delay minus 20 out of 80, or after a click on Music on or off the music level (0 when music is off) | (75, 26) from the window's corner | While the screen is shown | FND-UI-065 |
| Effects bar | `BMP/20106`, filled with `BMP/20107` from (76, 46) | the effects volume out of 127; 0 after a click while effects are off | (75, 45) from the window's corner | While the screen is shown | FND-UI-065 |
| Difficulty bar | `BMP/20106`, filled with `BMP/20107` from (66, 65) | the difficulty out of 3 | (65, 64) from the window's corner | While the screen is shown | FND-UI-065 |
| Difficulty name | the name from FMT-TEXT-004's difficulty list | the current difficulty | y 66, centred on x 106, from the window's corner | While the screen is shown | FND-UI-065 |
| Description | `APFM/11270`'s text | the label of the pointed-at control, or nothing | (18, 102) from the window's corner | While the pointer is over a control | FND-UI-034, FND-UI-065 |

A filled part is `164 * value / limit / 2` pixels wide and 9 high.

The window is `WIND/16500`, 210 x 116, with its corner at (55, 42) when the Game Menu opens it
(FND-UI-058).

## Mouse input

Rectangles are from the window's corner.

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Music on or off | (49, 24, 16, 16) | Not known | Turns music on or off (RULE-CONFIG-001). | FND-UI-033, SRC-MANUAL-1994 |
| Message delay down | (66, 29, 9, 8) | Not known | Lowers the message-delay value (RULE-CONFIG-005). | FND-UI-033, FND-UI-034, FND-CONFIG-010 |
| Message delay up | (159, 29, 9, 8) | Not known | Raises the message-delay value (RULE-CONFIG-005). | FND-UI-033, FND-UI-034, FND-CONFIG-010 |
| Sound effects on or off | (49, 43, 16, 16) | Not known | Turns sound effects on or off (RULE-CONFIG-001). | FND-UI-033, SRC-MANUAL-1994 |
| Sound volume down | (66, 48, 9, 8) | Not known | Lowers the sound effects volume (RULE-CONFIG-004). | FND-UI-033, FND-UI-034, FND-CONFIG-010 |
| Sound volume up | (159, 48, 9, 8) | Not known | Raises the sound effects volume (RULE-CONFIG-004). | FND-UI-033, FND-UI-034, FND-CONFIG-010 |
| Difficulty down | (56, 67, 9, 8) | Not known | Selects the next easier difficulty (RULE-CONFIG-002). | FND-UI-033, SRC-MANUAL-1994 |
| Difficulty up | (149, 67, 9, 8) | Not known | Selects the next harder difficulty (RULE-CONFIG-002). | FND-UI-033, SRC-MANUAL-1994 |
| Animations on or off | (49, 78, 16, 16) | Not known | Toggles the saved animation state at `PREF/100` offset `0x07` when its guard permits. | FND-UI-033, FND-CONFIG-010 |
| About | (67, 78, 16, 16) | Not known | Opens the About view (SCR-UI-023). | FND-UI-034 |
| Voice on or off | (85, 78, 16, 16) | Not known | Turns voice effects on or off (RULE-CONFIG-001). | FND-UI-033, SRC-MANUAL-1994 |
| Game Menu | (109, 78, 28, 16) | Not known | Returns to SCR-UI-006. | FND-UI-033, SRC-MANUAL-1994 |
| Return to game | (139, 78, 28, 16) | Not known | Closes the screen. | FND-UI-033, SRC-MANUAL-1994 |

## Keyboard input

| Key | Enabled when | Effect | Evidence |
|---|---|---|---|
| Esc | While the screen is shown | Closes the screen. | FND-UI-064, FND-UI-065 |
| F4, `A` | While the screen is shown | Turn music or animations on or off as in the game view; the window stays open and the button showing the setting is repainted. | FND-UI-066, FND-CONFIG-218 |
| Any other key | While the screen is shown | Goes to the game view's key routine, overlay 190 `+139B`, with the screen's close routine. | FND-UI-065, FND-INPUT-012 |

## Other input

None known.

## Sounds

| Sound | Resource | Played when | Evidence |
|---|---|---|---|
| Effects test | sound id 231 | Effects are turned on, or the volume changes while they are on | FND-UI-065 |
| Difficulty | sound id 205, 143, 231 or 24 for difficulties 0 to 3 | A difficulty arrow is clicked while effects are on | FND-UI-065 |

## States

None known.

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Whether the Game Menu's `BMP/10000` is drawn under it (Q-UI-003).
- The new-game values, message-delay timing units and upper bound, and which frame each
  button shows in each state (FMT-CONFIG-003, Q-CONFIG-001, Q-CONFIG-002, Q-CONFIG-007). The
  music bar has no control on this screen (RULE-CONFIG-003, FND-CONFIG-219).
- The manual calls the default difficulty "Average", a name the executable's list of difficulties
  does not hold (FMT-TEXT-004, Q-CONFIG-001).
- What native widget treatment makes `BUTN/16302` at (67, 78) visible; its four
  extracted icon frames each have one uniform colour (FND-UI-033, FND-UI-034,
  Q-CONFIG-001, Q-UI-013).
