---
id: SCR-UI-010
title: Current Spell Effects
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-019, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-002, SCR-UI-006, SCR-UI-008, SCR-UI-009]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Screen picture | `RESOURCE.GFF#BMP/11000` | None | (0, 9, 320, 200) | While the screen is shown | FND-UI-019 |
| Title | `RESOURCE.GFF#BMP/20075` | None | (85, 11, 152, 23) | While the screen is shown | FND-UI-019 |
| Effects on each character | Not known | One icon per effect | In a row next to the character's icon | While the screen is shown | SRC-MANUAL-1994 |
| Counter spells and powers | Not known | One icon per spell or power that can end an evil effect | In a row at the bottom of the window | When a party member has one | SRC-MANUAL-1994 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Good effect, right button | Not known | Not known | Ends the effect and removes its icon. | SRC-MANUAL-1994 |
| Counter spell or power, then an effect | Not known | Not known | Casts the spell or uses the power against the chosen effect. | SRC-MANUAL-1994 |
| Navigation row | Not known | Not known | As on SCR-UI-002: opens SCR-UI-002, SCR-UI-008, SCR-UI-009 or this screen, or SCR-UI-006, or returns to the game. | SRC-MANUAL-1994 |

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

- Which window resource the screen uses, and where the effect icons are drawn (FND-UI-019,
  Q-UI-002, Q-UI-004).
- The effects come from the manual only (Q-UI-002).
