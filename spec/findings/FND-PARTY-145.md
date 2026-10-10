---
id: FND-PARTY-145
title: Byte 0x66 of a CHAR record is the character's base armor class, from which overlay 173 +2C00 computes the value an attack roll is compared against
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D520..0x0005D6F0
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py, field_stores.py, overlay_listing.py; a scratch script reading the CHAR resources through the GFF directory as details_chunks.py does)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A load copies a CHAR record's bytes `0x66` to `0x68` to `+0x21` to `+0x23` of the
character's FMT-COMBAT-002 record (FND-PARTY-051), and the generation finish copies `+0x21` of the
details record to `+0x12` of the combatant record (FND-PARTY-070), which a load fills from CHAR
byte `0x1C` (FND-PARTY-049).

**Overlay 173 `+2C00`** takes a slot as `SI`. It starts a value at `bp-4` from 10; for a slot
whose state byte at `4F49:0C33` is 1 it uses 5 and returns, for any state other than 1 or 2 it
uses -99 and returns, and for state 2 it takes the signed byte at `+0x21` of the slot's details
record (`+2C0C..+2C7E`). For state 2 it then goes through the slot's records from `2D40:2196`,
adding values from some of them to `bp-4` (`+2C88..+2D62`), subtracts the result of `27E5:00C1`
from `bp-6`, adds the result of overlay 197 `+27A7`, and returns the sum of `bp-4` and `bp-6`
(`+2D9E..+2DCF`). FND-PARTY-091 finds the attack routine overlay 173 `+2560` hitting on a roll
that is at least the THAC0 less bonuses less this routine's value for the target.

`field_reads.py <dsun> <inventory> --es --table 19c5 21 22 23` finds the read at overlay 173
`+2C76` and, of `+0x22` and `+0x23`, reads in overlay 197 only. `field_stores.py <dsun>
<inventory> --es 12` lists, of stores to `+0x12` of a combatant record, only the finish's at
overlay 184 `+10C3`.

**The shipped records.** Byte `0x66` is 10 in 12 of the 19 `CHAR` resources of the installed
`CHARSAVE.GFF`, 5 in resources 32, 34, 35, 36 and 42, 0 in resource 51 and 7 in resource 53. The
origin byte at `0x57` is 8, thri-kreen, in the six resources 32, 34, 35, 36, 42 and 51, and in no
other. Byte `0x1C` equals byte `0x66` in 15 records; it is 6 and 9 where `0x66` is 10 in
resources 31 and 33, and 10 in resources 51 and 53.

## Interpretation

Details byte `+0x21`, CHAR byte `0x66`, is the character's base armor class: 10 for most characters
and 5 for most thri-kreen. Overlay 173 `+2C00` computes the armor class of a
combatant from it, adjusted by the slot's records and a dexterity-like term, and an attack uses
it as the target value. The combatant copy at `+0x12` (CHAR `0x1C`) is set from the base at the
finish; that it differs in four shipped records shows something else changes it, through a store
this search does not find.

## Alternatives

- Which records from `2D40:2196` add to the value and by how much, and what `27E5:00C1` and
  overlay 197 `+27A7` return, were not read; the reading of them as items and dexterity rests on
  the shape of the computation.
- What CHAR bytes `0x67` and `0x68`, and the combatant byte `+0x12` in play, hold was not settled.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: in
`tools/research/exec-census/`, run `overlay_listing.py <dsun> 173 5D520 5D6F0`, `field_reads.py
<dsun> <inventory> --es --table 19c5 21 22 23` and `field_stores.py <dsun> <inventory> --es 12`.
Read bytes `0x1C` and `0x66` of each `CHAR` resource of `CHARSAVE.GFF`.
