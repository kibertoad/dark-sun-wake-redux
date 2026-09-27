---
id: SCR-UI-014
title: Save Game
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-036, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-SAVE-001, RULE-SAVE-002, SCR-UI-006]
---

## Drawn elements

`RESOURCE.GFF#WIND/18500` is a 320 x 181 window with no image in its own
image field. Positions below are relative to the window; its screen position
and native widget chrome are not established (FND-UI-036).

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| SAVE title | `BUTN/18300`, `ICON/18107` | SAVE | (126, 0, 67, 23) | Save mode | FND-UI-036 |
| SAVE action | `BUTN/18301`, `ICON/18108` | SAVE | (231, 30, 44, 15) | Save mode | FND-UI-036 |
| EXIT action | `BUTN/18302`, `ICON/18109` | EXIT | (231, 50, 44, 15) | Window shown | FND-UI-036 |
| Ten list rows | `BUTN/18304` to `/18313`, `ICON/18100` | Contents not established | (46, 31 + 11 * n, 163, 11), n = 0 to 9 | Window shown | FND-UI-036 |
| Name box | `EBOX/18400`, `BMP/10002` | Contents not established | (49, 147, 164, 12) control; image 161 x 12 | Window shown | FND-UI-036 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| A save slot | Window-local (46, 31 + 11 * n, 163, 11), n = 0 to 9 | Not known | Chooses the slot and lets the player type a description, per the manual; the row callback remains unread. | FND-UI-036, SRC-MANUAL-1994 |
| SAVE | Window-local (231, 30, 44, 15) | Not known | Saves the game in the chosen slot with the description (RULE-SAVE-002), per the manual; the button callback remains unread. | FND-UI-036, SRC-MANUAL-1994 |
| EXIT | Window-local (231, 50, 44, 15) | Not known | Not established. | FND-UI-036 |

The manual describes a list of save slots, a free one reading
`<available>`, and an OKAY button (SRC-MANUAL-1994, page 14). The shipped
button art reads SAVE, rather than OKAY (FND-UI-036).

## Keyboard input

| Key | Enabled when | Effect | Evidence |
|---|---|---|---|
| Printable keys | A slot is chosen | Type the description of the saved game. | SRC-MANUAL-1994 |

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

- What the ten rows show, how the window is positioned and painted, how long
  a description can be, whether a used slot can be overwritten, how the
  controls handle input, and how it leaves without saving remain open
  (FND-UI-036, FND-SAVE-004, Q-SAVE-001, Q-UI-005).
