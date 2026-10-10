---
id: FND-PARTY-135
title: The routines that mark a loaded object table entry unloaded are overlay 187 +071D, which has no call or pointer reference, and the eviction loop 31E0:0D09, which skips entries whose slot is below 0x30; script opcode 0x22 function 9 renumbers only unloaded entries
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007054D..0x0007060A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C0F9..0x0008C17C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D571..0x0008D63C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2FDA..2D40:2FF4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:1112..31E0:112B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:05BB..31E0:0AFC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:0094..289B:00F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0D09..31E0:0E1B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0F07..31E0:108C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0461..2D40:048D
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/immediate_search.py, direct_callers.py, overlay_listing.py, resident_listing.py); a Python byte search of DSUN.EXE
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. The table at the far pointer `DS:67B7` has 8-byte entries; the view draw `31BA:000E`
loads an entry whose word at `+6` is negative and stores the slot it gets there (FND-PARTY-130,
FND-PARTY-131).

`immediate_search.py <dsun> 67B7` lists 188 instructions. Decoding the 60 bytes after each
`les` of `DS:67B7` and keeping the writes to a word at `+6` gives four:

- `2D40:2FF0` stores the slot at `bp-2`, after copying the slot's state bytes to `4F49:0C33`
  (`2D40:2FDA..2D40:2FF4`).
- `31E0:1127`, in `31E0:0EFF`, stores the slot in `SI` (`31E0:1112..31E0:112B`).
- Overlay 187 `+07B3` stores the negated absolute value of the slot record's word at `+0x1B`,
  after `+071D` has written the slot's 37-byte record at `DS:67BB + slot * 0x25` through
  `37FC:00E8`; it then stores 0 in the record's word at `+1` and, when its third argument is not
  0, calls `2D40:2410` with the slot (`+071D..+07D9`).
- Overlay 204 `+03A9`, in function 9 of script opcode `0x22` (word 9 of the table at `cs:06C7`
  is `0x0339`, FND-PARTY-122).

**The eviction loop.** `31E0:0D09` takes a byte limit, capped at 6, and two flags. For each
entry of the table below the count at `DS:264E`, reached through a far pointer it keeps at
`bp-4`, it skips an entry whose word at `+6` is below 4 (`31E0:0D33..31E0:0D3A`) or below 0x30
(`31E0:0D93..31E0:0D9B`), and one whose position lies inside the rectangles at `DS:14D7..14DD`
and `DS:6563..6569` unless its second flag is set, or whose kind in bits 0 to 2 of `+5` is above
the limit. For the rest, when its third flag is set and the kind is below 6 it calls `31E0:05BB`
with the entry's index and 1, and otherwise it calls `31E0:0B4A` with the entry's word at
`+6` and, when that returns a value other than 0, stores at `+6` the negated word at `+0x1B` of
that slot's record (`31E0:0D9D..31E0:0DF5`).

**The load's slot.** `31E0:0EFF` takes its slot from its third argument and uses 0x13F for -1
(`31E0:0F07..31E0:0F1D`); the view draw passes -1 (`31BA:0128..31BA:0130`). For 0x13F it takes
a slot from `2D40:0461`, and only when that returns 9999 it calls `31E0:0D09` with the byte at
its `bp+0xC` and two zero flags (`31E0:1065..31E0:108C`). **`2D40:0461`** returns the result of
`2D40:0C3F` with `DS:56ED` when its argument is set, and when that gives 9999 or the argument is
0, with `DS:56F1` (`2D40:0461..2D40:048D`).

`direct_callers.py <dsun> 187+071D` finds its trampoline `56EF:0066` and no caller. A byte search
of the file finds no far call `9A 66 00 D8 05` and no `66 00 D8 05` or `68 D8 05` that would
pass the pointer.

**Function 9** (`+0339..+03BC`) takes the script's slot or object number in `SI`. When it is above
0 it calls `+17B1` with it and the dwords at `bp+0xA` and `bp+0xE`. Otherwise it calls `+17B1` for
each slot `2D40:0D94` returns until that returns 9999, and then, for each entry from 0 below the
word at `DS:264E` whose word at `+6` equals `SI`, stores the negated absolute value of its word at
`bp+0xA` there (`+0339..+03BC`). **`+17B1`** calls `31E0:05BB` with the slot record's word at `+1`
and 1, 1 and 1, then `31E0:0121`, and stores the result in `4C13:0363` when it is not -1
(`+17B1..+187C`). `31E0:05BB` (inventory row `31E0:05BB..31E0:0AFC`) holds no store to a word at
`+6` through `ES`; it stores 0 in an entry's byte at `+5` at `31E0:076F` and `31E0:096C`. Before
each of those it calls `289B:0094` with an entry number, the dword at `DS:67B7`, the count word
`DS:264E`, the slot records at `DS:67BB` and 0x140 (`31E0:074B..31E0:0760`,
`31E0:094E..31E0:095D`). **`289B:0094`** lowers the count by 1 and, when entries follow the one
named, moves them down by one entry with `rep movsw` and calls `289B:007A`
(`289B:0094..289B:00F0`); it writes no word of an entry other than by that copy.

## Interpretation

Outside a region load, the eviction loop `31E0:0D09` is the referenced code that turns a loaded
entry back into an unloaded one, and it leaves entries whose slot is below 0x30 alone, which
covers the four party slots (FND-PARTY-131). Overlay 187 `+071D` also does that, and nothing in
the file calls it or holds its address; opcode `0x22` function 9 only renumbers entries whose
word at `+6` is 0 or below. So the view draw that overlay 182 `+19F8` runs before a level gain
finds a party member's entry loaded, and the eviction it can start calls `31E0:0B4A` and
`31E0:05BB` only for slots of 0x30 and above, not for a party member's.

## Alternatives

- This replaces FND-PARTY-132, which said no referenced code unloads a loaded entry: its search
  kept writes within 60 bytes of a `les` of `DS:67B7` and missed `31E0:0DF5`, which writes through
  the pointer at `bp-4`. A write through another kept pointer is still not excluded.
- What the lists at `DS:56ED` and `DS:56F1` hold was not read; a party slot on them would let a
  load reuse it.
- `289B:007A`, which `289B:0094` calls with -1, the slot records and the removed entry's number,
  was not read; its arguments point it at the slot records, not the table.
- Overlay 187 `+071D` may be reached through a computed transfer that builds the trampoline
  address at run time.
- A region load rebuilds the table; whether a level gain can run between a load and the view
  draw that loads the party was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `immediate_search.py <dsun> 67B7`, then decode 60
bytes after each `les` hit; `direct_callers.py <dsun> 187+071D`; `overlay_listing.py <dsun> 187
7054D 7060A`, `overlay_listing.py <dsun> 204 8C0F9 8C17C` and `overlay_listing.py <dsun> 204 8D571
8D63C`; `resident_listing.py <dsun> 2D40:2FDA..2D40:2FF4 31E0:1112..31E0:112B 31E0:05BB..31E0:0AFC
289B:0094..289B:00F0 31E0:0D09..31E0:0E1B 31E0:0EFF..31E0:1095 2D40:0461..2D40:048D`; and search the file
for the bytes `9A 66 00 D8 05`, `66 00 D8 05` and `68 D8 05`.
