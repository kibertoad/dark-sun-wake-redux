---
id: FMT-TEXT-002
title: Bitmap font glyph
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RESOURCE.GFF"]
byte_order: little
size: null
text: false
definition: fmt_text_002.ksy
evidence: [FND-TEXT-001]
conflicting: []
split_with: []
related: []
---

## Layout

One glyph of a bitmap font (FMT-TEXT-001), whose `height` it takes from the font.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `width` | Width in pixels: 0, 2, 4, 5, 6 or 7 in the shipped font. | supported | FND-TEXT-001 |
| `0x02` | `width * height` | `UINT8[width * height]` | `pixels` | Palette indices: 0, 20 or 254 in the shipped font. | supported | FND-TEXT-001 |
| | | | | Total size `2 + width * height` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The 256 glyphs of `RESOURCE.GFF#FONT/100` in BLD-GOG-EN-1.1. 118 have width 0, among them the
glyphs for character codes 0 to 31 [FND-TEXT-001].

## Open questions

- Whether `pixels` runs row by row or column by column, which index is transparent, and whether
  the game draws the indices as stored or maps them to colours of its own (Q-TEXT-001).
