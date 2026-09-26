---
id: FMT-IMAGE-001
title: Image resource with a list of frames
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["*.GFF"]
byte_order: little
size: null
text: false
definition: fmt_image_001.ksy
evidence: [FND-IMAGE-001, SRC-DSUN-MUSIC-79B6927]
conflicting: []
split_with: []
related: []
---

## Layout

The layout of every resource under the tags `BMP `, `CBMP`, `ICON` and `PORT`, and of the region
tiles under `TILE` [FND-IMAGE-001]. A resource holds one or more frames, each a bitmap with its
own width and height. Frame `i` runs from `frame_offsets[i]` to `frame_offsets[i + 1]`, or to
`size` for the last frame, and that length is the `frame_size` of FMT-IMAGE-002. The files that
hold these tags are `RESOURCE.GFF` (`BMP `, `CBMP`, `ICON`), `OBJEX.GFF` (`BMP `, `CBMP`),
`GPLDATA.GFF` (`PORT`) and the region files `RGN*.GFF` (`TILE`).

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `size` | Size of the resource in bytes, equal to the size the GFF directory gives. | supported | FND-IMAGE-001 |
| `0x04` | 2 | `UINT16LE` | `frame_count` | Number of frames, 1 or more. | supported | FND-IMAGE-001 |
| `0x06` | `frame_count * 4` | `UINT32LE[frame_count]` | `frame_offsets` | Offset of each frame from the start of the resource. The first is `6 + frame_count * 4`, and each is larger than the one before. | supported | FND-IMAGE-001 |
| | `size - 6 - frame_count * 4` | `BYTE[size - 6 - frame_count * 4]` | `frames` | The frames, FMT-IMAGE-002, in order and with no bytes between them. | supported | FND-IMAGE-001 |
| | | | | Total size `size` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 7,731 resources under the five tags in the 26 installed `.GFF` files of BLD-GOG-EN-1.1, with
their 12,500 frames: every field agrees with the layout, and every frame decodes to its end
[FND-IMAGE-001, FND-IMAGE-002, FND-IMAGE-003]. The copies of `RESOURCE.GFF`, `OBJEX.GFF` and
`GPLDATA.GFF` on the disc were not checked. SRC-DSUN-MUSIC-79B6927 reads the same layout.

## Open questions

- What sets `CBMP` apart from `BMP `, and `ICON` from both. The game's cache picks `BMP ` or `CBMP`
  from a flag and treats them alike otherwise (FND-IMAGE-006, Q-IMAGE-003).
- Which palette the game draws each image with. No image names a palette, and `GPLDATA.GFF` and
  `OBJEX.GFF` hold none (FND-IMAGE-004). The title picture `RESOURCE.GFF#BMP/11011` looks right
  with the palette of the same number, `PAL/11011` (FND-IMAGE-007), and its number has no direct
  operand in the resident code (FND-IMAGE-008). The `PORT` tag occurs only in overlay code
  (FND-IMAGE-009) (Q-IMAGE-003).
- Whether the game reads `size`, or takes a resource's size from the GFF directory (Q-IMAGE-003).
