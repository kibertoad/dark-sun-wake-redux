---
id: SCR-UI-008
title: Inventory
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-AI-003, FND-COMBAT-023, FND-UI-021, FND-UI-030, SRC-MANUAL-1994, FND-PARTY-217, FND-PARTY-222, FND-PARTY-223, FND-PARTY-224, FND-PARTY-225, FND-PARTY-226]
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
| Character box `n`, left button | (12, 5 + 48 * n, 34, 34), `BUTN/11300 + n`, `n` from 0 to 3 | Not known | Shows that character's inventory; with an item picked up, the manual says the same. | FND-UI-030, SRC-MANUAL-1994 |
| Character box `n`, right button, item picked up | as above | Not known | Gives the item to that character without leaving the current inventory. | FND-UI-030, SRC-MANUAL-1994 |
| Leader button beside box `n` | `BUTN/11309 + n`, 10 x 9 | Not known | Makes the character the leader for walking and talking, and during combat only the character whose turn it is (RULE-PARTY-008, RULE-COMBAT-008). | FND-COMBAT-023, FND-UI-030, SRC-MANUAL-1994 |
| Computer-control button beside box `n` | `BUTN/11313 + n`, 10 x 9 | Not known | Turns computer control of the character on or off unless it is locked (RULE-AI-001). | FND-AI-003, FND-UI-030, SRC-MANUAL-1994 |
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
- Complete loaded placement and image rendering for the twelve backpack
  origins, the other fourteen main origins, other draw paths and their frame
  treatment by the backend (Q-PARTY-085). FND-PARTY-226 maps the
  local dimension-dependent placement requests and cleanup branches.
  FND-PARTY-222 binds the twelve source origins to the drawing lookup;
  FND-PARTY-223 maps the other main origins. Neither validates native pixels.
- Complete resource/buffer admission for the bitmap-number resolver,
  record-selection preservation and cache lifecycle (FND-PARTY-224,
  FND-PARTY-225): resource/buffer validity is Q-PARTY-088, cache-specific
  binding/selection preservation is Q-PARTY-089, remaining direct cache
  accesses are Q-PARTY-087, field-six accessor requests remain only
  through script operands, Q-PARTY-092 (FND-PARTY-233, FND-PARTY-234,
  FND-PARTY-235) and record creation/copy values are Q-PARTY-091. Its local OJFF-number, image-word and
  zero-on-failure branches are recorded in FND-PARTY-225.
- The effects of the other buttons remain unestablished; code is known for the leader
  and computer-control buttons (FND-COMBAT-023, FND-AI-003). The inventory opener
  requests `WIND/13500` with overlay 189's handler, and its redraw projects item
  positions from the object graph (FND-PARTY-217); the other event branches and
  native control states remain Q-UI-002.
