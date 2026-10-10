---
id: FND-PARTY-154
title: The spell and psionic power windows of a new level lay out 21 frames from their WIND resources, highlight a frame and name its spell or power while the pointer is on it, learn on a left-button release and open a description on a right-button release
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0009..3D72:0515
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093222..0x00093378
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009353C..0x00093930
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093C8B..0x00093C9C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093DE4..0x0009438E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068A3F..0x00068A7D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:02F8..3F96:036D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062B45..0x00062B6D
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x6237C..0x62732
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x62733..0x62B07
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/resident_listing.py, overlay_listing.py; a scratch script reading WIND, BUTN and APFM resources through the GFF directory as details_chunks.py reads a directory)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. FND-PARTY-104 reads how the two windows are opened, filled and closed and what
learning a spell or power changes; this finding reads their layout and the events of their frames.

**Control events.** `3D72:0009` handles a mouse event whose word at `+0x0C` of the packet is the
driver's event bits (FND-PARTY-078). It finds the control under the pointer and calls `3D72:0515`
with an event code for a control; `3D72:0515` calls the control's callback, the far pointer at
`+0x62` of its record, when the callback is not null and the code is set in the control's mask,
the word at `+0x58`, passing a 24-byte event of kind 5 with the code and the control's number
(`3D72:0515..3D72:0590`). The codes are: 8 to the control that was under the pointer before the
event, on every event (`3D72:0245..3D72:0259`); when the control under the pointer changed, 4 to
the old one and then 2 to the new one (`3D72:025C..3D72:0295`); for an `APFM` control under the
pointer, 0x100 when bit 3 (right button down) is set and 0x40 when bit 1 (left button down) is
set (`3D72:02E3..3D72:0361`, table at `cs:0509`), and 0x80 when bit 4 (right button up) is set
and 0x20 when bit 2 (left button up) is set (`3D72:039E..3D72:04B8`, table at `cs:04F1`).

**The callback setter.** Overlay 182 `+01EF` (trampoline `+004D`) stores the callback through
`3F96:02AB` and calls `3F96:02F8` with mode 1, which ORs the given mask into the control's mask
(`3F96:033D..3F96:0349`).

**The resources.** `RESOURCE.GFF#WIND/17500` is 189 x 117 with image 17000 and 23 children:
`BUTN/17300` at (23, 102) and `BUTN/17301` at (121, 102), 46 x 10 with text `XXXXXX` and `EXIT`,
and the `APFM` frames 11213 to 11233, 18 x 18, at (47 + 19c, 24 + 20r) for frame `11213 + 5r + c`
with `r` from 0 to 2 and `c` from 0 to 4, then 11228 to 11230 at (142, 24), (142, 44) and
(142, 64) and 11231 to 11233 at (28, 24), (28, 44) and (28, 64). `WIND/17501` is 159 x 113 with
image 17001 and 24 children: `BUTN/11319` at (63, 92), 42 x 12, text `XXXXX`; `BUTN/11320` at
(12, 92), 48 x 12, text `XXXXXX`; `BUTN/17302` at (102, 92), 42 x 12, text `EXIT`; and the same
21 frames at (32 + 19c, 10 + 20r), then (127, 10), (127, 30), (127, 50), (13, 10), (13, 30) and
(13, 50). The masks of frames 11213 to 11218 are 0x1E6 and of the others 0xE6; the masks of the
buttons are 0xA0.

**The spell window.** The fill `+00C2` gives frame `0x2BCD + i` of each listed spell `n` the
callback `+03DC` (trampoline `+0070`) with mask 0x66, draws `ICON 21000 + n` at the frame's
position plus 1, and draws `ICON` 15104, loaded by `+0218` into `4E71:002F`, over the frame
through overlay 190 `+0000` with last argument 0. It gives the frames past the list a null
callback (`+00F1..+020C`). `+0218` places the window at (65, 41) (`+02CE..+02E2`) and writes
`CHOOSE A SPELL,` with the character's name through `+07D0` (`+037E..+03BA`). The callback
switches on the code through `cs:0612`:

- 2 draws `ICON` 15104 over the frame with last argument 1 and writes `LEARN` with the spell's
  name, from overlay 177 `+09A4` (`+040B..+049F`);
- 4 draws it with last argument 0 and writes `CHOOSE A SPELL,` with the character's name
  (`+04A2..+0519`);
- 0x20 learns the spell (FND-PARTY-104);
- 0x80 closes the open description window, when `55CB:0000` is not 0, through overlay 213
  `+02D5` and redraws the window, calls overlay 213 `+16EE` with `n + 1`, and sets a null
  callback on all 21 frames (`+05A8..+060C`).

The handler `+0622` (trampoline `+007A`) closes the window and returns -1 for event kind 1, calls
`2C5F:0182` and returns 0 for kind 6, and handles kind 2 for the two buttons (FND-PARTY-104);
`EXIT` asks with `YOU HAVEN'T CHOSEN` and the choices `EXIT` and `CANCEL`, and on `CANCEL` calls
overlay 182 `+1FDB` with 6 and redraws (`+0628..+0728`). `+0218` sets the level byte `DS:43EC`
to the window's highest level, writes it with `LEVEL %d` into `BUTN/17300` and, when it is 1,
calls `3EBE:071F` on the button with 1, which closes the button to presses (FND-PARTY-077,
`+0324..+036E`).

**The psionic power window.** The fill `+0C84` gives each listed power `p` callback `+0EF0`
(trampoline `+005C`) with mask 0x66 and draws `ICON 21235 + p` and `ICON` 15104 with last
argument 5 (`+0D03..+0DE8`); `+0A84` places the window at (80, 43) (`+0B2B..+0B3C`). The callback switches through `cs:122E`:

- 2 draws the frame with last argument 8; for a power of rank above 0 it writes `ENHANCE TO
  LEVEL %d` with the power's name, and for rank 0 `LEARN` with the name (`+0F1E..+1067`);
- 4 draws it with last argument 5 and writes `PICK A PSIONIC POWER,` with the character's name
  (`+1069..+10C5`, `+11A8..+11C6`);
- 0x20 learns or enhances the power (FND-PARTY-104);
- 0x80 closes an open description as above, calls overlay 213 `+16EE` with `p + 236`, and sets a
  null callback on all 21 frames (`+11C9..+1229`).

The handler `+0E2D` (trampoline `+0052`) treats kinds 1 and 6 as `+0622` does. Kind 2 on
`BUTN/11320` steps the group byte `DS:9D7B + 4E71:0B44` through 0, 1 and 2, writes the group's
name, `Kinetics`, `Metabolic` or `Telepath` from the far pointers at `DS:2BA0`, into the button
and redraws; on `BUTN/17302` it asks with `YOU HAVE MORE`, `EXIT` and `CANCEL` (`+0E30..+0EEF`).

**Spell 0.** Overlay 177 `+0A45` with spell 0 and a class code returns 1 for class codes 11 and
18 only, as for every spell below 115 (`+0A53..+0A75`). So overlay 211 `+208C` of a slot and 1,
the spell window's highest level (FND-PARTY-104), takes `x` as the greater of the slot's levels
in class codes 11 and 18; code 18 is not among the 17 codes of FMT-PARTY-001.

## Interpretation

Event code 2 is the pointer entering a control, 4 leaving it, 0x20 a left-button release on it
and 0x80 a right-button release. In both windows, pointing at a frame highlights it and names the
spell or power in the text line, moving off restores the line, a left click learns, and a right
click opens a description of the spell or power, by its DATA number plus 1, after which the
frames take no events. The frames are filled in number order, so the first 15 choices fill the
centre block of five columns by three rows and the rest the right and left columns.

For a Preserver the highest spell level the window offers is `(L + 1) >> 1` for a Preserver level
`L` below 10 and `L >> 1` from 10: 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 5, 6, 6, 7 and 7 for levels 1 to
15.

## Alternatives

- What overlay 213 `+16EE` shows, and whether closing it gives the frames their callbacks back,
  was not read; the window's fill runs again on a redraw through `3A8E:060D`, which the level and
  group buttons call.
- What the last argument of overlay 190 `+0000` selects, a colour or a frame style, was not read.
- `XXXXXX` and `XXXXX` are the resources' placeholder texts, replaced by the code before the
  window is shown.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `resident_listing.py <dsun>
3D72:0009..3D72:0515`, `resident_listing.py <dsun> 3F96:02F8..3F96:0360`, `overlay_listing.py
<dsun> 182 68A3F 68A7D`, `overlay_listing.py <dsun> 209 93222 93930`, `overlay_listing.py <dsun>
209 93DE4 9438E` and `overlay_listing.py <dsun> 177 62B45 62BDA`. Read `WIND` 17500 and 17501 of
`RESOURCE.GFF` through its directory: the width, height and image words at `+0xBE`, the child
count at `+0xF3`, and the 30-byte children from `+0x105` with the tag at `+4`, the number at `+8`
and the position at `+0xC`; for each child resource read the size at `+0x28` and the mask at
`+0x58`.
