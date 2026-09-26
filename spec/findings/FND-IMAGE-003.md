---
id: FND-IMAGE-003
title: PLAN and PLNR image frames pack dictionary indices into a bit stream
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x3FEAAE..0x4159BF
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x4EE9F..0x5066C
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x4FB8F9..0x4FB922
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x16324C..0x16417F
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

4,689 of the 12,500 frames in FND-IMAGE-001 have, after their width and height, the byte `0xFF`
and the four ASCII bytes `PLAN` (799 frames: 598 `BMP `, 201 `ICON`) or `PLNR` (3,890 frames:
3,284 `BMP `, 61 `CBMP`, 367 `ICON` and all 178 `PORT`). In each of them:

- The next byte is a bit count: 0 to 3 in every `PLAN` frame (25, 76, 78 and 620 frames), 4 to 7
  in every `PLNR` frame (1,974, 1,245, 496 and 175 frames).
- Then come `2` to the power of the bit count bytes, and then a stream of bits, read from the
  most significant bit of each byte down, in groups of the bit count.
- In a `PLAN` frame, `width * height` groups use up the stream to its last byte, with only the
  unused low bits of that byte left over.
- In a `PLNR` frame, the groups read as commands: a nonzero group stands for one pixel; a zero
  group followed by another zero group stands for one pixel of group value 0; a zero group
  followed by a nonzero group `n` repeats the previous pixel's group value `n + 2` times. Read
  that way, the commands give exactly `width * height` pixels with 0 bytes of the stream left in
  3,741 frames, and 1, 2 and 3 bytes left in 128, 20 and 1. Across all frames, 309,457 commands
  are a zero pair and 571,883 a repeat.
- The 25 `PLAN` frames with a bit count of 0 have one byte after the bit count and nothing else.
  That byte is 0 in 6 of them and nonzero in 19.

The first byte after the bit count is 0 in 415 of the `PLAN` frames and 2,516 of the `PLNR`
frames. Examples are frame 3 of `RESOURCE.GFF#BMP/11014` (`PLAN`, 320x200, 3 bits),
`RESOURCE.GFF#BMP/10000` (`PLNR`, 210x116, 4 bits), `RESOURCE.GFF#BMP/20047` (`PLAN`, 6x5,
0 bits) and `GPLDATA.GFF#PORT/18` (`PLNR`).

## Interpretation

The bytes after the bit count are a small dictionary of palette indices, and each group of bits
picks one of them, so a frame with few colours takes fewer bits per pixel. `PLNR` adds run-length
commands on top of the same dictionary and bit order. The bytes left over after a `PLNR` frame's
last pixel are most likely padding, or the tail of a last command that covers more pixels than
the frame has left.

## Alternatives

A frame with a bit count of 0 has nothing to read, and the files cannot tell whether the game
fills it with its one dictionary byte or leaves it empty. The same holds for how the game treats
a dictionary value of 0, which the files show only as common in the first dictionary slot. A
command stream that ends mid-command could be read differently by the game, which the files
cannot show either.

## How to reproduce

For each frame of FND-IMAGE-001 whose bytes after the dimensions are `0xFF` and `PLAN` or `PLNR`,
read the bit count and the dictionary, then read groups of bits most significant bit first and
count pixels as above until `width * height`, and compare the position reached with the frame's
end.
