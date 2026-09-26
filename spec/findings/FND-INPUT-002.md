---
id: FND-INPUT-002
title: Six captures show the Walk, ranged-attack and Look pointers and their invalid versions
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

The owner took six captures at the opening camera in Tyr, one for each pointer mode over a place
where it could act and one where it could not, with DOSBox's own screenshot command, as 320x200
PNG files kept outside the repository. Each of `ICON/19101` to `ICON/19110`, coloured with
`PAL/1000` as FND-INPUT-001 describes, was compared at every placement in each capture, counting
its drawn pixels whose colour equals the capture's. In each capture exactly one image matches all
its drawn pixels:

| Capture | XXH3-128 | What the owner confirmed it shows | Image | Top-left at | Pixels equal |
|---|---|---|---|---|---|
| `dsun_000.png` | `bf9c6c06cb7a756308a55bd52f2d10a4` | Walk, over a place the party can reach | `ICON/19101` | (194, 116) | 73 of 73 |
| `dsun_001.png` | `9b7b6e95e90cf78e6fd02796e4190c05` | Attack, over a place with nothing to attack | `ICON/19106` | (194, 116) | 207 of 207 |
| `dsun_002.png` | `af1ab7d011ed0b90c153e14c631233e2` | Attack, over the first hostile character | `ICON/19105` | (94, 54) | 55 of 55 |
| `dsun_003.png` | `ed4a049245900bc9a707c3113b7685fd` | Look, over a place with nothing to look at | `ICON/19108` | (188, 66) | 213 of 213 |
| `dsun_004.png` | `603a1030c6627c16397427d27f01a638` | Look, over a character | `ICON/19107` | (202, 49) | 178 of 178 |
| `dsun_005.png` | `1c3d6f3f92118f69cbeb713e2c573257` | Walk, over a place the party cannot reach | `ICON/19102` | (136, 123) | 211 of 211 |

No other image matches all its pixels anywhere in these captures. `ICON/19103` and `ICON/19104`
match at most 18 of their 88 and 221 pixels in any of them.

## Interpretation

The game draws the pointer image with its top-left pixel at the pointer's position. In Attack
mode over the first hostile character the pointer is the ranged-attack image, and over empty
ground its invalid version; the hand-to-hand pair does not appear in these captures.

## Alternatives

Earlier notes described the two Attack captures as showing the hand-to-hand pair, and resolved
the object under the pointer in the valid one to the first hostile character's object definition;
the image matches show the ranged pair, and the object lookup was not repeated. Why the game chose
the ranged image, whether from the party leader's readied weapon or the distance to the target,
is not shown.

## How to reproduce

Decode the ten images named above, colour them with `PAL/1000`, and for each capture count, at
every placement, the drawn pixels whose colour equals the capture's.
