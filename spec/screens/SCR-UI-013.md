---
id: SCR-UI-013
title: Load Game
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
| LOAD title | `BUTN/18300`, changed to `ICON/18101` in Load mode | LOAD | (126, 0, 67, 23) control; image 70 x 23 | Load mode | FND-UI-036 |
| LOAD action | `BUTN/18301`, changed to `ICON/18102` in Load mode | LOAD | (231, 30, 44, 15) | Load mode | FND-UI-036 |
| EXIT action | `BUTN/18302`, `ICON/18109` | EXIT | (231, 50, 44, 15) | Window shown | FND-UI-036 |
| Ten list rows | `BUTN/18304` to `/18313`, `ICON/18100` | Contents not established | (46, 31 + 11 * n, 163, 11), n = 0 to 9 | Window shown | FND-UI-036 |
| Name box | `EBOX/18400`, `BMP/10002` | Contents not established | (49, 147, 164, 12) control; image 161 x 12 | Window shown | FND-UI-036 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| A saved game's row | Window-local (46, 31 + 11 * n, 163, 11), n = 0 to 9 | Not known | Chooses that saved game, per the manual; the row callback remains unread. | FND-UI-036, SRC-MANUAL-1994 |
| LOAD | Window-local (231, 30, 44, 15) | Not known | Loads the chosen saved game (RULE-SAVE-002), per the manual; the button callback remains unread. | FND-UI-036, SRC-MANUAL-1994 |
| EXIT | Window-local (231, 50, 44, 15) | Not known | Not established. | FND-UI-036 |

The manual describes a list of saved-game names and an OKAY button
(SRC-MANUAL-1994, page 14). The shipped button art reads LOAD, rather than
OKAY (FND-UI-036).

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

- What the ten rows show, how the window is positioned and painted, how its
  controls handle input, and how it leaves without loading remain open
  (FND-UI-036, FND-SAVE-005, FND-SAVE-006, Q-SAVE-001, Q-UI-005).
