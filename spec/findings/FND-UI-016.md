---
id: FND-UI-016
title: A conversation capture shows the speech window at (0,0) and the response window at (0,140)
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: DOSBox screenshot compared with Python 3.14.7 and Pillow 12.3.0
environment: GOG DOSBox 0.74-2 with dosbox_darksun2.conf and dosbox_darksun2_single.conf (machine=svga_s3, memsize 16, cycles fixed 15000, sbtype sb16), game started by RAVAGER.BAT
---

## Observation

The owner captured the first conversation of a new game in Tyr while its first responses were on
screen, with DOSBox's own screenshot command, as a 320x200 PNG, `dsun_008.png`, XXH3-128
`ba00faaf1ac7aa3f88df64f8847f8548`, kept outside the repository. The owner confirmed that it
shows that conversation.

Frames were decoded as FMT-IMAGE-002 and coloured through `RESOURCE.GFF#PAL/1000`, each 6-bit
component `c` widened to `(c << 2) | (c >> 4)`. Counting the frame's drawn pixels whose colour
equals the capture's at the placement given:

| Image | Placement | Pixels equal |
|---|---|---|
| `RESOURCE.GFF#BMP/12003`, 320 x 200 with 18,516 drawn pixels | (0, 0) | 13,047 |
| the same | (0, 140) | 15,575 |
| `RESOURCE.GFF#BMP/12002`, 243 x 47, 11,421 drawn | (75, 6) | 10,041 |
| the same | (75, 146) | 9,866 |
| `RESOURCE.GFF#ICON/12104`, 302 x 10, 2,416 drawn | (3, 153) | 1,754 |
| `ICON/12105` | (3, 161) | 2,180 |
| `ICON/12106` | (3, 169) | 1,615 |
| `ICON/12107` | (3, 177) | 1,708 |
| `ICON/12108` | (3, 185) | 2,220 |
| `GPLDATA.GFF#PORT/18`, 72 x 72, 5,169 drawn | (0, 0) | 5,169 |

`ICON/12104` matches best at (3, 153) of all placements. `ICON/12102` at (305, 4) and (305, 144)
matches 1 of 98 pixels, and `ICON/12100` at (305, 18) and (305, 158) none of 221. The pixels that
differ in the matched images are covered by text.

## Interpretation

The speech window, `RESOURCE.GFF#WIND/12500`, is drawn with its origin at (0, 0): its edit box
`EBOX/12400` sits at (75, 6) and shows its image `BMP/12002` there. The response window,
`WIND/12501`, has its origin at (0, 140): its five 300 x 10 rows at `y` 13 to 45 put their icons
at (3, 153) to (3, 185), 8 pixels apart. `BMP/12003` is drawn once for each window, at (0, 0)
and at (0, 140). The portrait `PORT/18` is drawn at (0, 0). The two windows' scroll buttons are
not drawn with their icons in this frame.

## Alternatives

The lower matches of `BMP/12002` at (75, 146) come from the texture `BMP/12003` draws there; no
edit box of `WIND/12501` names `BMP/12002`. That the rows' icons are drawn with their first frame
was assumed; other frames were not compared. Which script chose the portrait is a matter for the
conversation rules.

## How to reproduce

Decode the images named above, colour them with `PAL/1000`, and count, for each placement, the
drawn pixels whose colour equals the capture's. Search all placements of `ICON/12104` for the
best.
