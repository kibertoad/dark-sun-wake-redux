---
id: FND-PARTY-152
title: Combatant bytes 0x12 and 0x17 have no reader through DS:19C9 besides the load and the generation finish, and byte 0x20 is field 33 of overlay 204's combatant field getter and setter
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E0E7..0x0008E2F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E35D..0x0008E49A
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py, field_stores.py, overlay_listing.py, direct_callers.py; a scratch script reading the CHAR resources through the GFF directory as details_chunks.py does)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A load copies CHAR bytes `0x1C`, `0x21` and `0x2A` to `+0x12`, `+0x17` and `+0x20` of
the character's FMT-COMBAT-001 record (FND-PARTY-049), whose table is at the far pointer
`DS:19C9` with records of 0x31 bytes; the details records at `DS:19C5` are 0x42 bytes.

**Reads.** `field_reads.py <dsun> <inventory> --es --table 19c9 12 17 20` finds 16 reads of `+0x12`
in the inventory, every one after a multiplication by 0x42 and a load of `DS:19C5`, which reads a
details record's origin; twelve reads of `+0x12` outside the inventory, in overlays 179, 195 and 208, of
which the four read (179 `+1451` and `+1D52`, 195 `+0CD8`, 208 `+0FAF`) follow `mov ax, es:[bx+4]`, a
multiplication by 0x42 and a load of `DS:19C5`; no read of
`+0x17`; and one read of `+0x20`, at overlay 204 `+2434` outside the inventory.

**Stores.** `field_stores.py <dsun> <inventory> --es 12 17 20` finds, of stores to `+0x12` after
a load of `DS:19C9` and a multiplication by 0x31, only the generation finish's at overlay 184
`+10C3` (FND-PARTY-145); the one store to `+0x17`, overlay 179 `+2505`, lowers the base
constitution of a details record when a raise succeeds, with `+0x12` of the same record (the
origin) stored at `+25E7`; and of stores to `+0x20` after a load of `DS:19C9` only overlay 204
`+26D5`.

**Overlay 204 `+2327`** takes a slot and a 32-bit field number. For a slot whose state byte at
`4F49:0C33` is 2 it compares the number with the nine dwords at `cs:2567`, 0, 13, 14, 18, 19, 23,
25, 32 and 33, and returns from the slot's combatant record the word at `+0x06`, the word at
`+0x00`, the word at `+0x02`, the bytes at `+0x0E`, `+0x0F`, `+0x15`, `+0x18`, `+0x1F` and
`+0x20` in that order; it has tables for slots of state 1 and 5 as well (`+2327..+2530`). **Overlay
204 `+259D`** stores a byte argument by the same nine field numbers, at `cs:2817`, and for field 33 at `+0x20`
(`+26C7..+26D9`).
`direct_callers.py <dsun> 204+2327 204+259D` finds the getter called near from `+01F5` and
through the trampoline `5787:00C0`, and the setter only through the trampoline `5787:00BB`; no far
call to either trampoline was found.

**The shipped records.** In the 19 `CHAR` resources of the installed `CHARSAVE.GFF`, byte `0x21`
is 7 in all and byte `0x2A` is 0 in all; byte `0x1C` is described in FND-PARTY-145.

## Interpretation

Nothing found reads combatant byte `+0x17` (CHAR `0x21`) by a fixed displacement, so its value 7
is carried through loads and saves without a found use. Combatant byte `+0x12` (CHAR `0x1C`) is
set from the base armor class at the generation finish and is not otherwise read through the
table pointer, so the four shipped records where it differs keep a value no found code consults.
Combatant byte `+0x20` (CHAR `0x2A`) is field 33 of the field getter and setter of overlay 204,
which also serve the facing (field 32) and the kind (field 23); whatever calls them through their
trampolines, perhaps the script interpreter, is what reads and writes it.

## Alternatives

- The search finds only reads with a fixed displacement from a base loaded from `DS:19C9`; a read
  through a copy of the record, a computed displacement or another pointer to the same record is
  not found (toolkit issue 468 asks for a search by the pointer's source).
- The callers of the trampolines `5787:00BB` and `5787:00C0` were not found; they may be called
  through a table of far pointers.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: in
`tools/research/exec-census/`, run `field_reads.py <dsun> <inventory> --es --table 19c9 12 17 20`,
`field_stores.py <dsun> <inventory> --es 12 17 20`, `overlay_listing.py <dsun> 179 66740 66900`,
`overlay_listing.py <dsun> 204 8E0E7 8E4A0` and `direct_callers.py <dsun> 204+2327 204+259D`, and
read the nine dwords and nine words at file `0x8E327` and at file `0x8E5D7`. Read bytes `0x21` and `0x2A`
of each `CHAR` resource of `CHARSAVE.GFF`.
