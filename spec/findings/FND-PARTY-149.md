---
id: FND-PARTY-149
title: The words at 0x81 and 0x83 of a CHAR record are sound numbers that overlays 179 and 206 play for the combatant in an attack and in the damage routine
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006530E..0x00065347
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008EA40..0x0008EA6E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D125..0x0005D160
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py, field_stores.py, overlay_listing.py, direct_callers.py; a scratch script reading the CHAR resources through the GFF directory as details_chunks.py does)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A load copies a CHAR record's bytes `0x81` to `0x86` to `+0x3C` to `+0x41` of the
character's FMT-COMBAT-002 record (FND-PARTY-051). `2C5F:0A22` takes a byte `n` and plays the
`BVOC` resource `n`, or else the installed file `SOUND` `n` `.VOC` (FND-SOUND-007).

`field_reads.py <dsun> <inventory> --es --table 19c5 3c 3d 3e 3f 40 41` finds, after a
multiplication by 0x42, reads of `+0x3C` at overlay 179 `+1053` and overlay 206 `+1567` and
`+1655`, of `+0x3E` at overlay 206 `+0109`, and of `+0x40` at overlay 174 `+064A` and overlay 198
`+02F0`; `field_stores.py <dsun> <inventory> --es` with the same displacements finds no store
through `DS:19C5`.

**Overlay 179 `+0D2F`** takes a slot in `DI` (`+0D37`). After it has compared an attack number
with the last natural attack of the slot whose rate at details `+0x25` is above 0
(`+0FE0..+100D`), it reads the word at `+0x3C` of that slot's details record, uses 0x62 (98) when it
is 0, and passes it to `2C5F:0A22` (`+102E..+1061`). `direct_callers.py <dsun> 179+0D2F` finds it
called from overlay 179 `+0D1A` and `+1299` and from overlay 193 `+04DE`.

**Overlay 206 `+0000`** maps a slot through `2D40:3E64`, reads the word at `+0x3E` of the details
record it gets and, when that is not 0, passes its low byte to `2C5F:0A22` (`+00F0..+011D`). The
damage routine, overlay 173 `+22C4` (FND-PARTY-086), calls overlay 206 `+0000` at `+282A` with its
`DI` slot and then, when the word at `DS:054F` is 0, plays sound 0x63 (99) (`+2805..+283B`).

Overlay 174 `+064A` uses `+0x40` only when it is 1 to 8, and overlay 198 `+02F0` only when it is
above 0. Overlay 206 `+1567` and `+1655` test `+0x3C` for 0.

**The shipped records.** In the 19 `CHAR` resources of the installed `CHARSAVE.GFF`, the word at
`0x81` is 67 in resources 30, 31 and 33, 68 in resource 32 and 0 in the others; the word at `0x83`
is 69 in resource 32 and 0 in the others; bytes `0x85` and `0x86` are 0 in all.

## Interpretation

Details word `+0x3C`, CHAR word `0x81`, is the sound a combatant's natural attack makes; a
combatant without one makes sound 98. Details word `+0x3E`, CHAR word `0x83`, is a sound the
damage routine plays for a combatant through overlay 206, with no sound when it is 0. Four shipped
characters carry their own attack sound and one, resource 32, a thri-kreen, also the second
sound.

## Alternatives

- Whether overlay 179 `+0D2F`'s `DI` is the attacker or the target, and the same for the slot
  the damage routine passes to overlay 206, was not settled from the callers; the read of the
  natural attack rates of the same slot suggests the attacker for `+0x3C`.
- What the count at `+0x40` (CHAR byte `0x85`) counts in overlays 174 and 198, and what `+0x41`
  holds, was not read.
- The sound files 67, 68, 69, 98 and 99 were not listened to.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: in
`tools/research/exec-census/`, run `field_reads.py <dsun> <inventory> --es --table 19c5 3c 3d 3e
3f 40 41`, `field_stores.py <dsun> <inventory> --es 3c 3d 3e 3f 40 41`, `overlay_listing.py <dsun>
179 6500F 65360`, `direct_callers.py <dsun> 179+0D2F 206+0000`, `overlay_listing.py <dsun> 206
8EA40 8EA80` and `overlay_listing.py <dsun> 173 5D0F0 5D160`. Read the words at `0x81` and
`0x83` and bytes `0x85` and `0x86` of each `CHAR` resource of `CHARSAVE.GFF`.
