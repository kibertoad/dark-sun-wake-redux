---
id: SCR-UI-020
title: Exit to DOS choice
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-053, FND-SAVE-010, FND-COMBAT-051, FND-PARTY-045, FND-UI-028, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-006, SCR-UI-014]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Title | Not known | `EXIT: SAVE GAME?`, or `EXIT GAME?` while `combat_state` is not 0 | Box corner at (92, 71); the layout inside it is not known | While the choice is shown | FND-UI-053 |
| Items | Not known | `SAVE`, `QUIT` and `CANCEL` in three slots; the `SAVE` slot is empty while `combat_state` is not 0 | In the same box | While the choice is shown | FND-UI-053 |

The choice opens from the Game Menu's Exit to DOS button (`BUTN/10301`, SCR-UI-006), from `F3`,
`Alt-X` and `Ctrl-C`, and from `Esc` when the key routine has no screen to close (FND-UI-053, FND-SAVE-010).

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Save | Not known | `combat_state` is 0 | Opens SCR-UI-014; after its confirm button, the game ends. | FND-UI-053, FND-PARTY-045 |
| Quit | Not known | Always | Ends the game without saving: `exit_control_byte` becomes 0. | FND-UI-053 |
| Cancel | Not known | Always | Returns to the game in progress. | FND-UI-053 |

## Keyboard input

| Key | Enabled when | Effect | Evidence |
|---|---|---|---|
| `S`, `Q`, `C` in either case | While the choice is shown | Chooses the item whose label starts with the letter | FND-COMBAT-051, FND-UI-053 |
| `Esc` | While the choice is shown | Closes the choice as Cancel does | FND-COMBAT-051, FND-UI-053 |

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

- Which images draw the box, title and items, and where inside the box from the corner
  (92, 71) they sit, which overlay 172 `+018F` decides (FND-UI-053, Q-UI-005).
