---
id: SCR-UI-003
title: Stored-character list
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-027, SRC-YOUTUBE-FLOMVOSHEOM]
conflicting: []
split_with: []
related: [RULE-UI-001, SCR-UI-002]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | `RESOURCE.GFF#BMP/10005` | None | (0, 0, 320, 200) | While the screen is shown | FND-UI-027, SRC-YOUTUBE-FLOMVOSHEOM |
| Title | `RESOURCE.GFF#ICON/18103` | None | (110, 0, 67, 23) | While the screen is shown | SRC-YOUTUBE-FLOMVOSHEOM |
| Add button | `RESOURCE.GFF#ICON/18104` | None | (231, 30, 44, 15) | While the screen is shown | SRC-YOUTUBE-FLOMVOSHEOM |
| Exit button | `RESOURCE.GFF#ICON/18109` of `BUTN/18302` | None | (231, 50, 44, 15) | While the screen is shown | FND-UI-027, SRC-YOUTUBE-FLOMVOSHEOM |
| Delete button | `RESOURCE.GFF#ICON/18110` of `BUTN/18303` | None | (215, 148, 62, 15) | While the screen is shown | FND-UI-027, SRC-YOUTUBE-FLOMVOSHEOM |
| Ten list rows | `RESOURCE.GFF#ICON/18100` of `BUTN/18304` to `/18313` | None | (46, 31, 165, 11) to (46, 130, 165, 11), 11 apart | While the screen is shown | FND-UI-027 |
| Scroll up | `RESOURCE.GFF#ICON/12102` of `BUTN/10314` | None | (215, 30, 14, 10) | While the screen is shown | FND-UI-027, SRC-YOUTUBE-FLOMVOSHEOM |
| Scroll down | `RESOURCE.GFF#ICON/12101` of `BUTN/10315` | None | (215, 130, 14, 14) | While the screen is shown | FND-UI-027, SRC-YOUTUBE-FLOMVOSHEOM |

The controls are the children of `RESOURCE.GFF#WIND/18501`, a 320 x 181 window, at their child
positions with the window at (0, 0).

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| List row | (46, 31 + 11 * n, 163, 11) for row `n` from 0 to 9, mask 208 | Not known | Not known. | FND-UI-027 |
| Add | (231, 30, 44, 15) | Not known | Not known. | FND-UI-027 |
| Exit | (231, 50, 44, 15) | Not known | Returns to SCR-UI-002. | FND-UI-027, SRC-YOUTUBE-FLOMVOSHEOM |
| Delete | (215, 148, 62, 15) | Not known | Not known. | FND-UI-027 |
| Scroll up | (215, 30, 14, 14) | Not known | Not known. | FND-UI-027 |
| Scroll down | (215, 130, 14, 14) | Not known | Not known. | FND-UI-027 |
| Name box | (49, 147, 164, 12), `EBOX/18401`, mask 10 | Not known | Not known. | FND-UI-027 |

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

- The background, the title and the add button's image come from a recorded playthrough only.
  The window's records name `ICON/18107` and `ICON/18108` for the title and the add button, whose
  art shows the save screen's words, so the game may put other images in their place when it
  shows this list (FND-UI-027, Q-UI-004).
- What the rows show, how the list scrolls and what add, delete and the name box do
  (FND-UI-027, Q-UI-002).
- Where the window sits on the screen (Q-UI-003).
