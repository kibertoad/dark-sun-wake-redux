---
id: FND-TALK-004
title: The first conversation's capture shows the menu title and the labels of entries 0, 1, 2, 3 and 7 of GPL 135's first menu, in that order
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: DOSBox screenshot read by eye against strings decoded with Python 3.14.7
environment: GOG DOSBox 0.74-2 with dosbox_darksun2.conf and dosbox_darksun2_single.conf (machine=svga_s3, memsize 16, cycles fixed 15000, sbtype sb16), game started by RAVAGER.BAT
---

## Observation

`dsun_008.png`, XXH3-128 `ba00faaf1ac7aa3f88df64f8847f8548`, is the owner's capture of the first
conversation of a new game in Tyr while its first responses are on screen (FND-UI-016). Its text
was compared with the strings of kind 5 in `GPLDATA.GFF#GPL/135` and `#MAS/99` (FMT-SCRIPT-002),
decoded outside the repository:

- The speech box shows the string at offset 122 of `GPL/135`, the one the `0x4F` at 118 prints
  (FND-TALK-002).
- The lower window shows six lines. The first is the string `MAS/99` assigns to global string 4,
  drawn in capitals although the string has lower-case letters. The next five are, from the top,
  the labels of entries 0, 1, 2 and 3 of the menu at offset 253 of `GPL/135`, whose strings start
  at offsets 257, 299, 319 and 365, and the string `MAS/99` assigns to global string 5, which is
  entry 7's label. The first four start one character further right than the fifth, and each of
  their strings starts with two spaces.
- The labels of entries 4, 5 and 6, whose strings start at 406, 456 and 499, are not shown.

## Interpretation

The menu instruction (FND-TALK-001) draws its title as row 0 and each offered entry below it in
the order the entries appear in the script. At this point entries 0 to 3 and 7 are offered and 4
to 6 are not: local flags 0 to 3 are 1, local number 0 is not 2, and local flags 5 and 9 are 0.

## Alternatives

Which part of the game turns the title into capitals is not known: the row routine of overlay
188, the font, or a style the title row is drawn with.

## How to reproduce

Decode the strings named above as FMT-SCRIPT-002 describes and compare them with the capture.
