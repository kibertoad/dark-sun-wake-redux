---
id: FND-PARTY-036
title: Every ICON frame needs at most 6 paragraphs for the pointer save, and the pointer becomes a larger image only through 14 direct call sites
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000689BB..0x00068A3F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068DEC..0x00068E8D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0C8D..2C5F:0D26
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092B50..0x00092B9A
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0001ACA8..0x0001ADA6
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 and xxhash 4.0.1, MZ relocations and FBOV fixups applied (tools/research/exec-census/direct_callers.py, overlay_listing.py, resident_listing.py, gff_tag_numbers.py, image_frame_sizes.py)
environment: null
---

## Observation

In the installed `DSUN.EXE`, with `DS` the data segment `57E0`, and the pointer-image setters
`3D72:120B` and `3D72:12ED` and the pointer save `3E06:0002` of FND-PARTY-035.

**`ICON` frames.** `RESOURCE.GFF` holds all 400 `ICON` resources, with 615 frames. For a frame
of width `w` and height `h`, the pointer save reserves `((w >> 2) + 1) * (h + 1)` bytes. The
largest is 85 bytes, 6 paragraphs, for the 16 by 16 frames of `ICON` 3044 to 3047. `ICON` 19110,
254 bytes at `0x0001ACA8`, has one frame of 13 by 15 pixels, 64 bytes, 4 paragraphs.

**Direct calls of `3D72:12ED`.** Searching the load image for far calls with a relocated segment
word, each overlay's code for far calls with the segment word `0x0160` (descriptor 44, segment
`3D72`) in its fixup list, and segment `3D72` for near calls finds five sites and no others:

| Site | Routine | Image passed |
|---|---|---|
| overlay 182 `0x00068E70` | overlay 182 `+0x059C` | the resource it loads into `DS:45A6` |
| overlay 182 `0x0006A3FB` | overlay 182 `+0x19F8` | the far pointer at `4E71:0A59`, when the word at `DS:1440` is 5 |
| `2C5F:0CBF` | `2C5F:0C8D` | the far pointer at `4E71:0A5D` |
| overlay 208 `0x00092B67` | overlay 208 `0x00092A83` | the far pointer at `4E71:0A5D` |
| overlay 208 `0x00092B95` | overlay 208 `0x00092A83` | the far pointer at `4E71:0A59` |

Each passes frame 0, and each skips the call when the far pointer is 0. The same search finds no
direct call of `3D72:120B`, which loads only `ICON` resources.

**Overlay 182 `+0x059C`** takes a number `n` and a byte flag. It returns at once when `n` and
the flag equal the word at `DS:0FCC` and the byte at `DS:0FCF`. Otherwise it uses the tag `ICON`
when the flag is nonzero and `BMP ` when it is 0. It asks `38FF:05B5` about resource `n` under
that tag. When that returns 0 and the size it gives is above `0x800`, it returns. Otherwise it
loads the resource through `38FF:04AB` into `DS:45A6`, passes it to `3D72:12ED` with frame 0, and
stores `n` and the flag in `DS:0FCC` and `DS:0FCF`.

**The callers of `+0x059C`.** It is entered by near calls at overlay 182 `+0x0180`, `+0x1833` and
`+0x189D`, and through trampoline `56BD:006B`, by 4 far calls in the load image and 58 in overlay
code. The flag each passes:

- 1 (`ICON`) at the three near calls, at the load-image calls at file offsets `0x0001DF3E` and
  `0x0001F18F`, and at 50 of the overlay calls.
- 0 (`BMP `) at the load-image call at `0x0001EA07` and at the overlay calls at `0x00063C14`
  (overlay 178), `0x00075C7D`, `0x000777F7`, `0x00076BE1` and `0x0007714E` (overlay 189), and
  `0x00090BE1`, `0x0009106C` and `0x000910C6` (overlay 207).
- The byte at `[bp-1]` at `0x0002250C`, in `2C5F:0C8D`. That routine sets the byte to 1 on entry
  and to 0 only on the path at `2C5F:0CE1` when `572F:0025` returns nonzero in `AL`, where the
  number comes from `572F:0020`.

**Overlay 182 `+0x016B`**, which runs before the gate (FND-PARTY-031, steps 3 and 7), calls
`+0x059C` with `0x4AA6` (19110) and flag 1 when the double word at `DS:4454` is 0.

## Interpretation

The pool room left for the pointer save and the caret is 1,006 paragraphs, and a held caret
reservation is at most 1,000 (FND-PARTY-035). Any `ICON` pointer image fits beside it, so the
gate routine's two reservations cannot fail on space while the pointer is an `ICON`. That holds
for the image overlay 182 `+0x016B` sets before the gate. The pointer becomes something other than
an `ICON` only through the flag-0 calls of `+0x059C`, the flag-0 path of `2C5F:0C8D`, and the
four other calls of `3D72:12ED`.

## Alternatives

- A `BMP ` or other image is the pointer when the gate runs: not ruled out. Whether any of the
  routines holding those sites runs between program start and the gate was not read.
- `3D72:120B` or `3D72:12ED` is reached through an indirect call: not ruled out; indirect calls
  were not searched (Q-PARTY-011 covers the unresolved ones on the paths).

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<install>` its directory, run in `tools/research/exec-census/`:
`direct_callers.py <dsun> 3D72:12ED 3D72:120B 182+059C 186+0572`, where `186+0572` is the
control and gives the nine far calls of FND-PARTY-034 and one near call; `overlay_listing.py
<dsun> 182 0x689BB 0x68A3F`, `overlay_listing.py <dsun> 182 0x68DEC 0x68E8D`,
`overlay_listing.py <dsun> 208 0x92B50 0x92B9A` and `overlay_listing.py <dsun> 189 0x76BC1
0x76BE6`; `resident_listing.py <dsun> 2C5F:0C8D..2C5F:0D26`; and, for each number
`gff_tag_numbers.py <install> ICON` lists, `image_frame_sizes.py <install>/RESOURCE.GFF ICON
<number>`. The flag at each call of `+0x059C` is the word pushed before the number: `6A 01` or
`6A 00` in the bytes before the call, or the push read from the listing.
