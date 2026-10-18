---
id: FND-IMAGE-011
title: No port write in the inventoried code changes the CRT timing of BIOS mode 0x13, so the game shows its 320x200 canvas with that mode's timing
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:2973..1BF3:2A4A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:5814..1BF3:586B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E8B0..0x0008E948
tool: tools/research/exec-census/port_out_census.py at the commit that adds this finding, and overlay_listing.py and resident_listing.py; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

FND-VIDEO-003 reads the set-mode routine 1BF3:2973: for play it asks
the BIOS for mode 0x13 through `INT 10h` and then unchains the planes,
and the FLI player asks for plain mode 0x13. A byte search for `CD 10`
finds the eleven sites that finding names in the load image and one in
overlay code, at file 0x88D40 in overlay 199, where the pair is the
high byte of one word and the low byte of the next in a run of words
(0x10D9, 0x10D6, 0x10D3, 0x10D0, 0x10CD), not an instruction.

The census decodes every range of every row of the function inventory
(`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`), 200,692 of the 336,752 load
image bytes and 203,366 of the 258,376 bytes of overlay code, and finds
207 OUT or OUTS instructions. For each it takes the last writer of DX
and of AX before it in the same range and the site was checked against
its surrounding instructions. By port:

| Port | Sites | Register index and change |
|---|---|---|
| CRTC index 0x3D4 | 1BF3:2A10..2A1E | 0x14: bit 6 cleared; 0x17: bit 6 set (set-mode, mode 0x100 or above) |
| CRTC index 0x3D4 | 1BF3:585E | 0x0C, start address high (page change, FND-PARTY-195) |
| CRTC, DX from the BIOS word 0040:0063 | overlay 205 +0036, +003B, +004D, +0095 | 0x0C and 0x0D, start address; 0x08, preset row scan; 0x13, offset |
| Attribute 0x3C0 | overlay 205 +0052, +0057 | index 0x13 with bit 5 set, horizontal pel panning |
| Sequencer 0x3C4/0x3C5 | set-mode and drawing routines in 1BF3, 2707 and 4400 | 1: bit 5 set and cleared again; 2: map mask; 4: bit 3 cleared, bit 2 set |
| Graphics 0x3CE/0x3CF | same | 4: read map; 5: write mode bit 0 and bit 4; 6: bit 1 cleared |
| DAC 0x3C7, 0x3C8, 0x3C9 | 1BF3:2A96..2AA3, 2AB3, 2ADB, 58DF..58F2; 3796:04CA, 0530..0537, 055E | palette entries |
| Monochrome 0x3B4, 0x3B8, 0x3BF | 1BF3:298C..29A5 | only on set-mode's branch for mode 7 |
| Timer 0x40, 0x43 | 1000:12C3, 4842:05A6..05B7 | not video |
| DX plus 4 and 5 from a stored base | 4734:01C5, 4734:01CF | not a video port |

The sequencer and graphics sites at 1BF3:46CA and 1BF3:46DE send the
words kept at CS:1192 and CS:1194. Every store to them in segment 1BF3
writes a word whose low byte is 2 (CS:1192) or 4 (CS:1194): the stores
at 4690, 4679, 4E11 and 5140 follow loads of AL with those values, and
those at 46C3, 46D7, 4E4E and 517D change only the high byte of the
word they reload.

The other 23 sites are not instructions: 20 decode from the byte after
a `CD 3F` pair in the overlay trampoline tables of segments 5671 to
57CE (FMT-EXE-004), two from the text that follows the call at
1000:02C1, and one from inventory row 0x00066243, which starts one byte
inside the instruction at 0x00066242 in overlay 179.

Outside the inventory, every E6, E7, EE, EF, 6E or 6F byte with a load
of DX with 0x3B0..0x3DF (`BA xx 03`) or a read of word 0x63 (`16 63
00`) in the 32 bytes before it was listed: four bytes at file 0x9AD38
to 0x9AD40, which follow `BA C8 03` and write the DAC.

No site writes the miscellaneous output register (0x3C2), CRTC
registers 0x00 to 0x07, 0x09, 0x10 to 0x12, 0x15 or 0x16, or bits 0
and 3 of sequencer register 1. The positive controls are 1BF3:2A10
(DX from an immediate), overlay 205 +0095 (DX from 0040:0063) and
4842:05A6 (an immediate port).

## Interpretation

The game leaves the horizontal and vertical timing, the clock and the
line doubling of BIOS mode 0x13 as the BIOS set them: 200 rows of 320
pixels, each row scanned twice for 400 lines. Its CRTC writes change
only addressing (dword and byte mode for the unchained layout, the
start address for page changes and scrolling, the offset for a wider
virtual row) and the preset row scan and pel panning used for
scrolling. The canvas is therefore shown at the pixel aspect of mode
0x13 on whatever display shows it: where the 400 lines fill a 4:3
picture, a pixel is 1.2 times as tall as it is wide.

## Alternatives

- Reading overlay 205's writes of CRTC 0x08 and attribute 0x13 as a
  change of timing fails: preset row scan and pel panning move the
  picture by rows and pixels inside the same frame.
- A write through code outside the inventory with DX formed farther
  than 32 bytes before the OUT, or through a callee that sets DX, is
  not covered; the census's linear writer search also does not follow
  jumps into a site. Each listed site was checked by hand.
- The 1.2 ratio is the arithmetic of 320 by 200 on a 4:3 picture; a
  display set to another shape shows another ratio, which no reading
  of the game decides.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the locked evidence Python. Run
`port_out_census.py` with the installed DSUN.EXE and
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`. List overlay 205 file
8E8B0..8E948 with overlay_listing.py and 1BF3:2973..1BF3:2A4A and
1BF3:5814..1BF3:586B with resident_listing.py. Search the load image
and each overlay's code for the bytes `CD 10`. The census output is
retained under GAME_DIR/analysis/video-timing-20261018/. No original
function or game process was run.
