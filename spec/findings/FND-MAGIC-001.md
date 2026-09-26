---
id: FND-MAGIC-001
title: The Use screen names the spell class and level or the psionic discipline its icons belong to
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

The four owner captures that show the Use screen (Cast Spells and Use Psionics, SCR-UI-009) of
each party member in turn, the same captures as FND-UI-019, each 320x200 and kept outside the
repository, in the game of FND-PARTY-020:

| Capture | XXH3-128 | Member | Captions | Icons in the upper panel |
|---|---|---|---|---|
| `dsun_013.png` | `baadb84dc7bc46f3e17ec2a185da4cf1` | first | MAGE, LEVEL 1 | 4 |
| `dsun_014.png` | `7d6830defd079e9bdcd5fb6d4fe282db` | second | CLERIC, LEVEL 1 | 12 |
| `dsun_015.png` | `7b2b954ea6566b16220f4e787aa33a56` | third | CLERIC, LEVEL 1 | 11 |
| `dsun_016.png` | `fd3ceb8a3c0ce814b1b5ee7aaee3266c` | fourth | PSIONIC, Metabolic | 1 |

The captions are two boxes in the bottom bar of the screen. The members are those of
FND-PARTY-020: a preserver, psionicist and thief; a cleric; a fighter and druid; and a gladiator.
All four were above level 1 in their classes when the captures were taken. In the second and
third captures two of the icons are drawn darker than the rest, and in the fourth the one icon
is. The second capture also shows one icon in the lower panel.

## Interpretation

The Use screen shows one group of spells or powers at a time and names it in the two boxes: the
spell class (MAGE for a preserver, CLERIC for a cleric or a druid) and the spell level, or
PSIONIC and the discipline. LEVEL 1 is a spell level, since every member's class levels are
higher. The fourth member's discipline is psychometabolism, which the screen labels Metabolic.

## Alternatives

What the darker icons mean (spells with no slot left, as the manual describes for its orange X,
or something else) and what the lower panel holds are not shown. Which group the screen opens on,
and how the player moves to another, are not shown by single captures. Earlier notes gave the
third member's captions as MAGE and LEVEL 1; the capture shows CLERIC.

## How to reproduce

Hash each capture to find it and read the two caption boxes of the bottom bar and the icons of
the upper panel.
