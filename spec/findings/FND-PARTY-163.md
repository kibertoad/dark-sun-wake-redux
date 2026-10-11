---
id: FND-PARTY-163
title: The saving throw modifier overlay 197 +0206 adds 2 against fire and lightning effects for a target with a druid level from overlay 176 +076C, which counts a human's held-back former druid class
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00083606..0x0008369C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000838E9..0x00083A04
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066E59..0x00066E8E
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/direct_callers.py, overlay_listing.py; a scratch script that prints the word at +0x1A and the name at +0x20 of DATA resources read through the GFF directory as details_chunks.py reads a directory)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 179 `+2A46`, the saving throw, takes a slot `t` and adds to its roll the
result of overlay 197 `575A:00A7` with `t` as the first argument (`+2B79..+2BA9`, FND-PARTY-099).
`direct_callers.py <dsun> 197+0206` finds that trampoline and one far call, at overlay 179
`+2B9E`.

**Overlay 197 `+0206`** keeps its first argument in `si` and a sum in `di`, starting from 0
(`+020E..+0211`). It stores the word at `+0x1A` of `DATA` `n`, `n` being its argument at `bp+0x14`
read through `2C5F:0003`, in `bp-4` (`+0288..+0299`). When that word has a bit of 0x82
(`+04E9..+04F0`), it calls overlay 176 `+076C` (trampoline `5691:0061`) with `si` and each of the
class codes 5, 6, 7 and 8, takes the greatest result, and adds 2 to `di` when it is above 0
(`+04F3..+05CF`). It then calls `27E5:00C1` with `si` and 12 and adds 2 when the result is above 0
and the word at `+0x11` of `DATA` `n` has a bit of 6 (`+05D2..+0601`). It returns `di`, or -100 on
one branch (`+0E84..+0E8E`).

Overlay 176 `+076C` returns the level of the first class position holding the code, with no test
of the origin byte, and `27E5:00C1` returns 0 for a human's former class whose level is not below
the current class's (FND-PARTY-162).

**The effects.** Of the `DATA` resources below 1000 of `RESOURCE.GFF`, 28 have a bit of 0x82 in
the word at `+0x1A`: with 0x202, `BURNING HANDS` (1), `FLAMING SPHERE` (14), `FIREBALL` (27),
`MINUTE METEORS` (33), `FIRE SHIELD` (42), `WALL OF FIRE` (54 and 207), `DELAYED BLAST FIREBALL`
(83), `FINGER OF DEATH` (84), `INCENDIARY CLOUD` (93), `METEOR SWARM` (105), `FOCUS HEAT` (174),
`PRODUCE FIRE` (180), `FLAME STRIKE` (198), `FIRE SEEDS` (209), `FIRESTORM` (222) and `MONS296`;
with 0x222, `FLAMING ARROW` (28); with 0x280, `SHOCKING GRASP` (10), `LIGHTNING BOLT` (32) and
`CHAIN LIGHTNING` (69); with 0xFFFF, `PRISMATIC SPHERE` (109); and with 0x2, `MONS278`, `MONS282`,
`MONS291`, `MONS297`, `MONS298` and `MONS309`.

## Interpretation

A target with a druid level gets 2 added to its saving throw against fire and lightning
effects. The level comes from overlay 176 `+076C`, so a dual-classed human whose former class is
druid gets the bonus even while its current class's level is not above the druid level, when the
other routines hold the former class back (FND-PARTY-162). The bonus for a psionicist level goes
through `27E5:00C1` and is held back.

## Alternatives

- Whether bit 0x02 marks fire and bit 0x80 lightning rests on the names of the records that
  carry them; no table of the bits was read.
- The other calls of overlay 176 `+076C` in overlays 174, 197, 204 and 208 were not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `direct_callers.py <dsun> 197+0206`,
`overlay_listing.py <dsun> 197 83606 836A0`, `overlay_listing.py <dsun> 197 838E9 83A04`,
`overlay_listing.py <dsun> 197 84250 8428F` and `overlay_listing.py <dsun> 179 66E50 66EC0`. For
each `DATA` resource below 1000 of `RESOURCE.GFF`, print the word at `+0x1A` when it has a bit of
0x82, with the 32 bytes at `+0x20`.
