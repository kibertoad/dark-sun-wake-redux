---
id: FND-UI-017
title: The notice shown before the first conversation uses the speech window alone, with an empty portrait
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

The owner captured the frame shown just before the first conversation of FND-UI-016, a notice of
an experience award, as `dsun_007.png`, XXH3-128 `44c51599dde4c5ea4231e2a3ffd5be28`, kept
outside the repository.

With the images coloured as in FND-UI-016, `RESOURCE.GFF#BMP/12003` at (0, 0) matches 13,277 of
its 18,516 drawn pixels and at (0, 140) 228. `RESOURCE.GFF#BMP/12002` at (75, 6) matches 10,329
of 11,421. The portrait well at the left of the upper panel is empty, and no response rows are
drawn.

## Interpretation

The notice uses the speech window at (0, 0) with its text box, and no response window.

## Alternatives

Which window resource the notice uses is inferred from the matching images; `WIND/12500` and
`WIND/12502` both place an edit box with `BMP/12002`, at (75, 6) and (56, 6), and the match at
(75, 6) points to `WIND/12500`. The script path that produces the notice is not known.

## How to reproduce

Compare `BMP/12003` at (0, 0) and (0, 140) and `BMP/12002` at (75, 6) with the capture as in
FND-UI-016.
