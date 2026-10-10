---
id: FND-PARTY-146
title: Bytes 0x1D and 0x67 of a CHAR record are the character's current and base movement rate, 12 or 15 for a thri-kreen, which a fight turns into ten movement points per unit
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E7F4..0x0006E8CD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000852F6..0x00085316
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000855D5..0x000856A5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B825..0x0005B8FA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D8BC..0x0005D8E8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:05F5..2C5F:0619
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py, field_stores.py, overlay_listing.py, resident_listing.py, direct_callers.py; a scratch script reading the CHAR resources through the GFF directory as details_chunks.py does)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`, whose data segment starts at file `0x4D000` (FND-PARTY-144). A load copies a CHAR
record's bytes `0x1C` and `0x1D` to `+0x12` and `+0x13` of the character's FMT-COMBAT-001 record
(FND-PARTY-049) and bytes `0x66` to `0x68` to `+0x21` to `+0x23` of its FMT-COMBAT-002 record
(FND-PARTY-051).

**Generation.** Overlay 184 `+1764..+183D` tests the origin byte at `+0x12` of the details record:
for origin 8, thri-kreen, it stores 15 at details `+0x22` and at combatant `+0x13`, and for any other
origin 12 at both (`+1786`, `+1796`, `+1828`, `+1838`).

**Rebuild.** Overlay 197 `+1E7B`, which rebuilds the current ability scores (FND-PARTY-142),
first copies details `+0x22` to combatant `+0x13` (`+1EF6..+1F16`). It then goes through the slot's
23-byte records from `2D40:2196` and dispatches on each kept record's word at `+0x10` less 0x140
through the eight words at `cs:2376`: codes 0x140 to 0x145 adjust the six scores, 0x146 does
nothing, and 0x147 goes to `+21D5`. There, a record whose word at `+0` is 0x324 or 0x323 (by
absolute value) lowers combatant `+0x13` by 2, one whose word is 0x322 lowers it by 3, and any
other adds the record's word at `+0x0E` (`+21D5..+22A5`).

**Reads.** `field_reads.py <dsun> <inventory> --es --table 19c9 13` finds, besides the rebuild's
own reads, overlay 197 `+251A` and overlay 206 `+15A0` and `+168E`; the two in overlay 206 read
`+0x13` of a details record (gender), loaded from `DS:19C5`. Overlay 197 `+24FB` returns
combatant `+0x13` of a slot plus the byte at `+5` of its entry at `4E71:0251` with code 0x2D, if it
has one. `direct_callers.py <dsun> 197+24FB` finds one caller, overlay 173 `+0F06`, in the fight
set-up: it stores ten times the result as the combatant's word at `4C4F:022E` (`+0F06..+0F18`),
and then, by the result of overlay 197 `+2499`, sets it to 0, halves it, doubles it, replaces it
with the byte at `+5` of the slot's entry with code 0x38, divides it by 9, or leaves it (`+0F1E..+0FD3`, table
at `cs:1088`).

**Spending.** Overlay 173 `+2F1B` takes a slot and a direction from 0 to 7, subtracts the
direction's byte from the eight bytes copied from `DS:0551` from the combatant's word at
`4C4F:022E`, and sets the word to 0 when fewer than 10 remain (`+2F9C..+2FC8`). The bytes at
`DS:0551` are 10, 14, 10, 14, 10, 14, 10 and 14. `2C5F:05FF` divides the word by 10 and formats it
with the text at `DS:0F60`, `Move : %d` (`2C5F:05F5..2C5F:0619`).

**The shipped records.** Byte `0x67` is 12 in 11 of the 19 `CHAR` resources of the installed
`CHARSAVE.GFF`, 15 in resources 32, 34, 35, 36 and 42 (five of the six thri-kreen), and 64 in resources 50,
51 and 53. Byte `0x1D` equals byte `0x67` in 18 records; in resource 52 it is 13 over a base of 12.
Byte `0x68` is 0 in all 19.

## Interpretation

Details byte `+0x22`, CHAR byte `0x67`, is the character's base movement rate: 12, or 15 for a
thri-kreen. Combatant byte `+0x13`, CHAR byte `0x1D`, is the current rate, rebuilt from the base
whenever the scores are, with the slot's records of code 0x147 adding to it and three particular
records taking 2 or 3 from it. At the start of a fight round a combatant gets ten movement
points per unit of rate, changed by its condition, and spends 10 for a straight step and 14 for a
diagonal one; the status line shows the points left as whole units. A saved character keeps both
rates, so resource 52's 13 is a rate raised by one of its records.

## Alternatives

- What the records with words 0x322 to 0x324 at `+0` are, and which conditions `+2499` reports,
  was not read; the reading of the 0x147 records as items that change the rate rests on their
  sharing the dispatch with the score codes.
- Which direction numbers are diagonal depends on which way direction 0 points, which
  FND-PARTY-144 leaves open; the table only shows that alternate directions cost 14.
- Byte `0x68` (details `+0x23`) was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: in
`tools/research/exec-census/`, run `overlay_listing.py <dsun> 184 6E7D0 6E8D0`, `overlay_listing.py
<dsun> 197 8527B 85322`, `overlay_listing.py <dsun> 197 855D5 856B0`, `overlay_listing.py <dsun>
197 858FB 8594B`, `field_reads.py <dsun> <inventory> --es --table 19c9 13`, `direct_callers.py
<dsun> 197+24FB`, `overlay_listing.py <dsun> 173 5B7B9 5B91A`, `overlay_listing.py <dsun> 173
5D83B 5D8F0` and `resident_listing.py <dsun> 2C5F:05E0..2C5F:0620`. Read the eight words at file
`0x85776`, the eight bytes at file `0x4D551` and the text at file `0x4DF60`, and bytes `0x1D`,
`0x67` and `0x68` of each `CHAR` resource of `CHARSAVE.GFF`.
