---
id: SCR-UI-001
title: Start window
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-024, SRC-YOUTUBE-FLOMVOSHEOM, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-PARTY-006, SCR-UI-002]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Background | None, black | None | (0, 0, 320, 200) | While the screen is shown | SRC-YOUTUBE-FLOMVOSHEOM |
| Stone frame | `RESOURCE.GFF#BMP/20029` | None | (3, 44, 314, 112) | While the screen is shown | FND-UI-024, SRC-YOUTUBE-FLOMVOSHEOM |
| Crest | `RESOURCE.GFF#BMP/20028` | None | (47, 24, 222, 33) | While the screen is shown, over the frame | FND-UI-024, SRC-YOUTUBE-FLOMVOSHEOM |
| Start game | `RESOURCE.GFF#ICON/19111` of `BUTN/19300` | None | (94, 70, 127, 12) | While the screen is shown | FND-UI-024 |
| Create characters | `RESOURCE.GFF#ICON/19112` of `BUTN/19301` | None | (50, 87, 220, 12) | While the screen is shown | FND-UI-024 |
| Load saved game | `RESOURCE.GFF#ICON/19113` of `BUTN/19302` | None | (64, 104, 191, 13) | While the screen is shown | FND-UI-024 |
| Exit to DOS | `RESOURCE.GFF#ICON/19114` of `BUTN/19303` | None | (92, 120, 127, 12) | While the screen is shown | FND-UI-024 |

The four buttons are the children of `RESOURCE.GFF#WIND/19500`, a 320 x 200 window, at their
child positions; their first frames are listed here.

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Start game | (94, 70, 127, 12) | Always | Starts a game with the party the game supplies (RULE-PARTY-006). | FND-UI-024, SRC-MANUAL-1994 |
| Create characters | (50, 87, 220, 12) | Always | Opens SCR-UI-002. | FND-UI-024, SRC-MANUAL-1994, SRC-YOUTUBE-FLOMVOSHEOM |
| Load saved game | (64, 104, 192, 12) | Always | Loads a saved game. | FND-UI-024 |
| Exit to DOS | (92, 120, 127, 12) | Always | Leaves the game. | FND-UI-024 |

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

- Where the game draws the frame and the crest, and in which order, is taken from a recorded
  playthrough of an unknown release; no record or code gives it (FND-UI-024, Q-UI-003).
- Which frame of each icon the game shows when a button is pointed at, pressed or unavailable,
  given that three icons have a 1 x 1 third frame (FND-UI-024, Q-UI-002).
- What the load and exit buttons do, and whether the first two do what the manual says, is not
  shown by any code; no routine names the four buttons by their numbers (FND-UI-012, Q-UI-002).
- Whether the title picture and palette shown before this window change when it opens
  (Q-UI-004).
