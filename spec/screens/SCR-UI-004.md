---
id: SCR-UI-004
title: Character generation
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-026, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-UI-001, SCR-UI-002, SCR-UI-005]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Class names | `RESOURCE.GFF#ICON/2002` to `/2009` of `BUTN/2002` to `/2009`: `CLERIC`, `DRUID`, `FIGHTER`, `GLADIATOR`, `PRESERVER`, `PSIONICIST`, `RANGER`, `THIEF` | None | (217, 10) to (217, 66), 8 apart, each 7 high | While the screen is shown | FND-UI-026 |
| Exit button | `RESOURCE.GFF#ICON/18109` of `BUTN/18302`, `EXIT` | None | (258, 154, 44, 15) | While the screen is shown | FND-UI-026 |
| Done button | `RESOURCE.GFF#ICON/19100` of `BUTN/19304`, `DONE` | None | (243, 174, 59, 18) | While the screen is shown | FND-UI-026 |

The controls are the children of `RESOURCE.GFF#WIND/19503`, a 320 x 200 window, at their child
positions.

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Portrait, left button | (0, 0, 100, 116), `BUTN/2001` | Not known | Shows the next race and gender. | FND-UI-026, SRC-MANUAL-1994 |
| Portrait, right button | as above | Not known | Shows the previous race and gender. | FND-UI-026, SRC-MANUAL-1994 |
| Class name `n` | (217, 10 + 8 * n) and the button's size, for `BUTN/2002 + n`, `n` from 0 to 7 | Not known | Chooses the class; the manual marks chosen classes with diamonds. | FND-UI-026, SRC-MANUAL-1994 |
| Die | (135, 75, 50, 42), `BUTN/2010` | Not known | Rolls new ability scores. | FND-UI-026, SRC-MANUAL-1994 |
| Ability row `n`, left button | (4, 139 + 7 * n, 50, 5), `BUTN/2012 + n`, `n` from 0 to 5 | Not known | Raises the ability. | FND-UI-026, SRC-MANUAL-1994 |
| Ability row `n`, right button | as above | Not known | Lowers the ability. | FND-UI-026, SRC-MANUAL-1994 |
| Name box | (40, 125, 95, 8), `EBOX/4003`, mask 10 | Not known | Lets the player type a new name. | FND-UI-026, SRC-MANUAL-1994 |
| Exit | (258, 154, 44, 15) | Not known | Leaves without making the character; the manual gives no destination. | FND-UI-026, SRC-MANUAL-1994 |
| Done | (243, 174, 59, 18) | Not known | Finishes the character and returns to SCR-UI-002. | FND-UI-026, SRC-MANUAL-1994 |
| Character icon | (135, 20, 45, 35), `BUTN/2027` | Not known | Not known. | FND-UI-026 |
| Unlabelled areas | `BUTN/2011` (79, 145, 82, 7) and `/2018` (79, 174, 58, 5) | Not known | Not known. | FND-UI-026 |

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

- What picture the screen draws under its controls, and whether the name box draws
  `BMP/19004`, its image. No capture of this screen has been compared (FND-UI-026, Q-UI-004).
- The effects come from the manual and the controls' places in its picture; no code names these
  controls by their numbers (FND-UI-012, Q-UI-002). Which icon frame shows which state, what the
  two unlabelled areas are, and where the list of SCR-UI-005 is placed are not known.
- Where the exit button returns to (Q-UI-002).
