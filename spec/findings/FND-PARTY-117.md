---
id: FND-PARTY-117
title: An RDFF actor is loaded into an object the loader takes from free lists that start at object 4, so it can occupy a party object 0 to 3 only after 2D40:03C1 releases that object
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-PARTY-119]
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073BC6..0x00073C65
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:03C1..2D40:048D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0C31..2D40:0CC2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0F51..31E0:0FE6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:1000..31E0:10FC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:067D..2D40:069F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31BA:0112..31BA:013C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0152..31E0:0175
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069189..0x000691A0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070A49..0x00070A6F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073A9E..0x00073ABE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007405B..0x0007408B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0545..31E0:055B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0C96..31E0:0CA7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:31C1..2D40:31E9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000609F5..0x00060A07
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006F016..0x0006F022
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000713E4..0x000713F8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000773BD..0x000773D0
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/immediate_search.py, direct_callers.py, resident_listing.py, overlay_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. The object table at `4F49:0C33` has three bytes per object: a kind byte (2 for a
combatant, FND-PARTY-051) and a word at `+1` that holds the record number of a used object and
the next object of a free list for a free one. Objects 0 to 3 are the party slots.

**The free lists.** Overlay 188 `+0D26` stores 9999 (0x270F, the empty mark) in the list heads
`DS:56EF`, `DS:56F1` and `DS:56ED`, clears the table, calls `2D40:03C1` for each object from 0x30
to 0x13E, `2D40:0C92` for each from 0x140 to 0x207, and `2D40:0CC2` with `DS:56ED` for each from
4 to 0x2F (`+0D26..+0DC5`). `2D40:0CC2` pushes an object onto the list it is given.
`2D40:0C92` pushes an object from 0x140 to 0x207 onto `DS:56EF` and otherwise only stores 0 in
its kind byte (`2D40:0C92..2D40:0CC2`). **`2D40:03C1`**, the release, clears the object's 19-byte
entry at `4F49:08A3` and its six bytes at `DS:55CB` when the object is below 0x30, and pushes it
onto `DS:56ED` when it is below 0x30, onto `DS:56EF` when it is from 0x140 to 0x207, and onto
`DS:56F1` otherwise (`2D40:03C1..2D40:0453`). `2D40:0C3F` pops the head of the list it is given,
or returns 9999 when the list is empty. `2D40:0453` pops `DS:56F1`; `2D40:0C31` pops `DS:56EF`;
`2D40:0461` pops `DS:56ED` when its argument is not 0 and `DS:56F1` when that gives nothing
(`2D40:0453..2D40:048D`, `2D40:0C31..2D40:0C92`). `immediate_search.py <dsun> 56ED 56EF 56F1`
finds no other store to the heads.

**The loader.** `31E0:0EFF` takes a record number, a word, an object, a byte and flags. It uses
object 0x13F when the object is -1. After the `OJFF` request it requests the `RDFF` of the record
number through `2D40:000A` when bit 2 of the flags is set and the number is below 9000 or above
13,998 (`31E0:1000..31E0:102D`). For object 0x13F it then calls `2D40:0461` with 1 when the
object's kind byte is 2, and with 0 otherwise, up to three times with calls of `31E0:0D09` between,
and copies the three bytes of object 0x13F's entry to the object it gets (`31E0:1065..31E0:10FC`).
Before that it treats an object below 4 apart, choosing a word from the bytes at `DS:068E` and
`DS:6281` (`31E0:0FB3..31E0:0FE6`).

**Its callers.** `direct_callers.py <dsun> 31E0:0EFF` finds five. `2D40:069A` passes object -1
and flags 4, overlay 182 `+094B` object -1 and flags 6, and `31BA:0137` object -1; overlay 187
`+0C3A` passes the object of the region record it reads and flags 0, and `31E0:0170` the object
and flags its own caller passes. Of the other `RDFF` requests (FND-ACTOR-005), six in overlays
178, 191 and 201 load into an object whose kind byte is 0, overlay 188 `+0C07` into object 0x13F,
and overlay 188 `+11D6` into an object from `2D40:0453` or `2D40:0C31`.

**The releases.** `direct_callers.py <dsun> 2D40:03C1` finds ten far callers and one near.
Overlay 188 `+0D8E` is the start-up loop above. Overlay 187 `+15C3` releases its object only when
it is 4 or more (`+15B4..+15C8`). Overlays 175 `+0F06` and 189 `+2B5A` release an object they took
from `2D40:0453` (`+0E15..+0E27`, `+2A0D..+2A20`), overlay 188 `+12C3` one from `2D40:0453` or
`2D40:0C31`, and overlay 184 `+1F8D` and `+2093` one they took from the same allocators.
`31E0:0556`, in `31E0:044C`, releases the object in `di`, and `31E0:0CA2`, in `31E0:0B4A`, the
object at `[bp + 6]`; `direct_callers.py` finds each of those two routines called once, from
`31E0:0AC9` and `31E0:0DE3`. `2D40:31E6` releases the object at `[bp + 6]` of its routine, after
calling `573B:00AC` when that object's kind byte is 2.

## Interpretation

An `RDFF` actor that the loader places gets an object from 4 to 0x2F, from the combatant list, or
from 0x30 up, never 0 to 3, while no party object has been released: the lists start without
them and every allocator pops a list. The two loads that take a given object take one whose kind
byte is 0, so a filled party slot is not reused. A party object can therefore pass to an `OBJEX`
actor only through `2D40:03C1`, which puts any object below 0x30 back on the combatant list, and
of its callers only `31E0:044C`, `31E0:0B4A` and the routine at `2D40:31E6` can be given one.

## Alternatives

- The three release routines' callers were followed one level only; whether a script, a death or
  a party change passes them a party object is open.
- `31BA:0137` builds its arguments through stack writes at `[bp + 2]`; its object -1 and flags 7
  are read from that sequence, not traced.
- A store of a party object's kind byte or record word outside these routines, which would make an
  object of 0 to 3 look free to the kind-0 loads, was not searched for beyond the stores
  `immediate_search.py <dsun> 0C33 0C34` lists.
- The meaning of the words chosen from `DS:068E` and `DS:6281` for objects below 4 was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `immediate_search.py <dsun> 56ED 56EF 56F1` and
`immediate_search.py <dsun> 0C33 0C34`; `direct_callers.py <dsun> 31E0:0EFF 2D40:03C1 2D40:0453
2D40:0461 2D40:0CC2 31E0:044C 31E0:0B4A`; search the file for the bytes `52 44 46 46` and read
the code before each; and `resident_listing.py` and `overlay_listing.py` over the ranges above.
