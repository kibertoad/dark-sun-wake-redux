---
id: SCR-UI-008
title: Inventory
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-AI-003, FND-COMBAT-023, FND-UI-021, FND-UI-030, SRC-MANUAL-1994, FND-PARTY-217, FND-PARTY-222, FND-PARTY-223, FND-PARTY-224, FND-PARTY-225, FND-PARTY-226, FND-UI-061, FND-COMBAT-025]
conflicting: []
split_with: []
related: [RULE-AI-001, RULE-COMBAT-008, RULE-PARTY-008, RULE-ITEM-001, RULE-ITEM-002, RULE-ITEM-003, RULE-ITEM-004, RULE-UI-001, SCR-UI-002, SCR-UI-006, SCR-UI-009, SCR-UI-010, SCR-UI-018, SCR-UI-019]
---

## Drawn elements

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Screen picture | `RESOURCE.GFF#BMP/13001`, or an image with the same outlines | None | (0, 0, 320, 200) | While the screen is shown | FND-UI-021 |
| Controls | the `ICON` of each `BUTN` child of `RESOURCE.GFF#WIND/13500` | None | each child's position, with the window at (0, 0) | While the screen is shown | FND-UI-030 |
| Backpack placement origins | `APFM/11230` through `/11241` placed by `WIND/13500` | Inventory position codes 14 through 25 | Two columns of six: x=186 and 204; y=18,36,54,72,90,108; numbering runs down the first column, then down the second | Source layout; current loaded rectangles and native rendering remain unvalidated | FND-PARTY-222 |
| Other main placement origins | `APFM/11216` through `/11229` placed by `WIND/13500` | Inventory position codes 0 through 13 | Source mapping below | Source layout; individual equipment meanings and native rendering remain open | FND-PARTY-223 |
| Item pictures | A BMP resource number from the selected attached record's word `0x0C`, resolved when zero | The selected graph record's picture | Slot lookup followed by the dimension-dependent request below | A returned nonnull acquisition pointer continues; acquisition status is not tested; temporary-view failure exits before drawing | FND-PARTY-224, FND-PARTY-226 |

The manual's picture (SRC-MANUAL-1994, page 11) shows the four character boxes down the left
edge, the active character's portrait in the centre surrounded by fourteen body slots, twelve
backpack slots at the upper right, the character's data along the right side, a description box
under the portrait, the party's money along the bottom, and the navigation row at the lower right.

The other fourteen main slot origins are window-relative. Their APFM
identifier is 11216 plus the stored position code; their logical index is
four plus that code. The code-to-origin mapping is:

- 0: (57, 62).
- 1: (64, 18).
- 2: (57, 42).
- 3: (57, 83).
- 4: (57, 101).
- 5: (57, 121).
- 6: (165, 121).
- 7: (111, 18).
- 8: (159, 18).
- 9: (165, 62).
- 10: (165, 83).
- 11: (165, 101).
- 12: (165, 42).
- 13: (165, 141).

No individual equipment names are assigned here.

FND-PARTY-223 records these source placements. Loaded rectangles, the
current window origin, generic frame treatment and native pixels remain
unvalidated. The bitmap-number cache retains only a number: the drawing
path requests the image separately, including a zero result from its
resolver (FND-PARTY-224).

For the current loaded slot origin (x,y), frame-zero dimensions both
signed-at-most 16 request drawing on handle 1 at
(x+1+trunc0(int16(16-width)/2), y+1+trunc0(int16(16-height)/2)).
Arithmetic wraps at word width; division truncates toward zero.
If either signed dimension exceeds 16, the caller requests a temporary
view from (x+1,y+1) to (x+16,y+16), then requests drawing at (x+1,y+1)
unless that view request returned FFFF. These are caller requests;
actual clipping and pixels remain unvalidated (FND-PARTY-226).

## Mouse input

Rectangles assume the window at (0, 0), where the outlines of FND-UI-021 put the controls.

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Character box `n`, left button, spell or power pending | (12, 5 + 48 * n, 34, 34), `BUTN/11300 + n`, `n` from 0 to 3 | A spell or power is waiting for its target | Casts it on that character. | FND-UI-061 |
| Character box `n`, left button | as above | Otherwise | Shows that character's inventory; nothing on the current character's own box. | FND-UI-030, FND-UI-061, SRC-MANUAL-1994 |
| Character box `n`, right button, item picked up | as above | `n` is not the current character | Gives the item to that character without leaving the current inventory, or reports a full backpack. | FND-UI-030, FND-UI-061, SRC-MANUAL-1994 |
| Character box `n`, right button | as above | No item is picked up, or `n` is the current character | Shows that character's inventory. | FND-UI-061 |
| Leader button beside box `n` | `BUTN/11309 + n`, 10 x 9 | Not known | Makes the character the leader for walking and talking, and during combat only the character whose turn it is (RULE-PARTY-008, RULE-COMBAT-008). | FND-COMBAT-023, FND-UI-030, SRC-MANUAL-1994 |
| Computer-control button beside box `n` | `BUTN/11313 + n`, 10 x 9 | Not known | Turns computer control of the character on or off unless it is locked (RULE-AI-001). | FND-AI-003, FND-UI-030, SRC-MANUAL-1994 |
| Drop | (186, 130, 42, 12), `BUTN/13300` | An item is picked up | Drops the item at a party member's place. | FND-UI-030, FND-UI-061, SRC-MANUAL-1994 |
| Split | (186, 142, 42, 12), `BUTN/13302` | A grouped item with a quantity above 1 is picked up | Moves half the quantity, rounded down, to the first empty backpack slot (RULE-ITEM-002). | FND-UI-030, FND-UI-061, SRC-MANUAL-1994 |
| More | (235, 159, 42, 12), `BUTN/13303` | A store is shown | Shows the next page of the store's items. | FND-UI-030, FND-UI-061 |
| Sell | (277, 159, 42, 12), `BUTN/13304` | A store is shown and an item is picked up | Shows the store's offer for the item. | FND-UI-030, FND-UI-061 |
| View character | (163, 181, 16, 16), `BUTN/10300` | Not known | Closes the screen and opens SCR-UI-002. | FND-UI-030, FND-UI-061, SRC-MANUAL-1994 |
| View inventory | (187, 181, 16, 16), `BUTN/11304` | Always | Nothing. | FND-UI-030, FND-UI-061 |
| Cast spells or use psionics | (211, 181, 16, 16), `BUTN/11305` | Not known | Closes the screen and opens SCR-UI-009. | FND-UI-030, FND-UI-061, SRC-MANUAL-1994 |
| Current spell effects | (235, 181, 16, 16), `BUTN/11306` | Not known | Closes the screen and opens SCR-UI-010. | FND-UI-030, FND-UI-061, SRC-MANUAL-1994 |
| Game Menu | (258, 181, 28, 16), `BUTN/11308` | After the party has begun the adventure | Closes the screen and opens SCR-UI-006. | FND-UI-030, FND-UI-061, SRC-MANUAL-1994 |
| Return to game | (288, 181, 28, 16), `BUTN/10308` | Not known | Closes the screen; with an item picked up, sets pointer mode 3. | FND-UI-030, FND-UI-061, SRC-MANUAL-1994 |
| Name box | (59, 5, 95, 8), `EBOX/4003`, mask 10 | Not known | Not known. | FND-UI-030 |

The item slots are not buttons of `WIND/13500`; the manual's effects for them (picking up and
placing items, the flashing outlines of valid slots, right-clicking an item for its summary,
opening pouches and chests) belong to the item rules RULE-ITEM-001 and RULE-ITEM-003, and a store
shown beside the screen to RULE-ITEM-004 (Q-ITEM-002). A frame event on `APFM/13200` or on an open
pouch or chest view's slots closes that view and redraws the screen (FND-UI-061).

## Keyboard input

| Key | Enabled when | Effect | Evidence |
|---|---|---|---|
| `I`, `i` | While the screen is shown | Nothing. | FND-UI-061 |
| Any key | While an item's view is open | Closes the view and does nothing else. | FND-UI-061 |
| 1 to 4 | As below | Act as a left click on character box 1 to 4. | FND-UI-061, FND-COMBAT-025 |
| Esc, Alt-X, Ctrl-C, F3 | As below | Go to the game view's key routine; Esc closes the screen as Return to game does. | FND-UI-061, FND-COMBAT-025 |
| Other keys | No store is shown | Go to the game view's key routine, overlay 190 `+139B`, with the screen's close routine. | FND-UI-061 |
| Other keys but `S` and `s` | A store is shown | Nothing. | FND-UI-061 |

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
- What Split does with a full backpack, where the handler's failure test never holds and it
  asks to draw logical position 259 (FND-UI-061, Q-UI-024).
- Which slot clicks of an open pouch or chest view close it, given that the handler pairs the
  two slot groups with the opposite flags to the ones overlay 175 registers them with
  (FND-UI-061, FND-EXE-598, Q-UI-025).
- Where the slots, the data panel, the description box and the money bar are drawn, and what
  the 90 x 125 frame `APFM/13200` at (75, 36) is (FND-UI-030, Q-UI-001, Q-ITEM-002).
- Whether anything moves the loaded rectangles of the twelve backpack origins and the other
  fourteen main origins, whose source origins FND-PARTY-222 and FND-PARTY-223 give, and where
  items with other positions appear (Q-PARTY-085).
- What the item draw paths and their backend requests put on the screen; FND-PARTY-226 maps
  the local dimension-dependent placement requests and cleanup branches (Q-PARTY-100).
- Complete resource/buffer admission for the bitmap-number resolver,
  record-selection preservation and cache lifecycle (FND-PARTY-224,
  FND-PARTY-225): resource/buffer validity is Q-PARTY-088, cache-specific
  binding/selection preservation is Q-PARTY-089, remaining direct cache
  accesses are Q-PARTY-087, no generic accessor request selects field
  six (FND-PARTY-234, FND-PARTY-235, FND-PARTY-238, FND-PARTY-239) and
  record creation/copy values are Q-PARTY-091. Its local OJFF-number, image-word and
  zero-on-failure branches are recorded in FND-PARTY-225.
- Which frame shows each control state (FND-UI-030, Q-UI-013).
