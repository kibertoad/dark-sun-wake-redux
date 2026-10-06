---
id: RULE-IMAGE-001
title: Decoding an image frame, and the row encoding
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-IMAGE-005, FND-IMAGE-010, SRC-DSUN-MUSIC-79B6927]
conflicting: []
split_with: []
related: [RULE-IMAGE-002, FMT-IMAGE-002]
---

## Summary

A frame of an image is stored in one of three encodings. This rule picks the encoding and decodes
the row encoding, in which each row lists the runs of pixels it draws; the two planar encodings
are RULE-IMAGE-002.

## When it runs

Whenever the game draws or caches a frame of a `BMP `, `CBMP`, `ICON`, `PORT` or `TILE`
resource. Region tiles and object images both reach the routine that picks the encoding
[FND-IMAGE-005]. When the game decodes a frame, and whether it keeps the decoded pixels, is not
known.

## Parameters

- `frame: FMT-IMAGE-002`: the frame to decode.
- `pixels: UINT8[frame.width * frame.height]`: the palette index of each pixel, which the caller makes with every element 0. Pixel `(x, y)` of the frame is element `y * frame.width + x`.
- `drawn: UINT8[frame.width * frame.height]`: whether the frame draws each pixel, made and indexed the same way as `pixels`.

## Inputs

The frame only. The elements of `frame.body` are unsigned bytes.

## Procedure

```text
let body = frame.body
if count(body) >= 5 and body[0] == 0xFF and body[1] == 0x50 and body[2] == 0x4C:
    # 0x50 0x4C 0x41 0x4E spells PLAN, 0x50 0x4C 0x4E 0x52 spells PLNR
    if body[3] == 0x41 and body[4] == 0x4E:
        call RULE-IMAGE-002(frame, false, pixels, drawn)
        return
    if body[3] == 0x4E and body[4] == 0x52:
        call RULE-IMAGE-002(frame, true, pixels, drawn)
        return
let p: INT32 = 0
while true:
    let row: INT32 = body[p]
    p = p + 1
    if row == 0xFF:
        return
    let last = false
    while not last:
        let x: INT32 = body[p] + 256 * (body[p + 1] & 1)
        last = (body[p + 1] & 0x80) != 0
        let end: INT32 = p + 4 + body[p + 3]
        let out: INT32 = row * frame.width + x
        p = p + 4
        while p < end:
            let code = body[p]
            let n = code / 2 + 1
            for k in 0..n:
                if (code & 1) == 0:
                    pixels[out + k] = body[p + 1 + k]
                else:
                    pixels[out + k] = body[p + 1]
                drawn[out + k] = 1
            if (code & 1) == 0:
                p = p + 1 + n
            else:
                p = p + 2
            out = out + n
```

## Outputs

No return value. Sets `pixels` and `drawn` for every pixel a run covers, and leaves the rest 0.
A row-encoded frame marks every pixel of a run as drawn, palette index 0 included.

## Edge cases

A frame whose body is the single byte `0xFF` draws nothing. `TILE/0` is such a frame in 18 of the
20 region files, and no other shipped frame is. A row may be missing, and every shipped frame
gives its rows in rising order. The pixel count in the third byte of a run's header is not needed to
decode it, since the byte count ends the run, and in every shipped run it equals the number of
pixels the codes give [FND-IMAGE-002].

## What the sources say

SRC-DSUN-MUSIC-79B6927 reads the same three encodings, picks them by the same tag, and decodes
rows and runs as above, with bit 0 of a run's second byte as the ninth bit of its start column.

## Differences between builds

None known.

## Open questions

- The routine at `2D40:3BEC` picks the encoding [FND-IMAGE-005], but the code that decodes a
  row-encoded frame has not been read. The procedure rests on the first gameplay frame of the
  opening region [FND-IMAGE-010] and on the source. That every shipped frame decodes to its end
  under it (FND-IMAGE-002) fits it, and is circumstantial (Q-IMAGE-002).
- Whether the game checks the `0xFF` before the tag, and what it does with a run that reaches
  past the frame's width. No shipped run does (FND-IMAGE-002, Q-IMAGE-002).
- Bit 0 of a run's second byte is set in 52 runs, all in `RESOURCE.GFF#BMP/18001`, which no
  capture shows (FND-IMAGE-002, Q-IMAGE-002).
