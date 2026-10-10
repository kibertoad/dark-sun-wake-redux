---
id: FND-PARTY-141
title: The four unloaders left on the DUAL window's routes free only a slot of 4 or above or the slot of an object they placed themselves, which comes from the free lists, so no routine the DUAL path reaches frees a party member's slot
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065C06..0x00065CFC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E502..0x0007E628
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008EB0D..0x0008EB4B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008F2D6..0x0008F39E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080D73..0x00080EC0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:044D..371D:04C8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:01F9..371D:0334
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:073E..371D:0769
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0160..31E0:017D
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/direct_callers.py, immediate_search.py, resident_listing.py, overlay_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. FND-PARTY-140 leaves four routines that call the unload `31E0:05BB` on the routes from
overlay 209 `+169C`.

**Overlay 179 `+1373`** takes its first argument as `DI` and does not change it. Its call at
`+1A17` unloads the entry of slot `DI`; `+1A08`, where that call starts, is reached only by the
jump at `+192B`, which runs when `DI` is 4 or above (`+1926..+1A1C`). The four jump tables of the
routine (at `cs:28C1`, `cs:28A9`, `cs:2881` and `cs:2855` with their targets) do not hold
`+1A08`.

**`31E0:0121`** calls the object load `31E0:0EFF` with its first argument, the entry count at
`DS:264E` and its second argument as the slot, and returns the slot the load gives
(`31E0:0160..31E0:017D`); for a slot of -1 the load takes one from the free lists through
`2D40:0461` (FND-PARTY-135).

**Overlay 193 `+003C`** places an object through `31E0:0121` with -1 as the slot at `+022D` and
`+0302`. After the first it passes the slot to overlay 206 `+0986`, which returns its first
argument (`+0986..+0A4E`), and unloads that slot's entry at `+0258`; after the second it unloads
the placed slot's entry at `+0334` (`+0212..+0338`).

**Overlay 206 `+0000`** places an object through `31E0:0121` with -1 as the slot at `+01C1`,
keeps the slot in `DI`, and unloads its entry at `+01F6` (`+01BD..+01FB`).

**`371D:050F`** unloads the entry of the slot in its first argument when its second argument is
not 0 (FND-PARTY-140). Overlay 193 `+05B6` passes 0 as the second argument. `371D:04B3` passes
the first word of a record and a flag, and is called at `371D:0476` by `371D:044D` for a record
of kind 3 (`371D:044D..371D:04C8`). `371D:01F9` adds a record with the kind in its fourth
argument (`371D:01F9..371D:0334`); of its four callers, two add kind 3: overlay 193 `+2BCB`,
whose first record word is `SI`, the slot that `31E0:0121` returned at `+2A92` for a slot of -1
(`+2A83..+2BD0`), and `371D:0766` in `371D:050F`, which adds a record with its own slot
(`371D:073E..371D:0769`). `immediate_search.py <dsun> 2A62 2A63 2A65` finds the table used only
in segment `371D` and the byte at `DS:2A62` set in overlay 187.

## Interpretation

None of the four frees a party member's slot. Overlay 179 `+1373` unloads only slots of 4 and
above, and the other three unload objects that they or overlay 193 `+29EB` placed themselves,
with slots from the free lists. The free lists hold a party slot only after a release has freed
one (FND-PARTY-136), and FND-PARTY-138 to FND-PARTY-140 find no other release on the DUAL path
that frees one. So on the routes the walks read, the object load on the DUAL path never hands on
a party member's slot, the experience award that needs one does not run from the message window
(FND-PARTY-134), and the DUAL path does not store another slot in the word at `4E71:0B44` before
overlay 209 `+14FF`. Overlay 184 `+1DCE` then takes the class bytes of the character whose records
it copies.

## Alternatives

- The walks leave 34 or more computed transfers unresolved, among them the callbacks of window
  objects and buttons; a route through one of them is not excluded.
- A saved game may restore the record table at `DS:2A63` with a party slot in a kind 3 record;
  the load of a save was not read.
- The conditions along the routes, such as whether overlay 209 `+169C` places an object at all
  for the keys the DUAL window accepts, were not read; they can only remove routes.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `overlay_listing.py` over the overlay ranges
above (`overlay_listing.py <dsun> 179 65653 66700` and so on), `resident_listing.py <dsun>
31E0:0121..31E0:03CA 371D:01F9..371D:0334 371D:044D..371D:04C8 371D:06F0..371D:0770`,
`direct_callers.py <dsun> 371D:050F 371D:044D 371D:01F9` and `immediate_search.py <dsun> 2A62
2A63 2A65`; read the jump tables of overlay 179 `+1373` as 16-bit words at file `0x642E0` plus
each table offset and plus the offset the indirect jump adds.
