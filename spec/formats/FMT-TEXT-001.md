---
id: FMT-TEXT-001
title: Bitmap font resource
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RESOURCE.GFF"]
byte_order: little
size: null
text: false
definition: fmt_text_001.ksy
evidence: [FND-TEXT-001]
conflicting: []
split_with: []
related: []
---

## Layout

The layout of the `FONT` resource, of which the game has one, `RESOURCE.GFF#FONT/100`
[FND-TEXT-001]. Glyph `i` is the FMT-TEXT-002 record at `glyph_offsets[i]`; the records follow
the offset table in glyph order with no bytes between them.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x000` | 2 | `UINT16LE` | `glyph_count` | Number of glyphs, 256. | supported | FND-TEXT-001 |
| `0x002` | 2 | `UINT16LE` | `height` | Height of every glyph in pixels, 9. | supported | FND-TEXT-001 |
| `0x004` | 4 | `BYTE[4]` | `unk_04` | Purpose unknown. All 0. | supported | FND-TEXT-001 |
| `0x008` | 256 | `UINT8[256]` | `char_map` | The glyph for each character code: the identity, entry `c` holding `c`. | supported | FND-TEXT-001 |
| `0x108` | `glyph_count * 2` | `UINT16LE[glyph_count]` | `glyph_offsets` | Offset of each glyph's record from the start of the resource. | supported | FND-TEXT-001 |
| | `2 * glyph_count + height * total_width` | `FMT-TEXT-002[glyph_count]` | `glyphs` | The glyph records, where `total_width` is the sum of their widths. | supported | FND-TEXT-001 |
| | | | | Total size `264 + 4 * glyph_count + height * total_width` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

`RESOURCE.GFF#FONT/100` of BLD-GOG-EN-1.1, 8,299 bytes, every byte accounted for; the disc's
copy is identical [FND-TEXT-001].

## Open questions

- What `unk_04` holds, and whether the game reads `char_map` or indexes the glyphs by character
  code directly. The map is the identity, so both give the same glyph (Q-TEXT-001).
- Nothing locates the code that loads or draws the font: the `FONT` tag occurs only as data with
  no recorded reference (FND-TEXT-002, Q-TEXT-001).
