---
id: FND-PARTY-155
title: The spell and psionic power windows write two centred text lines, highlight a frame by drawing another frame of ICON 15104, pause 100 retraces after a spell is learned, and open a description window whose closing refills the frames
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093930..0x00093B62
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000933F7..0x00093470
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093CB4..0x00093D08
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00078340..0x000783D2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006F7B8..0x0006F7D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:2A74..1BF3:2A8C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00099535..0x000995F6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009A94E..0x0009AB3A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0D0C..3A8E:0D3C
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x3273F..0x3287F
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/resident_listing.py, overlay_listing.py; scratch scripts reading WIND, APFM and EBOX resources and counting SPIN resources through the GFF directories as details_chunks.py reads a directory)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. The windows are those of FND-PARTY-104 and FND-PARTY-154.

**The text line.** Overlay 209 `+07D0` takes a window, a row `y`, a width `w`, two colours `a` and
`b`, a background colour `c` and two far strings `s1` and `s2`. It writes, through `401B:0172` with
the format `%C%C%C%s`, the string held at the far pointer `DS:9C18` in colour `c` at row `y` and
then `s1` in colours `a` and `b` at row `y`, and the string held at `DS:9C14` in colour `c` and then
`s2` in `a` and `b` at row `y + 7`. Each string's column is `w / 2` less half its width from
`2C5F:03B7`. It then copies `s1` to `DS:9C18` and `s2` to `DS:9C14` through `1000:406D` and passes
the rectangle covering the old and new text to `4328:0BD5` (`+07D5..+09F9`). `+0218`
allocates the two 80-byte buffers through `444C:0008` (`+0297..+02BE`). The
spell window's calls pass `y` 0x51, `w` 0xBD, `a` 0xB5, `b` 0xC2 and `c` 0xC0; the psionic window's
pass 0x46, 0x9F, 0xE3, 0xFE and 0xE7. With `LEARN` and `ENHANCE TO LEVEL %d` the name is `s2`; with
`IS LEARNED!` the name is `s1`.

**The frame highlight.** Overlay 190 `+0000` takes a flag, a column, a row, a word, a far image and
a frame number `k`; when the image is not null it passes them all to `2D40:3BEC`, which draws frame
`k`, and passes the rectangle of frame `k`, from the frame's width and height words, to
`4328:0BD5` (`+000B..+0089`). So the last argument the windows pass with `ICON` 15104 is a frame
of that icon: 0 for a spell frame and 1 for one under the pointer, 5 for a psionic frame and 8 for
one under the pointer.

**The pause.** Overlay 186 `+00B8` with `n` calls `1BF3:2A74` `n` times (`+00BD..+00D0`).
`1BF3:2A74` reads the port at the BIOS word `0040:0063` plus 6 until bit 3 is clear and then until
it is set, and returns. The spell window calls it with 100 after writing `IS LEARNED!` and before
closing.

**The description.** Overlay 213 `+16EE` takes `m`, the DATA number plus 1. It builds
`RESOURCE.GFF#WIND/15503` at (86, 56), with the handler `+034F` (trampoline `+0048`), and stores it
at `55CB:0000`. For `m - 1` below 235 it draws `ICON 21000 + m - 1`, and for `m - 1` from 235 to
268 `ICON 21000 + m - 1` too, at the window's position plus (9, 8). It reads `SPIN` resource `m`
into a 200-byte buffer from offset 5, puts five spaces before it, and sets the text of `EBOX/15400`
through `4228:002B`, or sets `     UNKNOWN` when the resource is missing. It gives `APFM/15200`
the callback `+034F` with mask 0x60 (`+1711..+18D5`). `WIND/15503` is 145 x 87 with image 15002 and
the children `APFM/15200` at (0, 0), 145 x 87, and `EBOX/15400` at (9, 15). `RESOURCE.GFF` holds
269 `SPIN` resources, numbered 1 to 269.

The handler `+034F` closes the window through `+02D5` for event kind 1 and for code 0x20 on
`APFM/15200`; for code 0x80 it keeps `DS:2E62`, closes the window through `+02D5` and goes on to
code gated by `DS:08CC` (`+035B..+03DB`). `+02D5` closes the window, stores 0 at `55CB:0000` and -1
at `DS:2E62`, and when `DS:1431` is not null calls overlay 182 `+1FDB` with 4 and `3A8E:060D` with
that window (`+02D8..+031C`).

**The refill.** `3A8E:0D0C` stores its far pointer argument at `+0xF9` of the window
(`3A8E:0D1D..3A8E:0D32`); the spell and psionic windows store their fill routines there
(`+030B`, `+0B65`), and `3A8E:060D` calls the pointer at `+0xF9` when it is not null
(`3A8E:08AA..3A8E:08B7`).

**The picks button.** `+0A84` calls `3EBE:071F` with `BUTN/11319` and 1 on every opening
(`+0B9B..+0BA8`).

## Interpretation

Both windows show a two-line message centred in the window: in the spell window from row 81 of
the window, screen rows 122 and 129, and in the psionic window from row 70, screen rows 113 and
120, centred on screen column 159 in both. The new text is drawn after the old text is overdrawn in
the background colour. A frame under the pointer is highlighted by drawing another frame of the
icon over it.

`1BF3:2A74` waits for the start of a vertical retrace, the bit 3 of the VGA input status register,
so 100 calls wait 100 retraces, about 1.4 seconds at the 70 Hz of mode 13h, while `IS LEARNED!` is
shown.

A right-button release on a frame opens a 145 x 87 description of the spell or power with its icon
and its `SPIN` text. A left or right click on the description closes it, and closing it redraws
the spell or psionic window, whose fill routine sets the frames' callbacks again, so the frames
answer the pointer once the description is gone. The picks button of the psionic window only
shows the count.

## Alternatives

- The VGA mode the game runs in when these windows show was not read here; at another refresh
  rate the pause is 100 retraces of that rate.
- What the code after a right-button release on the description does when `DS:08CC` is not 0 was
  not read; it reads `DS:2E62`, which `+02D5` sets to -1.
- What overlay 182 `+1FDB` does with 4 and 6 was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `overlay_listing.py <dsun> 209 93930 93B62`,
`overlay_listing.py <dsun> 209 93BE4 93DA6`, `overlay_listing.py <dsun> 190 78340 783D2`,
`overlay_listing.py <dsun> 186 6F7B8 6F7D4`, `overlay_listing.py <dsun> 213 99535 99650`,
`overlay_listing.py <dsun> 213 9A94E 9AB3A`, `resident_listing.py <dsun> 1BF3:2A74..1BF3:2A8C`,
`resident_listing.py <dsun> 3A8E:0D0C..3A8E:0D3C` and `resident_listing.py <dsun>
3A8E:060D..3A8E:0900`. Read `WIND` 15503 of `RESOURCE.GFF` as FND-PARTY-154 reads `WIND` 17500,
and count the `SPIN` entries of its directory.
