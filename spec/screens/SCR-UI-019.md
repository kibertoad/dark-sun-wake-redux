---
id: SCR-UI-019
title: Store panel
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-008, RULE-ITEM-004]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Store panel | Not known | Six item slots and their prices | Beside the inventory screen | During a store interaction | SRC-MANUAL-1994 |
| More | Not known | A choice for further sale items | Not known | When the store has more than six items | SRC-MANUAL-1994 |
| Sell | Not known | A choice for selling a selected item | Not known | During a store interaction | SRC-MANUAL-1994 |
| Sale-item highlight | Not known | A flashing highlight if affordable, a steady highlight otherwise | Around the item pointed at | While the pointer is on a sale item | SRC-MANUAL-1994 |

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Sale item | Not known | Not known | Buys the item and puts it on the pointer for placement in inventory. | SRC-MANUAL-1994 |
| More | Not known | More sale items exist | Displays further sale items. | SRC-MANUAL-1994 |
| Sell | Not known | An item is selected | Sells the selected item. | SRC-MANUAL-1994 |
| Return to Game | Not known | Not known | Leaves the store. | SRC-MANUAL-1994 |

## Keyboard input

None known.

## Other input

None known.

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Item affordable | The pointer is on an item the party can afford | The pointer leaves it | SRC-MANUAL-1994 |
| Item unaffordable | The pointer is on an item the party cannot afford | The pointer leaves it | SRC-MANUAL-1994 |

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Which resource graph draws the panel, its position, the number of item pages and the exact highlight treatment (Q-UI-005).
- How prices, affordability and sale values are calculated belongs to RULE-ITEM-004 and remains open (Q-ITEM-002).
