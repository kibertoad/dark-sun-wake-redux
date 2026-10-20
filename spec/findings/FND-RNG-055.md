---
id: FND-RNG-055
title: DATA 293 reaches the cast routine with its byte 0x1A set only as the mode-0 special attack of a combatant whose special attack byte is 6, or cast back by a target whose byte is 13; no enemy list, psionic list, shipped item or shipped script supplies it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080580..0x00080617
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005F5ED..0x0005F724
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008451A..0x00084610
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092FF1..0x00093037
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000625D8..0x00062603
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000655E4..0x00065602
tool: tools/research/exec-census/direct_callers.py, overlay_listing.py, item_words.py, data_bytes.py and gff_tag_numbers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. FND-RNG-053 finds that the cast routine overlay
193 `+003C` reseeds through overlay 204 `+0000` for `DATA` 293 only when its byte argument at
`bp+0x1A` is not 0. `direct_callers.py <dsun> 193+003C 193+0000 193+2290 176+0196 177+0492
204+18CD 179+12B1 179+1304` gives `+003C` the near call in `+0000` and the cast queue's far call at
overlay 179 `+125C`, and `+0000` the near call at `+22CF` in the spell dispatcher `+2290` and far
calls at overlay 176 `+03BD` and `+048A`, overlay 177 `+04FE` and overlay 204 `+19AF`. `+0000`
passes its eleven argument words to `+003C` unchanged, the number third and the byte eleventh.

**The dispatcher** `+2290` takes twelve argument words, the number `d` third (`bp+0xA`). For `d`
of 328 or more it returns 0. When its byte at `bp+0x1A` is not 0, or `d` is 269 or more, it calls
`+0000` with `d` and with its byte at `bp+0x1C` as `+0000`'s byte at `bp+0x1A`; otherwise it sends
`d` below 235 to overlay 177 `+0492`, which calls `+0000` with that `d` (`+04D8..+0503`), and `d`
from 235 to 268 to overlay 176 `+0196` as `d - 235` (`+2290..+2327`). Overlay 176 `+03BD` adds
235 back and `+048A` passes 235 plus its counter 0 to 33 (FND-PARTY-122). So only the dispatcher
passes 269 and above, and its byte at `bp+0x1C` decides the reseed. The eleven far calls of
`+2290` push, as the words at `bp+0x1C` and `bp+0x1A`:

| Call | Number | `bp+0x1C` | `bp+0x1A` |
|---|---|---|---|
| overlay 172 `+0DE7` | the last-cast table at `DS:445E`, less 1 | 0 | 0 |
| overlay 173 `+29AC` | an item's power, word `+0x10` less 1 | 1 | 1 |
| overlay 174 `+0099` | the enemy spell choice | 1 | 0 |
| overlay 174 `+07A8` | 235 plus an enemy psionic pick | 1 | 0 |
| overlay 174 `+0900` | overlay 198 `+0149` of the caster's special attack byte, mode 0 | 1 | 0 |
| overlay 189 `+076A` | `DS:43F5` | 0 | 1 |
| overlay 189 `+331A` | an item's power | 0 | 1 |
| overlay 208 `+179F` | `DS:9BE2` | 1 | the byte at `DS:9BD3` |
| overlay 211 `+0901` | `DS:43F5` | 0 | 0 |
| overlay 211 `+166E` | its `SI` | 0 | 0 |
| overlay 213 `+0A4F` | an item's power | 0 | 1 |

FND-PARTY-242 and FND-PARTY-121 read the numbers: the 13 enemy spell lists hold nothing above 230,
the psionic picks are 234 to 268, and overlay 198 `+0149` returns 293 only for special attack 6 in
mode 0 (FND-PARTY-157). `item_words.py <archive> 294` over `OBJEX.GFF` (708 item chunks) and
`CHARSAVE.GFF` (172) finds no item whose word `+0x10` is 294; `DARKSAVE.GFF` and `DARKRUN.GFF`
hold no item chunk. Overlay 204 `+19AF` passes a script's fourth argument modulo 1000 for function
46 with 9999, and none of the shipped calls gives 293 (FND-PARTY-237, FND-PARTY-241).

**The special attack.** Overlay 174 `+084D` (called from overlay 173 `+162B`) takes an object,
gets `+07ED`'s number, which is -1 when the object's combatant byte `+0x0E` is 0 and otherwise
overlay 198 `+0149` of that byte and mode 0, and returns 0 for -1. Otherwise it calls `+0181` with
the number and two locals by address and returns 0 when that gives 0; then it calls the
dispatcher at `+0900` with the object as caster, -1, the number, the locals scaled by 16 plus 8,
and bytes 1 and 0; and then overlay 198 `+0549` with the object, which stores 0 in byte `+0x0E`
for special attacks 6 and 17 (FND-PARTY-113) (`+084D..+0924`).

**The queue.** The callers of overlay 179 `+12B1` and `+1304` pass the numbers 305, 307, 124, 308,
311 and -1, overlay 198's modes 1 to 4, which never give 293 (FND-PARTY-157), and overlay 197
`+1202`'s `SI` (FND-PARTY-107, FND-RNG-051); overlay 179 `+131C`, in `+1304`, passes 307 when the
queue is full (`+1304..+1322`). Overlay 197 `+111A`, called from the hit routine with the hit's
number, reaches `+1202` when the target's combatant byte `+0x0E` is 13 (`+11AC`), the number is not
-1, the `DATA` record's byte `+0x17` is -1 (`+11C3`) and its word `+1` is above -2 (`+11D5`), the
number was not negated on entry (`+11DC`) and `roll_sum(1, 6)` is 6 (`+11E2..+11F5`). It then
queues the same number with the target as the new caster. `data_bytes.py <resource> 1`, `2` and
`17` give `DATA` 293 word `+1` as 0 and byte `+0x17` as -1. A queued cast always has byte 1
(FND-RNG-053).

## Interpretation

In the shipped data the cast of `DATA` 293 that reseeds the generator comes from a combatant whose
special attack byte is 6, when overlay 174 `+084D` uses its special attack: it casts 293 once and
the byte is then cleared, so the combatant does it at most once until the byte is set again. A
target whose special attack byte is 13 casts a 293 that hit it back one time in six, and that
cast reseeds too. The party's own routes carry byte 0, except the target cursor of overlay 208,
whose number comes from the party's chosen spell, psionic power or item.

## Alternatives

- Which combatants have special attack 6 or 13 was not read. The `CHAR` records are the only
  `CHAR` resources (`gff_tag_numbers.py` finds them only in `CHARSAVE.GFF`) and hold 0
  (FND-PARTY-113); script function 49 never names field 18 in the shipped scripts; how a
  monster's combatant record gets its byte `+0x0E` was not read.
- The numbers overlay 208 `+1612`'s callers store at `DS:9BE2`, and the values of `DS:43F5` and
  overlay 211's `SI`, were not followed past FND-PARTY-242.
- Calls through stored pointers other than the trampolines were not searched.
- No emulated call or run was made; the overlays are outside the resident harness.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, `<dsun>` the installed
`DSUN.EXE`, `<resource>` the installed `RESOURCE.GFF`: `direct_callers.py <dsun> 193+003C 193+0000
193+2290 176+0196 177+0492 204+18CD 179+12B1 179+1304`; `overlay_listing.py <dsun> 172 59430
5A6FE`, `173 5A920 5EA7C`, `174 5EE00 5FB41`, `176 61540 62067`, `177 62100 6300E`, `179 642E0
66F95`, `189 749B0 77F3A`, `193 7E2F0 80ECE`, `197 83400 86F67`, `198 87310 878CC`, `208 92EA2
93088`, `211 95E10 97F00` and `213 99260 9AB3A`, reading the pushes before each call above;
`item_words.py <archive> 294` for `OBJEX.GFF`, `CHARSAVE.GFF`, `DARKSAVE.GFF` and `DARKRUN.GFF`;
`data_bytes.py <resource> 1`, `2` and `17`; `gff_tag_numbers.py <install> CHAR`. Outputs are kept
under `GAME_DIR/analysis/rng-293-20261020/`. Nothing was executed.
