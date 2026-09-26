---
id: FND-UI-019
title: The Cast and Effects screens draw BMP 11000 at (0,9) and their titles at y 11
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

Six owner captures, each 320x200, kept outside the repository:

| Capture | XXH3-128 | What the owner confirmed it shows |
|---|---|---|
| `dsun_020.png` of an earlier session | `e271811bb763bc5a50fbd5e6ce1c0003` | the screen the `C` key opens, first party member selected |
| `dsun_021.png` of an earlier session | `6187665d5e4135103d7a20ae35340dd7` | the screen the `E` key opens, same member |
| `dsun_013.png` | `baadb84dc7bc46f3e17ec2a185da4cf1` | the spell or psionics screen of the first party member |
| `dsun_014.png` | `7d6830defd079e9bdcd5fb6d4fe282db` | the same screen, second member |
| `dsun_015.png` | `7b2b954ea6566b16220f4e787aa33a56` | the same screen, third member |
| `dsun_016.png` | `fd3ceb8a3c0ce814b1b5ee7aaee3266c` | the same screen, fourth member |

With the images coloured as in FND-UI-016, `RESOURCE.GFF#BMP/11000` (320 x 200, 47,609 drawn
pixels) matches best, over x from -4 to 4 and y from -4 to 15, at (0, 9) in all six: 37,325,
39,062, 37,364, 34,597, 35,201 and 38,224 pixels in the order of the table. At (0, 0) it matches
10,996 to 12,581.

`BMP/20080` (104 x 23, 2,321 drawn) matches 2,318 pixels at (109, 11) in the first capture and in
`dsun_013.png` to `dsun_016.png`. `BMP/20075` (152 x 23, 3,425 drawn) matches 3,419 at (85, 11) in
the second capture.

## Interpretation

The Cast Spells and Use Psionics screen and the Current Spell Effects screen draw `BMP/11000` with
its top-left at (0, 9), and their titles `BMP/20080` and `BMP/20075` at (109, 11) and (85, 11).
The pixels of `BMP/11000` that differ are covered by the screens' contents.

## Alternatives

Earlier notes placed the titles at (108, 0) and (84, 0); those placements assumed the base at
(0, 0). Nothing here shows which window resource, if any, these screens use.

## How to reproduce

Decode the images, colour them with `PAL/1000`, and count equal pixels over the range of
placements for `BMP/11000` and over all placements for the two titles.
