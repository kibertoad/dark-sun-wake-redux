---
id: FND-MAGIC-011
title: DS:264E is written by the placed-object insert and remove helpers, three recounts (the SAVE reader, the region change and the ETAB load) and two direct increments; a load leaves it at the count saved, or unchanged when a read fails, before the Load window calls overlay 182 +1167 with 0, so the party loader runs after a load only when the saved count, or the count before a failed load, is 0
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:0004..289B:00F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:00A2..31E0:0121
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:01DD..31E0:01E7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:03CA..31E0:044C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:074B..31E0:0780
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0948..31E0:0971
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:069E..371D:06C9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007089C..0x000709D7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070CEE..0x00070DE7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071043..0x000710BE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089242..0x000892E6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068BB5..0x00068C51
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069118..0x000692CC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000699B7..0x00069B53
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069E27..0x00069F2D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007DEC3..0x0007DF3A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004F64E..0x0004F650
tool: tools/research/exec-census/immediate_search.py, resident_listing.py, overlay_listing.py and direct_callers.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7; a short read of the word at file 0x4F64E and a search of each listed resident segment for the nearest preceding bytes 55 8B EC
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x68850 + x` for overlay 182, `0x6FE30 + x` for 187, `0x7D300 +
x` for 192 and `0x89160 + x` for 200 (FND-EXE-623). The placed-object table is the 8-byte
entries under the far pointer `DS:67B7`, of which `DS:264E` counts the ones in use (FND-ACTOR-004).

**The uses.** `immediate_search.py <dsun> 264E` prints 89 hits. Dropping the ones that start
inside another instruction (`gs:`-prefixed reads, the `push 0x264e` printed one byte before each
real one, and overlay 187 `0x7077C`), the instructions that write the word directly are:

| Site | Instruction | In routine |
| --- | --- | --- |
| `31E0:01E3` | `inc word [264E]` | `31E0:0121`, which appends a placed entry (FND-ACTOR-004) |
| `31E0:0404` | `inc word [264E]` | `31E0:03CA`, whose one direct caller is overlay 193 `+095B` |
| overlay 187 `+0B81`, `+0B95` (file `0x709B1`, `0x709C5`) | `mov word [264E], 0`, `inc` | the `SAVE` reader `+0A6C` |
| overlay 187 `+1270`, `+1278` (file `0x710A0`, `0x710A8`) | `mov word [264E], 0`, `inc` | the region change `+1132` |
| overlay 200 `+0137`, `+0170` (file `0x89297`, `0x892D0`) | `mov word [264E], 0`, `inc` | `+00E2` |

No add, sub, dec, xchg or pop has `0x264E` as its destination; the "add or subtract forms" FND-PARTY-021
counted are reads such as `sub ax, word [0x264e]` (printed at the normalised address `32BC:000C`). Four
instructions push the immediate `0x264E`, the word's address: `31E0:0103`, `31E0:0752`,
`31E0:094F` and `371D:06A5`. No `mov` loads it into a register.

**The writers through the pushed address.** Each push is the third word pushed before a far call
into segment `289B`, after `push 0x140; push ds; push 0x67bb` and before `push dword [0x67b7]`:

- `31E0:0103` before `289B:0004` (`31E0:0110`), in `31E0:00A2`. `289B:0004` loads the pointer
  from `[bp+0xE]`, keeps the count and stores it plus 1 (`inc word [si]` at `289B:0015`), then
  opens a gap in the 8-byte table by a binary search (`289B:00F0`) and copies the new entry into
  it (`289B:0052..+006E`). `31E0:00A2` returns -1 without inserting when the count is 1,500 or
  more (`31E0:00A9..+00B4`) and sets bit 3 of the entry's byte 5 (`or al, 8` at `31E0:00CB`).
  `direct_callers.py` finds one caller of `31E0:00A2`, overlay 187 `+1525` in `+146C`.
- `31E0:0752` and `31E0:094F` before `289B:0094` (`31E0:075B`, `31E0:0958`), both in the unload
  routine `31E0:05BB` (FND-PARTY-118), and `371D:06A5` before `289B:0094` (`371D:06B0`), in
  `371D:050F`. `289B:0094` does nothing when the count is 0, otherwise stores the count less 1
  (`289B:00A9..+00AA`) and closes the gap; each caller then stores 0 in byte 5 of the entry at the
  new count (`31E0:0763..+076F`, `31E0:0960..+096C`, `371D:06B8..+06C4`).
  `direct_callers.py` finds the callers of `371D:050F` at overlay 193 `+05B6` and `371D:04C2`.

`289B:0154` (called at `31E0:0442` with the count's value) passes `289B:0004` and `289B:0094` a
local copy (`lea ax, [bp-2]`), not `DS:264E`. `direct_callers.py` finds no other caller of
`289B:0004` than `31E0:0110` and `289B:01CF`, and no other of `289B:0094` than the three above and
`289B:018D`, both inside `289B:0154`.

Every inserted entry has bit 3 of byte 5 set: `31E0:0121` stores `(flags & 7) + 0x48`
(`31E0:0152..+0159`), `31E0:00A2` ors in 8, `31E0:03CA` ors in 8 and 0x60 (`31E0:03F2..+03FE`).

**The three recounts.** Each stores 0 and then counts the entries from the start of the table
while byte 5 has bit 3:

- the reader `187 +0A6C` (FND-SAVE-015), only when its success byte `[bp-1]` is still 1 after
  the loop (`+0B78..+0BA5`). Before the loop, for the table entry whose pointer equals
  `DS:67B7`, it fills `0x2EE0` bytes there through `1000:3FA2` (`+0AC3..+0ADC`), so the entries
  after those read from `SAVE` have byte 5 equal to 0. The writer `+08E3` gives that entry the
  length `8 * DS:264E` (FND-SAVE-015).
- the region change `187 +1132`, after its call of the reader at `+121B`, whether the reader
  succeeded or not (`+1221..+1229`, `+1268..+128C`).
- overlay 200 `+00E2(n)`, which returns unless `DS:265B` is set and `n` is 1 to 99, fills the
  table through `1000:3FA2`, reads `ETAB` `n` (`0x42415445`) into it through `38FF:04AB`, and on
  success counts the entries with a nonzero word `+6` while setting bit 3 of each, up to 1,500
  (`+00EC..+0184`). `direct_callers.py` finds its callers at overlay 182 `+0456` and `+0539`,
  both in `+0365`: `+0539` is the far call FND-PARTY-021 reads on the path where `DS:264E` is 0.

The word at file `0x4F64E`, `DS:264E` in the load image, is 0.

**The load path.** The Load window's confirm branch (FND-PARTY-045) calls the Load routine
`192 +05ED` (`+0BDF`), then, unless `DS:627C` is set, overlay 182 `+19F8` (`+0C21`) and
`28C9:2522` (`+0C26`), and when that returns anything but 1 overlay 182 `+1167` with 0 (`+0C30..+0C32`).
`direct_callers.py <dsun> 182+1167` finds only this call and overlay 194 `+01B5` (Start Game).

Inside `+05ED`, `187 +0EBE` (`0x70CEE..0x70DE7`) returns before the reader when the copy over
`DARKRUN.GFF` fails. Otherwise it calls `182 +15D7(DS:140C, 0, 1, 0)`, the reader `+0A6C(0)` at
`+0F2C`, and `182 +15D7(DS:140C, 0, 0, 0)` at `+0F9C` (FND-SAVE-022).

- `+15D7` with its third argument 1 sets `DS:0DAB` to 4 and tears down the map display when
  `DS:0DAB` is nonzero, and otherwise calls `3D72:0B84` only for a third argument of 0; neither
  branch calls `+0365` (`+15E8..+168C`).
- With its third argument 0 and `DS:0DAB` equal to 4, `+15D7` calls `+0365` with B, its second
  argument, equal to 0 (`+16BE..+16C7`), and `+0365` then jumps past both count tests to its exit
  (`+03EE..+03F4`). Before that test `+0365` calls only `49E9:00FD`, `49E9:0142`, its own `+0297`,
  `362C:01FA` and `3796:0387` or `3796:03BF` (`+0370..+03E8`). After it `+15D7` calls `200 +0386`,
  `187 +22B3`, `182 +08C8`, `200 +01D9` and `182 +0840` (`+16DD..+1703`).
- `182 +08C8` walks the entries below `DS:264E` with bit 3 set and, for an entry whose object is
  `0x1AE`, places object `0x1AF` through `31E0:0121` (`+09FD..+0A31`). It can only raise the count.
  `+0840` reads two resources through `38FF:04AB` and returns (`+0840..+088A`).

In `+1167`, before the test at `+12DD`, the calls are `187 +2B6C` and `+236F` when `DS:13F7` is
set, `+016B`, `190 +11A1`, `+01EF` twice, `39D1:0448`, `362C:0000`, `1BF3:27A8` twice, `+0840`,
`200 +0386` and `3D72:0B84` (`+116E..+12BD`). None of the routines named in this section holds
one of the direct writers or one of the calls of `289B:0004` and `289B:0094` above, and none is a
direct caller of `31E0:00A2`, `31E0:03CA` or `200 +00E2`. Of the 21 direct callers of `31E0:0121`,
overlay 182 has `+04E0` (in `+0365`, past the B test), `+06FF` (the party loader), `+0A31` (in
`+08C8`, above) and `+1363` (in `+1167`, after `+1300`). The 29 callers of `31E0:05BB` include no
site in overlays 182 or 192 and, in overlay 187, `+1119` (in `+0FB7`), `+11FC` (in `+1132`),
`+1451` (in `+135B`), `+1A68` (in `+1A39`) and `+1BD4` (in `+1B67`), whose own callers are the
region change, overlay 188 and resident code.

## Interpretation

`DS:264E` is the count of entries at the head of the placed-object table with bit 3 of byte 5
set. It is raised by the two append routines and the insert, lowered by the two remove routines,
and recounted from the table by the `SAVE` reader, the region change and the `ETAB` load. It is
not part of any `SAVE` resource; the save writes the first `8 * DS:264E` bytes of the table, and
a load that reads all 61 resources recounts it from them, so the count after a successful load
is the count at the save.

The Load window's confirm calls `+1167` with 0 after the load, and `+1167` reaches the party
loader only when the count is 0 at `+12DD` (FND-PARTY-021). Between the reader's recount and that
test, the code read here only raises the count (`182 +08C8`). So a load refills the pools of slots
0 to 3 through the loader only when the saved game had no placed entries. The party loader
places the four slots through `31E0:0121` and the `ETAB` load fills the region's objects
before it, so a game saved after Start Game holds placed entries unless every one was removed
before the save; this reading does not show a way to remove them all.

When the copy over `DARKRUN.GFF` fails, or one of the 61 reads fails, the count keeps the value it
had before the load. From the title screen that is 0 at start-up, since nothing on the title
path read by FND-PARTY-045 runs a writer; such a failed load then reaches `+1167` with a count of
0, which reaches the loader if `+1167` passes its `DS:0DAB` test and its two `1BF3:27A8` calls.

## Alternatives

- A resident routine called on the load path reaches `31E0:05BB` or `371D:050F` and empties the
  table between the recount and `+12DD`: the resident callees of the routines named here (`2D40`,
  `28C9:2522`, `3D72`, `39D1`, `362C`, `1BF3`, `49E9`, `3796`, `444C`) were not walked, and
  `31E0:05BB` has resident callers at `27AF:0004` (normalised), `31E0:0DC3` and `371D:0714`
  (normalised `378E:0004`). A reach run from overlay 192 `+0BC4` to `+0C32` with these two
  routines and the six direct writers as targets would settle it.
- The table is written through a pointer the search does not name: only immediates and
  displacements equal to `0x264E` were searched. A block copy or fill whose destination range
  covers `DS:264E` was not searched for.
- A save made with a count of 0: whether play can reach a save with no placed entry was not
  read.
- What `28C9:2522` returning 1 means, which skips `+1167` after a load, was not read
  (FND-PARTY-045 leaves it open).
- Whether `DS:0DAB` is 0 at `+11BE` after a load, which `+1167` needs to reach the gate at all,
  was not read; `+15D7` sets it to 4 when it was nonzero.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`: `immediate_search.py <dsun> 264E`, keeping the instruction starts; `resident_listing.py
<dsun> 31E0:00A2..31E0:0180 31E0:01A0..31E0:0200 31E0:03C0..31E0:0450 31E0:0730..31E0:0790
31E0:0930..31E0:0980 289B:0000..289B:0200 371D:069E..371D:06C9`; `overlay_listing.py <dsun> 182
68BB5 68C64`, `182 69090 693A2`, `182 699B7 69B53`, `182 69E27 69F60`, `187 7089C 70962`, `187
70960 709F0`, `187 70CEE 70F62`, `187 71040 710E0`, `192 7D8ED 7DA80`, `192 7DE40 7DF80` and `200
89242 895CF`; `direct_callers.py <dsun> 182+1167 182+063D 289B:0004 289B:0094 31E0:0121 31E0:03CA
31E0:00A2 31E0:05BB 371D:050F 187+0A6C 187+1132 187+135B 187+1A39 187+1B67 187+146C 187+0FB7
200+00E2`. Read the word at file `0x4F64E`. For `371D:050F`, search back from file `0x37880 +
0x5200 - 0x10000` for the bytes `55 8B EC`.
