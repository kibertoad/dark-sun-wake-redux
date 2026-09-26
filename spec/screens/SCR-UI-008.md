---
id: SCR-UI-008
title: Inventory
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-UI-021, FND-UI-030, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-ITEM-001, RULE-ITEM-002, RULE-ITEM-003, RULE-ITEM-004, RULE-UI-001, SCR-UI-002, SCR-UI-006, SCR-UI-009, SCR-UI-010]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Screen picture | `RESOURCE.GFF#BMP/13001`, or an image with the same outlines | None | (0, 0, 320, 200) | While the screen is shown | FND-UI-021 |
| Controls | the `ICON` of each `BUTN` child of `RESOURCE.GFF#WIND/13500` | None | each child's position, with the window at (0, 0) | While the screen is shown | FND-UI-030 |

The manual's picture (SRC-MANUAL-1994, page 11) shows the four character boxes down the left
edge, the active character's portrait in the centre surrounded by fourteen body slots, twelve
backpack slots at the upper right, the character's data along the right side, a description box
under the portrait, the party's money along the bottom, and the navigation row at the lower right.

## Mouse input

Rectangles assume the window at (0, 0), where the outlines of FND-UI-021 put the controls.

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Character box `n`, left button | (12, 5 + 48 * n, 34, 34), `BUTN/11300 + n`, `n` from 0 to 3 | Not known | Shows that character's inventory; with an item picked up, the manual says the same. | FND-UI-030, SRC-MANUAL-1994 |
| Character box `n`, right button, item picked up | as above | Not known | Gives the item to that character without leaving the current inventory. | FND-UI-030, SRC-MANUAL-1994 |
| Small buttons beside each box | `BUTN/11309` to `/11316`, 10 x 9 | Not known | One turns computer control of the character in combat on or off, the other makes the character the leader. | FND-UI-030, SRC-MANUAL-1994 |
| Drop | (186, 130, 42, 12) | An item is picked up | Drops the item to the ground. | FND-UI-030, SRC-MANUAL-1994 |
| Split | (186, 142, 42, 12) | A grouped item is picked up and the backpack has an empty slot | Splits the group in half (RULE-ITEM-002). | FND-UI-030, SRC-MANUAL-1994 |
| Two further buttons with mask 160 | (235, 159, 42, 12) and (277, 159, 42, 12) | Not known | Not known. | FND-UI-030 |
| View character | (163, 181, 16, 16), `BUTN/10300` | Not known | Opens SCR-UI-002. | FND-UI-030, SRC-MANUAL-1994 |
| View inventory | (187, 181, 16, 16), `BUTN/11304` | Not known | Not known; this screen is already shown. | FND-UI-030 |
| Cast spells or use psionics | (211, 181, 16, 16), `BUTN/11305` | Not known | Opens SCR-UI-009. | FND-UI-030, SRC-MANUAL-1994 |
| Current spell effects | (235, 181, 16, 16), `BUTN/11306` | Not known | Opens SCR-UI-010. | FND-UI-030, SRC-MANUAL-1994 |
| Game Menu | (258, 181, 28, 16), `BUTN/11308` | After the party has begun the adventure | Opens SCR-UI-006. | FND-UI-030, SRC-MANUAL-1994 |
| Return to game | (288, 181, 28, 16), `BUTN/10308` | Not known | Closes the screen. | FND-UI-030, SRC-MANUAL-1994 |
| Name box | (59, 5, 95, 8), `EBOX/4003`, mask 10 | Not known | Not known. | FND-UI-030 |

The item slots are not buttons of `WIND/13500`; the manual's effects for them (picking up and
placing items, the flashing outlines of valid slots, right-clicking an item for its summary,
opening pouches and chests) belong to the item rules RULE-ITEM-001 and RULE-ITEM-003, and a store
shown beside the screen to RULE-ITEM-004 (Q-ITEM-002).

## Keyboard input

None known.

## Other input

None known.

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| No item picked up | The screen opens | The player picks up an item | SRC-MANUAL-1994 |
| Item picked up, the pointer shows the item | The player left-clicks an item | The player places or drops it | SRC-MANUAL-1994 |

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Which image and colours the game draws: the textured areas of the captures differ from
  `BMP/13001` under `PAL/1000` (FND-UI-021, Q-UI-004).
- Which of the two 42 x 12 buttons at the right is which, and what the buttons at (235, 159) and
  (277, 159) are; the manual's store screen names a sell button and a button for more items
  (FND-UI-030, Q-UI-002).
- Where the slots, the data panel, the description box and the money bar are drawn, and what
  the 90 x 125 frame `APFM/13200` at (75, 36) is (FND-UI-030, Q-UI-001, Q-ITEM-002).
- The effects come from the manual only; no code that names these buttons is known (FND-UI-012,
  Q-UI-002).
