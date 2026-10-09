---
id: FND-PARTY-035
title: The caret routine releases its earlier entries before making new ones, and the pointer save takes at most 2 paragraphs unless a routine swaps in another pointer image
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 41E1:0215..41E1:02B0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 409B:0F40..409B:0FA2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 409B:11B7..409B:12A4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0196..39D1:01E0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:02C0..39D1:02E4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:76C2..1BF3:7777
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:120B..3D72:1365
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00570282..0x00570329
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 and xxhash 4.0.1, MZ relocations applied for a load image at segment 0x1000 (tools/research/exec-census/immediate_search.py, resident_listing.py, gff_tag_numbers.py, image_frame_sizes.py)
environment: null
---

## Observation

In the installed `DSUN.EXE`, with `DS` the data segment `57E0`, and the reservation routine
`1BF3:27A8`, the sub-entry routine `1BF3:282D` and the release routine `1BF3:28C5` of
FND-PARTY-030. Segment `409B` and segment `41E1` address the same code: `41E1:000B` is
`409B:146B`.

**The caret's earlier entries.** `41E1:000B` (FND-PARTY-033) returns at once unless the word at
`DS:A179` and the far pointers at `DS:A17F` and `DS:A183` are nonzero, its far argument is
nonzero, and that argument equals `DS:A17F`. When the word at `DS:A18F` is not `0xFFFF` it then
calls `41E1:0215` with that same argument. `41E1:0215` (`409B:1675` to `409B:1710`) tests the
same five conditions, and when all hold it calls `1BF3:28C5` for `DS:A18F` when that word is
above 1 and for `DS:A191` when that word is above 1, then sets both words to `0xFFFF`. It returns
0. So each call of `41E1:000B` that reaches its new sub-entry and reservation has first released
the ones it kept before. When the new sub-entry fails, `DS:A191` is still `0xFFFF`; when the new
reservation fails, the sub-entry stays in `DS:A18F` and is released by the next call. The startup
routine `39D1:0009` sets `DS:A179` to 0 and both words to `0xFFFF` at `39D1:02C0` to `39D1:02D8`.
Decoding every byte of the load image and of each overlay's code, the other stores to `DS:A18F` and
`DS:A191` are the ones in these two routines.

**The caret's height.** The same search finds five stores to `DS:A189` and no others, in two
routines that each return the result of a call to `41E1:000B` as their own:

- `409B:11B7`, which marks the object of its far argument, sets `DS:A179` to 1, stores the
  argument in `DS:A17F`, sets `DS:A189` to 9 (`409B:1236`), and then stores the word described
  below (`409B:127D`), or 9 when that word is 0 (`409B:1284`).
- The routine `409B:0D48`, in the part the inventory gives as `409B:0EA8` to `409B:0FA2`,
  stores the same word at `409B:0F83`, or 9 at `409B:0F8A`.

The word is at offset 4 from `T + 4 * k`, where `T` is the far pointer at offset `0x9C` of the
object and `k` the word at offset `0x8A` of it. `41E1:000B` reserves (0, 0, 1, `DS:A189` − 1),
which is `DS:A189` bytes. Above 16,000 bytes the reservation fails (FND-PARTY-030), so a held
caret reservation is at most 1,000 paragraphs.

**The pointer image.** `39D1:0009` loads resource 100 under the tag `ICON` through `38FF:0438`
at `39D1:0196` to `39D1:01A7` and stores the far result at `DS:A145` and, at `39D1:01D8`, at
`DS:A14D`. No other instruction stores to `DS:A145` or `DS:A147`. The pointer save `3E06:0002`
(FND-PARTY-033) chooses its image by the word at `DS:A13D`: 0xFFFF makes no reservation; 0 uses
the image at `DS:A14D` and the frame in the word at `DS:A151`; 2 and 3 take paths without a
reservation; any other value uses the image at `DS:A145` with that value as the frame.

**Frame sizes.** `1BF3:76C2` and `1BF3:771C` take a far image and a frame number. When the
number is not below the word at offset 4 of the image, they return `0xFFFF`. Otherwise they
read the double word at offset `6 + 4 * frame` and return the word at that offset from the
image, or the word after it. That is the frame's width or height (FMT-IMAGE-001, FMT-IMAGE-002).
The pointer save makes no reservation when either is below 1.

**`ICON` 100.** `RESOURCE.GFF` is the only installed archive with an `ICON` table and holds the
only `ICON` 100. That resource, 167 bytes at `0x00570282`, has two frames: 6 by 8 pixels and 4 by
7. The reservations (0, 0, 6, 8) and (0, 0, 4, 7) are 18 and 16 bytes, 2 and 1 paragraphs.

**Other pointer images.** Two routines store to `DS:A14D` after startup.

- `3D72:120B`, with a word `n`, does one of three things:
  - When `n` is 0, it releases the loaded image when `DS:A149` is nonzero, and stores
    `DS:A145` at `DS:A14D` and 0 at `DS:A149`.
  - When `n` equals `DS:A149`, it does nothing.
  - Otherwise it releases the loaded image in the same way, loads `ICON` `n` through
    `38FF:0438`, and stores it at `DS:A14D` and `n` at `DS:A149`.
- `3D72:12ED`, with a far image pointer and a word, does one of two things:
  - When the pointer is 0, it does the same as `3D72:120B` with 0 and also clears `DS:A151`.
  - Otherwise it stores the pointer at `DS:A14D` and the word at `DS:A151`.

The joined `DSUN.EXE` inventory in `coverage/` has no row that starts at `3D72:120B`.

## Interpretation

Caret entries do not pile up. Of the holders FND-PARTY-033 lists, while START GAME runs at most one caret sub-entry and one caret
reservation are held, and the reservation takes `(rows + 15) >> 4` paragraphs, with `rows` the
height the caret's object gives, or 9, and at most 1,000 paragraphs. With the startup pointer
image, a held pointer save takes at most 2 paragraphs. Together that is at most 1,002 of the
1,006 paragraphs and 4 of the 251 entries left beside the two screen entries, the startup
reservation (FND-PARTY-032) and the gate's own two (FND-PARTY-030), so neither of the gate
routine's reservations can fail on space unless `3D72:120B` or `3D72:12ED` has put a larger image
in place, with `DS:A13D` 0, before the gate.

## Alternatives

- The caret routine accumulates entries: ruled out. `41E1:0215` passes the same tests as
  `41E1:000B` with the same argument, and releases both words.
- The pointer image is always `ICON` 100: ruled out. `3D72:120B` and `3D72:12ED` replace it.
  Which of their callers run before the gate, and with what images, was not read.
- A tall caret fills the pool: ruled out as a cause on its own. The values in the table at
  offset `0x9C` of an edit-field object were not read, but above 16,000 rows the caret
  reservation fails and holds nothing, and at or below it the caret takes at most 1,000
  paragraphs.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<install>` its directory, run in `tools/research/exec-census/`:
`immediate_search.py <dsun> A189 A18F A191 A179` and `immediate_search.py <dsun> A145 A147 A14D A14F
A151 A13D`; `resident_listing.py <dsun> 409B:0F40..409B:0FA2 409B:11B7..409B:12A4
409B:146B..409B:1710 39D1:0009..39D1:02FC 1BF3:76C2..1BF3:7777 3D72:120B..3D72:1365
3E06:0002..3E06:021F`; `gff_tag_numbers.py <install> ICON`; and `image_frame_sizes.py
<install>/RESOURCE.GFF ICON 100`. The search prints hits as linear paragraph and offset; the
stores to `DS:A14D` at linear `0x3E968` and `0x3E9F8` are `3D72:1248` and `3D72:12D8`.
