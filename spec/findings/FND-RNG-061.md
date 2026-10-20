---
id: FND-RNG-061
title: No call or interrupt the two DS censuses list as made with DS changed leads to overlay 193 +003C, overlay 201 +05D5 or overlay 204 +1F06 except the main routine's call, so DS equals SS at their entry after startup under the censuses' rules
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1D6F..1000:1D96
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1DE6..1000:1DFC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000574CE..0x000574D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2877..1000:2883
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2984..1000:298E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2707:0182..2707:018D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0001C3FD..0x0001C41D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2707:0292..2707:02A8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:40D2..1BF3:40E4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0426..4842:0437
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:04AA..4842:04B4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5664:0020..5664:0034
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008337B..0x00083384
tool: tools/research/exec-census/ds_restore_census.py, walk_routine_table.py, reach_config.py, segment_register_writes.py, window_operands.py, direct_callers.py, resident_listing.py and overlay_listing.py at the commit that adds this finding; scientific_method_engine 19.10.0 reach; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. FND-RNG-056's census of the 255 routines of the
coverage inventory that write `DS` lists 166 calls and 92 `INT`s made with `DS` `other`, and 24
routines that can return with it changed. The inventory lacks part of the code FND-RNG-058's walk
from the 21 established entries decodes: 27,443 of its 165,558 instructions lie outside every
inventory range, and 76 `DS` writes in 32 routines among them (in the overlay manager `4AE5`,
`1000:1D66`, `1000:2877`, five routines from `1000:29AA` to `1000:2B6A`, `1BF3:639B`, `289B:02AE`,
`289B:02BD`, two in `44B6`, `4842:040C`, and four of overlay code starting at file offsets
`0x9ACD0` to `0x9ADC0`).

**Second census.** `walk_routine_table.py` writes the walk's 2124 routines and their decoded
ranges as a routine table, and `ds_restore_census.py` run over it reports 210 routines with 538
`DS` writes, 213 walked in 3 rounds and 19 that can return with `DS` `other`. Six of the 19 are
not among FND-RNG-056's 24: `1000:0000` (the walk's routine includes the exit path at
`1000:02AD`), `277B:0005` (which far-calls the critical-error resume `1000:1D4A`), the two
targets `289B:02AE` and `289B:02BD` of `289B:01DC`'s `call bx`, and `4AE5:0263` and `4AE5:0466`
of the overlay manager. Eleven of FND-RNG-056's 24 are not among the 19: `1000:02AD`,
`1BF3:438D`, `1BF3:4404` and `1BF3:4C91` lie inside the walk's routines `1000:0000`,
`1BF3:42FB`, `1BF3:43AE` and `1BF3:4C09`, and the walk does not decode `1000:1622`, `45CD:0138`,
`1BF3:40DD`, `1BF3:5C17`, `4842:182E`, `2707:02A8` or `2707:0395`. The sound driver functions of
the table at `4842:0D7E`, which the game enters through the far return at `4842:040A`
(FND-EXE-585), are inventory rows the first census walks, and neither census flags a call of
`4842:03F4`.

**Reach queries.** Each query copies FND-RNG-050's (its 112 computed-transfer declarations, the
table jump and the call that does not return), with overlay 193 `+003C`, overlay 201 `+05D5` and
overlay 204 `+1F06` as its only targets, no leaves, and as positive control the start's first
direct call or, without one, its first return. The starts are:

| Starts | Source |
|---|---|
| 65 | the direct targets of the first census's flagged calls in the load image, `2778:0006` and `4237:0003` among them |
| 22 | `1BF3:738F`, which overlay 170 `+0040` and `+019D` far-call; the declared targets of the first census's flagged indirect calls (`4AE5:1257`, `4AE5:0EA2`, `4AE5:1155`, `1000:02FC`, `1BF3:59C1`, `1BF3:5E12`, the ten of the table at `1BF3:61B8`, `289B:02AE`, `289B:02BD`, `45CD:011A`, whose `call bx` at `45CD:00BB` loads `BX` from the local word set to 0x11A at `45CD:006A`), and the `INT 23` and `INT 24` handlers `1000:0338` and `1000:1D66` |
| 16 | the routines whose paths leave their ranges with `DS` `other`, FND-RNG-056's unread changers other than `1BF3:40DD`, `1000:1F50`, and `1000:0163`, the exit after the main call |
| 26 | the second census's flagged callees not queried above, and the four targets of `1BF3:654E`'s table at `CS:6393` |
| 4 | the timer callbacks `4654:09A9` and `31E0:31A7` (FND-EXE-585) and the DIGPAK callbacks `4842:105F` and `4842:107E` (FND-EXE-591) |

Six starts are a lone `retf` or `iret` and have no control; every other query reaches its
control, and none has a gap or a contested start. Only the walk from the
main routine `277B:0024`, which `1000:0000` far-calls at `1000:0158`, reaches the three entries.
The walks from `1000:0220` and `1000:1D66` each leave one transfer unresolved, and that from
`277B:0024` two, the far table jumps in segment `2707`.

**Readings.** `1BF3:40DD`, one of FND-RNG-056's unread changers, starts at the second byte of
the instruction `add di, dx` at `1BF3:40DC` in `1BF3:3C90` (`1BF3:40D2..1BF3:40E4`), whose
inventory row ends at `1BF3:40DC`. A walk from `1BF3:3C90` decodes `1BF3:40DC`, `1BF3:40E4` and
the routine's `retf` at `1BF3:423E` and reaches no target; the query from `1BF3:40DD` fails its
controls on contested instructions. FND-RNG-058's walk reaches neither. Overlay 196 `+040B`,
the third timer callback, adds 1 to a word and returns with no call (`0x8337B..0x83384`). The flagged far jump at `2707:0188`
indexes eight far pointers at `2707:018D` (`0x1C3FD..0x1C41D`) by its fifth argument, below 8
(`2707:0182..2707:018D`); each target, `2707:01AD` to `2707:026E`, reaches `2707:0292` with no
call, and `2707:0292` pops the saved registers and `DS` and returns (`2707:0292..2707:02A8`). The
critical-error handler `1000:1D66` loads `DS` with 0x57E0 and calls through `DS:A4D0` only when
the word at `DS:A4CE` is not 0 (`1000:1D6F..1000:1D96`); `DS:A4CE..A4D4` hold 0 in the file
(`0x574CE..0x574D4`), and are written, with 1 and a far pointer, only at `1000:1DEF`,
`1000:1DF2` and `1000:1DF6` in `1000:1DE6` (`1000:1DE6..1000:1DFC`), which has no direct caller and is not on the walk.
`1000:2877` pops its return address into `DS:3916..391A` (`1000:2877..1000:2883`), the walk's
only writes there, and leaves through `jmp [0x3916]` at `1000:298A` (`1000:2984..1000:298E`).
The four `INT 3F` and the `DS` writes the first census finds at `5664:0025`, `5699:003E`,
`56A2:002A`, `56BD:0089` and `5773:002F` decode the bytes of the trampoline table, three-byte
operands after each `int 0x3F` (`5664:0020..5664:0034`); the second census has none there.
`segment_register_writes.py` over the walk's table finds `SS` written, besides FND-RNG-053's
sites, only in the timer interrupt handler `4842:040C`, which saves `SS` and `SP` in its code
segment before loading its own stack (`4842:0426..4842:0437`) and loads them back before its
return (`4842:04AA..4842:04B4`). `4842:182E` pushes `DS` at `4842:1834` and pops it at
`4842:1866` before its `retf`.

## Interpretation

`DS` changes only through the writes the two censuses walk, and each census flags every call,
`INT` and path exit made after a write it cannot show restores `DS`. Every flagged call leads only
to code that reaches none of the three entries, except the main routine's call, made with `DS`
and `SS` both 0x57E0 (FND-RNG-056); the `INT`s other than `23` and `24` go to services outside the
game's code, and the handlers that call back into it reach none of the entries. So on the paths
after startup `DS` at the entry of overlay 193 `+003C`, overlay 201 `+05D5` and overlay 204
`+1F06` is the main routine's, equal to `SS`, and the clock helper's wrappers store into its frame
(FND-RNG-053). The unread changers only add flags under the censuses' rules, and their walks
reach no entry. The unresolved jump at `1000:298A` goes to the return offset `1000:2877` saved,
and the call at `1000:1D8D` is not made on the walk, since its flag stays 0.

## Alternatives

- Each census keeps `DS` as `entry`, `data` or `other` under the rules its docstring lists; a
  rule that marks a changed `DS` as restored would hide flags.
- Code behind the walk's nine unresolved transfers is outside the second census; FND-EXE-589
  finds it in the file and never run, or in the XMS driver, a handler installed before the
  program or sound driver code.
- Driver code the game loads from resources is not read, apart from the callbacks FND-EXE-591
  names.
- No emulated call or run was made; the overlays are outside the resident harness.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, `<dsun>` the installed
`DSUN.EXE` and `<walk>` FND-RNG-058's walk report: `walk_routine_table.py <dsun> <walk>
<table>`, `ds_restore_census.py <dsun> <table>` (about ten minutes) and
`segment_register_writes.py <dsun> <table>`; for each start above, a copy of FND-RNG-050's query
with that start, the three targets and its control, run with `reach_config.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> <query>` and `python -I -m
scientific_method_engine reach <config>`; `window_operands.py <dsun> <walk> A4CE..A4D8
3916..3918`; `direct_callers.py <dsun> 1000:1DE6 1000:2877`; `resident_listing.py <dsun>
1000:1D66..1000:1E12 1000:2877..1000:298E 2707:0109..2707:02A8 1BF3:40C0..1BF3:4240
4842:040C..4842:04E0 4842:182E..4842:186B 5664:0000..5664:0040 45CD:0038..45CD:00D0`; `overlay_listing.py <dsun> 196
8337B 833C2`. Queries, reports, tables and listings are kept under
`GAME_DIR/analysis/rng-dsreach-20261020/`. Nothing was executed.
