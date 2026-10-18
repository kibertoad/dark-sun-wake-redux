---
id: FMT-SAVE-002
title: Saved game state in a GREQ resource
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["CHARSAVE.GFF"]
byte_order: little
size: 9
text: false
definition: fmt_save_002.ksy
evidence: [FND-COMBAT-049, FND-SAVE-001, FND-SAVE-002, FND-SAVE-004, FND-SAVE-005, FND-SAVE-013, FND-SAVE-014, FND-UI-046]
conflicting: []
split_with: []
related: [RULE-SAVE-002]
---

## Layout

A `GREQ` resource, one for each saved game, numbered as the saved game is, from 1 to 10. Saving
writes it from five globals of the game and loading copies it back to them [FND-SAVE-004,
FND-SAVE-005].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `subregion_left` | The left edge of the current subregion in map pixels, which the view and the overhead map keep to. Saved from and loaded into the word at `DS:14D7` in BLD-GOG-EN-1.1. 0, 752, 1,024, 1,376 or 1,632 in the shipped resources. | supported | FND-COMBAT-049, FND-SAVE-001, FND-SAVE-004, FND-SAVE-005, FND-SAVE-013, FND-UI-046 |
| `0x02` | 2 | `UINT16LE` | `subregion_top` | The top edge, the word at `DS:14D9`. 0 or 256 in the shipped resources. | supported | FND-COMBAT-049, FND-SAVE-001, FND-SAVE-004, FND-SAVE-005, FND-SAVE-013, FND-UI-046 |
| `0x04` | 2 | `UINT16LE` | `subregion_right` | The right edge, the word at `DS:14DB`. 1,008 or 2,048 in the shipped resources. | supported | FND-COMBAT-049, FND-SAVE-001, FND-SAVE-004, FND-SAVE-005, FND-SAVE-013, FND-UI-046 |
| `0x06` | 2 | `UINT16LE` | `subregion_bottom` | The bottom edge, the word at `DS:14DD`. 784, 1,056 or 1,568 in the shipped resources. | supported | FND-COMBAT-049, FND-SAVE-001, FND-SAVE-004, FND-SAVE-005, FND-SAVE-013, FND-UI-046 |
| `0x08` | 1 | `UINT8` | `map_page` | The overhead map's page, which picks its picture within the region (SCR-UI-022). The byte at `DS:4459`. 0, 1, 2 or 4 in the shipped resources. No 16-bit window of the resource holds a `CHAR` resource number. | supported | FND-SAVE-001, FND-SAVE-002, FND-SAVE-004, FND-SAVE-005, FND-SAVE-013, FND-UI-046 |
| `0x09` | | | | Total size 9 | | |

A script sets the four edges through function 33 of opcode `0x22` from two packed numbers
`x * 1000 + y`, the corner and the size in 16-pixel cells, each edge at most 2,048 across and
1,568 down, the size of a region; a region change and the debug `Subregions off` key set them to
the whole region. Function 34 sets `map_page`, and the start of play sets it to 0
[FND-SAVE-013]. The shipped scripts set 137 different subregions, listed with their calls in
FND-SAVE-014's value file, and pass with them a `map_page` that numbers the subregion within
its region, 0 to 24 where it comes from a literal or a table index. One script passes 255,
which the overhead map reads as -1, and three pass a global that other scripts also lower as a
counter [FND-SAVE-014].

The routines keep a tenth byte beside the nine, the byte at `DS:4458`: the picture number the
defeat window draws, which script functions 19 and 35 set. Saving leaves it out, and loading
copies a stack byte the 9-byte read never wrote into it [FND-SAVE-004, FND-SAVE-005,
FND-SAVE-013].

## Enumerations and flags

None.

## Differences between builds

None known. The disc's copy of `CHARSAVE.GFF` holds no `GREQ` resource [FND-SAVE-001].

## Coverage

The ten `GREQ` resources of the installed `CHARSAVE.GFF`, every one 9 bytes [FND-SAVE-001].

## Open questions

None known.
