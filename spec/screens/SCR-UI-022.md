---
id: SCR-UI-022
title: Overhead map
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
resolution: 320x200
evidence: [FND-SAVE-013, FND-UI-044, FND-UI-046, SRC-MANUAL-1994]
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
| Object marks | None | One mark per object slot, drawn with a mark value: 1 for the leader in slots 0 to 3, 16 for the leader in slot 4 and for every kind-2 slot above 4; when the byte at `57E0:143C` is not 0, also 100 for kind-1 slots and 11 for kind-5 slots. Other party members get no mark. | `map_x + scale(width, extent_x, obj_x - min_x)`, `map_y + scale(height, extent_y, obj_y - min_y)`, for an object whose position lies inside the region bounds (ends included) | When the region map loaded | FND-UI-046 |

The region bounds are the four words at `57E0:14D7` (`min_x`), `57E0:14D9` (`min_y`), `57E0:14DB`
(`max_x`) and `57E0:14DD` (`max_y`); `extent_x = max_x - min_x` and `extent_y = max_y - min_y`.
`scale(a, b, c)` is `c * a * 1000` in 32 bits, divided by `b` and then by 1000, both divisions
unsigned, and 0 when `b` is 0; its low word is used. Slots are the 320 entries of the object handle
table; a slot of kind 0, 3, 4 or above 5 while `57E0:143C` is not 0 keeps the mark value of the
slot before it. An object's position is the pair of words at `57E0:67C5` and `57E0:67C7` plus 37
times its slot [FND-UI-046].

`map_page` is set to 0 when play starts, by script function 34 of opcode `0x22` and by loading a
saved game, which also restores the region bounds (FMT-SAVE-002) [FND-SAVE-013].

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
- What a mark looks like: whether the drawing routine sets one pixel in the colour of the mark
  value, and what the call with 19101 and 1 after the marks adds. The manual describes a blue
  party marker and red markers for monsters and other characters (FND-UI-046, SRC-MANUAL-1994,
  Q-UI-009).
- What the word at `57E0:0DAB` and the byte at `57E0:143C` stand for, which decide whether `O`
  opens the map and which objects get marks (Q-COMBAT-003, Q-COMBAT-004).
