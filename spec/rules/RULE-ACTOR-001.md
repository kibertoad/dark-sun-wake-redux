---
id: RULE-ACTOR-001
title: Drawing a region's placed objects
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-ACTOR-003, FND-IMAGE-010, SRC-DSUN-MUSIC-79B6927]
conflicting: []
split_with: []
related: [RULE-REGION-001, RULE-IMAGE-001]
---

## Summary

After the terrain, the game draws each object a region places, in the order the region's entity
table lists them. An object's image goes at the entity's position less the offsets of the object's
definition, with no mirroring.

## When it runs

After RULE-REGION-001 has drawn the terrain of the same view, whenever the game draws a region.
What triggers a redraw, and whether the game draws objects in another order once they move, are
not known.

## Parameters

`entities`, the records, each an FMT-REGION-006, of the region's FMT-REGION-005 in stored order;
`definitions`, a list in which `definitions[n]` is the FMT-ACTOR-001 of the `OJFF` resource `n` of
`OBJEX.GFF`; `images`, a list in which `images[n]` is the first frame, an FMT-IMAGE-002, of the
`BMP ` resource `n` of `OBJEX.GFF`; `origin_x` and `origin_y`, the world position of the view's
top-left pixel; `width` and `height`, the view's size in pixels; and `screen`, a list of
`width * height` palette indices for the view, row by row, as RULE-REGION-001 left it.

## Inputs

None beyond the parameters.

## Procedure

```text
for i in 0..count(entities):
    let entity = entities[i]
    let definition = definitions[entity.object]
    let frame = images[definition.image]
    let pixels: UINT8[] = []
    let drawn: UINT8[] = []
    for k in 0..frame.width * frame.height:
        append(pixels, 0)
        append(drawn, 0)
    call RULE-IMAGE-001(frame, pixels, drawn)
    let left: INT32 = entity.x - definition.x_offset - origin_x
    let top: INT32 = entity.y - definition.y_offset - definition.vertical_offset - origin_y
    for fy in 0..frame.height:
        for fx in 0..frame.width:
            let sx: INT32 = left + fx
            let sy: INT32 = top + fy
            if drawn[fy * frame.width + fx] != 0 and sx >= 0 and sx < width and sy >= 0 and sy < height:
                screen[sy * width + sx] = pixels[fy * frame.width + fx]
```

## Outputs

No return value. Sets each pixel of `screen` that an object's image draws, and leaves the others
as they were.

## Edge cases

Objects that overlap are drawn in stored order, so a later entity covers an earlier one. An entity
whose image lies wholly outside the view draws nothing. The position subtracts the definition's
`vertical_offset`, which is what `31E0:0E1B` subtracts [FND-ACTOR-003]; the entity's own
`vertical_offset` holds the same value in every shipped record.

## What the sources say

SRC-DSUN-MUSIC-79B6927 draws the objects in the same order at the same place, using the entity's
`vertical_offset`, and draws the image mirrored left to right when bit 7 of the entity's flags is
set.

## Differences between builds

None known.

## Open questions

- Whether the game mirrors an object whose entity has `unk_flags_bit_7` set. None of the 22
  objects in the one compared frame has it set (FND-IMAGE-010, Q-ACTOR-005).
- Whether the game always shows the first frame of an object's image. The compared frame shows
  first frames only (FND-IMAGE-010), and the code that picks a frame has not been found
  (Q-ACTOR-005).
- Whether the drawing order is the stored order or the order of the slot records `31E0:0EFF`
  fills (FND-ACTOR-003, FND-ACTOR-004). The compared frame matches the stored order
  (FND-IMAGE-010), but it was not checked for overlaps that would tell the two apart. (Q-ACTOR-005)

- Whether the pixels an image leaves undrawn keep what was there before, as for the terrain
  (RULE-REGION-001). (Q-ACTOR-005)
