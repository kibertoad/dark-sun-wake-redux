---
id: FND-UI-020
title: The View Character screen draws BMP 11000 at (0,9) and its title BMP 20079 at (56,11)
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

Four owner captures, each 320x200, kept outside the repository, which the owner confirmed show
the View Character screen of each of the four party members in turn:

| Capture | XXH3-128 |
|---|---|
| `dsun_021.png` | `90e55ced95c1ead838eccae46a845c7c` |
| `dsun_022.png` | `29ec0ffae615b45ec73c1dd6fcda1aaf` |
| `dsun_023.png` | `e7e06fd2ba22b58cfb88747188fff9cc` |
| `dsun_024.png` | `249d9f193c50a4f2ee7c27996e03b314` |

With the images coloured as in FND-UI-016, `RESOURCE.GFF#BMP/11000` matches best at (0, 9) in
all four, 34,559, 35,047, 34,581 and 34,725 of its 47,609 drawn pixels. `BMP/20079` (210 x 23,
4,759 drawn) matches 4,747 pixels at (56, 11) in all four.

## Interpretation

The View Character screen draws `BMP/11000` at (0, 9) and its title `BMP/20079` at (56, 11).

## Alternatives

Earlier notes placed the title at (55, 0) over the base at (0, 0). Whether the View Character screen uses
`RESOURCE.GFF#WIND/11500`, whose buttons match the manual's navigation icons, is not shown by
these captures; its buttons were not compared.

## How to reproduce

As in FND-UI-019, for `BMP/11000` and `BMP/20079`.
