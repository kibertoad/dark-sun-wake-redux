---
id: SCR-UI-012
title: Conversation
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-016, FND-UI-017, FND-UI-032, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-TALK-001, RULE-SCRIPT-007]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Upper panel | `RESOURCE.GFF#BMP/12003` | None | (0, 0, 320, 200) | While the screen is shown | FND-UI-016, FND-UI-017 |
| Lower panel | `RESOURCE.GFF#BMP/12003` | None | (0, 140, 320, 200) | While responses are offered | FND-UI-016, FND-UI-017 |
| Speaker's portrait | a `GPLDATA.GFF#PORT` frame the conversation chooses, `PORT/18` in the capture | None | (0, 0, 72, 72) | While responses are offered; the well is empty in the notice state | FND-UI-016, FND-UI-017 |
| Speech box | `RESOURCE.GFF#BMP/12002` of `EBOX/12400` | The speech or notice text | (75, 6, 243, 47) | While the screen is shown | FND-UI-016, FND-UI-017, FND-UI-032 |
| Response row `n` | `RESOURCE.GFF#ICON/12104 + n` of `BUTN/2076 + n` | The text of response `n` | (3, 153 + 8 * n, 302, 10), `n` from 0 to 4 | While responses are offered | FND-UI-016, FND-UI-032 |

The upper window is `RESOURCE.GFF#WIND/12500` with its origin at (0, 0), and the lower window
`WIND/12501` with its origin at (0, 140).

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Response row `n` | (3, 153 + 8 * n, 300, 10), `n` from 0 to 4 | A response is offered in the row | Chooses the response. | FND-UI-016, FND-UI-032, SRC-MANUAL-1994 |
| Speech scroll buttons | (305, 4, 14, 14) and (305, 18, 14, 35), `BUTN/2093` and `/2094` | Not known | Not known. | FND-UI-016, FND-UI-032 |
| Response scroll buttons | (305, 144, 14, 14) and (305, 158, 14, 35), `BUTN/2095` and `/2096` | Not known | Not known. | FND-UI-016, FND-UI-032 |
| Upper window | (0, 0, 300, 58), `BUTN/12300` | Not known | Not known. | FND-UI-032 |

## Keyboard input

| Key | Enabled when | Effect | Evidence |
|---|---|---|---|
| 1 to 5 | A response is offered in the row with that number | Chooses the response. | SRC-MANUAL-1994 |

## Other input

None known.

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Notice: upper window only, empty portrait well, no rows | The game shows a notice, such as the experience award before the first conversation | Not known | FND-UI-017 |
| Responses offered: both windows, portrait and rows | A conversation offers responses | The player chooses one | FND-UI-016, SRC-MANUAL-1994 |

## Timing

None known.

## Differences between builds

None known.

## Open questions

- What the scroll buttons and the upper window's button do, and why the capture shows the scroll
  buttons without their icons (FND-UI-016, Q-UI-002).
- Which frame shows a pointed-at or chosen row, and how a notice is dismissed (FND-UI-032,
  Q-UI-002, Q-UI-004).
- When `WIND/12502` and `WIND/12503` are used (FND-UI-032, Q-UI-002).
- The scripts print the speech (RULE-SCRIPT-007) and offer the responses through the menu
  instruction (RULE-TALK-001), which shows its title as a row above the responses; where the
  title row is drawn and how rows past the fifth are reached are not measured (Q-TALK-001).
