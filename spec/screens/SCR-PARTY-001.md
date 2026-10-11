---
id: SCR-PARTY-001
title: Learn a spell
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-PARTY-104, FND-PARTY-154, FND-PARTY-155]
conflicting: []
split_with: []
related: [RULE-PARTY-013, SCR-PARTY-002]
---

## Drawn elements

The window is `RESOURCE.GFF#WIND/17500`, 189 x 117, placed at (65, 41). Rectangles below are on
the screen.

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Window picture | image 17000 of `WIND/17500` | None | (65, 41, 189, 117) | While the window is shown | FND-PARTY-154 |
| Spell icon `i` | `ICON 21000 + n` for the `i`-th listed spell `n`, with `ICON` 15104 drawn over it | The spell | frame `APFM/11213 + i` plus 1 | While the window is shown and `i` is below the number of listed spells | FND-PARTY-104, FND-PARTY-154 |
| Text lines | None | `CHOOSE A SPELL,` over the character's name; `LEARN` over the spell's name while the pointer is on a spell; the spell's name over `IS LEARNED!` after a choice | Two lines centred on column 159, at rows 122 and 129, in colours 0xB5 and 0xC2; the old text is first overdrawn in colour 0xC0 | While the window is shown | FND-PARTY-154, FND-PARTY-155 |
| Frame | `ICON` 15104, frame 0, or frame 1 while the pointer is on the frame | None | over each spell frame | While the window is shown and the frame holds a spell | FND-PARTY-154, FND-PARTY-155 |
| Description | `RESOURCE.GFF#WIND/15503`, image 15002; `ICON 21000 + n`; the text of `SPIN/n + 1` in `EBOX/15400`, or `     UNKNOWN` | The spell's icon and description | window (86, 56, 145, 87), icon at (95, 64), text box at (95, 71) | After a right-button release on spell `n`'s frame | FND-PARTY-155 |
| Level button | `BUTN/17300` | `LEVEL` and the spell level shown | (88, 143, 46, 10) | While the window is shown | FND-PARTY-104, FND-PARTY-154 |
| Exit button | `BUTN/17301` | `EXIT` | (186, 143, 46, 10) | While the window is shown | FND-PARTY-154 |

Frame `APFM/11213 + 5r + c` is at (112 + 19c, 65 + 20r, 18, 18) for `r` from 0 to 2 and `c` from
0 to 4; frames 11228 to 11230 are at (207, 65), (207, 85) and (207, 105), and 11231 to 11233 at
(93, 65), (93, 85) and (93, 105). Spells fill the frames in that number order.

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| A spell's frame, pointer enter | the frame | The description window is not open | Highlights the frame and shows `LEARN` with the spell's name. | FND-PARTY-154 |
| A spell's frame, pointer leave | the frame | As above | Removes the highlight and shows `CHOOSE A SPELL,` with the character's name. | FND-PARTY-154 |
| A spell's frame, left release | the frame | As above | Learns the spell (RULE-PARTY-013), shows its name with `IS LEARNED!`, waits 100 vertical retraces and closes the window. | FND-PARTY-104, FND-PARTY-154 |
| A spell's frame, right release | the frame | As above | Closes an open description, opens the spell's description, and stops all frames from answering the pointer until it closes. | FND-PARTY-154, FND-PARTY-155 |
| Description window, left release | (86, 56, 145, 87) | The description window is open | Closes the description and redraws the window, whose frames answer the pointer again. | FND-PARTY-155 |
| Description window, right release | (86, 56, 145, 87) | The description window is open | Closes the description and redraws the window, whose frames answer the pointer again. | FND-PARTY-155 |
| Level button | (88, 143, 46, 10) | The highest level offered is above 1 | Adds 1 to the level shown, going back to 1 after the highest, and refills the frames with that level's unknown spells. | FND-PARTY-104, FND-PARTY-154 |
| Exit button | (186, 143, 46, 10) | Always | Asks `YOU HAVEN'T CHOSEN` with `EXIT` and `CANCEL`; `EXIT` closes the window with no spell learned. | FND-PARTY-104, FND-PARTY-154 |

## Keyboard input

None known.

## Other input

| Device | Input | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Unknown | Window event kind 1 | Window shown | Closes the window with no spell learned. | FND-PARTY-154 |
| Unknown | Window event kind 6 | Window shown | Calls `2C5F:0182`; the window stays open. | FND-PARTY-154 |

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Choosing | The window opens at the highest level offered | A spell is learned or the player exits | FND-PARTY-104 |
| Description open | The right button is released on a spell | The left or right button is released on the description, or an event of kind 1 | FND-PARTY-154, FND-PARTY-155 |

## Timing

After a spell is learned, `IS LEARNED!` stays for 100 vertical retraces, about 1.4 seconds at 70 Hz, before the window closes (FND-PARTY-155).

## Differences between builds

None known.

## Open questions

None.
