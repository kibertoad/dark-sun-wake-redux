---
id: FND-PARTY-119
title: The object allocators hand out objects from 5 up and no release puts an object below 5 on a free list, so neither an RDFF load through them nor the DROP of a party member gives a party object to an OBJEX actor
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
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
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:05BB..31E0:0612
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:09AF..31E0:09DC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0ABC..31E0:0AD1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:044C..31E0:05BB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0C31..2D40:0D59
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007940B..0x00079453
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058F25..0x00059014
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/immediate_search.py, direct_callers.py, resident_listing.py, overlay_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. The object table at `4F49:0C33` has three bytes per object: a kind byte (2 for a
combatant, FND-PARTY-051) and a word at `+1` that holds the record number of a used object and
the next object of a free list for a free one. Objects 0 to 3 are the party slots.

**Adding to a list.** `2D40:0CC2` takes an object and the address of a list head. It does
nothing when the object is below 5, is 0x208 or more, or is 0x13F, or when the object is already
on the list. Otherwise it stores 0 in the object's kind byte, puts the head in the object's word
and makes the object the head, and lowers the counter `DS:55C6` for `DS:56EF` or `DS:55C8` for
`DS:56F1` (`2D40:0CC2..2D40:0D59`). `2D40:0C3F` pops the head of the list it is given, raising
the same counters, or returns 9999 (0x270F, the empty mark) when the list is empty.

**The lists.** Overlay 188 `+0D26` stores 9999 in the heads `DS:56EF`, `DS:56F1` and `DS:56ED`,
clears the table, calls `2D40:03C1` for each object from 0x30 to 0x13E, `2D40:0C92` for each from
0x140 to 0x207, and `2D40:0CC2` with `DS:56ED` for each from 4 to 0x2F (`+0D26..+0DC5`), so
`DS:56ED` holds 5 to 0x2F. `2D40:0C92` passes an object from 0x140 to 0x207 to `2D40:0CC2` with
`DS:56EF`, and otherwise stores 0 in its kind byte (`2D40:0C92..2D40:0CC2`). `2D40:03C1`, the
release, clears the object's 19-byte entry at `4F49:08A3` and its six bytes at `DS:55CB` when the
object is below 0x30, and passes the object to `2D40:0CC2` with `DS:56ED` when it is below 0x30,
`DS:56EF` when it is from 0x140 to 0x207, and `DS:56F1` otherwise (`2D40:03C1..2D40:0453`).
`2D40:0453` pops `DS:56F1`, `2D40:0C31` pops `DS:56EF`, and `2D40:0461` pops `DS:56ED` when its
argument is not 0 and `DS:56F1` when that gives nothing (`2D40:0453..2D40:048D`,
`2D40:0C31..2D40:0C92`). `immediate_search.py <dsun> 56ED 56EF 56F1` finds no other store to the
heads.

**The loader.** `31E0:0EFF` takes a record number, a word, an object, a byte and flags, and uses
object 0x13F when the object is -1. After the `OJFF` request it requests the `RDFF` of the record
number through `2D40:000A` when bit 2 of the flags is set and the number is below 9000 or above
13,998 (`31E0:1000..31E0:102D`). For object 0x13F it then calls `2D40:0461` with 1 when the
object's kind byte is 2 and with 0 otherwise, up to three times with calls of `31E0:0D09`
between, and copies the three bytes of object 0x13F's entry to the object it gets
(`31E0:1065..31E0:10FC`). Before that it treats an object below 4 apart, choosing a word from the
bytes at `DS:068E` and `DS:6281` (`31E0:0FB3..31E0:0FE6`). Of its five callers, `2D40:069A`
passes object -1 and flags 4, overlay 182 `+094B` object -1 and flags 6, and `31BA:0137` object
-1; overlay 187 `+0C3A` passes the object of the region record it reads and flags 0, and
`31E0:0170` the object and flags its own caller passes. Of the other `RDFF` requests
(FND-ACTOR-005), six in overlays 178, 191 and 201 load into an object whose kind byte is 0,
overlay 188 `+0C07` into object 0x13F, and overlay 188 `+11D6` into an object from `2D40:0453`
or `2D40:0C31`.

**DROP.** Overlay 190 `+10CB..+1113` shows `LAST ONE: CAN'T DROP` in one case and otherwise asks
`DROP THIS CHARACTER?` and, when the answer is 1, calls overlay 171 `+0D15` with 0. That routine
takes the slot `si` from the word at `4E71:0B44` (with 0) or from the table at `DS:40C8` (with
another value), makes another filled slot current through `56BD:0020` when `si` is the current
slot at `4C13:0369`, and calls `31E0:05BB` with the placed index in the word at
`DS:67BC + 0x25 * si`; it then stores 8 at `4F49:0330 + si` and 0 at `4E71:0A55 + si`
(`+0D15..+0E03`). `31E0:05BB` takes the placed entry's object `si` from the word at `+6` of its
8 bytes at `DS:67B7`, calls `25AF:071F` for a kind 2 object below 4, and reaches `31E0:0ABC`,
which calls `31E0:044C` with the object (`31E0:05BB..31E0:0612`, `31E0:09AF..31E0:09DC`,
`31E0:0ABC..31E0:0AD1`). `31E0:044C` stores -1 at `+0x10` of the object's record at
`DS:67BB + 0x25 * object`, calls `2D40:2410` and `2D40:03C1` with the object, and fills the record
with 0xFF (`31E0:0549..31E0:056C`).

**The other releases.** `direct_callers.py <dsun> 2D40:03C1` finds ten far callers and one near.
Overlay 187 `+15C3` releases its object only when it is 4 or more; overlays 175 `+0F06`, 184
`+1F8D` and `+2093`, 188 `+12C3` and 189 `+2B5A` release objects they took from `2D40:0453` or
`2D40:0C31`; `31E0:0CA2`, in `31E0:0B4A`, and `2D40:31E6`, in `2D40:2F34`, release the object
their routine is given.

## Interpretation

The allocators hand out objects from 5 up, and no release puts an object below 5 on a list, since
`2D40:0CC2` passes over them. So an `RDFF` actor that the loader places with object -1, or that
overlay 188 places, never gets a party object, and dropping a party member, which releases its
object through `2D40:03C1`, leaves that object off every list. An `OBJEX` actor could take a
party object only through a load that names it directly: overlay 187 `+0C3A` with flags 0, which
requests no `RDFF`, or a kind-0 load of overlays 178, 191 or 201, whose object comes from an
argument or a map lookup.

## Alternatives

- Whether the map lookup that overlay 191 uses, or the argument overlay 178 receives, can name a
  dropped party object was not read. After a drop the object's record is filled with 0xFF and its
  kind byte is left as `2D40:03C1` and `2D40:2410` leave it, which was not read.
- `31BA:0137` builds its arguments through stack writes at `[bp + 2]`; its object -1 is read from
  that sequence, not traced.
- What the 8 at `4F49:0330 + slot` means for the party code was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `immediate_search.py <dsun> 56ED 56EF 56F1`;
`direct_callers.py <dsun> 31E0:0EFF 2D40:03C1 2D40:0453 2D40:0461 2D40:0CC2 31E0:05BB 171+0D15`;
search the file for the bytes `52 44 46 46` and read the code before each; and
`resident_listing.py` and `overlay_listing.py` over the ranges above.
