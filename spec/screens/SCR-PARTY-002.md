---
id: SCR-PARTY-002
title: Learn or enhance a psionic power
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-PARTY-104, FND-PARTY-154]
conflicting: []
split_with: []
related: [RULE-PARTY-013, SCR-PARTY-001]
---

## Drawn elements

The window is `RESOURCE.GFF#WIND/17501`, 159 x 113, placed at (80, 43). Rectangles below are on
the screen.

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Window picture | image 17001 of `WIND/17501` | None | (80, 43, 159, 113) | While the window is shown | FND-PARTY-154 |
| Power icon `i` | `ICON 21235 + p` for the `i`-th listed power `p`, with `ICON` 15104 drawn over it | The power | frame `APFM/11213 + i` plus 1 | While the window is shown and `i` is below the number of listed powers | FND-PARTY-104, FND-PARTY-154 |
| Text line | None | `PICK A PSIONIC POWER,` and the character's name; `LEARN` and the power's name, or `ENHANCE TO LEVEL %d` and the name for a known power, while the pointer is on a power | Not known | While the window is shown | FND-PARTY-154 |
| Picks button | `BUTN/11319` | The number of picks left | (143, 135, 42, 12) | While the window is shown | FND-PARTY-104, FND-PARTY-154 |
| Group button | `BUTN/11320` | `Kinetics`, `Metabolic` or `Telepath` | (92, 135, 48, 12) | While the window is shown | FND-PARTY-104, FND-PARTY-154 |
| Exit button | `BUTN/17302` | `EXIT` | (182, 135, 42, 12) | While the window is shown | FND-PARTY-154 |

Frame `APFM/11213 + 5r + c` is at (112 + 19c, 53 + 20r, 18, 18) for `r` from 0 to 2 and `c` from
0 to 4; frames 11228 to 11230 are at (207, 53), (207, 73) and (207, 93), and 11231 to 11233 at
(93, 53), (93, 73) and (93, 93). Powers fill the frames in that number order.

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Pointer onto a power's frame | the frame | No description has been opened from the window | Highlights the frame and shows `LEARN` or `ENHANCE TO LEVEL %d` with the power's name. | FND-PARTY-154 |
| Pointer off a power's frame | the frame | As above | Removes the highlight and shows `PICK A PSIONIC POWER,` with the character's name. | FND-PARTY-154 |
| Left button released on a power's frame | the frame | As above | Learns the power at rank 1 or raises its rank by 1 (RULE-PARTY-013), takes one pick, and closes the window when no picks are left. | FND-PARTY-104, FND-PARTY-154 |
| Right button released on a power's frame | the frame | As above | Closes an open description, opens the power's description, and stops all frames from answering the pointer. | FND-PARTY-154 |
| Group button | (92, 135, 48, 12) | Not known | Shows the next group of the three and refills the frames with its powers. | FND-PARTY-104, FND-PARTY-154 |
| Exit button | (182, 135, 42, 12) | Not known | Asks `YOU HAVE MORE` with `EXIT` and `CANCEL`; `EXIT` closes the window and the picks left are lost. | FND-PARTY-104, FND-PARTY-154 |

## Keyboard input

None known.

## Other input

| Device | Input | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Unknown | Window event kind 1 | Window shown | Closes the window; the picks left are lost. | FND-PARTY-154 |
| Unknown | Window event kind 6 | Window shown | Calls `2C5F:0182`; the window stays open. | FND-PARTY-154 |

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Choosing | The window opens, with two picks at Psionicist level 4 or an odd level and one otherwise | The last pick is used or the player exits | FND-PARTY-104 |
| Description open | The right button is released on a power | Not known | FND-PARTY-154 |

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Where the text line is drawn, and what the frame highlight looks like (FND-PARTY-154, Q-PARTY-066).
- What the description window shows and whether the frames answer the pointer again after it
  closes (FND-PARTY-154, Q-PARTY-066).
- Whether the picks button answers the pointer (FND-PARTY-154, Q-PARTY-066).
