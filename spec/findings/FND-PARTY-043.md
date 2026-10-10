---
id: FND-PARTY-043
title: The call through [di+0x6393] picks one of four text-layout routines, and the call through the text record's +0x0C field is skipped once the record is set
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:639B..1BF3:6552
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:5CFD..1BF3:5D12
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:5945..1BF3:5956
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:00E8..39D1:00F1
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/resident_listing.py, overlay_listing.py, store_values.py, immediate_search.py, direct_callers.py, trampoline_target.py); scientific-method-engine 15.0.0 `reach` (tools/research/exec-census/reach_config.py with q011_6393_reach.json and q011_text_window_reach.json)
environment: null
---

## Observation

**`call [di+0x6393]`.** The call at file `0x1767E` is `1BF3:654E`, in the routine that starts at
`1BF3:639B`. That routine pushes DS and loads it from CS (`1BF3:63A1`, `63A2`). Up to the call,
DS changes only in two pairs that push it, load it with `lds si, cs:[0x1052]` and pop it back
(`1BF3:63A4..63B4`, `1BF3:6482..6492`), and in the call of `1BF3:599E`, which pushes and pops it
around its own `lds`. The index is the word at `1BF3:106E` doubled and masked with 7, so 0, 2, 4
or 6. The four words at `1BF3:6393`, just before the routine, are `659B`, `65DA`, `65BE` and
`6600`. A `reach` run from `1BF3:659B`, `65BE`, `65DA` and `6600` with the 29 targets of
FND-PARTY-040 (`q011_6393_reach.json`) reaches 16 routines and 1,628 instructions, no target, no
gap, and one unresolved call, `lcall es:[si+0xc]` at `0x16E3E`.

**`lcall es:[si+0xc]`.** The call at `0x16E3E` is `1BF3:5D0E`. At `1BF3:5CFD` the routine ORs the
two words at `ES:SI+0x0C` and `+0x0E` and jumps past the call when both are 0. `ES:SI` there is the
far pointer at `1BF3:1052`, which the routine reloads with `les si, cs:[0x1052]` after the call
(`1BF3:5D1B`). That pointer is 0 in the file. Its only stores are in `1BF3:5945`, which stores
its far argument. The eight direct calls of `1BF3:5945` pass `DS:A03D` seven times and once the
far pointer at `DS:A057` (`39D1:0623`). `DS:A057` is written only at `39D1:0600` and `0604`,
with what `1BF3:7AAB` returns: the far pointer at `1BF3:1052`.

`DS:A03D` is 0 in the file. The only instructions naming `0xA03D` as an immediate are the seven
pushes for `1BF3:5945` and two direct stores to its first word. The field at `+0x0C`, the double
word at `DS:A049`, is stored to only at `39D1:00E8`, which writes 0, in the graphics initializer
`39D1:0009` just before that routine's own call of `1BF3:5945` with `DS:A03D` (`39D1:013E`).
Nothing stores to `DS:A04B` by its address. No instruction in segment `1BF3` that writes memory
uses the displacement `0x0C` or `0x0E` with a base or index register, other than byte additions
that decode from other instructions' bytes. The two routines that load `ES:DI` from `1BF3:1052`
write only `+0x02` and `+0x04` (`1BF3:61F8`, `1BF3:6207`).

**Before the record is set.** `39D1:0009` is called only from overlay 180 `+04FE`
(`0x000676DE`), after the run of `MAS` 99. Until then `1BF3:1052` is 0. A `reach` run
(`q011_text_window_reach.json`) starts from the routines called before that point: those main
(`277D:0004`) calls before overlay 180 `+0141` (`1000:09B0`, `1000:1D38`, `1000:2C77`,
`1000:2E86`, `1000:3787`, overlay 180 `+0867`), the start-up records (FND-PARTY-040), every
routine overlay 180 `+0141` calls before `+04FE`, the five handlers `MAS` 99 runs and the hook
`2D40:37F3` (FND-PARTY-041), and the pointer values of FND-PARTY-042's run. Its leaves are the
exit worker `1000:0388`, the script error routine overlay 188 `+1901` (FND-PARTY-042), and overlay
199 `+0C21`, which the hook calls only while `DS:143D` is nonzero (FND-PARTY-041). Its target is
`0x16E3E` and its control the call site `0x0006888D`. It reaches 444 routines and 19,362
instructions without stopping at the limit, does not reach `0x16E3E`, and leaves 35 unresolved
calls: `0x5546`, `0x5C37`, `0x7110`, `0x71BC`, `0x772C`, `0x7B8A`, `0x9A3C`, `0x9B83`, `0xA04A`,
`0xA09A`, `0xA219`, `0xAAA1`, `0xAAFB`, `0xB704`, `0xB79C`, `0xB7C9`, `0xC667`, `0xC675`,
`0x3C6D9`, `0x3CB07`, `0x3F007`, `0x3F071`, `0x3F57D`, `0x3FDBB`, `0x3FDE2`, `0x4041B`,
`0x40FA5`, `0x40FB0`, `0x40FBD`, `0x40FCB`, `0x40FF5`, `0x40FFF`, `0x4111B`, `0x41129` and
`0x41270`. With only the exit worker as a leaf it reaches `0x16E3E` through the error routine,
and with the error routine also a leaf, through the hook and overlay 199 `+0C21`.

## Interpretation

`call [di+0x6393]` runs with DS equal to `1BF3` and calls one of `1BF3:659B`, `65BE`, `65DA` or
`6600`, which reach none of the targets. Their only further indirect call is the one at
`0x16E3E`. The text record that call reads is `DS:A03D` from `39D1:0009` on, and that record's
`+0x0C` field is set to 0 there and never written again, so from then on the call is skipped.
Before `39D1:0009` runs, the call is not reached from the routines that run first, except
through the script error routine or overlay 199 `+0C21`, which FND-PARTY-041 and FND-PARTY-042
exclude before the gate, or through the 35 calls the run leaves unresolved. `0x7B8A`
(`DS:3916`) and `0x4041B` (`DS:0086` in the overlay manager's data segment) are not in
FND-PARTY-039's table, and the `DS:3F42` record values were not added as starts.

## Alternatives

- A write through a pointer that a search by displacement does not see, such as a block copy
  into `DS:A03D`, sets the field: the immediate search finds no other use of the record's
  address, but a pointer derived from `1BF3:1052` and copied elsewhere is not covered.
- While `1BF3:1052` is 0, a draw reads `0000:000C`, the interrupt 3 vector, as the field: that
  happens only if a draw routine runs before `39D1:0009`, which the second run reaches only through
  the routes named above.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `resident_listing.py <dsun>
1BF3:639B..1BF3:6640 1BF3:599E..1BF3:59C1 1BF3:5CDE..1BF3:5D50 1BF3:5938..1BF3:5960
1BF3:7AAB..1BF3:7AC0 1BF3:61F8..1BF3:6230 39D1:00D0..39D1:0110 39D1:05E0..39D1:0640
277D:0004..277D:0264`; `overlay_listing.py <dsun> 180 67321 676E3`; `store_values.py <dsun>
1052 1054 A049 A04B A057 A059`; `immediate_search.py <dsun> A03D A049 A04B 1052`;
`direct_callers.py <dsun> 1BF3:5945 1BF3:7AAB 39D1:0009 180+0141`; and `trampoline_target.py
<dsun> 5702:00B6 5773:0020 56B2:0034`. Then, in the repository root, for
`q011_6393_reach.json` and `q011_text_window_reach.json` run `reach_config.py` and `python -I -m
scientific_method_engine reach` as FND-PARTY-040 gives.
