---
id: SCR-UI-021
title: Load or Save choice
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-053, FND-COMBAT-051, FND-UI-028, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-006, SCR-UI-013, SCR-UI-014]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Title | Not known | `LOAD/SAVE GAME` | Box corner at (92, 71); the layout inside it is not known | While the choice is shown | FND-UI-053 |
| Items | Not known | `LOAD` and `SAVE` in the first two of three slots; the `SAVE` slot is empty while `combat_state` is not 0, and the third is empty | In the same box | While the choice is shown | FND-UI-053 |

The choice opens from the Game Menu's Load or save button (`BUTN/10302`, SCR-UI-006)
(FND-UI-053).

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Load | Not known | Always | Opens SCR-UI-013. | FND-UI-053 |
| Save | Not known | `combat_state` is 0 | Opens SCR-UI-014. | FND-UI-053 |

## Keyboard input

| Key | Enabled when | Effect | Evidence |
|---|---|---|---|
| `L`, `S` in either case | While the choice is shown | Chooses the item whose label starts with the letter | FND-COMBAT-051, FND-UI-053 |
| `Esc` | While the choice is shown | Closes the choice without loading or saving | FND-COMBAT-051, FND-UI-053 |

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
