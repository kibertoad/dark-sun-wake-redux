---
id: FND-VIDEO-006
title: The cinematic fallback shows two or three BMP resources per cinematic for up to 8 seconds each, and more for cinematic 5
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56EF:011A..56EF:011E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:167B..57E0:1692
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; far calls in overlays found through their fixup words and the segment table at file offset 0x4B080; the GFF directories read as FMT-GFF-001 describes
environment: null
---

## Observation

`DS` is the data segment `57E0`. The entry at `56EF:011A` leads to offset `0x28B3` of overlay 187,
`DSUN.EXE+0x000726E3`, a routine that takes a byte `n`. It keeps the byte at `DS:14E5`, calls
`5787:005C`, and sets `DS:14E5` to 0. When `n` is 1 it first runs itself with 0, and when
`44B6:0011` then returns a value with bit 0 or 1 set it goes to the end. It calls `1BF3:4723`
with 1 and with 0 and sets the word at `DS:631B` to 1.

The words at `DS:167B` and `DS:1687`, indexed by `n`, give a count and a first number:

| `n` | Count | First number | Numbers shown |
|---|---|---|---|
| 0 | 3 | 11009 | 11009 to 11011 |
| 1 | 3 | 11150 | 11150 to 11152 |
| 2 | 3 | 11158 | 11158 to 11160 |
| 3 | 3 | 11161 | 11161 to 11163 |
| 4 | 2 | 11153 | 11153 to 11154 |
| 5 | 3 | 11155 | 11155 to 11157 |

For each number in turn it calls `56BD:0057` with it and looks up the resource of type `BMP `
(`0x20504D42`) and that number through `38FF:04AB`. When one is found it passes it to
`2D40:3BEC`, the image routine of FND-IMAGE-005, with 1, 0 and 0, and calls `1BF3:4C09` with 1. It then calls `1000:12FA` with 10 up to
800 times, stopping early when `DS:631B` is not 0 and `44B6:0011` returns a value with bit 0 or 1
set. When it did not stop early it calls `1BF3:4FEB` with 0; in both cases it calls `1BF3:4723`
with 0 and frees the resource. When `n` is 0 and it stopped early, it shows no more numbers.

After the last number it calls `1BF3:4723` with 1, runs offset `0x2A21` when `n` is 5, and puts
back the kept byte in `DS:14E5`. Offset `0x2A21` looks up `BMP ` 11164 the same way, draws it,
frees it and calls `571F:00BB` with 1, then looks up the numbers from 11166 on and draws each
centred on the 320x200 screen, which was not read further.

All the numbers in the table, 11164 and 11166 to 11174 are `BMP ` resources of `RESOURCE.GFF`.

## Interpretation

When an FLI cannot play, the game shows a short slideshow for the cinematic instead: the
cinematic's two or three still pictures, each for up to 8 seconds (800 waits of 10 ms,
FND-TIME-004), with a key moving on to the next. For the opening, cinematic 1, it first shows the
pictures of 0, and a key during those skips the whole opening. Cinematic 5 goes on to more
pictures, which read as the closing credits. `1BF3:4FEB` is probably a fade that a key skips. The
last picture of 0, `BMP` 11011, is the title picture of FND-IMAGE-007: the routine reaches its
number as 11009 plus 2, so no instruction holds 11011 (FND-IMAGE-008).

## Alternatives

`5787:005C`, `56BD:0057`, `1BF3:4723`, `1BF3:4C09`, `1BF3:4FEB` and `571F:00BB` were
not read, so fading and what `56BD:0057` does with the number are inferred. The rest of
offset `0x2A21`, and what `DS:14E5` is for, were not read.

## How to reproduce

Disassemble overlay 187 from file offset `0x726E3` to `0x728E0`, resolving far calls through the
segment table at `0x4B080`; read the 12 words at file offset `0x4E67B`; list the `BMP ` resources
of `RESOURCE.GFF`.
