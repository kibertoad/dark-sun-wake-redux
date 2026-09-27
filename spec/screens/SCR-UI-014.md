---
id: SCR-UI-014
title: Save Game
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-036, FND-UI-037, FND-SAVE-004, FND-SAVE-007, SRC-MANUAL-1994]
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
| Ten list rows | `BUTN/18304` to `/18313`, `ICON/18100` | Description from `STXT/1` for a readable saved game; exact paint not established | (46, 31 + 11 * n, 163, 11), n = 0 to 9 | Window shown | FND-UI-036, FND-SAVE-007 |
| Name box | `EBOX/18400`, `BMP/10002` | Contents not established | (49, 147, 164, 12) control; image 161 x 12 | Window shown | FND-UI-036 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| A save slot | Window-local (46, 31 + 11 * n, 163, 11), n = 0 to 9 | Not known | Stores the selected row index and passes its name field to the name box. | FND-UI-036, FND-UI-037 |
| SAVE | Window-local (231, 30, 44, 15) | Not known | Calls the Save Game routine with the selected row after closing the window (RULE-SAVE-002). | FND-UI-036, FND-UI-037, FND-SAVE-004 |
| Name box | Window-local (49, 147, 164, 12) | Not known | The callback's event-2 branch takes the same path as SAVE. | FND-UI-036, FND-UI-037 |
| EXIT | Window-local (231, 50, 44, 15) | Not known | Closes the window without directly calling Save Game; the later transition remains open. | FND-UI-036, FND-UI-037 |

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

- How the ten rows paint descriptions or unavailable slots, which are selectable, how the window is positioned
  and painted, how long a description can be, whether a used slot can be
  overwritten, what physical input maps to callback event 6, and the later
  transition after EXIT remain open (FND-UI-036, FND-UI-037, Q-SAVE-001,
  Q-UI-005).
