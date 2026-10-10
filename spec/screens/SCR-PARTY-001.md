---
id: SCR-PARTY-001
title: Learn a spell
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-PARTY-104, FND-PARTY-154]
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
| Text line | None | `CHOOSE A SPELL,` and the character's name; `LEARN` and the spell's name while the pointer is on a spell; the spell's name and `IS LEARNED!` after a choice | Not known | While the window is shown | FND-PARTY-154 |
| Level button | `BUTN/17300` | `LEVEL` and the spell level shown | (88, 143, 46, 10) | While the window is shown | FND-PARTY-104, FND-PARTY-154 |
| Exit button | `BUTN/17301` | `EXIT` | (186, 143, 46, 10) | While the window is shown | FND-PARTY-154 |

Frame `APFM/11213 + 5r + c` is at (112 + 19c, 65 + 20r, 18, 18) for `r` from 0 to 2 and `c` from
0 to 4; frames 11228 to 11230 are at (207, 65), (207, 85) and (207, 105), and 11231 to 11233 at
(93, 65), (93, 85) and (93, 105). Spells fill the frames in that number order.

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Pointer onto a spell's frame | the frame | No description has been opened from the window | Highlights the frame and shows `LEARN` with the spell's name. | FND-PARTY-154 |
| Pointer off a spell's frame | the frame | As above | Removes the highlight and shows `CHOOSE A SPELL,` with the character's name. | FND-PARTY-154 |
| Left button released on a spell's frame | the frame | As above | Learns the spell (RULE-PARTY-013), shows its name with `IS LEARNED!`, calls overlay 186 `+00B8` with 100 and closes the window. | FND-PARTY-104, FND-PARTY-154 |
| Right button released on a spell's frame | the frame | As above | Closes an open description, opens the spell's description, and stops all frames from answering the pointer. | FND-PARTY-154 |
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
| Description open | The right button is released on a spell | Not known | FND-PARTY-154 |

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Where the text line is drawn, and what the frame highlight looks like (FND-PARTY-154, Q-PARTY-066).
- What overlay 186 `+00B8` does with 100 after a spell is learned (FND-PARTY-104, Q-PARTY-066).
- What the description window shows and whether the frames answer the pointer again after it
  closes (FND-PARTY-154, Q-PARTY-066).
