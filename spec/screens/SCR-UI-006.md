---
id: SCR-UI-006
title: Game Menu
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-028, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-UI-001, SCR-UI-002, SCR-UI-007, SCR-UI-008, SCR-UI-009, SCR-UI-010, SCR-UI-013, SCR-UI-014, SCR-UI-020, SCR-UI-021, SCR-UI-022]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Panel | `RESOURCE.GFF#BMP/10000` | None | (0, 0, 210, 116) from the window's corner | While the menu is shown | FND-UI-028 |
| Fourteen buttons | the `ICON` of each `BUTN` child of `RESOURCE.GFF#WIND/10500` | None | each child's position from the window's corner, as FND-UI-028 lists | While the menu is shown | FND-UI-028 |

The window is `WIND/10500`, 210 x 116, drawn over the game view; where the game places it is not
known.

## Mouse input

Rectangles are from the window's corner.

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| View character | (49, 24, 16, 16) | Not known | Opens SCR-UI-002. | FND-UI-028, SRC-MANUAL-1994 |
| View inventory | (81, 24, 16, 16) | Not known | Opens SCR-UI-008. | FND-UI-028, SRC-MANUAL-1994 |
| Cast spells or use psionics | (113, 24, 16, 16) | Not known | Opens SCR-UI-009. | FND-UI-028, SRC-MANUAL-1994 |
| Current spell effects | (145, 24, 16, 16) | Not known | Opens SCR-UI-010. | FND-UI-028, SRC-MANUAL-1994 |
| Exit to DOS | (49, 51, 16, 16) | Not known | Opens the save, quit or cancel choice (SCR-UI-020). | FND-UI-028, SRC-MANUAL-1994 |
| Load or save | (73, 51, 16, 16) | Not known | Opens the load or save choice (SCR-UI-021). | FND-UI-028, SRC-MANUAL-1994 |
| Preferences | (97, 51, 16, 16) | Not known | Opens SCR-UI-007. | FND-UI-028, SRC-MANUAL-1994 |
| Overhead map | (121, 51, 16, 16) | Not known | Opens SCR-UI-022. | FND-UI-028, SRC-MANUAL-1994 |
| Center on leader | (145, 51, 16, 16) | Not known | Centres the view on the party leader. | FND-UI-028, SRC-MANUAL-1994 |
| Collapse party | (44, 78, 28, 16) | Not known | Switches between showing the leader alone and the whole party. | FND-UI-028, SRC-MANUAL-1994 |
| Walk | (76, 78, 16, 16) | Not known | Sets the pointer to walk and returns to the game. | FND-UI-028, SRC-MANUAL-1994 |
| Look | (97, 78, 16, 16) | Not known | Sets the pointer to look and returns to the game. | FND-UI-028, SRC-MANUAL-1994 |
| Attack | (116, 78, 16, 16) | Not known | Sets the pointer to attack and returns to the game. | FND-UI-028, SRC-MANUAL-1994 |
| Return to game | (139, 78, 28, 16) | Not known | Closes the menu. | FND-UI-028, SRC-MANUAL-1994 |

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

- Where the game places the window on the screen (Q-UI-003).
- Which icon frame shows a pointed-at, pressed or unavailable button, and whether the menu shows a
  description line (FND-UI-028, Q-UI-002).
- The effects come from the manual only; no code that handles these buttons is known
  (Q-UI-002).
- What the frames `APFM/10201` (mask 110) and `APFM/11270` do, and why each button has a frame of
  the same rectangle (FND-UI-028, Q-UI-001).
