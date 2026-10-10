---
id: FND-PARTY-147
title: The word at 0x53 of a CHAR record is the key of the character's details record, which is 0 for a free record and which generation sets to the new combatant identifier
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006ED7D..0x0006EE5E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E181..0x0006E1A9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00074AB9..0x00074B2E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065C22..0x00065C52
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2D1C..2D40:2D78
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py, field_stores.py, overlay_listing.py, resident_listing.py, details_chunks.py; scratch scripts reading the CHAR resources of CHARSAVE.GFF and the details chunks of OBJEX.GFF as details_chunks.py does)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A load copies a CHAR record's word at `0x53` to the word at `+0x0E` of the
character's FMT-COMBAT-002 record (FND-PARTY-051), and the word at `0x10` to the FMT-COMBAT-001
record's word at `+6` (FND-PARTY-049, FND-PARTY-050).

**Generation.** Overlay 184 `+1CED`, which the generation finish calls (`+10E4`), counts a word
up from 0x8015 to below 0x8300 until it finds one that no combatant record of slots 0 to 3 holds at
`+6` and that none of 40 calls of `38FF:04AB` with the tag `CACT` returns; it stores that
word at `4E4F:0037`, at `4E4F:0071`, at `+0x0E` of the slot's details record and at `+6` of its
combatant record (`+1D12..+1DAB`). When `DS:112E` is -1, overlay 184 `+1102..+1113` stores 0 in the
slot's state byte at `4F49:0C33` and at `+0x0E` of its details record.

**Reads.** `field_reads.py <dsun> <inventory> --es --table 19c5 e` finds 20 reads in the
inventory, all `cmp word ptr es:[bx + 0xe], 0` after a load of `DS:19C5` and a multiplication
by 0x42, in overlays 176, 189, 190, 191, 207, 210 and 213. At overlay 189 `+0109..+0137` the
slot in `4E71:0B44` is printed by the name of the record at `DS:142D` when the word is not 0 and as
`INACTIVE CHARACTER` (`DS:1AB2`) when it is 0. Outside the inventory, `2D40:2D38` and
`2D40:2D3F` are the search FND-PARTY-051 reads in `2D40:2C87`: for a slot above 4, the details
records 5 to 16 are searched for one whose word at `+0x0E` equals the word at `+0x0E` of the data
being loaded, and else the last one whose word is 0 is taken (`2D40:2D1C..2D40:2D78`). Overlay 179
`+1952` reads the word for a struck combatant's details record and compares it with the six words
at `cs:28A9`, 7020, 7045, 7046, 7047, 7050 and 7051, jumping to one of three cases
(`+1942..+196E`).

**The shipped records.** In 14 of the 19 `CHAR` resources of the installed `CHARSAVE.GFF` the
word at `0x53` equals the word at `0x10`, which runs from 0x8001 to 0x801B in them. In the other
five the word at `0x10` is 0x82FD to 0x8300 and the word at `0x53`, read as signed, is -21
(resource 31), -14 (33), -7039 (50), -7036 (51) and -7038 (53).

**OBJEX.** Of the 375 details chunk headers in the installed `OBJEX.GFF` that `details_chunks.py`
finds, 353 in `RDFF` resources, the signed word at `+0x0E` of the data is between -7111 and -7007
in all, with 93 distinct values; -7007 occurs 35 times and -7033 29 times. No resource of any
installed GFF archive is numbered 7000 to 7120.

## Interpretation

The word at `+0x0E` of a details record, CHAR word `0x53`, is the record's key. A details record
with key 0 is free, and a party slot whose details record has key 0 holds no character. A
character made in generation gets the new combatant identifier as its key, so the two words
agree. Creatures loaded beyond the party share a details record when their keys agree, and the
creature records of `OBJEX` use keys from -7111 to -7007, many shared, as a key of the creature's
kind would be. The five shipped characters whose keys differ from their identifiers are of the
kind that was not made by generation: their identifiers are at the top of the range generation
leaves out.

## Alternatives

- What the -7000 range numbers, and why overlay 179 compares the key with positive values in that
  range, was not read; a creature loaded in play may carry a positive key.
- Which routine reaches overlay 184 `+1102` with `DS:112E` at -1, and so when a slot is emptied,
  was not traced.
- The reads in overlays 176, 190, 191, 207, 210 and 213 were not each read for what they skip.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: in
`tools/research/exec-census/`, run `overlay_listing.py <dsun> 184 6ED7D 6EE5E`, `overlay_listing.py
<dsun> 184 6E160 6E1B0`, `field_reads.py <dsun> <inventory> --es --table 19c5 e`,
`field_stores.py <dsun> <inventory> --es e`, `overlay_listing.py <dsun> 189 74AB0 74B30`,
`resident_listing.py <dsun> 2D40:2C87..2D40:2E10` and `overlay_listing.py <dsun> 179 65C10 65C52`,
and read the twelve words at file `0x66B89`. Read the words at `0x10` and `0x53` of each `CHAR`
resource of `CHARSAVE.GFF`, and the word at `+0x0E` of the data of each details chunk that
`details_chunks.py` finds in `OBJEX.GFF`.
