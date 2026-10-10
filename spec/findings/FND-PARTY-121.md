---
id: FND-PARTY-121
title: The enemy spell choice of overlay 174 scores each spell by a weight byte that is 0 for DATA 104 and 225, its other path and the item powers pass other numbers, and only RDFF 3 has 104 in its spell list
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005BE09..0x0005BEBD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005EE00..0x0005EF67
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005F5ED..0x0005F668
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062159..0x0006237F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000624A4..0x00062592
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006260A..0x000626B7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062CC8..0x00062D36
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087310..0x0008753C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080580..0x000805C7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00077C1A..0x00077C40
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000999A9..0x000999CD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005A1FA..0x0005A21C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000750D5..0x0007511F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00047A70..0x00047C08
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00041980..0x00041A6B
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00438D3C..0x00438DBD
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/direct_callers.py, trampoline_target.py, overlay_listing.py, segment_references.py, enemy_spell_lists.py, details_chunks.py, item_words.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. `direct_callers.py <dsun> 193+2290` finds the spell dispatcher's trampoline
`573B:0089` and eleven far calls, in overlays 172, 173, 174 (three), 189 (two), 208, 211 (two) and
213. The dispatcher takes a caster, a second object and a `DATA` number `d`.

**Overlay 174 `+0000`** (trampoline `5682:0020`) takes an object, a count and a near list of
spell numbers. It calls `+00B6` with them, and with the number that returns, when it is not
negative, calls the dispatcher with the object, -1 and that number (`+0000..+00B6`). **`+00B6`**
starts with -1 as its choice and 0 as its best score; for each spell in the list it skips one for
which, for an object below 4, overlay 197 `+300D` and overlay 177 `+0BC8` both return 0, and
scores the rest through `+012F`; a spell whose score is above the best becomes the choice
(`+00B6..+012F`). **`+012F`** returns the product of `1000:0822` and the signed byte at
`4C78:0000 + spell`, divided by 0x8000 (`+012F..+0167`). `segment_references.py <dsun> 4C78`
finds the segment table's row for `4C78`, which gives it 0xEB bytes, and the two reads in `+012F`
and `+0167`, and no other reference. `enemy_spell_lists.py <dsun> 104,105,225` gives the byte 0 for
spells 104, 105 and 225; the bytes for 66 to 114 are all 0.

**Its caller.** Overlay 173 `+1595` is `+0000`'s one caller. It passes the object `si` and the
list and count that overlay 177 `+0400` fills for `si`, with the far pointer to the list, kind 3
and level -1, which take the spells 0 to 234 that overlay 177 `+050A` accepts (`+14E9..+159D`,
FND-PARTY-110). For an object of 4 or more `+050A` returns whether overlay 177 `+0BC8` of the
object and the spell is above 0 (`+050A..+05B7`). **`+0BC8`** takes the byte at `+0x38` of the
object's FMT-COMBAT-002 record, finds the spell's index in list `b` of that byte through `+03A4`,
and returns the word at `4D62:0002 + 0x21 * r + 2 * index`, `r` being the row overlay 177 `+0000`
gives the object, or 0 when the spell is not in the list (`+0BC8..+0C36`). **`+03A4`** returns -1 for a byte
of 13 or more, and otherwise the index of the spell among the first 15 words of the far pointer at
`529C:0014 + 4 * b`, stopping at -1 (`+03A4..+0400`). **`+0059`**, which overlay 177 `+0000`
calls for an object without a row, sets the words of the row from the same list, each level's share being
overlay 177 `+05B7` of the object, 3 and the level, scaled by the object's hit points over its
greatest (`+0059..+027F`). `enemy_spell_lists.py <dsun>` gives the 13 lists: spell 104 is in list
1 only (8, 32, 27, 43, 58, 63, 69, 73, 83, 90, 93, 105, 104, 114), and no list holds 225.
`details_chunks.py <install>/OBJEX.GFF --byte 38` gives byte 0x38 of the 375 details chunks: 1 for
`RDFF` 3 alone, named Lord Warrior, with class codes 19 and 18 at levels 15 and 18; 0, which names
the empty list 0, for 334 chunks, among them `RDFF` 64, 65 and 67 (FND-PARTY-116); 2 for
`RDFF` 172 and one other; and 3, 4, 6, 7, 8 or 11 for the rest. In `CHARSAVE.GFF` the byte is 0
in all 22.

**Overlay 174 `+0900`**, in `+084D` (trampoline `5682:0048`, called from overlay 173 `+162B`),
passes the number `+07ED` returns for its object (`+084D..+0868`). `+07ED` returns -1 when byte
`+0x0E` of the object's 49-byte record is 0, and otherwise overlay 198 `+0149` of that byte and 0
(`+07ED..+084D`). **Overlay 198 `+0149`** switches on its byte less 1 and on its second argument
and returns -1 or one of the immediates 27, 270, 284, 286, 287, 288 to 295, 297, 298, 300, 301,
303, 304, 318 and 319 (`+0149..+022C`). Overlay 198 `+00EF` and `+013D`, which FND-PARTY-107
lists as passing a computed number to overlay 179 `+12B1`, pass what `+0149` returns for modes 1
to 4 (`+0000..+0149`).

**The item powers.** Overlay 173 `+29AC` (FND-PARTY-108), overlay 189 `+331A`, in `+326A`, and
overlay 213 `+0A4F` pass as `d` the word at `+0x10` of a 23-byte record of the table at `DS:19C1`,
less 1; when the word is 0, `+326A` returns without the call and the code before `+0A4F`
shows `NOTHING HAPPENS` (`189 +326A..+3290`, `213 +0749..+076D`). `item_words.py` over every `.GFF` file of the
installation finds 23-byte item chunks only in `OBJEX.GFF` (708) and `CHARSAVE.GFF` (172), and
none holds 105 or 226 at `+0x10`.

**The party's casts.** Overlay 172 `+0DE7` passes the slot at `DS:445C` as caster and as second
object (`+0DCA..+0DEC`).
Overlay 189 `+076A` passes the slot at `4E71:0B44` and the word at `DS:43F5` (`+0725..+076F`).
Overlay 193 `+22CF`, in the dispatcher, passes its own `d` on to overlay 193 `+0000` for `d` from 269
to 327, or below 328 when its byte argument at `bp+0x1A` is not 0 (`+2290..+22D7`), and overlay 177 `+04FE`, in `+0492`, is reached only from the dispatcher
at overlay 193 `+22F1`.

## Interpretation

An enemy casts a spell from its list only through overlay 174, which keeps a spell only when its
score is above 0, and the weight byte of spells 104 and 225 is 0, so no enemy casts them by that
choice, whatever its list or its level; the same holds for a party member that the choice runs for.
The other enemy path picks from the numbers of overlay 198 `+0149`, none of them 104 or 225, and no
shipped item record carries the power of either. Only `RDFF` 3 has spell 104 in its list, and the
Preservers of FND-PARTY-116 have list 0 or 2, which holds neither spell. Byte 0x38 of an
FMT-COMBAT-002 record selects an enemy's spell list.

## Alternatives

- The weight table is read through segment `4C78` only, and the search finds no write; a write
  through a pointer formed from another segment would change the result.
- The computed arguments of overlay 197 `+1202` (an object number from overlay 197 `+111A`, called
  from overlay 174 `+04D2` and overlay 179 `+0EC0`), overlay 204 `+19AF` and overlay 176 `+03BD`
  and `+048A`, from FND-PARTY-107, were not traced.
- An item record built at run time with 105 or 226 at `+0x10` was not looked for beyond the stores
  of FND-PARTY-108's search.
- Scripts that send code 59 are Q-PARTY-051.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<install>` the installation folder, run in `tools/research/exec-census/`:
`direct_callers.py <dsun> 193+2290 174+0000 174+084D 177+0492 197+111A`; `trampoline_target.py
<dsun> 5699:0052 5699:0048 5768:0020 575A:007F`; `segment_references.py <dsun> 4C78 529C`;
`enemy_spell_lists.py <dsun> 104,105,225`; `details_chunks.py <install>/OBJEX.GFF --byte 38` and
the same for `CHARSAVE.GFF`; `item_words.py` with `105,226` over each `.GFF` file; and
`overlay_listing.py` over the ranges above.
