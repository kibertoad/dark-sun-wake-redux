---
id: FND-PARTY-160
title: No routine that sets a character's level tests the origin's level limit or a prime requisite, and README table 3's limits are not in the executable or the archives
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009493C..0x00094A16
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C303..0x0006C4A0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000956A0..0x00095730
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_stores.py, overlay_listing.py; a scratch script searching DSUN.EXE and every installed GFF archive for byte patterns)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A details record's level bytes are at `+0x1E` to `+0x20`, one per class position, and
the records are 0x42 bytes at the far pointer `DS:19C5` (FND-PARTY-055).

**The writers.** `field_stores.py <dsun> <inventory> --es 1e 1f 20` lists 20 stores through ES at
those displacements. Those that form the address from `DS:19C5` and 0x42, or from the generation
working record at `DS:1429` (FND-PARTY-063), are:

- overlay 183 `+136A` to `+14BB`, in the generation's level setting `+1323`, which stores 0, then
  6 or 7, then adds 1 while the experience reaches the next `DATA` 1000 threshold (FND-PARTY-065);
- overlay 210 `+07CB`, in `+0740`, which adds 1 for a level gained in play, called by `+08BC` only
  while the level is below 15 (FND-PARTY-081);
- overlay 210 `+0C86` and `+0D46`, the level drain and its re-raise (FND-PARTY-096,
  FND-PARTY-105);
- overlay 190 `+06D1`, which stores 0.

The other stores use other tables: the combatant records at `DS:19C9` (resident `28C9:18AC` and
overlays 173 and 177) and dwords at `+0x20` in resident `37FC`. The search misses stores whose
`ES` comes from an earlier `les`, and the listing of overlay 209 shows two more: `+17DC` copies,
for `k` from 2 down to 1, the class byte, level byte and byte `+0x38` of position `k - 1` to
position `k`, then stores its class argument at `+0x1B`, 1 at `+0x39` and `+0x1E`, and 0 in the
dwords at `+0x04` and `+0x00` (`+17DC..+18B6`): a new class at position 0 with level 1, the
earlier classes moved up one position.

Of these routines, FND-PARTY-065 and FND-PARTY-081 read every test in `+1323` and `+08BC`: the
class bytes, the experience, the `DATA` 1000 thresholds and the bound 15. `+17DC..+18B6` tests
only its loop counter. None reads the origin byte at `+0x12` except to limit a human to position 0
(FND-PARTY-081), and none reads a score.

**The table.** README table 3 (SRC-README-1.1), as RULE-PARTY-007's list gives it, has eight rows
of eight limits, one per class and one per origin. A search of `DSUN.EXE` and of every installed
`.GFF` archive for each row and each column with more than two distinct values, eight bytes in
order, with the table's "no limit" written as 255, 0, 99, 20 or 15, finds none of the 70 patterns.

## Interpretation

The game does not apply the origin level limits of README table 3 or the prime requisite bonus
of its table 8. A character of any origin rises in every class it holds up to level 15, the bound
every level gain applies. Which classes an origin may take is decided at generation by the
game's own table (RULE-PARTY-009) and, for a later class, by the DUAL list (Q-PARTY-007).

## Alternatives

- A limit stored in another form, such as a table of per-origin words or a computed bound, would
  not match the byte search; no routine that sets a level reads such a table.
- A level set through a copy of the details record, or a block copy, is outside the store search;
  the `CHAR` load is such a copy and sets whatever the record holds (FND-PARTY-051).

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: in
`tools/research/exec-census/`, run `field_stores.py <dsun> <inventory> --es 1e 1f 20` and
`overlay_listing.py <dsun> 209 9493C 94A00`. Take RULE-PARTY-007's 64 limits as eight rows of
eight, form each row and each column with more than two distinct values as 8 bytes with 255
replaced by 255, 0, 99, 20 and 15 in turn, and search `DSUN.EXE` and each installed `*.GFF` file
for each.
