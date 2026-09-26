---
id: SCR-UI-015
title: Character-box menu
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-002, SCR-UI-003, SCR-UI-004]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Choice menu | Not known | Choices that depend on whether the character box is empty and whether the adventure has begun | Not known | After a right click on a character box | SRC-MANUAL-1994 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| New | Not known | An empty character box was chosen | Opens character generation (SCR-UI-004). | SRC-MANUAL-1994 |
| Add | Not known | An empty character box was chosen | Opens the stored-character list (SCR-UI-003). | SRC-MANUAL-1994 |
| Cancel | Not known | An empty character box was chosen | Closes the menu. | SRC-MANUAL-1994 |
| Edit | Not known | A character was chosen before the adventure | Opens character generation for that character (SCR-UI-004). | SRC-MANUAL-1994 |
| Edit name | Not known | A character was chosen after the adventure began | Allows changing the name on SCR-UI-002. | SRC-MANUAL-1994 |
| Drop | Not known | A character was chosen | Removes the character from the party and stores the character for later addition. | SRC-MANUAL-1994 |
| Dual | Not known | An eligible human character was chosen | Offers a second class. | SRC-MANUAL-1994 |

## Keyboard input

None known.

## Other input

None known.

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Empty box choices | The player right-clicks an empty character box | A choice is made or the menu closes | SRC-MANUAL-1994 |
| Occupied box before adventure | The player right-clicks a character box before starting the adventure | A choice is made or the menu closes | SRC-MANUAL-1994 |
| Occupied box during adventure | The player right-clicks a character box after starting the adventure | A choice is made or the menu closes | SRC-MANUAL-1994 |

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Which native control draws the menu, its placement and appearance, and which handlers select its choices (Q-UI-005).
- The exact eligibility check for Dual belongs to the party rules; the manual's description alone does not establish it (Q-PARTY-007).
