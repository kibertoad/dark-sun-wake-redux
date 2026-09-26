---
id: FND-INPUT-001
title: ICON 19101 to 19110 are ten one-frame pointer images
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x1A375..0x1ADA6
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`RESOURCE.GFF#ICON/19101` to `ICON/19110` (FMT-IMAGE-001) lie one after another from `0x1A375`.
Each holds one frame:

| Resource | Size | Drawn pixels |
|---|---|---|
| `ICON/19101` | 10 x 13 | 73 |
| `ICON/19102` | 16 x 16 | 211 |
| `ICON/19103` | 16 x 17 | 88 |
| `ICON/19104` | 16 x 17 | 221 |
| `ICON/19105` | 14 x 15 | 55 |
| `ICON/19106` | 16 x 16 | 207 |
| `ICON/19107` | 14 x 15 | 178 |
| `ICON/19108` | 16 x 16 | 213 |
| `ICON/19109` | 16 x 17 | 213 |
| `ICON/19110` | 13 x 15 | 185 |

Decoded with `PAL/1000`, they show in order: an arrow; the arrow under a crossed circle; a sword;
the sword under the crossed circle; an arrow shaft with fletching; that shaft under the crossed
circle; an eye; the eye under the crossed circle; a glowing orb under the crossed circle; and an
hourglass.

## Interpretation

These are the pointer images the manual pictures (SRC-MANUAL-1994, pages 4 and 5): Walk and Can't
Walk, the hand-to-hand attack and its invalid version, the ranged attack and its invalid version,
Look and its invalid version, a spell that cannot be cast, and the hourglass shown while the game
is busy. Each image's top-left pixel is where the manual says to aim.

## Alternatives

The roles of `ICON/19109` and `ICON/19110` come from the manual's descriptions alone; no capture
shows them.

## How to reproduce

Read the ten resources through the directory, decode each frame, and colour it with `PAL/1000`.
