---
id: FND-EXE-579
title: Six of the runtime library's computed calls in DSUN.EXE go to handlers inside the file, and declaring them reaches two more interrupt 21 sites, neither selecting AH=4B
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:030B..1000:0338
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0338..1000:0388
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0388..1000:03D1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1D38..1000:1D4A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1D66..1000:1E12
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:36DE..1000:36EA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:252C..1000:2530
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 167B:011A..167B:0125
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0005..277B:0008
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:026F..277B:027A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067329..0x00067342
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067942..0x00067945
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067A28..0x00067A2B
tool: scientific-method-engine 19.3.0 (reach, Capstone 5.0.9, pypcode 4.0.1) and xxhash 4.0.1 (tools/research/exec-census/reach_config.py with q007_dsun_hooks_census.json, dsun_interrupt_census.py, far_pointer_words.py, store_values.py, trampoline_target.py)
environment: null
---

## Observation

Addresses, regions and the walk are those of FND-EXE-578, whose walk leaves 83 computed transfers
unresolved. Eight of them are in the runtime library code of segment `1000` and call through far
pointers in DGROUP (`57E0`). Every store to those pointers, from a search of every byte offset of
the regions for an instruction that stores to the word with no base or index register, and every
caller of the routines that store them:

| Call | Pointer | Value at load | Writers | Targets |
|---|---|---|---|---|
| `1000:0346`, in the interrupt 23 handler `1000:0338` | `DS:A4C6..A4CA` | 0 | `1000:0361`, from its argument; it then installs `1000:0338` as the interrupt 23 handler through `1000:04A3` (AH=25) | the only caller, the far call at overlay 180 `+015D`, passes `56B2:002A`: overlay 180's trampoline 2, code `+0848` |
| `1000:03A1`, for each entry of the exit table | `DS:A446 + 4 * i`, for `i` below the count `DS:3572` | 0 | `1000:030B`, from its argument, for at most 32 entries | the only callers, the far calls at `167B:0120` and overlay 180 `+014F`, pass `167B:0029` and `56B2:0025`: overlay 180's trampoline 1, code `+0762` |
| `1000:03B1` | `DS:3676..367A` | `1000:0387`, a `retf` | `1000:36DE..36EA`, the value `1000:3F14` | `1000:0387`, `1000:3F14` |
| `1000:03C9` | `DS:367A..367E` | `1000:0387` | none | `1000:0387` |
| `1000:03CD` | `DS:367E..3682` | `1000:0387` | none | `1000:0387` |
| `1000:1DA6`, in the interrupt 24 handler `1000:1D66` | `DS:A4D4..A4D8` | 0 | `1000:1DBA`, from its argument; it sets `DS:A4CE` to 0 and installs `1000:1D66` as the interrupt 24 handler | the only caller, the near call at `1000:1D43` in `1000:1D38`, forwards its argument; the only caller of `1000:1D38`, `277B:0275`, passes `277B:0005` |
| `1000:1D8D`, in `1000:1D66` when `DS:A4CE` is not 0 | `DS:A4D0..A4D4` | 0 | `1000:1DE6`, from its argument; it sets `DS:A4CE` to 1 | `1000:1DE6` has no caller, so `DS:A4CE` is never 1 and the call never runs |
| `jmp [398C]` at `1000:252C` | `DS:398C` | `129F` | none | `1000:129F` |

The segment words of the overlay 180 pointers, `05A0` at `+014A` and `+0158`, are in overlay 180's
fixup list, so each is descriptor 180 times 8 (FMT-EXE-005), the overlay's header segment `56B2`.
The segment words `067B` at `167B:011B` and `177B` at `277B:0270` carry MZ relocations.
`167B:0029` and `277B:0005` start with `push bp`, `1000:3F14` with `push si`, and the
overlay 180 targets with `push bp`, `mov bp, sp`.

No other caller: a decode of `E8` and `E9` at every byte offset of file `0x5200..0x15200` (the
64 KiB frame of segment `1000`) finds no near call to `1000:030B`, `1000:0361`, `1000:1DBA` other
than `1000:1D43`, or `1000:1DE6`, and only `1000:1D43` to `1000:1DBA`. The words of the file equal
to each routine's offset (`far_pointer_words.py`) give no relocated far call, pointer, fixup word or
immediate beside a segment word other than the callers in the table. Overlay 190 pushes `1D38`
after `push ds`, which makes a pointer into DGROUP. The words equal to the handler offsets `0338`
and `1D66` are, apart from the setters, a segment word `0338` (descriptor 103) pushed as a data
pointer in overlay 180, values in data tables outside the regions, and bytes inside other
instructions; none passes either handler to a set-vector call.

**The walk with these targets.** The query `q007_dsun_hooks_census.json` is FND-EXE-578's query
with six `indirectCalls` declarations, each exhaustive: `1000:0346` to overlay 180 `+0848`,
`1000:03A1` to `167B:0029` and overlay 180 `+0762`, `1000:03B1` to `1000:0387` and `1000:3F14`,
`1000:03C9` and `1000:03CD` to `1000:0387`, and `1000:1DA6` to `277B:0005`. With it, `reach` leaves
77 transfers unresolved, the 83 less the six declared, and adds none. It reaches 1,384 routines and
decodes 106,039 instructions in 1,501 ranges. Of the 100 interrupt 21 sites
in the regions, 82 are reached and 18 remain candidates. The two newly reached:

- `1000:0881`, AH 2D (`mov ah, 2Dh` at `1000:0877`).
- `1000:2F0F`, in `1000:2F03`, which takes AH from its argument. FND-EXE-578 found its only caller,
  `1000:1E3C`, which passes 54.

The interrupt 2F sites, the stack-built calls and every other site keep the values FND-EXE-578
gives.

## Interpretation

The Ctrl-Break, exit, exit-hook and critical-error calls of the runtime library go to code inside
the file: two routines in overlay 180, `167B:0029`, `1000:3F14`, the empty `1000:0387` and
`277B:0005`. The second critical-error call cannot run, and the `jmp [398C]` always goes to
`1000:129F`. None of these transfers leaves the file, and following them reaches no request with
AH 4B. The 18 interrupt 21 candidates of the census are now the only sites still read without a
walk to them, and they all have an immediate AH other than 4B (FND-EXE-578).

## Alternatives

- FND-EXE-577 recorded the same reading with FND-EXE-574's 17 starts, which miss four installed
  handlers (FND-EXE-578), and so reported 71 unresolved transfers. The six declarations, their
  targets and the two newly reached sites are the same.

- The store search finds direct stores only. A store through a base or index register, or a string
  or block copy, that writes one of these pointers is not excluded; the exit table's own writer
  uses an index register and was found by reading `1000:030B`.
- `1000:2877` saves its return address at `DS:3916..391A` and `1000:298A` jumps through `DS:3916`.
  That transfer and the other 76 unresolved ones are not classified here.
- The walk now reaches overlay 180's two routines through their trampolines, not through a load of
  overlay 180. The trampoline lands in the overlay manager (FMT-EXE-004), so in the original the
  call passes through `4AE5` before the overlay's code runs.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, in `tools/research/exec-census/`: run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> q007_dsun_hooks_census.json`,
then `python -I -m scientific_method_engine reach <config> > <report>`, then
`dsun_interrupt_census.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <report> <config>`.
`store_values.py <dsun> a4c6 a4c8 3676 3678 367a 367c 367e 3680 a4ce a4d0 a4d2 a4d4 a4d6 398c`
lists the direct stores. `far_pointer_words.py <dsun> 1000:030B 1000:0361 1000:1D38 1000:1DBA 1000:1DE6`
lists the words equal to each routine's offset, and `trampoline_target.py <dsun> 56B2:0025 56B2:002A`
resolves the two overlay 180 pointers. The near callers were found by decoding `E8` and `E9` at
every byte offset of file `0x5200..0x15200` and keeping those whose target is the routine.
