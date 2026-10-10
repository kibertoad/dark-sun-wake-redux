---
id: FND-PARTY-113
title: The 19 CHAR records of CHARSAVE.GFF hold 0 in bytes 0x18 and 0x19, which a load copies to bytes +0x0E and +0x0F of a party member's combatant record, and the two stores found to those bytes through DS:19C9 leave 0 in place or have no caller
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087859..0x000878AA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000878AA..0x000878CC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E35D..0x0008E5D7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0008E5D7..0x0008E60D
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x34..0x36
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x149..0x14B
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x27F..0x281
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x352..0x354
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x52F..0x531
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x72B..0x72D
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x81F..0x821
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xAE1..0xAE3
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xD86..0xD88
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xECF..0xED1
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x106A..0x106C
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1140..0x1142
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x13AD..0x13AF
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x14AE..0x14B0
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x16C3..0x16C5
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x197C..0x197E
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1B60..0x1B62
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x21CB..0x21CD
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x24CF..0x24D1
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_stores.py, data_bytes.py, overlay_listing.py, resident_listing.py, direct_callers.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A `CHAR` load copies a record's bytes `0x0A` to `0x3A` to bytes `0x00` to `0x30` of the
slot's FMT-COMBAT-001 record (FND-PARTY-049), so `CHAR` bytes `0x18` and `0x19` become the record's
bytes `+0x0E` and `+0x0F`, which overlay 197 `+111A` and `+1C52` test (FND-PARTY-112).

**The archive.** `data_bytes.py`, with its resource tag changed from `DATA` to `CHAR`, reads the
19 `CHAR` resources of the installed `CHARSAVE.GFF` (29 to 43 and 50 to 53, FND-PARTY-050): the
bytes at `0x18` and at `0x19` are 0 in all 19.

**The stores.** `field_stores.py <dsun> <inventory> --es E F` lists 33 stores through ES at
displacement `0x0E` or `0x0F`, and `--es D` one, which starts inside another instruction. Those
whose routine forms the pointer from `DS:19C9` and the FMT-COMBAT-001 size 0x31 are:

- overlay 198 `+0549` (trampoline `5768:0034`, called far from overlay 174 `+0917`), which reads
  byte `+0x0E` of the record of its argument and switches on it less 1, bounded by 16, through 17
  words at `cs:059A`: for 1 to 4 it stores 0x12, for 6 and 17 it stores 0, and for any other
  value, 0 among them, it stores nothing (`+0549..+0599`);
- overlay 204 `+259D` (trampoline `5787:00BB`), which, for an object whose byte at
  `4F49:0C33 + 3 * object` is 2, compares a dword argument with nine dwords at `cs:2817` (0, 13,
  14, 18, 19, 23, 25, 32 and 33) and for 18 stores its byte argument at `+0x0E` and for 19 at
  `+0x0F` (`+259D..+2816`). `direct_callers.py` finds no caller other than the trampoline, and
  the file holds neither the bytes `9A BB 00 60 06` (a far call through descriptor 204's segment
  word) nor `BB 00 60 06` or `BB 00 87 57` (a far pointer to the trampoline), nor the word
  `0x259D` in an immediate or displacement.

The other hits take their pointer from `DS:19C1` (overlays 173, 190 and 197), `DS:19C5`
(overlay 184), segment `03B0` (overlay 193) or segment `52A2` (`31E0:3388`), or from arguments
and locals of resident routines that do not read `DS:19C9`: `31E0:0E1B`, `31E0:31F7`,
`31E0:3568`, `31E0:3A66`, `37FC:077C`, `38FF:02B5` and `3BA6:00D1`.

## Interpretation

A party member loaded from the shipped `CHAR` records has 0 in bytes `+0x0E` and `+0x0F` of its
combatant record, and the two stores found to those bytes through `DS:19C9` do not change that:
overlay 198 `+0549` leaves 0 alone, and overlay 204 `+259D` has no caller the searches find. Type
0 has the type word 0, so overlay 197 `+1C52` gives such a member 100 percent, and none of the
record-byte tests of FND-PARTY-112 ends the hit for it.

## Alternatives

- Block copies into the combatant records (string moves or a call of `1000:0452`) other than the
  `CHAR` load, and the `CHAR` records a new or imported character gets, were not searched; either
  could give a party member another type.
- The callers of the seven resident routines above, which could pass a pointer to a combatant
  record, were not followed.
- A far call to overlay 204 `+259D` through a computed offset is outside the byte searches.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `data_bytes.py` with `tag == "DATA"` changed to
`"CHAR"`, on `<install>/CHARSAVE.GFF` with offsets `18` and `19`; `field_stores.py <dsun>
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --es E F` and `--es D`, reading each hit's routine for the
source of its pointer; `overlay_listing.py <dsun> 198 87859 878AA` and `204 8E35D 8E5D7`; read the
17 words at file `0x000878AA` and the nine dwords and nine words at file `0x0008E5D7`;
`direct_callers.py <dsun> 198+0549 204+259D` and the routines above; and search the file for the
byte strings above and `immediate_search.py <dsun> 259D`.
