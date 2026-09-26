---
id: SCR-UI-014
title: Save Game
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-SAVE-001, RULE-SAVE-002, SCR-UI-006]
---

## Drawn elements

None known.

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| A save slot | Not known | Not known | Chooses the slot and lets the player type a description. | SRC-MANUAL-1994 |
| OKAY | Not known | Not known | Saves the game in the chosen slot with the description (RULE-SAVE-002). | SRC-MANUAL-1994 |

The manual describes the screen as a list of save slots, a free one reading
`<available>`, and an OKAY button (SRC-MANUAL-1994, page 14).

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

- Which window, images and controls make up the screen, how long a description can be, whether a
  used slot can be overwritten, and how the screen leaves without saving (FND-SAVE-004,
  Q-SAVE-001).
