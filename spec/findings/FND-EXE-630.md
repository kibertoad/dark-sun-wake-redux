---
id: FND-EXE-630
title: The one routine of overlay 201 that no entry placed, script request 53's per-slot callback, centres the view on a slot, removes its object, plays the frames of BMP 11016 at its place with sound 132, and redraws the screen and fades the palette back in
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008A191..0x0008A368
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 1 row of overlay 201 (header segment `5778`, 9
trampolines, code from file offset `0x897C0`, 2,984 bytes) that no spec location covered, 471
bytes. The row below is one row of the inventory; "offset" is the overlay code offset, the file
offset is `0x897C0` plus it, and `DS` is `57E0`. A slot's 37-byte record is at
`DS:67BB + 37 * slot`, with its position in the words `+3` and `+5`; `DS:1408` and `DS:140A` are
the view's corner. Request 53 of overlay 204's script request dispatcher `+0169` (the last word of
its table at `+06C7`) calls overlay 204 `+1A68` with its `SI`, the state -1, the far pointer
`0648:0048` and its two dwords; `0648` is descriptor 201's fixup word and `5778:0048` the
trampoline that enters `+09D1` (FND-EXE-586). `+1A68` calls it with each slot the target names
(FND-PARTY-156).

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+09D1` | `0x8A191..0x8A368` | `5778:0048` | Takes a slot `s`. Calls `2C5F:01F3` with `x = word +3 + byte +0x13 / 2 + 8`, `y = word +5 + byte +0x14 / 2 + 8` and 1, and `28C9:2A25` twice. With `x` now `word +3 - DS:1408` and `y` now `word +5 + byte +0x14 - DS:140A`, removes the slot's object through `31E0:05BB` with record word `+1`, 1, 0 and 1, then calls `28C9:2A25`, `2C5F:0139`, `28C9:2A25`, overlay 182 `+1856`, `28C9:2A25`, overlay 190 `+3169` with 1 and overlay 190 `+319E`. Requests `BMP` 11016 (`0x2B08`) through `38FF:04AB`; when that returns 0 and gives a picture, for each of its frames `i` (count from `1BF3:6706`) it draws frame `i` through `2D40:3BEC` with 1, `x`, `y` less the frame's height (`1BF3:771C`), 0 and the picture, calls `3D72:0D83` and overlay 190 `+319E`, plays sound 132 (`0x84`) through `2C5F:0A22` (FND-PARTY-149) after the first frame when `DS:13F7` is set, and waits 400 ms through `1000:12FA`; then frees the picture through `444C:0092`. Then calls `1BF3:348C` with the words 1, 0, 0, `0x13F`, `0xC7` and `0xF0`, `1BF3:4C09` with 1, `1BF3:4723` with 1, `1BF3:4FEB` with 0, `1BF3:4723` with 0, overlay 182 `+19F8`, `3D72:0D83`, overlay 204 `+1D2E` with 5, `2C5F:0139` and `3D72:0942`, and returns 1. No direct call was found; request 53 passes its trampoline to `+1A68`. |

## Interpretation

`+09D1` is the same kind of sequence as the removal at overlay 201 `+05D5` (FND-UI-048): it
brings the slot into view, takes its object off the map and plays a picture over the place it
stood, its frames standing on the object's bottom edge, with a sound on the first frame and 400 ms
between frames. It ends by redrawing the screen and fading the region palette back in through
`1BF3:4FEB` and overlay 204 `+1D2E` with 5, as `+05D5` does. A script reaches it through request
53 for one object, the party or a group; it returns 1 for each slot, so the request counts the
slots it handled. Bytes `+0x13` and `+0x14` of the 37-byte record read as the object's width and
height from how they centre and stand the picture. The name stays `fn_201_09D1` until a reading
shows which scripts use request 53.

## Alternatives

The trampoline `5778:0048` may also be reached by a computed far call other than the one through
`+1A68`, and no direct call of `+09D1` was found. `2C5F:01F3`, `28C9:2A25`, `31E0:05BB`,
`1BF3:348C`, `1BF3:4C09`, `1BF3:4723`, `1BF3:6706`, `3D72:0D83`, `3D72:0942`, overlay 190 `+3169`
and `+319E` and overlay 182 `+19F8` are not read here; the reading of them as centring the view,
removing an object, a frame count, redraws and a palette fade rests on their arguments and on
FND-UI-048, FND-EXE-602 and FND-EXE-606. What `BMP` 11016 shows was not checked, and the unit of
the wait through `1000:12FA` is taken from FND-RNG-030.

## How to reproduce

Run `overlay_listing.py` for overlay 201 against the installed `DSUN.EXE` over
`0x8A191..0x8A368` and `direct_callers.py` for `201+09D1`, read overlay 204 `+0169..+01A9` and the
table at `+06C7`, and compare the range with the rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.
