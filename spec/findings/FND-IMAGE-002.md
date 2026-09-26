---
id: FND-IMAGE-002
title: Row-encoded image frames are lists of rows of run-length-coded runs ended by 0xFF
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x34E3E9..0x3501F0
  - build: BLD-GOG-EN-1.1
    file: RGN032.GFF
    offset: 0x2F..0xAE
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x641BC..0x6572D
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

Of the 12,500 frames in FND-IMAGE-001, 7,811 do not start with the byte `0xFF` followed by the
ASCII `PLAN` or `PLNR` after their width and height: 3,078 `BMP `, 1,643 `CBMP`, 47 `ICON` and all
3,043 `TILE` frames. In every one of them the bytes after the height read as follows, with no
byte left over:

- A row number, 0 to `height - 1`, or `0xFF`, which ends the frame. Row numbers rise within a
  frame and never repeat. 7,576 frames list all `height` rows and then `0xFF`; 235 list fewer.
  The `0xFF` is the frame's last byte in all 7,811. In 18 frames, `TILE/0` of 18 of the 20
  region files, it is the only byte after the height.
- After each row number, one or more runs. A run is four bytes, a start column, a flags byte, a
  pixel count and a byte count, followed by that many bytes. Of 332,439 runs, the flags byte is
  `0x80` in 222,439, `0x00` in 109,948, `0x01` in 45 and `0x81` in 7. The run after one whose flags
  have bit 7 set is the next row number.
- The run's bytes expand to exactly its pixel count when read as codes: a code with bit 0 clear
  is followed by `code / 2 + 1` bytes copied as they are, and a code with bit 0 set is followed
  by one byte repeated `code / 2 + 1` times. 314,517 runs use the first form at least once and
  165,347 the second.
- With 256 added to the start column when flags bit 0 is set, every run fits inside the frame's
  width.

`OBJEX.GFF#BMP/599` has 13 frames in this encoding and `RGN032.GFF#TILE/1` one. All 52 runs with
flags bit 0 set are in one frame, the single 320x200 frame of `RESOURCE.GFF#BMP/18001`.

## Interpretation

These frames store only the runs of drawn pixels in each row, each run compressed with a PackBits
style code; the columns no run covers are not stored. Bit 7 of a run's flags marks the last run
of its row, and bit 0 carries the ninth bit of the start column for frames wider than 256 pixels.

## Alternatives

Bit 0 of the flags could mean something other than the start column's ninth bit; the files show
only that the reading keeps every run inside its frame, and it occurs in one frame only. A frame
that lists all `height` rows still ends with `0xFF`, so the files cannot tell whether the game
stops after `height` rows or always reads on to the `0xFF`. Whether a pixel the runs do not
cover is left undrawn is a question of what the game does with the frame (RULE-IMAGE-001).

## How to reproduce

For each frame of FND-IMAGE-001 that does not start with `0xFF` and `PLAN` or `PLNR` after its
dimensions, read row numbers and runs as above until the row number `0xFF`, expand each run's
codes and compare the result with the run's pixel count, then check that the `0xFF` is the
frame's last byte.
