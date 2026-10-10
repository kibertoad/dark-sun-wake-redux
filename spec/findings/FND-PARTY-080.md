---
id: FND-PARTY-080
title: Three of the four callers of overlay 184 +1DCE pass the records of the slot in the word at 4E71:0B44, and on the fourth, the DUAL path of overlay 209 +123E, the calls before +1DCE reach a store to that word only through overlay 172 +05DB and overlays 173, 188 and 210
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007930C..0x00079349
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079479..0x000794BD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E696..0x0006E6D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D394..0x0006D3C7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000985D5..0x00098608
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009439E..0x00094664
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059A0B..0x00059A84
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093160..0x00093174
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093378..0x0009338B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00093BE4..0x00093BF7
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py, trampoline_target.py); scientific-method-engine 18.1.0 `reach` (tools/research/exec-census/reach_config.py with the query q026_dual_reach.json)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 184 `+1DCE` copies the details and combatant records it is given to
`4E4F:0029` and `4E4F:006B`, and takes the class bytes from the details record numbered by the
word at `4E71:0B44` (FND-PARTY-073). `direct_callers.py` finds four calls of it:

- Overlay 184 `+1639`, in `+1606`. Overlay 190 `+0FFA..+1004` pushes the word at `4E71:0B44` and
  calls `+1606` through trampoline `56DD:0048`, the only call of `+1606` found, when the byte at
  `+0x14` of that slot's combatant record is 0. `+1606` stores its argument in `DS:112E`, 0 in
  `DS:42C0`, and passes the details and combatant records numbered by its argument to `+1DCE`
  (`+160D..+1639`), with no other call or store between.
- Overlay 184 `+0334` and overlay 212 `+0403` push the combatant and details records numbered by
  the word at `4E71:0B44` read just before the call (`+0304..+0331`, `+03D5..+0402`).
- Overlay 209 `+14FF`, in the routine at `+123E` (trampoline `57B0:0043`), pushes the records
  numbered by its own argument (`+14DD..+14FE`). Overlay 190 `+1178` is the only call of that
  routine found; it pushes the word at `4E71:0B44` and runs when the slot's details record has a
  level byte at `+0x1E` of 2 or more and origin byte 1, the case for which overlay 190 offers the
  label `DUAL` (`+0F19..+0F4C`, `+1139..+1178`).

A search of the file for an ES-overridden store to offset `0x0B44` (`mov` from any register,
`mov` of an immediate, `add`, `sub`, `inc`, `dec`, `pop`) finds 11, at file `0x58342`,
`0x6D874`, `0x75CBB`, `0x78933`, `0x7BA2D`, `0x8A743`, `0x93386`, `0x93BF2`, `0x9316F`,
`0x9614A` and `0x98828`. The last is overlay 212 `+0628`, at the start of the routine at `+061C`,
which stores its argument at `[bp + 0x0E]` there.

**The DUAL routine's calls.** Between its entry and `+14FF`, overlay 209 `+123E` stores nothing
at `4E71:0B44` and makes these calls: overlay 209 `+19A7` (near); overlay 172 `+05DB` (trampoline
`566A:002A`); `28C9:2ABD`; overlay 190 `+11C1` (`571F:0039`) and `+11A1` (`571F:0034`); overlay
182 `+1FDB` (`56BD:0089`) and `+016B` (`56BD:0048`); overlay 183 `+191D` (`56CC:0043`); overlay
186 `+00D4` (`56E9:002A`); overlay 183 `+189B` (`56CC:003E`); `401B:0172` three times; overlay
183 `+1827` (`56CC:002F`) and `+1881` (`56CC:0039`); and overlay 212 `+0A36` (`57C9:0020`), at
`+14D5` (`+126F..+14D5`).

**The run.** `q026_dual_reach.json`, built by `reach_config.py` as in FND-PARTY-040 (205
regions, 7,562 far transfers, 157 jump tables). Starts: the 14 routines above. Targets: the 11
stores. Leaf: the exit worker `1000:0388` (FND-PARTY-039, FND-PARTY-043). Control: the far call
at `0x6C929` in overlay 183 `+191D`. It reads 937 routines and 77,126 instructions without
stopping at its limit, reaches the control, reports no gap and no contested instruction, and
leaves 34 calls unresolved: the run-time, `DS:3F42`, `DS:02F6` and `DS:030A` calls and the
formatter calls of FND-PARTY-039, the window, `APFM` and `DS:A0F1`/`DS:A0F5` calls of
FND-PARTY-044, `0x16E3E` (FND-PARTY-043), two `call bx` at `0x1DE0E` and `0x1DE19`, `lcall
[0xa119]` at `0x336C1` and `0x336F0`, and `lcall` through `DS:348A`, `DS:3475`, `DS:3479`,
`DS:3471` and `DS:346D` at `0x3CB07`, `0x3F007`, `0x3F071`, `0x3FDBB` and `0x3FDE2`. It reaches
three targets, all in overlay 209: `0x9316F` (`+000F`), `0x93386` (`+0226`) and `0x93BF2`
(`+0A92`). Every read route to each passes through overlay 172 `+05DB`, `2C5F:0139`, `31BA:000E`,
`31E0:0EFF`, overlay 173 `+22C4` and `+30C7`, overlay 188 `+1604`, and overlay 210 `+0B44`,
`+08BC` and `+0740`, and for the last two overlay 209 `+0218` or `+0A84`. The fewest-call chains,
of 20 to 22 calls, go from overlay 172 `+05DB` through overlay 182 `+1856`, called at overlay 172
`+064F`.

**The three stores.** Overlay 209 `+0000`, `+0218` and `+0A84` each store their first argument
at `4E71:0B44` at their start (`+0007..+000F`, `+021E..+0226`, `+0A8A..+0A92`).

**Overlay 172 `+05DB`** loops while `DS:40CD` is nonzero, building events for the window at
`4C4D:0007` with `39D1:097D` and passing each to `+0816`; it then stores 0 in `DS:40CC`, and
unless `28C9:2522` returns 2 stores 1 in the byte at `4C4D:0006` and calls overlay 182 `+1856`
(`+05E1..+064F`). Overlay 182 `+1856` returns at once while `DS:0DAB` is 0 (FND-PARTY-029).

## Interpretation

On the edit path of overlay 190 (`+1606`) and on the calls from overlay 184 `+0334` and overlay
212 `+0403`, the class bytes `+1DCE` reads are those of the character whose records it copies.
On the DUAL path the word can change before `+1DCE` only through overlay 172 `+05DB`, and from
there through the routines of overlays 173, 188 and 210 that every route shares to one of three
overlay 209 routines that store their argument in the word. The shortest route goes through
overlay 182 `+1856`, which returns at once while `DS:0DAB` is 0; the other routes were not
followed. Whether the stored argument is another slot, and whether the word is set back before
`+14FF`, depends on that chain.

## Alternatives

- FND-PARTY-064 recorded the same readings of the four callers and the 11 stores, but named the
  overlay 212 routine the DUAL path calls at `+14D5` as `+0020`, the offset of its trampoline in
  segment `57C9`; the routine is `+0A36`. It did not follow the DUAL path's calls.
- The 34 unresolved calls add routes: the run treats them as reaching nothing.
- The value of `DS:0DAB` while overlay 190 offers `DUAL`, and the routes that avoid overlay 182
  `+1856`, were not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `direct_callers.py <dsun> 184+1DCE 184+1606
209+123E`; `overlay_listing.py <dsun> 190 79190 79350`, `190 79479 794C0`, `184 6E696 6E6D8`,
`184 6D390 6D3CC`, `212 985C8 9860C`, `212 9881C 98830`, `209 9439E 94668`, `209 93160 93178`,
`209 93378 9338E`, `209 93BE4 93BFA` and `172 59A0B 59A90`; and `trampoline_target.py <dsun>
566A:002A 571F:0039 571F:0034 56BD:0089 56BD:0048 56CC:0043 56E9:002A 56CC:003E 56CC:002F
56CC:0039 57C9:0020`. Search the file for the byte 0x26 followed by an opcode and ModRM byte of
the listed stores with a direct address and then the bytes `44 0B`. Then, in the repository root,
run `reach_config.py` and `python -I -m scientific_method_engine reach` for
`q026_dual_reach.json` as FND-PARTY-040 gives.
