---
id: FND-PARTY-162
title: The class flags, the class level query 27E5:00C1 and the spell list routines of overlay 177 ignore a human's former class until the current class's level is higher, while overlay 176 +076C, which overlays 174, 197, 204 and 208 call, has no such test; overlay 190 offers DUAL to a human of level 2 or more
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:00C1..27E5:0154
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000626B7..0x000627A1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000628AF..0x00062990
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062C00..0x00062CC4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061CAC..0x00061D2E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079259..0x00079294
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079479..0x000794BF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097DC5..0x00097E50
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py, direct_callers.py, overlay_listing.py, resident_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A details record is 0x42 bytes at the far pointer `DS:19C5`, with the class bytes at
`+0x1B` to `+0x1D`, the level bytes at `+0x1E` to `+0x20`, one per class position, and the origin
byte at `+0x12`, 1 for a human. A class change puts the new class at position 0 and moves the
earlier ones up (FND-PARTY-161).

**Reads.** `field_reads.py <dsun> <inventory> --es --table 19c5 1b 1e` lists the reads of the
class and level bytes, and `--table 19c5 12` those of the origin byte. Among the routines that
read a position other than 0, those that also test the origin byte for 1 are `27E5:000F`,
`27E5:00C1`, `27E5:0154`, overlay 177 `+05B7`, `+07AF` and `+0B00`, and overlay 210's level
routines (FND-PARTY-081).

**The resident routines.** `27E5:000F` ORs a class's flag into the word at `+0x10` for position 0
and for each later position whose level is above 0 and, for a human, below position 0's level
(FND-PARTY-085). `27E5:00C1` takes a slot and a class code and returns the level of the first
position holding the code; for a human it returns 0 when that position is not 0 and position 0's
level is not above the position's level (`+00E3..+014F`, FND-PARTY-081). `direct_callers.py <dsun>
27E5:00C1` finds 12 far calls: the load image at `0x1EF0B`, overlay 173 `+2D9E` and `+34F4`,
overlay 197 `+05D5`, `+1B61` and `+2631`, overlay 209 `+0017` with code 11 (the Preserver spell
gate, FND-PARTY-104) and `+0A0F` with code 12 (the Psionicist level window, FND-PARTY-084),
overlay 210 `+01CC` with code 12 (the psionic points, FND-PARTY-081), and overlay 211 `+1FDC`,
`+200E` and `+202A`. Overlay 211 `+1FB5` takes, for a spell number below 0xEB, the greatest result
of `27E5:00C1` over the class codes 0 to 19 for which overlay 177 `+0A45` (trampoline `5699:0020`) accepts the spell, less 7
for codes 13 to 16; for a number below 0x10D the result for code 12; and for 0x132 the greatest
over codes 1 to 4 (`+1FBD..+2050`).

**Overlay 177.** `+05B7` loops over the class positions `k` of a party slot's details record. It
passes over a position whose class byte is 0, and over a position `k` other than 0 of a human whose
level is not below position 0's level (`+05FD..+0698`). `+07AF` does the same (`+07EB..+0882`), and
`+0B00`, which returns 1 when `+0A45` accepts the spell for the class of some position, the same
(`+0B0A..+0BBD`, FND-PARTY-110). FND-PARTY-110 and FND-PARTY-111 read `+05B7` and `+07AF` as the
counts of spells per level and `+0A45` as the class test of the cast list.

**Overlay 176 `+076C`** (trampoline `5691:0061`) takes a party slot `s` and a class code `c`. For
positions 0 to 2 of the details record that the word at `+0x04` of the slot's combatant record
names, the combatant being the word at `4F49:0C34 + 3 * s`, it returns
the level byte of the first position whose class byte is `c`; when none is, it returns 1 for `c`
of 12 and 0 otherwise (`+076C..+07ED`). It does not read the origin byte. `direct_callers.py <dsun>
176+076C` finds three near calls in overlay 176 and far calls from overlay 174 `+0793`, about 140
from overlay 197 (`+04F6` to `+0E20` and `+1270`), 33 from overlay 204 (`+08B0` and `+0D6C` to
`+0F37`) and two from overlay 208 (`+0611`, `+0B54`). At overlay 197 `+04F3..+0560`, when the word
at `bp-4` has a bit of 0x82, it takes the greatest of the results for codes 5 to 8, then for 7
and 8, and compares them.

**Overlay 190.** At `+0F19..+0F54` the View Character window labels a button `DUAL` when the
level byte at `+0x1E` of the slot in `4E71:0B44` is at least 2 and the origin byte is 1, and
`CANCEL` otherwise. At `+1139..+117D` the same two tests lead to a call of the trampoline
`57B0:0043`, which enters overlay 209 `+123E`, the DUAL window, with that slot.

## Interpretation

For a human, the game counts a former class only once the current class's level is higher than
the former class's: its flag in the word at `+0x10`, its level in `27E5:00C1`, and its spells in
the counts and in the cast list. A former class at the same level as the current one counts as
not held. So a human dual-classed from a level 5 preserver casts no preserver spells and gets no
preserver flag until the new class reaches level 6. Overlay 176 `+076C` returns a former class's level whatever the current one;
overlay 197 uses it for a level per class group in its hit routines. Whether those uses can reach
a former class the other routines hold back was not read.

The DUAL button needs only a human of level 2 or more in the current class; the class tests are
in the DUAL window (FND-PARTY-161), which shows `NO OTHER CLASSES AVAILABLE` when none passes.

## Alternatives

- The calls of `27E5:00C1` in the load image and overlays 173 and 197, and what overlays 174, 197,
  204 and 208 do with `176 +076C`'s result, were not read.
- A read of the class or level bytes through a copy of the record, another pointer or code
  outside the inventory is outside the search (toolkit issues 468 and 477).

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: in
`tools/research/exec-census/`, run `field_reads.py <dsun> <inventory> --es --table 19c5 1b 1e` and
`--table 19c5 12`; `direct_callers.py <dsun> 27E5:00C1 176+076C`; `resident_listing.py <dsun>
27E5:00C1..27E5:0154`; and `overlay_listing.py <dsun>` with 177 `626B7 627A1`, 177 `628AF 62990`,
177 `62C00 62CC4`, 176 `61CAC 61D2E`, 197 `838C0 83980`, 190 `79259 79294`, 190 `79479 794BF`, 209
`93160 93180`, 209 `93B62 93B78`, 210 `95110 95170` and 211 `97DC5 97E50`.
