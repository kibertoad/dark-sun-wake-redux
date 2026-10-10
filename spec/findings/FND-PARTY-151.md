---
id: FND-PARTY-151
title: Byte 0x7D of a CHAR record selects one of 13 spell lists from which overlay 177 +0059 fills a caster's prepared spells, byte 0x7C is the footprint and byte 0x7B has no reader found
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062159..0x0006237F
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py, field_stores.py, overlay_listing.py, direct_callers.py; a scratch script reading the CHAR resources through the GFF directory as details_chunks.py does)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A load copies a CHAR record's bytes `0x7B` to `0x7D` to `+0x36` to `+0x38` of the
character's FMT-COMBAT-002 record (FND-PARTY-051), whose byte `+0x37` is the object's footprint
(FND-EXPLORE-002).

`field_reads.py <dsun> <inventory> --es --table 19c5 36 37 38` finds, after a multiplication by
0x42, no read of `+0x36`, reads of `+0x37` at `2D40:347E` and overlays 188 and 197, and reads of
`+0x38` at overlay 177 `+0159`, `+0194`, `+01DF`, `+0BFB`, `+0C66`, `+0D36`, `+0D84`, `+0DCB`,
`+0E70` and `+0EB7`. `field_stores.py <dsun> <inventory> --es 36 37 38` finds no store to them
through `DS:19C5`.

**Overlay 177 `+0059`** takes a slot in `SI`. It calls `+0372` for an index and returns when that
is 0xFF; it clears the 0x1E bytes at `4D62:0002 + 0x21 * index` (`+0064..+008E`). It takes the
current hit points of the slot's combatant record, times 256, over the word at `+0x08` of its
details record (the greatest hit points), and for each level from 1 to 9 stores the count `+05B7`
returns with 3 and the level, and that count scaled by the hit point ratio (`+0091..+0132`). It
then reads byte `+0x38` of the slot's details record and returns when it is 13 or more
(`+0134..+0164`). Otherwise it takes the far pointer at `529C:0014 + 4 * byte` as a list of words
ending in -1 (`+0194..+01B5`), and for each of up to 15 words whose level from `+09C5` is 9 or less
and whose level still has a scaled count above 0 it stores 1 in the word at `4D62:0002 + 0x21 *
index + 2 * position`, lowers the count and adds 1 to the word, repeating the list while it sets one
(`+01BA..+0278`). `direct_callers.py <dsun> 177+0059` finds it called from overlay 177 `+004F`
and through the trampoline `5699:006B`.

**The shipped records.** In the 19 `CHAR` resources of the installed `CHARSAVE.GFF`, byte `0x7B`
is 1 in eleven records, 4 in resources 50 to 53 and 0 in resources 30 to 33; bytes `0x7C` and
`0x7D` are 0 in all.

## Interpretation

Details byte `+0x38`, CHAR byte `0x7D`, names the spell list of a combatant that casts without a
spell book of its own: overlay 177 `+0059` prepares spells from list 0 to 12, as many per level as
the caster's slots allow and fewer as it is wounded. Every shipped character has list 0, which
matters only if the routine runs for a party slot. Byte `0x7C` is the footprint, 0 in every
shipped character record. Nothing found reads byte `0x7B`.

## Alternatives

- The 13 far pointers at `529C:0014` were not followed to their lists, and whether they are set
  at run time was not read; the bytes the load image holds at their targets were not checked as
  lists of spell numbers.
- What `+0372`'s index numbers, and whether overlay 177 `+0059` runs for party slots, was not read.
- `+09C5` was taken as giving a spell's level from its use with the levels 1 to 9.
- Byte `0x7B` may be read through a computed address or a copy of the record.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: in
`tools/research/exec-census/`, run `field_reads.py <dsun> <inventory> --es --table 19c5 36 37 38`,
`field_stores.py <dsun> <inventory> --es 36 37 38`, `overlay_listing.py <dsun> 177 62159 62380` and
`direct_callers.py <dsun> 177+0059`. Read bytes `0x7B` to `0x7D` of each `CHAR` resource of
`CHARSAVE.GFF`.
