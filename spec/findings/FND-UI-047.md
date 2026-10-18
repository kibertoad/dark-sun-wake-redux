---
id: FND-UI-047
title: An overhead-map mark is one pixel in the colour index of its mark value, written to graphics slot 1, after which the map shows that page and sets the pointer to ICON 19101
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:2AF4..1BF3:2C18
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0001380C..0x00013810
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2A81..28C9:2ABD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089A92..0x00089ABD
tool: tools/research/exec-census/resident_listing.py and overlay_listing.py at commit 2932e9d3; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Overlay 201 +02D2..+02DE (file 0x89A92..0x89A9E) pushes DI, y, x and
1 and far-calls 1BF3:2AF4, so the routine sees the slot number 1 at
[BP+6], x at [BP+8], y at [BP+0A] and the mark value at [BP+0C]
(FND-UI-046). The caller removes the eight bytes and does not read a
result.

1BF3:2AF4 saves DS, SI and DI and sets DS to CS. It doubles the slot
word and copies that slot's words from the arrays at CS:0004,
CS:0204, CS:0404, CS:0604, CS:0804, CS:0A04 and CS:0C04 to scratch
words at CS:0E04 to CS:0E10, with the column and row counts at
CS:0E12 and CS:0E14. FND-CONFIG-192 reads the same slot arrays as
segment, count, left, top, right, bottom and flags. While the flag
word has bit 0x40 set, it replaces the slot offset with the slot's
CS:0004 word and reads that slot's flags again; it stores the last
flag word at CS:0E22 and copies that root slot's segment, count,
left, top, right and bottom to CS:0E16 to CS:0E20, its row count
`bottom - top + 1` to CS:0E26 and its column-group count
`(right >> 2) - (left >> 2) + 1` to CS:0E24. There is no bound on
the slot number or the chain.

It then tests x against the first slot's left and right words and y
against its top and bottom words, all signed comparisons with both
ends included. A point outside skips to the return with nothing
written. Inside, it loads ES from the root's segment and computes

    offset = (x >> 2) - (root_left >> 2) + (y - root_top) * groups

with both shifts logical and the product's low word. A group count
of 80 takes a shift-and-add path that gives the same low word as the
multiplication. It reads the byte at CS:26DC + (x AND 3), writes
index 2 and that byte to port 0x3C4 in one word output, and stores
the low byte of the mark value at ES:offset. It restores DI, SI, DS
and BP and returns far. AX is not set to a result.

The four bytes at CS:26DC (file 0x1380C) are 0x01, 0x02, 0x04 and
0x08. A search of all of segment 1BF3's code for operands naming
CS:26DC to CS:26DF finds 33 instructions, every one a byte read with
an index register, the read at 2C03 among them; none writes the
table. Writers from other segments through a segment register
holding 1BF3 are not covered by that search.

FND-CONFIG-193's initializer sets slot 1 to segment 0xA400, bounds
(0, 0) to (319, 199) and flags 0; slot 0 is 0xA000 with the same
bounds. FND-PARTY-195 reads 1BF3:5814, which on each page change
swaps the segments in slots 0 and 1 so that slot 0 always holds the
page being shown and slot 1 the other one.

After the loop, overlay 201 +02EB pushes 1 and calls 28C9:2A81. That
routine stores 0 in the word at DS:0D9E when its byte argument is
nonzero, decrements the word when it is above 0 (signed), and, when
the word is then 0, calls 28C9:2AAE, which stores 1 in the bytes at
DS:2FB8 and DS:33BA, and far-calls 4328:0B4C. With the argument 1
both calls are always made. FND-CONFIG-171 and FND-PARTY-195 read
4328:0B4C: when the word at DS:2FB8 is nonzero, the words at DS:33B6
and DS:33B8 differ and the word at DS:33BA is nonzero, it makes the
other VGA page the shown one through 1BF3:5814 with 0xFFFF, waiting
for the vertical retrace, copies the shown page onto the hidden one
when the word at DS:A2B2 is nonzero, and clears DS:2FB8.

Overlay 201 +02F3 then pushes 1 and 0x4A9D (19101) and calls overlay
182 +059C. FND-CONFIG-187 reads that routine: unless 19101 and the
byte 1 equal the number and byte cached at DS:0FCC and DS:0FCF, it
requests `ICON` 19101 into the far pointer at DS:45A6 and passes it
with 0 to 3D72:12ED, which stores it as the pointer image at DS:A14D
(FND-PARTY-035), and caches 19101 and 1. FND-INPUT-001 and
FND-INPUT-002 identify `ICON/19101` as the 10 by 13 Walk pointer.

The picture-request failure branch at +0154 jumps to +02EB, so these
two calls also run when the region map did not load.

## Interpretation

Each mark is a single pixel. Its colour is the palette index equal
to the mark value: 1 for the leader in party slots 0 to 3, 16 for the
leader in slot 4 and for every kind-2 object above slot 4, 100 for
kind-1 objects and 11 for kind-5 objects when DS:143C is nonzero. The
pixel goes to graphics slot 1, the hidden page, at screen coordinates
(x, y), on the plane x AND 3 of a 320 by 200 unchained page whose row
is 80 bytes, the same page the frame and region pictures were drawn
on through 2D40:3BEC with page 1 (FND-UI-046). A later object's mark
replaces an earlier one at the same pixel, since slots are drawn in
order 0 to 319. Clipping is to the whole screen, so a mark is never
clipped to the picture: an object exactly on the region's maximum x or
y bound lands one column right of, or one row below, the region map,
since `scale(size, extent, extent)` equals `size`.

The call with 1 after the marks makes the drawn page visible and
copies it back to the hidden page, provided DS:33B6 and DS:33B8 hold
different page numbers as they did in FND-PARTY-195's run. The call
with 19101 and 1 sets the mouse pointer image to the Walk pointer
`ICON/19101` while the map is shown.

## Alternatives

- Reading DI as an argument that selects a mark shape or icon fails:
  the routine uses only the low byte of the fourth word, as the byte
  it stores.
- Reading the store as a full-byte write of the colour holds only
  under the graphics controller's default write mode 0 with set/reset
  disabled, bit mask 0xFF and no rotate or logical function. The
  routine sets only the sequencer's map mask; FND-PARTY-195 shows the
  page copy clearing write mode 1 after use, but no reading here shows
  the other graphics registers at this call.
- Reading the call with 19101 as a sound or a picture fails:
  FND-CONFIG-187's nonzero byte selects the `ICON` type, and the
  stored pointer at DS:A14D is the pointer image FND-PARTY-035 reads.
- Which red, green and blue the indexes 1, 11, 16 and 100 have while
  the map is shown depends on the palette in the VGA DAC, which no
  reading here shows (Q-UI-011). The manual's blue party marker and
  red markers for others are a source, not evidence of these values.
- Who writes DS:33B6 and DS:33B8, and so whether the page change
  always happens here, is not read beyond FND-PARTY-195's run.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the locked evidence Python.
List 1BF3:2AF4..1BF3:2C18 and 28C9:2A81..28C9:2ABD with
resident_listing.py, and overlay 201 file 89A92..89ABD with
overlay_listing.py. Read four bytes of the relocated load image at
1BF3:26DC (file 0x1380C, header 0x5200). Disassemble 1BF3:0E50 to
1BF3:CD60 linearly with Capstone, skipping undecodable bytes, and
keep instructions whose operands contain 0x26dc, 0x26dd, 0x26de or
0x26df; the read at 1BF3:2C03 is the positive control, and stores
through a segment register other than CS or DS set to CS are not
searched. Retained listings are under
GAME_DIR/analysis/ui-map-mark-20261018/. No original function or game
process was run and no image was viewed.
