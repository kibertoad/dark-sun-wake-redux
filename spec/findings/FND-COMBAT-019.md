---
id: FND-COMBAT-019
title: In the capture of an enemy striking, the panel shows the enemy's name with ???/??? and a number 11 is drawn over the struck figure
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

The owner capture `dsun_011.png`, 320 x 200, XXH3-128 `e95ae1fcdb152c23667ecadf449f86ae`, kept
outside the repository, from the same fight as FND-COMBAT-018. The owner labels it as an enemy
striking, with combat damage being inflicted.

At (215, 4) `RESOURCE.GFF#BMP/19003` matches 2,594 of its 3,098 opaque pixels, and the others lie
in (243, 8) to (284, 31), where the panel shows four centred lines: `Draxan`, `???/???`, `Okay`
and `Move : 9`. Over the group of figures in the middle of the street a red and black splash is
drawn with the number `11` in white on it; the pure red pixels of the area from (140, 80) to
(195, 125) lie between (151, 94) and (189, 118). The party and the attackers stand in one group.

## Interpretation

During an enemy's turn the panel shows the enemy's name, question marks in place of its two
numbers, its condition and its movement count. A hit shows the damage done as a number over the
target.

## Alternatives

The legacy record read the fourth line as `Moves 20`; enlarged, it reads `Move : 9`. That the
number is the damage, and that it is 11, rests on the owner's label; which figure was struck, and
by whom, cannot be told from the frame.

## How to reproduce

Find the capture by its hash, compare `BMP/19003` at (215, 4) as in FND-COMBAT-018, and list the
pixels of the area whose colour is pure red.
