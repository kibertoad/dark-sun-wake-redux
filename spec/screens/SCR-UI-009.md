---
id: SCR-UI-009
title: Cast Spells and Use Psionics
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-019, FND-MAGIC-001, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-002, SCR-UI-006, SCR-UI-008, SCR-UI-010]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Screen picture | `RESOURCE.GFF#BMP/11000` | None | (0, 9, 320, 200) | While the screen is shown | FND-UI-019 |
| Title | `RESOURCE.GFF#BMP/20080` | None | (109, 11, 104, 23) | While the screen is shown | FND-UI-019 |
| Name of the pointed-at spell or psionic power | Not known | The name | At the bottom of the window | While the pointer is on a spell or power | SRC-MANUAL-1994 |
| Group captions | Not known | The spell class and spell level of the icons shown, such as MAGE and LEVEL 1, or PSIONIC and the discipline, such as Metabolic | Two boxes in the bottom bar | While the screen is shown | FND-MAGIC-001 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Type and level buttons | Not known, in the bottom row | Not known | Show the spells or powers of another type or level. | SRC-MANUAL-1994 |
| Spell or power icon | Not known | The character can still cast it; an orange cross over the icons marks a level with no casts left | Makes the pointer the spell's icon for choosing a target and returns to the game; some spells, such as healing, are cast here by clicking the target's character icon. | SRC-MANUAL-1994 |
| Navigation row | Not known | Not known | As on SCR-UI-002: opens SCR-UI-002, SCR-UI-008, this screen or SCR-UI-010, or SCR-UI-006, or returns to the game. | SRC-MANUAL-1994 |

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

- Which window resource the screen uses, and where its buttons, icons and names are drawn
  (FND-UI-019, Q-UI-002, Q-UI-004).
- The effects come from the manual only (Q-UI-002). Targeting after the choice belongs to the
  magic rules.
