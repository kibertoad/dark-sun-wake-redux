---
id: SCR-UI-018
title: Item summary
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-008]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Summary box | Not known | Information about the selected item | Not known | After a right click on an inventory item | SRC-MANUAL-1994 |
| Spell icon | Not known | A spell associated with an item or scroll | Upper-right corner of the box for a usable magical item; other placement not known | When the selected item offers one | SRC-MANUAL-1994 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Spell icon | Not known | The item offers a usable spell | Uses the item's spell, or learns the spell when the item is a scroll. | SRC-MANUAL-1994 |

## Keyboard input

None known.

## Other input

None known.

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Item summary | An inventory item is right-clicked | The box closes | SRC-MANUAL-1994 |
| Spell summary | A spell icon is right-clicked | The box closes | SRC-MANUAL-1994 |

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Which native controls draw the box, what item fields it shows, and how the spell icon is enabled (Q-UI-005).
