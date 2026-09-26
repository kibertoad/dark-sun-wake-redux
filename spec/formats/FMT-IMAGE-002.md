---
id: FMT-IMAGE-002
title: Image frame
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["*.GFF"]
byte_order: little
size: null
text: false
definition: fmt_image_002.ksy
evidence: [FND-IMAGE-001, FND-IMAGE-002, FND-IMAGE-003, FND-IMAGE-010, SRC-DSUN-MUSIC-79B6927]
conflicting: []
split_with: []
related: [RULE-IMAGE-001, RULE-IMAGE-002]
---

## Layout

One frame of an image resource (FMT-IMAGE-001), `frame_size` bytes long, where `frame_size` is
the distance to the next frame's offset or to the end of the resource. `body` is in one of three
encodings, told apart by its first five bytes [FND-IMAGE-002, FND-IMAGE-003]:

- Planar, `PLAN` or `PLNR`: the byte `0xFF`, the four ASCII bytes `PLAN` or `PLNR`, a bit count
  `bits`, then a dictionary of `2` to the power of `bits` palette indices, then a stream of groups
  of `bits` bits, most significant bit first. `PLAN` frames use 0 to 3 bits, `PLNR` frames 4 to 7.
- Rows, in every other frame: a row number, then that row's runs, repeated, and the byte `0xFF`
  after the last row. A run is a start column, a flags byte (bit 0 the start column's ninth bit,
  bit 7 set on the row's last run, the other bits 0 in every shipped run), a pixel count, a byte
  count, and that many bytes of PackBits style codes.

RULE-IMAGE-001 decodes `body` into `width * height` palette indices and a flag per pixel that
says whether the frame draws it.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `width` | Width in pixels: 1 to 320 in the shipped frames. | supported | FND-IMAGE-001 |
| `0x02` | 2 | `UINT16LE` | `height` | Height in pixels: 1 to 200 in the shipped frames. | supported | FND-IMAGE-001 |
| `0x04` | `frame_size - 4` | `BYTE[frame_size - 4]` | `body` | The encoded pixels, a compressed block that RULE-IMAGE-001 decodes, and RULE-IMAGE-002 for a `PLAN` or `PLNR` frame. | supported | FND-IMAGE-002, FND-IMAGE-003 |
| | | | | Total size `frame_size` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 12,500 frames of the installed `.GFF` files of BLD-GOG-EN-1.1: 7,811 in rows, 799 `PLAN` and
3,890 `PLNR`, each decoding to exactly `width * height` pixels and using its bytes to the end, apart
from up to three bytes after the last command in 149 `PLNR` frames [FND-IMAGE-002,
FND-IMAGE-003]. Frames of all three encodings give the pixels the game draws in the first
gameplay frame of the opening region [FND-IMAGE-010].

## Open questions

- What the up to three bytes after the last command of 149 `PLNR` frames are (FND-IMAGE-003,
  Q-IMAGE-002).
- Whether the game checks the `0xFF` before `PLAN` or `PLNR`, or only the four letters
  (FND-IMAGE-005, Q-IMAGE-002).
- At what pixel aspect the game shows its frames on a real display (Q-IMAGE-001).
