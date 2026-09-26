---
id: SCR-UI-005
title: Discipline and sphere lists
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-026, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-UI-001, SCR-UI-004]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Panel | `RESOURCE.GFF#BMP/20087` | None | (0, 0, 111, 64) from the window's corner | In either state | FND-UI-026 |
| Four discipline labels and a label for the sphere list | `RESOURCE.GFF#ICON/2038` to `/2041` and `/2046` | None | (7, 15) to (7, 47) from the window's corner, 8 apart | In the discipline state | FND-UI-026 |
| Four sphere labels and a label for the discipline list | `RESOURCE.GFF#ICON/2042` to `/2045` and `/2047` | None | (7, 15) to (7, 47) from the window's corner, 8 apart | In the sphere state | FND-UI-026 |

The window is `RESOURCE.GFF#WIND/19504` in the discipline state and `WIND/19505` in the sphere
state, each 110 x 64. The manual's picture shows the list at the lower right of SCR-UI-004; where
the game places it is not known.

## Mouse input

Rectangles are from the window's corner.

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| List entry `n` | (7, 15 + 8 * n) and the button's size, `n` from 0 to 3 | Not known | Not known. | FND-UI-026 |
| Switch label, discipline state | (7, 47) and the size of `BUTN/2046` | Never by the pointer search: its mask is 2 (RULE-UI-001) | Shows the sphere list. | FND-UI-026, SRC-MANUAL-1994 |
| Switch label, sphere state | (7, 47) and the size of `BUTN/2047` | Not known | Shows the discipline list. | FND-UI-026, SRC-MANUAL-1994 |

## Keyboard input

None known.

## Other input

None known.

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Discipline list, `WIND/19504` | Not known; the player chooses the discipline label in the sphere state | The player chooses the sphere label | FND-UI-026, SRC-MANUAL-1994 |
| Sphere list, `WIND/19505` | The player chooses the sphere label | The player chooses the discipline label | FND-UI-026, SRC-MANUAL-1994 |

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Which list shows first, where the game places it, and what choosing a discipline or sphere does
  (FND-UI-026, Q-UI-002, Q-UI-003).
- How the switch label of the discipline list is chosen when its mask keeps the pointer search
  from choosing it, and why the fourth discipline label has empty first and third frames
  (FND-UI-026, Q-UI-001).
- Whether the game draws `BMP/20087`, the image the windows name, as their panel (FND-UI-002,
  Q-UI-001).
