---
id: FND-PARTY-120
title: After a DROP the party object keeps no placed entry and kind 0, and the RDFF loads that take a kind-0 object get theirs from placed entries or from kind-2 objects, so none of them reaches it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2410..2D40:2452
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2452..2D40:24B4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:256E..2D40:2588
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:2DBD..31E0:2E5F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0568..28C9:05CF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:09EC..28C9:0A1C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00063DF0..0x00063E45
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007C260..0x0007C2CA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089D95..0x00089DB0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008A0B0..0x0008A114
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008F795..0x0008F7C0
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/immediate_search.py, direct_callers.py, resident_listing.py, overlay_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A DROP removes the member's placed entry through `31E0:05BB`, whose `31E0:044C` calls
`2D40:2410` and `2D40:03C1` with the party object and fills its record at
`DS:67BB + 0x25 * object` with 0xFF (FND-PARTY-119).

**The kind byte.** `2D40:2410` does nothing for an object of 9999 or more or whose kind byte at
`4F49:0C33 + 3 * object` is 0. Otherwise it lists the object's handles through `2D40:2196` into a
stack array and passes the array to `2D40:2452` (`2D40:2410..2D40:2452`), which collects their
numbers through `2D40:27B7` and calls `2D40:248D` (`2D40:2452..2D40:248D`). `2D40:248D` handles
each entry by its kind word at `+2` and, for an entry whose number is below 0x20C, calls
`2D40:0C92` with the number (`2D40:248D..2D40:24B4`, `2D40:256E..2D40:2588`). For an object
outside 0x140 to 0x207, `2D40:0C92` stores 0 in its kind byte (FND-PARTY-119).

**The kind-0 loads.** Overlay 178 `+0D40` takes its object as an argument and loads the `RDFF`
only when the object's kind byte is 0 (`+0D40..+0D95`); its one caller, through the trampoline
`56A2:0048`, is `28C9:0A17`, which passes the word at `DS:4261` (`28C9:09EC..28C9:0A1C`). The
three stores to `DS:4261`, at `28C9:06A4`, `28C9:0708` and `28C9:0BB7`, store what `28C9:0568`
returns. `28C9:0568` calls `31E0:2DBD` with the map origin `DS:1408`, `DS:140A` and a dword
argument, and returns its result, or, when that object's kind byte is 1, its word at
`4F49:0C34 + 3 * object` is not 9999 and the first word of its 0x17-byte record at `DS:19C1` is
0x16F7, the word at `+0x23` of the object's record at `DS:67BB` (`28C9:0568..28C9:05CF`).
`31E0:2DBD` walks the placed entries at `DS:67B7` from the last, passing over those whose object
word at `+6` is negative, whose first word lies outside `DS:6569` to `DS:6565`, or whose object's
record has -1 at `+0x10` (`31E0:2DBD..31E0:2E5F`). Overlay 191 `+0070` takes its object from
`31E0:2DBD` and loads the `RDFF` only when its kind byte is 0 (`+0070..+00DA`). Overlay 201
`+05D5` takes its object as an argument and loads only when the kind byte is 0
(`+08F0..+0954`); `direct_callers.py` finds its callers to be its trampoline `5778:0043` and
overlay 206 `+0E6B`, which calls it for an object whose kind byte is 2 and whose record's word at
`+0x1B` is 430 or -430 (`+0E45..+0E6B`).

## Interpretation

After a DROP the party object has kind 0 and no placed entry, and its record holds -1 at
`+0x10`. The loads that put an `RDFF` actor into a kind-0 object take the object from the placed
entries, which no longer name it, or from a caller that passes a kind-2 object, so none of them
fills a dropped party slot with an `OBJEX` actor. With the allocators of FND-PARTY-119 never
handing out objects below 5, no path found lets an `OBJEX` actor such as the Preservers of
FND-PARTY-116 become a party member, and a party member can cast spell `DATA` 104 only with the
debug byte `DS:13F8` set.

## Alternatives

- `28C9:0568` can return the object named at `+0x23` of a kind-1 object's record whose record
  starts with 0x16F7; what writes that word, and whether it can name a party object, was not read.
- Overlay 201 `+05D5` may change the kind byte of its object before its load; for a party object it
  is reached only when the party object's record number is 430, which was not looked for.
- The overlay stores to the kind byte that write a register (FND-PARTY-119 lists the resident
  ones) were not each read for a store of 0 to a party object.
- What the game does with a dropped party slot later, such as a new `ADD` in play, was not read; it
  loads a `CHAR`, not an `RDFF`.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `immediate_search.py <dsun> 4261` for the
stores; `direct_callers.py <dsun> 178+0D40 201+05D5 2D40:0C92`; and `resident_listing.py` and
`overlay_listing.py` over the ranges above.
