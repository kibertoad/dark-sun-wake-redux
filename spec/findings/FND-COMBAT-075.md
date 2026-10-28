---
id: FND-COMBAT-075
title: The difficulty word DS:143A sets a loaded creature's greatest hit points to 50, 100, 150 or 200 percent, rounded down, for details records above 4, and the CHAR load then sets the current hit points of every slot from 4 up to that maximum; the other readers are the save and load of PREF, the Preferences screen and script function 21
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007037C..0x000703DE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007028E..0x00070325
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0328..2D40:0399
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D6FB..0x0007D70B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D97E..0x0007D984
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008B610..0x0008B6C8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008B8D9..0x0008B922
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF29..0x0008BF4E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C487..0x0008C4F1
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C226..0x0008C22D
tool: tools/research/exec-census/immediate_search.py, overlay_listing.py, resident_listing.py, record_base_accesses.py and script_decode.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 187's code starts at file `0x6FE30`,
192's at `0x7D300`, 203's at `0x8B240` and 204's at `0x8BDC0`. Combatant records are the 49-byte
records under `DS:19C9` (FMT-COMBAT-001, word `+0` `hit_points`, word `+4` `details_index`, byte
`+0x14` `combat_mark`) and details records the 66-byte records under `DS:19C5` (FMT-COMBAT-002,
word `+0x08` `max_hit_points`).

`immediate_search.py <dsun> 143A` lists 21 instructions. Besides overlay 173's to-hit read at
file `0x5C9C9` (FND-COMBAT-065) and the writes of FND-CONFIG-026, the reads are these.

**Overlay 187 `+039A`, state 3** (`+054C..+05AE`, file `0x7037C..0x703DE`). This is the copy of a
chunk into the details table for a slot whose state byte at `4F49:0C33 + 3 * slot` is 3
(FND-PARTY-051, FND-ACTOR-015). After copying the 0x42 data bytes to record `r`, the number
`2D40:2C87` returned, it skips the rest when `r` is 4 or less (signed, `+056A..+056D`). Otherwise
it zero-extends the record's word `+0x08` to 32 bits, sign-extends `DS:143A` to `d`, multiplies
the word by `(d - 1) * 50 + 100` as signed 32-bit numbers, divides by 100 with `idiv` after
`cdq`, and stores the low 16 bits of the quotient back in word `+0x08` (`+057A..+05AA`). The
search's hit at file `0x703B1`, which FND-COMBAT-065 names, starts one byte inside the
instruction at `0x703B0`, the 32-bit `movsx` the listing decodes; the search lists both.

**Overlay 187 `+039A`, state 2** (`+045E..+04F5`, file `0x7028E..0x70325`). The combatant chunk
is copied as it is into record `r` of `DS:19C9`, including word `+0`. The routine then stores
9999 in words `+4`, `+0x0A`, `+0x08` and `+0x0C`, and 1 in byte `+0x14` when that byte is 0. It
does not read `DS:143A`.

**`2D40:000A`, after the chain** (`2D40:0328..2D40:0399`). When the slot `di` it was called with
has state byte 2: when the combatant's word `+4` is 9999 or more it sets the result to -1 and
calls `2D40:2410` with the slot (`2D40:0354..+0368`); otherwise, when `di` is 4 or more (signed),
it copies word `+0x08` of the details record that word `+4` names into the combatant's word `+0`
(`2D40:036A..+0396`). `record_base_accesses.py <dsun> <inventory> --source 19C5 8` lists this
read and the write at overlay 187 `+05AA`.

**Overlay 192 `+03FB` and `+05ED`** (file `0x7D6FB..0x7D70B`, `0x7D97E..0x7D984`). `+03FB`
copies `DS:143A` and eight bytes into a local block before the `SAVING GAME` sequence writes it as
`PREF/100`; `+05ED` copies the word back from the `PREF/100` it reads (FND-SAVE-004,
FND-SAVE-005, FND-CONFIG-026).

**Overlay 203** (file `0x8B610..0x8B6C8`, `0x8B8D9..0x8B922`). The difficulty arrows' branch
`+03D0` adds 2 and falls into `+03D5`, which subtracts 1, so the harder arrow adds 1 net; both
clamp to 0 to 3. The reads that follow pass the word to overlay 190 `+2943` with 0x41 and 0x30040
(the bar), index the far-pointer table at `DS:26B9` by 4 times the word (the label) and the word
table at `DS:26C9` by 2 times it (the sound, only with `DS:13F7` and `DS:1435` set). `+0699..+06DA`
draws the bar and label the same way (FND-CONFIG-010, FND-UI-060).

**Overlay 204 `+0169`** (from file `0x8BF29`), script opcode 0x22's function dispatcher
(FND-PARTY-237), takes the function number less 1, returns for a value above 0x34, and jumps
through the 53 words at `cs:06C7` (file `0x8C487..0x8C4F1`). Word 20, for function 21, is
`0x0466`: `mov di, [143A]` and a jump to the common return `+06C1`, which returns `di`
(`+0466..+046D`). `script_decode.py --all` lists 47 calls of function 21 in 19 resources:
`GPL/5`, 16, 17, 21, 81, 123, 124, 126, 133, 139, 142, 164, 185, 187, 188, 200, 234 and 324,
and `MAS/67`. Most store the value, or the value plus 1 or 2, or twice it, in a local; `GPL/139`
compares it with 3 and `GPL/188` compares it with 2 ten times.

## Interpretation

The difficulty scales the greatest hit points of a creature when its details record is loaded:
Easy 50 percent, Balanced 100, Hard 150 and Hideous 200, rounded down, since the product is never
negative. At the end of the same load every slot from 4 up gets current hit points equal to that
greatest value, so a creature appears at full scaled hit points. Party slots 0 to 3 keep their
records 0 to 3 (`2D40:2C87` returns a slot of 4 or less as it is) and are never scaled. Slot 4
uses record 4, which is not scaled, but has its current hit points set to the maximum. The scale
is applied to the resource's value each time a record is filled, so it does not compound, and a
creature already loaded keeps what it was given when the setting changes, as SRC-GAMEFAQS-81038
reports. The test is on the record number, not on hostility: any creature loaded into a record
above 4 is scaled, ally or enemy.

Examples: a greatest value of 7 becomes 3, 7, 10 and 14 on the four settings; a value of 1 becomes
0 on Easy, and the load then gives the creature 0 hit points with `combat_mark` 1.

The executable has no other combat use of the word. Scripts read it through function 21 in 47
places, which may change encounters by difficulty; those scripts were not read.

## Alternatives

This supersedes FND-COMBAT-070 to correct its GPL/188 comparison count from nine to ten.
The unchanged executable observations are retained from that finding. A fresh bounded decode
of GPL/188 confirms comparison expressions at script offsets 0x001B, 0x0058, 0x0095,
0x00D2, 0x010F, 0x014C, 0x0189, 0x01C6, 0x0203 and 0x0240; each tests difficulty
less than 2. Four further reads at 0x0427, 0x065F, 0x0922 and 0x09DE assign it to
local 7. These counts describe decoded sites, not their execution frequency.

Retained alternatives:

- The stored quotient keeps its low 16 bits: a greatest value above 32,767 on Hideous, or above
  21,845 on Hard, would wrap. Whether any creature's resource holds such a value was not checked.
- That a creature "appears" through `2D40:000A` rests on its callers listed by
  `direct_callers.py` (FND-ACTOR-003, FND-ACTOR-005, FND-ACTOR-015). Whether a saved game's
  creatures are restored through it again, which would scale them a second time from the
  resource, or by a block copy of the tables was not read.
- `2D40:2410`, called when the combatant's word `+4` is still 9999, was not read.
- The 47 script calls of function 21 were found by the decoder, not followed.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`: `immediate_search.py <dsun> 143A`; `overlay_listing.py <dsun> 187 701CA 70435`,
`192 7D300 7DA80`, `203 8B5FE 8B6E0`, `203 8B8C0 8B940` and `204 8BDC0 8C600`;
`resident_listing.py <dsun> 2D40:000A..2D40:00D0 2D40:02A9..2D40:03B9`;
`record_base_accesses.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --source 19C5 8`; read the
53 words at file `0x8C487`; `script_decode.py <GPLDATA.GFF> <OBJEX.GFF> --all` and search the
output for `22 21,`.

The comparison-count correction is reproduced with 
script_decode.py <GPLDATA.GFF> <OBJEX.GFF> GPL 188; the all-resource census command
above retains the total of 47 difficulty-query sites. This correction used the locked
Capstone 5.0.9 environment; the unchanged executable readings retain their original provenance.
