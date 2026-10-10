---
id: FND-PARTY-150
title: Byte 0x68 of a CHAR record is the character's magic resistance, a percentage that overlay 179 +2BD7 rolls 1d100 against
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084AB5..0x00084B60
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066EB7..0x00066EFA
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py, field_stores.py, overlay_listing.py, resident_listing.py, direct_callers.py; a scratch script reading the CHAR resources through the GFF directory as details_chunks.py does)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A load copies a CHAR record's byte `0x68` to `+0x23` of the character's
FMT-COMBAT-002 record (FND-PARTY-051).

`field_reads.py <dsun> <inventory> --es --table 19c5 23` finds one read of `+0x23` after a
multiplication by 0x42, at overlay 197 `+16E7`; `field_stores.py <dsun> <inventory> --es 23`
finds no store through `DS:19C5`.

**Overlay 197 `+16B5`** takes a slot, a number and a byte. It reads the signed byte at `+0x23` of
the slot's details record into `bp-2` (`+16C2..+16EC`) and calls overlay 193 `+1BA3` for the
slot's effect flags. When the flag at bit 0 of `bp-0xA` is set and the word at `+0x11` of the
record `2C5F:0003` returns for the number has a bit of 0x26 set, it adds 75 (`+16FC..+1720`). It
returns 0 when the value is 0. When bit 5 of `bp-0xB` is set it halves the value; when the number
is 0x40 it subtracts 30 plus the byte argument; it returns the value, raised to 0 when below it
(`+1724..+175F`). `direct_callers.py <dsun> 197+16B5` finds one caller, overlay 179 `+2C03`.

**Overlay 179 `+2BD7`** returns 0 when the byte at `DS:45AA` is not 0 and the byte at `DS:43CE` is
0. Otherwise it calls `28C9:391D` with 1 and 100, which sums one random number from 1 to 100,
calls overlay 197 `+16B5` with its three arguments, and returns 1 when the roll is below the value
`+16B5` returns, else 0 (`+2BD7..+2C19`). `direct_callers.py <dsun> 179+2BD7` finds one caller,
overlay 197 `+12DF`, one of the overlay 197 calls before a hit's saving throw (FND-PARTY-112).

**The shipped records.** Byte `0x68` is 0 in all 19 `CHAR` resources of the installed
`CHARSAVE.GFF`.

## Interpretation

Details byte `+0x23`, CHAR byte `0x68`, is the character's magic resistance in percent. A spell
or power aimed at a combatant is resisted when 1d100 falls below it, after the adjustments of
overlay 197 `+16B5`: 75 points more under one effect against some spells, half under another, and
30 plus the caster's byte less against the spell or power numbered 0x40. No shipped character has
any, and the search found no store to it, so a character in the party has none unless a loaded creature
record supplies it.

## Alternatives

- Which effects bits 0 of `bp-0xA` and 5 of `bp-0xB` are, what bits 0x26 of the word at `+0x11` of
  a spell's record mark, and what DATA number 0x40 is, was not read.
- That the byte argument is the caster's level rests on its use with 30, as in the rule for magic
  resistance against a caster of higher level; its source in overlay 197 `+12DF` was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: in
`tools/research/exec-census/`, run `field_reads.py <dsun> <inventory> --es --table 19c5 23`,
`field_stores.py <dsun> <inventory> --es 23`, `overlay_listing.py <dsun> 197 84AB5 84B60`,
`direct_callers.py <dsun> 197+16B5 179+2BD7`, `overlay_listing.py <dsun> 179 66EB7 66EFA` and
`resident_listing.py <dsun> 28C9:391D..28C9:3960`. Read byte `0x68` of each `CHAR` resource of
`CHARSAVE.GFF`.
