---
id: FND-EXE-629
title: The one routine of overlay 170 that no entry placed encodes a rectangle of a 320-byte-wide screen buffer as a row-encoded image frame, with run-length-coded runs of the pixels that differ from a transparent byte
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058023..0x00058205
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 1 row of overlay 170 (header segment `5660`, 4
trampolines, code from file offset `0x57C70`, 1,429 bytes) that no spec location covered, 482
bytes. The row below is one row of the inventory; "offset" is the overlay code offset and the
file offset is `0x57C70` plus it. The routine keeps its working words in the overlay's own code
bytes `CS:0010` to `CS:0024` and calls nothing. The frame layout it writes is the one
FND-IMAGE-002 reads from the shipped files.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+03B3` | `0x58023..0x58205` | `5660:0025` | Takes a source segment, `x0`, `y0`, `x1`, `y1`, a transparent byte `c` and a far destination. Loads `DS` with the segment (restoring it on return) and normalises the destination so its offset is below 16. Writes the words `x1 - x0 + 1` and `y1 - y0 + 1`. For each row `y` from `y0` to `y1` it scans the bytes at `320 * y + x0` to `320 * y + x1`. At the first byte other than `c` in the row it writes the byte `y - y0`. Each run of bytes other than `c`, at most 241 long, gets a word with its column less `x0` (so a column of 256 or more sets bit 0 of the high byte), a byte with its pixel count and a byte with its byte count, filled in after its codes. The codes are: for three or more equal bytes, up to 128 of them, the byte `2 * (n - 1) + 1` followed by the byte; otherwise up to 128 bytes copied, preceded by `2 * (n - 1)`, ending early where three equal bytes start. After a row's last run it sets bit `0x80` of that run's high byte; a row with no run writes nothing. At the end it writes `0xFF` and returns the destination offset reached less the offset it was given. It returns -1 when, before a code, the destination offset is `0xFF70` or more, and -2 when a run's codes take more than 255 bytes. No direct call was found. |

## Interpretation

`+03B3` is the encoder of the row-encoded frames of FND-IMAGE-002: it turns a rectangle of a
320-pixel-wide picture into a frame of rows of runs, leaving out the pixels equal to `c`, and its
code bytes are the PackBits style codes FND-IMAGE-002 decodes. The game ships its frames already
encoded, so this routine may serve a picture the game builds at run time, such as a screen copy.
Its result is the frame's length only when the destination's offset was already below 16, since
the end offset it subtracts from is the normalised one. The name stays `fn_170_03B3` until its
caller is found.

## Alternatives

The calls through trampolines may come from code no listing has reached; `+03B3` may be reached
only through its trampoline or a computed far call, or it may be unused. That the source segment
holds a 320-byte-wide picture rests on the stride of 320 in the code. Whether any caller passes a
destination offset of 16 or more, which would make the returned length wrong, is not known while
no caller is found.

## How to reproduce

Run `overlay_listing.py` for overlay 170 against the installed `DSUN.EXE` over
`0x58023..0x58205` and `direct_callers.py` for `170+03B3`, and compare the range with the rows of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`. Encode a synthetic rectangle with the rules above and
decode it with the reading of FND-IMAGE-002.
