---
id: RULE-REGION-001
title: Drawing a region's terrain tiles
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-REGION-005, FND-REGION-006, FND-IMAGE-010, SRC-DSUN-MUSIC-79B6927]
conflicting: []
split_with: []
related: [RULE-IMAGE-001, FMT-REGION-002]
---

## Summary

The ground of a region is a grid of 16x16 tiles, 128 across and 98 down. The game draws, for each
cell of the map, the tile the cell names at the cell's place in the world.

## When it runs

Whenever the game draws the terrain of a region, before anything placed on it. What triggers a
redraw and which part of the view it covers are not known.

## Parameters

`map`, the region's FMT-REGION-002; `tiles`, a list in which `tiles[n]` is the first frame, an
FMT-IMAGE-002, of the region file's `TILE` resource `n`; `origin_x` and `origin_y`, the world
position of the view's top-left pixel; `width` and `height`, the view's size in pixels; and
`screen`, a list of `width * height` palette indices for the view, row by row.

## Inputs

None beyond the parameters.

## Procedure

```text
for row in 0..98:
    for column in 0..128:
        let frame = tiles[map.cells[row * 128 + column]]
        let pixels: UINT8[] = []
        let drawn: UINT8[] = []
        for k in 0..256:
            append(pixels, 0)
            append(drawn, 0)
        call RULE-IMAGE-001(frame, pixels, drawn)
        for ty in 0..16:
            for tx in 0..16:
                let sx: INT32 = column * 16 + tx - origin_x
                let sy: INT32 = row * 16 + ty - origin_y
                if drawn[ty * 16 + tx] != 0 and sx >= 0 and sx < width and sy >= 0 and sy < height:
                    screen[sy * width + sx] = pixels[ty * 16 + tx]
```

## Outputs

No return value. Sets each pixel of `screen` that a tile draws, and leaves the others as they
were.

## Edge cases

Every tile is 16x16, so the cells do not overlap and the order of the loops does not change the
result. The game walks only the cells that reach the view, starting at column `origin_x / 16`
and row `origin_y / 16`, and clips the first and last ones [FND-REGION-006]; the procedure gives
the same pixels by testing each one. It decodes each tile once into a cache of 260 slots
[FND-REGION-005, FND-REGION-006], which does not change what is drawn.

## What the sources say

SRC-DSUN-MUSIC-79B6927 draws the map the same way.

## Differences between builds

None known.

## Open questions

- Whether the pixels a tile leaves undrawn keep what was there before. 8 tiles that the shipped
  maps name leave 1 or 4 of their pixels undrawn (FND-REGION-002), and the copy routine at
  `2707:001A` has not been read (FND-REGION-006, Q-REGION-004).
