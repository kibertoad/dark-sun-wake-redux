---
id: FND-RNG-057
title: The clock conversion has one caller, the clock helper, which passes the date and time the two DOS services return into its own frame; the inverse conversion has no caller, and the game writes the DOS time only at exit, with the time it has just read
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:092B..1000:0963
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0963..1000:09B0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006796B..0x0006797F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0860..1000:0886
tool: tools/research/exec-census/direct_callers.py, far_pointer_words.py, resident_listing.py and overlay_listing.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. FND-RNG-016 reads the clock conversion
`1000:386D` and FND-RNG-018 its initializer `1000:3B56`, which FND-RNG-054 finds called at
`1000:387A` in the conversion and at `1000:3993` in `1000:3986`.

`direct_callers.py <dsun> 1000:386D 1000:3986 1000:0963 1000:046E 1000:0481 1000:092B 1000:0860
1000:0873 180+0762` gives:

| Routine | Callers |
|---|---|
| the conversion `1000:386D` | the near call at `1000:098B` in the clock helper `1000:0963` |
| `1000:3986` | the near call at `1000:0943` in `1000:092B` |
| `1000:092B` | none |
| the clock helper `1000:0963` | the far calls at overlay 180 `+0657` and overlay 204 `+0017` |
| the date wrapper `1000:046E` | the near call at `1000:0973` in the helper |
| the time wrapper `1000:0481` | the near call at `1000:097D` in the helper and the far call at overlay 180 `+078F` |
| `1000:0860` | the near call at `1000:094F` in `1000:092B` |
| `1000:0873` | the near call at `1000:0959` in `1000:092B` and the far call at overlay 180 `+0799` |
| overlay 180 `+0762` | none, only its trampoline `56B2:0025` |

`far_pointer_words.py <dsun> 1000:386D 1000:3986 1000:092B 1000:0963` finds no stored far pointer
to any of them besides the two far calls of the helper.

**The helper** (`1000:0963..1000:09B0`) passes the date wrapper the near offset of its frame word
at `bp-4` and the time wrapper that of `bp-8`, then the conversion the two offsets, date first
(FND-RNG-009, FND-RNG-023). Nothing else is stored in those four words before the conversion
reads them. Both of its callers pass 0 as its one argument.

**`1000:092B`** takes a pointer to a doubleword, calls `1000:3986` with it and two frame buffers,
then `1000:0860` with the first and `1000:0873` with the second (`1000:092B..1000:0963`).
`1000:0860` and `1000:0873` load `AH` with 0x2B and 0x2D, `CX` and `DX` from the two words their
near argument points at, and call `INT 21` (`1000:0860..1000:0886`).

**Overlay 180 `+0762`**, which FND-EXE-577 finds reached from the exit table
through `1000:03A1`, calls the time wrapper with its frame word at `bp-4` and then `1000:0873`
with the same buffer (`+078B..+079E`).

## Interpretation

Every conversion the game makes is of the date and the time that the DOS services 0x2A and 0x2C
return in the clock helper's own frame, the date first; the callers that reseed add nothing to
them. `1000:092B`, which turns a doubleword through `1000:3986` into two buffers and hands them
to the DOS services that set the date and the time, is never called. The game writes the DOS time once, at exit, with the value it reads just
before, so within a run it does not change the clock the helper reads.

Which values the services return, and their ranges, is the runtime's (Q-RNG-006); whether the
wrappers' stores through `DS` reach the helper's `SS` frame on the reseed paths is Q-RNG-023.

## Alternatives

- Calls through registers or near pointer tables were not searched.
- No emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, `<dsun>` the installed
`DSUN.EXE`: `direct_callers.py <dsun> 1000:386D 1000:3986 1000:0963 1000:046E 1000:0481 1000:092B
1000:0860 1000:0873 180+0762`; `far_pointer_words.py <dsun> 1000:386D 1000:3986 1000:092B
1000:0963`; `resident_listing.py <dsun> 1000:0860..1000:0890 1000:0920..1000:09B0`; `overlay_listing.py <dsun> 180
671E0 67D64`. Outputs are kept under `GAME_DIR/analysis/rng-clock-20261020/`. Nothing was
executed.
