---
id: FND-IMAGE-010
title: The first gameplay frame of the opening region matches frames decoded from all three encodings
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: DOSBox screenshot compared with Python 3.14.7 and Pillow 12.3.0
environment: GOG DOSBox 0.74-2 with dosbox_darksun2.conf and dosbox_darksun2_single.conf (machine=svga_s3, memsize 16, cycles fixed 15000, sbtype sb16), game started by RAVAGER.BAT
---

## Observation

The game was started, the party the game offers was accepted, the opening cinematics were
skipped, and the first stable gameplay frame was saved as a 320x200 indexed PNG screenshot,
`dsun_019.png` with XXH3-128 `e8c0086991af1417b8e8a13120d33e82`, kept outside the repository.

A 320x200 picture of the opening region, `RGN032.GFF` (number 50), was built from its files for
the world rectangle whose top-left is `(1024,1368)`:

- each pixel of the `MAP ` grid's 16x16 `TILE` frames, `TILE` `n` for a map byte `n` at column
  `x` and row `y` drawn with its top-left at `(x * 16, y * 16)`;
- then, over them, the first frame of the image of each `ETAB` entry that reaches the rectangle,
  in the order the entries are stored, with its top-left at the entry's position less the
  offsets of its `OJFF` record and less the entry's vertical offset;
- every palette index turned into a colour through `RGN032.GFF#PAL/50`, each 6-bit component
  `c` widened to `(c << 2) | (c >> 4)`.

The frames were decoded as FMT-IMAGE-002, RULE-IMAGE-001 and RULE-IMAGE-002 describe. The tiles
are row-encoded frames, and the 22 objects that reach the rectangle are 13 `PLNR`, 8 `PLAN` and
one row-encoded frame. None of the 22 entries has bit `0x80` of its flags set.

The screenshot and the picture agree on 62,972 of the 64,000 pixels. The 1,028 that differ form
eight groups of touching pixels (counting diagonal neighbours):

| Pixels | Bounds, left-top to right-bottom |
|---|---|
| 448 | `(10,10)`-`(37,25)` |
| 367 | `(160,91)`-`(176,125)` |
| 110 | `(150,0)`-`(158,15)` |
| 69 | `(140,72)`-`(149,84)` |
| 26 | `(146,0)`-`(148,8)` |
| 6 | `(161,0)`-`(162,2)` |
| 1 | `(59,0)` |
| 1 | `(53,0)` |

The 367-pixel group is the party leader. The first frame of `OBJEX.GFF#BMP/599`, 17x35 pixels
with 367 it draws, matches the screenshot on all 367 when its top-left is placed at `(160,91)`.

## Interpretation

The decoded frames of all three encodings give the pixels the game draws, and the palette
widening gives its colours: every pixel of the scene outside the eight groups comes from a tile
or an object frame drawn this way. The groups are things the static files do not place: the
leader, an interface button in the top-left corner, the pointer, and small marks along the top
edge.

## Alternatives

The comparison covers only the pixels this one frame shows, and each object's first frame. A
decoding error in pixels that other objects cover, or in a frame this scene does not use, would
not show. The screenshot records colours, so two palette indices with the same colour cannot be
told apart. Which groups are the button, the pointer and the marks is a judgement made by eye.

## How to reproduce

Decode the tiles and object frames as described, compose the rectangle at `(1024,1368)` in the
order above, and compare each pixel's colour with the screenshot. Group the differing pixels by
touching neighbours. The object offsets and image numbers come from the `OJFF` records of
`OBJEX.GFF`, and the entries from `RGN032.GFF#ETAB/50`.
