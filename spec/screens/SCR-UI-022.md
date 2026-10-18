---
id: SCR-UI-022
title: Overhead map
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-IMAGE-012, FND-SAVE-013, FND-UI-044, FND-UI-046, FND-UI-047, FND-UI-048, FND-UI-050, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-006, SCR-EXPLORE-001]
---

## Drawn elements

The window is `RESOURCE.GFF#WIND/19502`, one full-window `BUTN/2099` [FND-UI-044].

| Element | Resource | Shows | Position | Shown when | Evidence |
|---|---|---|---|---|---|
| Frame picture | `RESOURCE.GFF#BMP/20120` | None | (27, 10) | When the picture loads | FND-UI-044 |
| Region map | `RESOURCE.GFF#BMP/n`, `n = 20100 + 25 * current_region + map_page`, where `map_page` is the byte at `57E0:4459`, set to 0 first when it is 25 or more (signed) | The region | Centred in the 186 x 139 area at (67, 30): x = 67 + (186 - width) / 2, y = 30 + (139 - height) / 2, both halvings of an unsigned difference | When the picture loads | FND-UI-044, FND-UI-046 |
| Object marks | None | One pixel per object slot, in the palette index equal to its mark value: 1 for the leader in slots 0 to 3, 16 for the leader in slot 4 and for every kind-2 slot above 4; when the byte at `57E0:143C` is not 0, also 100 for kind-1 slots and 11 for kind-5 slots. Other party members get no mark. The colours are under Mark colours below. | `map_x + scale(width, extent_x, obj_x - min_x)`, `map_y + scale(height, extent_y, obj_y - min_y)`, for an object whose position lies inside the region bounds (ends included); clipped only to the 320 x 200 screen, so an object on a maximum bound lands one pixel right of or below the region map | When the region map loaded | FND-IMAGE-012, FND-UI-046, FND-UI-047 |

The region bounds are the four words at `57E0:14D7` (`min_x`), `57E0:14D9` (`min_y`), `57E0:14DB`
(`max_x`) and `57E0:14DD` (`max_y`); `extent_x = max_x - min_x` and `extent_y = max_y - min_y`.
`scale(a, b, c)` is `c * a * 1000` in 32 bits, divided by `b` and then by 1000, both divisions
unsigned, and 0 when `b` is 0; its low word is used. Slots are the 320 entries of the object handle
table; a slot of kind 0, 3, 4 or above 5 while `57E0:143C` is not 0 keeps the mark value of the
slot before it. An object's position is the pair of words at `57E0:67C5` and `57E0:67C7` plus 37
times its slot [FND-UI-046].

The frame, the region map and the marks are drawn on the hidden page; a later mark replaces an
earlier one at the same pixel. After the marks the game makes that page the shown one, waiting for
the vertical retrace, when the words at `57E0:33B6` and `57E0:33B8` differ, and sets the mouse
pointer image to `RESOURCE.GFF#ICON/19101`, the Walk pointer. Both happen also when the region map
does not load [FND-UI-047].

`map_page` is set to 0 when play starts, by script function 34 of opcode `0x22` and by loading a
saved game, which also restores the region bounds (FMT-SAVE-002) [FND-SAVE-013].

### Mark colours

During play the palette is the current region's `RGN*.GFF#PAL/current_region`, and from the
start of play five ranges of it rotate by one entry on the first main-loop pass after each
eighth run of the timer callback: indexes 1 to 5, 6 to 10, 11 to 15, 16 to 23 and 24 to 31,
each colour moving one index up and the last wrapping to the first [FND-IMAGE-012]. Opening the
map runs no palette load of its own. The one DAC writer its first call, the view draw, can reach
is a picture sequence of overlay 201 that ends with the fade to `PAL/current_region` before the
map builds its window [FND-UI-048]. The marks therefore show:

- 100, kind-1 objects: (63, 51, 0) in every region.
- 1, the leader in slots 0 to 3: one colour per step, cycling through (0, 1, 63), (0, 1, 63),
  (15, 34, 63), (32, 56, 63), (15, 34, 63).
- 11, kind-5 objects: one colour per step, cycling through (12, 34, 12), (12, 34, 12),
  (13, 48, 13), (11, 62, 12), (12, 48, 13), with (13, 48, 13) in place of the last in region 66.
- 16, the leader in slot 4 and kind-2 objects above slot 4: one colour per step, cycling through
  the region's indexes 16, 23, 22, 21, 20, 19, 18, 17.

Colours are 6-bit red, green and blue. The cycles start wherever the rotation stands when the map
is drawn. Indexes 16 to 23 hold one of three lists [FND-IMAGE-012]:

- regions 50, 52, 54, 56, 57, 58, 59, 60, 65 and 67: (63, 63, 0), (63, 51, 2), (63, 40, 5),
  (63, 31, 8), (56, 21, 5), (58, 26, 5), (60, 33, 5), (63, 40, 5);
- regions 1, 51, 55, 61, 62, 63, 68, 69 and 255: (63, 63, 0), (63, 51, 2), (63, 40, 5),
  (63, 31, 8), (56, 21, 5), (49, 12, 2), (42, 5, 1), (36, 0, 0);
- region 66: (63, 63, 0), (60, 47, 1), (58, 33, 3), (56, 21, 5), (57, 25, 5), (59, 29, 5),
  (61, 35, 5), (63, 40, 5).

## Mouse input

| Region | Rectangle | Enabled when | Effect | Evidence |
|---|---|---|---|---|
| Map button | the whole window (`BUTN/2099`) | While the map is shown | Closes the map. When the pointer lies on the region map, its position is scaled back with `scale(extent, size, offset)` plus the bound's minimum, and the view is centred on that point (FND-COMBAT-049). | FND-UI-046 |

## Keyboard input

| Key | Enabled when | Effect | Evidence |
|---|---|---|---|
| `Esc`, `Enter`, `Space` (key words `0x011B`, `0x1C0D`, `0x3920`) | While the map is shown | Closes the map. | FND-UI-044, FND-UI-046 |

## Other input

None known.

## Sounds

None known.

## States

| State | Entered when | Left when | Evidence |
|---|---|---|---|
| Shown | Overhead map is chosen on SCR-UI-006, or `O` or `o` is pressed while the word at `57E0:0DAB` is not 0 and turn_combatant is below 4, unless a Shift key is held and the byte at `57E0:143C` is not 0 | The map button is clicked or a closing key is pressed | FND-UI-046 |

With a Shift key held and `57E0:143C` not 0, `O` instead shows `Subregions off` and sets the region
bounds to 0, 0, 2048 and 1568 [FND-UI-046].

## Timing

None known.

## Differences between builds

None known.

## Open questions

- Where the game places `WIND/19502` (Q-UI-003).
- What first puts the region palette in the DAC on a new game. START GAME's gate routine writes
  768 zero bytes to the DAC, and on a new game the region fade is reached from script opcode
  0x22's functions 1, 26 and 37 rather than from the start window or the gate routine. The
  first script the gate runs is `MAS/50`, whose instruction `0x05DC` is the first request for
  the fade on its route; whether a script it registers or an unresolved transfer writes the DAC
  before it is open (FND-UI-048, FND-UI-049, FND-UI-050, Q-UI-019).
- Whether anything changes the DAC while the map is shown, and whether the cycling ranges keep
  rotating then, rests on transfers no walk resolves (FND-IMAGE-012, FND-UI-048, Q-UI-012). The
  manual describes a blue party marker and red markers for monsters and other characters; the
  code gives kind-2 objects the cycling index 16, whose colours run from yellow to orange or red
  (SRC-MANUAL-1994, FND-IMAGE-012).
- What the word at `57E0:0DAB` and the byte at `57E0:143C` stand for, which decide whether `O`
  opens the map and which objects get marks (Q-COMBAT-003, Q-COMBAT-004).
