---
id: FMT-IMAGE-004
title: Palette colour
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RESOURCE.GFF", "RGN*.GFF"]
byte_order: little
size: 3
text: false
definition: fmt_image_004.ksy
evidence: [FND-IMAGE-004, FND-IMAGE-010, SRC-DSUN-MUSIC-79B6927]
conflicting: []
split_with: []
related: []
---

## Layout

One colour of a palette (FMT-IMAGE-003), in the form the VGA DAC takes: three components of six
bits each. The screen shows a component `c` at the 8-bit level `(c << 2) | (c >> 4)`
[FND-IMAGE-010].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x0` | 1 | `UINT8` | `red` | Red, 0 to 63. | supported | FND-IMAGE-004, FND-IMAGE-010 |
| `0x1` | 1 | `UINT8` | `green` | Green, 0 to 63. | supported | FND-IMAGE-004, FND-IMAGE-010 |
| `0x2` | 1 | `UINT8` | `blue` | Blue, 0 to 63. | supported | FND-IMAGE-004, FND-IMAGE-010 |
| | | | | Total size 3 bytes | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Every colour of the 40 installed `PAL ` resources lies in 0 to 63 [FND-IMAGE-004]. The colours
of `RGN032.GFF#PAL/50`, in this order and widened this way, give the colours of the first
gameplay frame of the opening region wherever a tile or object frame supplies the pixel
[FND-IMAGE-010].

## Open questions

- The widening to eight bits is how the emulator's screenshot shows the colours. What a real
  VGA card and display show is part of how the game is presented (Q-IMAGE-001).
