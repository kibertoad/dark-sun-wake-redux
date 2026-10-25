---
id: FND-EXE-613
title: Overlays 214, 215, 216 and 217 are one routine each that decode the FLI player's chunks, copying colour packets into a palette buffer, sending them to the VGA palette registers, expanding a byte-run frame and applying a line-compressed delta
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009ACD0..0x0009AD07
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009AD10..0x0009AD4D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009AD60..0x0009ADAD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009ADC0..0x0009AE1C
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported one row at `+0000` in each of four overlays that no spec
location covered, 285 bytes in all: overlay 214 (header segment `57D4`, code from file offset
`0x9ACD0`, 55 bytes), overlay 215 (header segment `57D7`, code from `0x9AD10`, 61 bytes),
overlay 216 (header segment `57DA`, code from `0x9AD60`, 77 bytes) and overlay 217 (header segment
`57DD`, code from `0x9ADC0`, 92 bytes). Each overlay has one trampoline, at `+0020` of its header
segment, and its row covers its whole code. Each row below is one row of the inventory; "offset"
is the overlay code offset, the file offset is the overlay's code offset plus it, and `DS` is
`57E0`. Callers are the hits of `direct_callers.py`, checked against the instructions around
them. All four are called only from overlay 196 `+0000`, the chunk dispatch of the FLI player
(FND-VIDEO-008), which names them by their trampolines. Each routine saves the registers it uses,
clears the direction flag, and loads `DS:SI` from its first far argument, the chunk's data after
the 6-byte chunk head. Counts and bytes are read with `lodsw` and `lodsb`.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| 214 `+0000` | `0x9ACD0..0x9AD07` | `57D4:0020` | Takes the data and a far destination in `ES:DI`. Reads a word, the packet count; for each packet it reads a byte and adds three times it to `DI`, then reads a count byte, 0 taken as 256, and copies three times that many bytes. The skip and count are added as the whole of `AX`, whose high byte is the high byte of the packet count, or 1 after a count byte of 0. Called from overlay 196 `+005A` for chunk type `0x0B` with the far pointer at `DS:24F2`. |
| 215 `+0000` | `0x9AD10..0x9AD4D` | `57D7:0020` | Takes the data. Starts a colour index at 0 and reads the packet count; for each packet it adds a skip byte to the index and reads a count byte, 0 taken as 256, and for each colour writes the index to port `0x3C8`, adds 1 to it, and writes the next three bytes to port `0x3C9`. Called from overlay 196 `+0046` for chunk type `0x0B`, before overlay 214, when the dispatch's last argument is not 0. |
| 216 `+0000` | `0x9AD60..0x9ADAD` | `57DA:0020` | Takes the data, a far destination and a line count. For each line it starts at the line's first byte, reads a packet count byte and for each packet reads a signed count byte: for a count of 0 or more it stores the next byte that many times, and for a negative count it copies the negation of the count bytes. The next line starts 320 bytes after the last. Called from overlay 196 `+00A2` for chunk type `0x0F` with the far pointer at `DS:24EE` and the word at `DS:24EA`, 200. |
| 217 `+0000` | `0x9ADC0..0x9AE1C` | `57DD:0020` | Takes the data and a far destination. Reads a word, the first line, and moves the destination 320 bytes per line down; reads a word, the line count. For each line it reads a packet count byte and for each packet adds a skip byte to the destination and reads a signed count byte: for a count of 0 or more it copies that many bytes, and for a negative count it stores the next byte the negation of the count times. The next line starts 320 bytes after the last. Called from overlay 196 `+0070` for chunk type `0x0C` with the far pointer at `DS:24EE`. |

The bytes at `DS:24E4` hold the far pointer `A000:0000` at `DS:24EE` and the word 200 at
`DS:24EA` (FND-VIDEO-008).

## Interpretation

The four routines decode the chunk types of the published FLI format that FND-VIDEO-008 lists:
overlay 214 and 215 are the 64-level colour chunk, type `0x0B`, kept in the palette buffer at
`DS:24F2` and, when asked, written to the VGA palette; overlay 216 is the byte-run chunk, type
`0x0F`, which draws a whole 320 by 200 frame; and overlay 217 is the line-compressed delta, type
`0x0C`, which skips lines and bytes and copies or fills runs. Their run signs are opposite: a
positive count repeats a byte in 216 and copies bytes in 217. This answers the chunk layout
question FMT-VIDEO-001 leaves open. In a colour chunk where a packet of 256 colours is followed by
another packet, overlay 214 would take that packet's skip and count each 256 larger, while overlay
215 would not, because only 214 loads 256 into `AX`. The names stay `fn_214_0000` to `fn_217_0000` until a
reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list. The packet layouts are read from the code alone; no installed FLI
file was decoded with them here. The count of the first line in 217 is multiplied as an unsigned
16-bit value, and a line count of 0 in 216 or 217 would run 65,536 lines; neither case was looked
for in the files.

## How to reproduce

Run `overlay_listing.py` for overlays 214, 215, 216 and 217 against the installed `DSUN.EXE` over
each file range in the table and `direct_callers.py` for `214+0000`, `215+0000`, `216+0000` and
`217+0000`, read overlay 196 `+0000..+0116`, and compare the ranges with the rows of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.
