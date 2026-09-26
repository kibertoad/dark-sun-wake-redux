---
id: RULE-IMAGE-002
title: Decoding a planar image frame
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-IMAGE-005, FND-IMAGE-010, SRC-DSUN-MUSIC-79B6927]
conflicting: []
split_with: []
related: [RULE-IMAGE-001, FMT-IMAGE-002]
---

## Summary

A planar frame stores each pixel as a short code of a fixed number of bits that indexes a small
dictionary of palette indices. A `PLNR` frame adds a way to repeat the previous pixel.

## When it runs

When RULE-IMAGE-001 finds the tag `PLAN` or `PLNR` at the start of a frame's body.

## Parameters

`frame`, an FMT-IMAGE-002, then `repeats`, which is true for a `PLNR` frame and false for a `PLAN`
frame, then `pixels` and `drawn` as RULE-IMAGE-001 passes them.

## Inputs

The frame only. The elements of `frame.body` are unsigned bytes. `frame.body[5]` is the number of
bits per code, `frame.body[6]` onwards the dictionary of `1 << bits` palette indices, and the
codes follow the dictionary.

## Procedure

```text
define read_image_bits(body: BYTE[], at: INT32, bits) -> INT16:
    let value = 0
    for k in 0..bits:
        let b: INT32 = at + k
        value = (value << 1) | ((body[b / 8] >> (7 - b % 8)) & 1)
    return value

let body = frame.body
let bits = body[5]
let at: INT32 = (6 + (1 << bits)) * 8
let symbol = 0
let left = 0
for i in 0..count(pixels):
    if not repeats:
        symbol = read_image_bits(body, at, bits)
        at = at + bits
    else if left > 0:
        left = left - 1
    else:
        let first = read_image_bits(body, at, bits)
        at = at + bits
        if first != 0:
            symbol = first
        else:
            let second = read_image_bits(body, at, bits)
            at = at + bits
            if second == 0:
                symbol = 0
            else:
                left = second + 1
    pixels[i] = body[6 + symbol]
    if pixels[i] != 0:
        drawn[i] = 1
```

## Outputs

No return value. Sets every element of `pixels` to a dictionary entry, and marks as drawn every
pixel whose palette index is not 0. Pixels run left to right along each row, top row first.

## Edge cases

In a `PLNR` frame a code of 0 followed by a nonzero `n` repeats the current symbol for this pixel
and the `n + 1` after it, and a code of 0 followed by 0 gives symbol 0 for one pixel, so symbol 0
costs two codes. A `PLAN` frame has no repeat and gives symbol 0 for a code of 0. With a bit count
of 0 every pixel reads no bits and takes `body[6]`.

## What the sources say

SRC-DSUN-MUSIC-79B6927 reads both planar encodings the same way.

## Differences between builds

None known.

## Open questions

- The lower routines that `2D40:3BEC` calls for `PLAN` and `PLNR` have not been read
  [FND-IMAGE-005]. The procedure rests on the first gameplay frame of the opening region, which
  shows 8 `PLAN` and 13 `PLNR` objects [FND-IMAGE-010], and on the source. That every shipped
  planar frame decodes to within three bytes of its end (FND-IMAGE-003) fits it, and is
  circumstantial (Q-IMAGE-002).
- What the game draws for the 25 `PLAN` frames with a bit count of 0. Their dictionary byte is
  not 0 in 19 of them, so the procedure fills those frames with one colour; no capture shows one
  (FND-IMAGE-003, Q-IMAGE-002).
- What the up to three bytes after the last code of 149 `PLNR` frames are, and what a repeat
  before any nonzero code gives. The procedure starts with symbol 0 (FND-IMAGE-003, Q-IMAGE-002).
