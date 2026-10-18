---
id: FND-PARTY-242
title: No route by which an enemy or a script reaches the hit routine passes DATA 313, so the item 313 strike can come only from a number a party member's own cast or item power supplies
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005F404..0x0005F5AD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F598..0x0007F798
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008CD47..0x0008CDCD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00047110..0x000478EF
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000479D0..0x00047A62
    kind: file-data
tool: tools/research/exec-census/hit_item_tables.py, overlay_listing.py, direct_callers.py, segment_references.py, immediate_search.py, enemy_spell_lists.py and item_words.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. The hit routine overlay 179 `+0D2F` sends effect code 59 for its item number 313 on
an attack roll of 20 (FND-PARTY-096). FND-PARTY-102, FND-PARTY-107 and FND-PARTY-122 list what
each caller passes as that number: fixed numbers other than 313, -1 from the fight's swing,
overlay 197 `+1202` repeating the running hit's number (or 0 or -1 from overlay 174), overlay 176
235 to 268, a script's function 46, and four computed sources read here.

**Overlay 193 `+13EC`**, reached near from `+1359` and `+148E`, passes the `DX` of overlay 193
`+12A8`, which loads it from the word at `+0` of the 18-byte row `51F1:0000 + 18 * si`, `si` being
its second argument (`+12AC..+12E6`). `hit_item_tables.py` reads the 112 rows from `51F1:0000` up
to `51F1:07E0`, where reads of other fields at `+0x7E0` begin: the words are -1 and DATA numbers
from 1 to 311, with no 313. `segment_references.py <dsun> 51F1` finds 72 references, and none of
the listed instructions around them stores through `ES`.

**Overlay 204 `+12B4`** passes its first argument (`+1307`). `direct_callers.py` finds only its
seven near calls, `+0F94` to `+100A`, which push the immediates 148, 157, 159, 148, 157, 159 and
178.

**The spell dispatcher overlay 193 `+2290`** passes its `d` to overlay 193 `+0000` for `d` from
269 to 327 (FND-PARTY-108). `direct_callers.py <dsun> 193+2290` finds 11 far calls:

- overlay 174 `+0099`, the enemy spell choice, passes a number from one of the 13 enemy lists
  (FND-PARTY-121). `enemy_spell_lists.py` prints them: none holds a number above 230;
- overlay 174 `+0900` passes one of overlay 198 `+0149`'s immediates, none of them 313
  (FND-PARTY-121);
- overlay 174 `+07A8`, in `+0604`, which returns at once for an object below 4, passes 235 plus
  `[bp-2]` (`+079D..+07A8`). `+06B6` stores in `[bp-2]` a byte picked at random from the list the
  far pointer at `5284:0003 + 4 * k` gives, `k` being byte `0x40` of the object's record less 1,
  from 0 to 7 (`+0634..+06B6`); nothing else stores `[bp-2]` before the call.
  `hit_item_tables.py` reads the eight lists: the numbers they give are 235 to 268, and list 0 is
  empty, which gives 234. `segment_references.py` finds the lists' segments named only by the
  pointer table and the segment table, and segment `5284` read by overlays 174 and 198;
- overlay 173 `+29AC`, overlay 189 `+331A` and overlay 213 `+0A4F` pass an item's power, the
  word at `+0x10` of its 23-byte record less 1 (FND-PARTY-121). `item_words.py <install>/OBJEX.GFF
  314` and the same over `CHARSAVE.GFF` find no item chunk with 314;
- overlay 172 `+0DE7` passes a number from the word table at `DS:445E`, 17 words for each party
  member at `0x22` bytes, less 1 (`+0C72..+0C8A`). Overlay 190 `+2503` writes that table: it
  shifts a member's entries and stores the number at `DS:6273` plus 1;
- overlay 189 `+076A` and overlay 211 `+0901` pass `DS:43F5`, which `immediate_search.py <dsun>
  43F5` finds stored at overlay 211 `+16ED` from its `SI` and at overlay 213 `+0AE2` from its
  `[bp-4]`, and overlay 211 `+166E` passes its `SI`. Overlays 189 and 211 compare `DS:43F5`
  with 235, 269 and 328;
- overlay 208 `+179F` passes `DS:9BE2`, which overlay 208 `+1612` stores from its third argument.
  `direct_callers.py <dsun> 208+1612` finds its six far callers in overlays 172, 189, 211 (three)
  and 213. The dispatcher calls in overlays 172 and 211, overlay 189 `+076A` and overlay 213
  `+0A4F` pass the party member at `DS:445C` or `4E71:0B44` as the caster.

## Interpretation

No enemy action and no shipped script gives the hit routine the number 313: the swing passes -1,
the enemy spell choice and the enemy psionic choice pass numbers below 269, the other enemy path
passes overlay 198's fixed numbers, the area rows hold no 313, and no shipped item has the power
313. So no enemy that can fight the party strikes with the item of `DATA` 313. A hit with 313 can
still come from a number the party's own routines supply: the last-cast table at `DS:445E`, the
chosen number at `DS:43F5` and the routines of overlays 211 and 213 that set it.

## Alternatives

- An item record built while the game runs, or one in a saved game made by a later game, with
  314 at `+0x10` is outside the archive search.
- Stores into the area rows or the psionic lists through a pointer formed elsewhere are outside
  the reference search.
- What `DS:6273`, overlay 211's `SI` at `+16ED` and `+166E` and overlay 213's `[bp-4]` hold was
  not read.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: `hit_item_tables.py <dsun> 313`; `enemy_spell_lists.py <dsun>`; `direct_callers.py
<dsun> 193+2290 204+12B4 208+1612`; `segment_references.py <dsun> 51F1` and `5284 527D 527E 527F
5280 5281 5282 5283`; `immediate_search.py <dsun> 43F5 445E`; `item_words.py
<install>/OBJEX.GFF 314` and `<install>/CHARSAVE.GFF 314`. List with `overlay_listing.py <dsun>`
overlay 174 `5F404 5F5AD`, overlay 193 `7F598 7F798`, overlay 204 `8CD47 8CDCD` and `8D074
8D0E0`, overlay 172 `5A051 5A21C`, overlay 189 `750D5 7511F`, overlay 190 `7A843 7A8E2`, overlay
208 `92EA2 92EED` and `92FF1 93034`, overlay 211 `966C4 96716`, `97429 97483` and `974EB 97501`,
overlay 189 `77C90 77CCF` and overlay 213 `99C8A 99CB4` and `99CF1 99D45`. The outputs are retained under
`GAME_DIR/analysis/hit-item-313-20261018/`. No original function or game process was run.
