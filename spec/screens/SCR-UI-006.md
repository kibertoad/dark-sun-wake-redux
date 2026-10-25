---
id: SCR-UI-006
title: Game Menu
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-028, FND-UI-046, FND-UI-053, FND-UI-055, FND-UI-056, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-UI-001, SCR-UI-002, SCR-UI-007, SCR-UI-008, SCR-UI-009, SCR-UI-010, SCR-UI-013, SCR-UI-014, SCR-UI-020, SCR-UI-021, SCR-UI-022]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Panel | `RESOURCE.GFF#BMP/10000` | None | (0, 0, 210, 116) from the window's corner | While the menu is shown | FND-UI-028 |
| Fourteen buttons | the `ICON` of each `BUTN` child of `RESOURCE.GFF#WIND/10500` | None | each child's position from the window's corner, as FND-UI-028 lists | While the menu is shown | FND-UI-028 |

The window is `WIND/10500`, 210 x 116, drawn over the game view with its corner at (55, 42)
(FND-UI-055, FND-UI-056).

## Mouse input

Rectangles are from the window's corner.

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| View character | (49, 24, 16, 16) | Not known | Closes the menu and opens SCR-UI-002 for the leader. | FND-UI-028, FND-UI-055 |
| View inventory | (81, 24, 16, 16) | Not known | Closes the menu and opens SCR-UI-008 for the leader. | FND-UI-028, FND-UI-055 |
| Cast spells or use psionics | (113, 24, 16, 16) | Not known | Closes the menu and opens SCR-UI-009 for the leader. | FND-UI-028, FND-UI-055 |
| Current spell effects | (145, 24, 16, 16) | Not known | Closes the menu and opens SCR-UI-010 for the leader. | FND-UI-028, FND-UI-055 |
| Exit to DOS | (49, 51, 16, 16) | Not known | Opens the save, quit or cancel choice (SCR-UI-020). | FND-UI-028, FND-UI-053 |
| Load or save | (73, 51, 16, 16) | Not known | Opens the load or save choice (SCR-UI-021). | FND-UI-028, FND-UI-053 |
| Preferences | (97, 51, 16, 16) | Not known | Closes the menu and opens SCR-UI-007. | FND-UI-028, FND-UI-055 |
| Overhead map | (121, 51, 16, 16) | Not known | Opens SCR-UI-022. | FND-UI-028, FND-UI-046, SRC-MANUAL-1994 |
| Center on leader | (145, 51, 16, 16) | Not known | Closes the menu and centres the view on the party leader. | FND-UI-028, FND-UI-055 |
| Collapse party | (44, 78, 28, 16) | Outside combat; in combat it does nothing | Switches between showing the leader alone and the whole party, as keys 6 and 5 do, and sets the button's frame from the new state; the menu stays open. | FND-UI-028, FND-UI-055 |
| Walk | (76, 78, 16, 16) | Not known | Sets the pointer mode to 1 (walk), closes the menu and returns to the game. | FND-UI-028, FND-UI-055 |
| Look | (97, 78, 16, 16) | Not known | Sets the pointer mode to 2 (look), closes the menu and returns to the game. | FND-UI-028, FND-UI-055 |
| Attack | (116, 78, 16, 16) | Not known | Sets the pointer mode to 4 (attack), closes the menu and returns to the game. | FND-UI-028, FND-UI-055 |
| Return to game | (139, 78, 28, 16) | Not known | Closes the menu. | FND-UI-028, FND-UI-055 |

## Keyboard input

Tab in the game view opens the menu (FND-INPUT-012, FND-UI-056).

| Key | Enabled when | Effect | Evidence |
|---|---|---|---|
| Tab | While the menu is shown | Nothing. | FND-UI-046 |
| Any other key | While the menu is shown | Goes to the game view's key routine, overlay 190 `+139B`, with the menu's handler. | FND-UI-046 |

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

- Which icon frame shows a pointed-at, pressed or unavailable button (FND-UI-028, Q-UI-013).
  Pointing at a button changes its border and writes no description in the bar below the
  buttons (FND-UI-056); what draws the border, and what the callbacks the opener registers do
  (FND-UI-055), remain (Q-UI-013).
- What the frames `APFM/10201` (mask 110) and `APFM/11270` do, and why each button has a frame of
  the same rectangle (FND-UI-028, Q-UI-001).
