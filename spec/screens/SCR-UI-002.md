---
id: SCR-UI-002
title: View Character
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-020, FND-UI-025, FND-UI-030, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-PARTY-001, RULE-PARTY-004, RULE-PARTY-008, RULE-UI-001, SCR-UI-001, SCR-UI-003, SCR-UI-004, SCR-UI-006, SCR-UI-008, SCR-UI-009, SCR-UI-010]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Screen picture | `RESOURCE.GFF#BMP/11000` | None | (0, 9, 320, 200) | While the screen is shown | FND-UI-020 |
| Title | `RESOURCE.GFF#BMP/20079` | None | (56, 11, 210, 23) | While the screen is shown | FND-UI-020 |
| Controls | the `ICON` of each `BUTN` child of `RESOURCE.GFF#WIND/11500` | None | each child's position from the window's corner, as FND-UI-030 lists | While the screen is shown | FND-UI-030 |

The window is `WIND/11500`, 320 x 189. Where the game places it is not known, so the rectangles
below are from the window's corner.

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Character box `n`, left button | (53, 30, 34, 34), (104, 30, 34, 34), (53, 90, 34, 34) and (104, 90, 34, 34) for `BUTN/11300` to `/11303` | Not known | Highlights the character in the box and shows the character's statistics on the right side of the screen. | FND-UI-030, SRC-MANUAL-1994 |
| Character box, right button, box holds a character | as above | Not known | Offers a choice to edit the character, which opens SCR-UI-004, to drop the character to disk, or to choose a second class for a human (RULE-PARTY-004). Once the adventure has begun, the edit choice changes only the name, in the name box. | FND-UI-030, SRC-MANUAL-1994 |
| Character box, right button, box empty | as above | Not known | Offers a choice to make a new character, which opens SCR-UI-004, to add a stored one, which opens SCR-UI-003, or to cancel. A party holds at most four characters (RULE-PARTY-001). | FND-UI-030, SRC-MANUAL-1994 |
| Small buttons beside each box | `BUTN/11309` to `/11316`, 10 x 9 | Not known | One turns computer control of the character in combat on or off, the other makes the character the leader for walking and talking (RULE-PARTY-008). | FND-UI-030, SRC-MANUAL-1994 |
| View character | (43, 155, 16, 16), `BUTN/10300` | Not known | Not known; this screen is already shown. | FND-UI-030, SRC-MANUAL-1994 |
| View inventory | (67, 155, 16, 16), `BUTN/11304` | Not known | Opens SCR-UI-008. | FND-UI-030, SRC-MANUAL-1994 |
| Cast spells or use psionics | (91, 155, 16, 16), `BUTN/11305` | Not known | Opens SCR-UI-009. | FND-UI-030, SRC-MANUAL-1994 |
| Current spell effects | (114, 155, 16, 16), `BUTN/11306` | Not known | Opens SCR-UI-010. | FND-UI-030, SRC-MANUAL-1994 |
| Game Menu | (223, 155, 28, 16), `BUTN/11308` | After the party has begun the adventure | Opens SCR-UI-006. | FND-UI-030, SRC-MANUAL-1994 |
| Return to game | (253, 155, 28, 16), `BUTN/10308` | Not known | Closes the screen. | FND-UI-030, SRC-MANUAL-1994 |
| Name box | (153, 28, 95, 8), `EBOX/4003`, mask 10 | Not known | Not known. | FND-UI-030 |
| Buttons with mask 160 | (132, 157, 42, 12) and (173, 157, 48, 12), `BUTN/11319` and `/11320` | Not known | Not known. | FND-UI-030 |

`BUTN/11318` at (151, 26), 123 x 11, has mask 84; the pointer search passes over it
(RULE-UI-001).

## Keyboard input

None known.

## Other input

None known.

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Party creation, four empty boxes | The player chooses to create characters on SCR-UI-001 | Not known | SRC-MANUAL-1994 |
| During the adventure | The player chooses View Character on SCR-UI-006 | The player returns to the game | FND-UI-020, SRC-MANUAL-1994 |

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Where the game places `WIND/11500`, and whether the picture at (0, 9) is drawn the same way in
  party creation. The captures of FND-UI-020 were taken during the adventure (Q-UI-003, Q-UI-004).
- What each box, its status areas and the frames of `WIND/11500` show, which of the two small
  buttons is which, and what the name box and the buttons with mask 160 do (FND-UI-030,
  Q-UI-002).
- The effects come from the manual only; no code that names these buttons is known (FND-UI-012,
  Q-UI-002).
- What `WIND/19501` and `WIND/19502` are for (FND-UI-025, Q-UI-001).
