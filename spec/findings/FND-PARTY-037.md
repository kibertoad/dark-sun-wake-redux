---
id: FND-PARTY-037
title: No direct route from program start to the party-loader gate reaches a call that makes the pointer an image other than an ICON
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0338..1000:0361
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0364..1000:0382
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067A28..0x00067A83
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067329..0x00067342
tool: scientific-method-engine 15.0.0 `reach` with Python 3.14.7, Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/reach_config.py with the query tools/research/exec-census/pointer_reach.json; direct_callers.py, overlay_listing.py, resident_listing.py, trampoline_target.py, immediate_search.py)
environment: null
---

## Observation

**The query.** In the installed `DSUN.EXE`, the engine's `reach` command was run with these
values (file offsets, from overlay code offsets as `descriptor+offset`):

- Starts, the 40 routines FND-PARTY-031 starts from: `0x53B0`, `0x5420`, `0x5BB0`, `0x6F38`,
  `0x7E77`, `0x8086`, `0x8987`, `0x3F531`, overlay 180 `+0141`, `+0867` and `+08E5`, overlay 187
  `+2546` and `+28B3`, overlay 194 `+037B`, `0x26434`, `0x2F88D`, `0x5899`, `0x2F62F`, `0x2CFDC`,
  `0x263BD`, `0x2172A`, `0x1D54C`, `0x3B361`, `0x264BF`, `3EBE:071F`, `28C9:2A81`, overlay 194
  `+0504`, overlay 187 `+2424`, `+206E` and `+2B6C`, overlay 182 `+236F`, `+016B`, `+01EF` and
  `+0840`, overlay 190 `+11A1`, overlay 200 `+0386`, `39D1:0448`, `362C:0000`, `3D72:0B84` and
  `1BF3:27A8`.
- Leaves, as in FND-PARTY-031, with the reason that each returns before any call while
  `DS:0DAB` is 0 (FND-PARTY-029): overlay 182 `+1856` and `+19F8`.
- Targets: the two pointer-image setters `3D72:12ED` and `3D72:120B`, and the 14 sites of
  FND-PARTY-036: `2C5F:0CBF`, `2C5F:0CF1` (the store that makes `2C5F:0C8D`'s flag 0),
  `0x0001EA07`, `0x00063C14`, `0x00075C7D`, `0x000777F7`, `0x00076BE1`, `0x0007714E`,
  `0x00090BE1`, `0x0009106C`, `0x000910C6`, `0x0006A3FB`, `0x00092B67` and `0x00092B95`.
- Positive controls: the near call at `0x000689D0` (overlay 182 `+0180`) and the far call at
  `0x00068E70` (overlay 182 `+0620`), which must be reached and resolved.

**The configuration** `reach_config.py` builds declares the code as 132 regions: the code of each
overlay at IP 0, and each resident segment-table span of flags 0 or 1 at its own segment and
offset (FND-EXE-571), keeping those that hold an entry. Their entries are the 2,338 starts of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`, the query's starts and leaves, and each overlay's
trampoline targets. It declares 7,562 far calls and jumps whose segment word has an MZ relocation or
an FBOV fixup, each with the code it transfers to, through the trampoline for an overlay target. It
also declares 157 `jmp cs:[bx+table]` sites whose index is bounded by `cmp bx, imm` and `ja` or
`jbe`, or by a word or double-word value scan with its count in `cx`.

**The result.** Both controls are reached and resolved. The closure holds 701 routines and 43,816
instructions. Of the targets only `3D72:12ED` is reached. Its fewest-call chain is overlay 194
`+037B`, its call at `0x000814F0` to overlay 182 `+059C` with `0x4A9D` (`ICON` 19101) and flag 1,
then `0x00068E70`. None of the 14 sites and not `3D72:120B` is reached. Both leaves are reached,
`+1856` from `0x00059A7F` and `+19F8` from `0x00059BFB` and `0x000885DE`.

The report lists 61 call or jump sites whose target it does not resolve. Sixty are the 60 sites
FND-PARTY-031 lists. The other is `lcall [0xA4C6]` at `1000:0346`, inside `1000:0338`, which
saves every register, calls through the far pointer at `DS:A4C6`, calls `1000:03EE` with 0 when
that returns 0, and ends with `iret`. The only stores to `DS:A4C6` and `DS:A4C8` are in
`1000:0361`, which stores its far argument there and passes `0x23` and `1000:0338` to `1000:04A3`.
The only direct call of `1000:0361` is at `0x0006733D` in overlay 180, with `0x05A0:0x002A`, which
the fixup makes trampoline `56B2:002A` and so overlay 180 `+0848`. That routine, the first time
the byte at `DS:09E6` is 0, sets it to 1 and passes the message at `DS:0BFE` ("Ctrl-Break pressed
by user. Game aborted") to `+0867`, which passes it to `4448:002C`; that routine calls
`1000:3603` with it and then `1000:03DF` with 1 (FND-CONFIG-062). `+0848` returns 1. A `reach` run from
`+0848` with the same targets reaches none of them.

The report also lists 87 interrupts, each taken to return, and 5 gaps and 4 contested
instructions, all between `0x000054D8` and `0x000054FD`. There the bytes of a message before
start-up code decode in two alignments, and every alignment reaches the `call 0x55EE` at
`0x000054F9` and the `retf` after it.

## Interpretation

Under the report's assumptions (each reached call and interrupt returns, each leaf calls nothing,
each declared table holds the routes its bound gives), no direct route from the places
FND-PARTY-031 starts from reaches a call that makes the mouse pointer an image other than an
`ICON` before the gate. The routes the pointer setter is reached by set `ICON` images. A route
through one of the 60 unresolved indirect calls of FND-PARTY-031 is not ruled out; the 61st runs
only on Ctrl-Break and ends the program. Interrupt `0x23` is DOS's Ctrl-Break interrupt, so
`1000:0361` reads as the Borland run time's `ctrlbrk`.

## Alternatives

- One of the 14 sites runs before the gate through a direct route: ruled out under the listed
  assumptions, and FND-PARTY-031's own graph, built independently, leaves the same 60 indirect
  calls.
- A route through an unresolved indirect call: not ruled out (Q-PARTY-011).
- The bound read for a declared jump table is wrong: each declaration names the instruction its
  bound comes from; a wrong bound could hide a route through a table entry left out.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in the repository root: `python -I
tools/research/exec-census/reach_config.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv
<config> tools/research/exec-census/pointer_reach.json` and then `python -I -m
scientific_method_engine reach <config>`. For the handler, replace the query's starts with
`180+0848` and its controls with none. Then, in `tools/research/exec-census/`, run
`immediate_search.py <dsun> A4C6 A4C8`, `direct_callers.py <dsun> 1000:0361`,
`trampoline_target.py <dsun> 56B2:002A`, `resident_listing.py <dsun> 1000:0338..1000:0388` and
`overlay_listing.py <dsun> 180 0x67A28 0x67A83`.
