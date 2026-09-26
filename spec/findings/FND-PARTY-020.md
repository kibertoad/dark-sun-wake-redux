---
id: FND-PARTY-020
title: The four View Character captures show a party whose names and scores match CHAR records 40, 41 or 53, 42, and 33 or 43
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

The four owner captures that show the View Character screen of each party member in turn, the
same captures as FND-UI-020, each 320x200 and kept outside the repository:

| Capture | XXH3-128 | Member |
|---|---|---|
| `dsun_021.png` | `90e55ced95c1ead838eccae46a845c7c` | first |
| `dsun_022.png` | `29ec0ffae615b45ec73c1dd6fcda1aaf` | second |
| `dsun_023.png` | `e7e06fd2ba22b58cfb88747188fff9cc` | third |
| `dsun_024.png` | `249d9f193c50a4f2ee7c27996e03b314` | fourth |

The owner took them in a game started with START GAME, after some play. Each shows the same four
members, in the same order, in the four portrait boxes and in the strip of four portraits along
the bottom; the member shown is the one whose box has a yellow frame. For each member the screen
draws a name, six values labelled STR, DEX, CON, INT, WIS and CHR, a line with the gender and
origin, a line with the alignment, and a line with one to three classes separated by slashes
above a line with a level for each:

| Member | Name | Six values | Gender, origin | Alignment | Classes | Matching records |
|---|---|---|---|---|---|---|
| first | Ar'Anda | 18 21 17 18 18 18 | female elf | chaotic neutral | Preserver, Psionic, Thief | 40 |
| second | Terrannus | 19 17 18 12 19 18 | male human | lawful good | Cleric | 41, 53 |
| third | Thy'rokh | 19 21 19 16 19 15 | female thri-kreen | true neutral | Fighter, Druid | 42 |
| fourth | Gerakis | 24 15 22 13 15 14 | male half-giant | chaotic good | Gladiator | 33, 43 |

The matching records are the `CHAR` records of the installed `CHARSAVE.GFF` whose name
(FND-PARTY-001) is the name drawn, ignoring case, and whose bytes `0x23..0x29` (FND-PARTY-003)
are the six values. Record 50 has the first member's name and a first score of 19, and record
51 holds the third member's scores under the name `Thy-rohk`, so neither matches. Records 41
and 53 have the same name and scores, and so do 33 and 43.

On the third member's screen, Druid and the level under it are drawn in red, while Fighter and
its level are drawn in the colour of the other members' class lines. The second member's Cleric
and its level are drawn in a darker yellow than the other members' classes.

Each member's screen also shows a PSI line with two values, for all four members.

## Interpretation

The party START GAME supplies is, in order, characters 40, 41 or 53, 42, and 33 or 43, which is
the set 40 to 43 that overlay 182 loads (FND-PARTY-013). The name the screen draws is the name
slot of the record, and the six values are its ability scores in the order of the labels.

## Alternatives

The captures do not tell 41 from 53 or 33 from 43; their other bytes were not compared with the
screen, because the play before the captures changed the experience, hit point and level values.
That the first member is a psionicist rests on the screen drawing Psionic in the class line.

## How to reproduce

Hash each capture to find it, read the name, the six values and the text lines from it, and
compare them with the names and bytes `0x23..0x29` of every `CHAR` record of the installed
`CHARSAVE.GFF`.
