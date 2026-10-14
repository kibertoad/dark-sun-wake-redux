---
id: FND-EXE-587
title: DSUN.EXE's window hook and window record callbacks, and overlay 190's callback arguments, go to overlay routines and one resident routine that their setters' callers pass as relocated constants
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0418..39D1:0448
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0752..39D1:0854
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0854..39D1:08A0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0B43..39D1:0B59
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:05AA..3A8E:05EF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:060D..3A8E:08E3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0CDC..3A8E:0D6C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000794E1..0x0007950D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000689BB..0x00068A30
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000570F1..0x000570F9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000531A2..0x000531A6
tool: scientific-method-engine 19.3.0 (reach, Capstone 5.0.9, pypcode 4.0.1) and xxhash 4.0.1 (tools/research/exec-census/reach_config.py with q007_dsun_gui_census.json, dsun_interrupt_census.py, far_pointer_words.py, callback_args.py, trampoline_target.py); Capstone 5.0.9 for the store and caller searches
environment: null
---

## Observation

Addresses, regions and the walk are those of FND-EXE-586, whose walk leaves 58 computed transfers
unresolved. This reads eight of them, in segments `39D1` and `3A8E`, and the 17 the walk reaches
once they are declared.

**The hooks at DGROUP `A0F1` and `A0F5`.** `39D1:07CC`, `083B` and `0B55` call far through DGROUP
(`57E0`) `A0F1`, and `39D1:0786` and `0899` through `A0F5`; each call is skipped when its pointer is
0. Both are 0 at load. The only instruction starts in the walk's decoded ranges and the inventory's
ranges that store to `A0F1..A0F8` by displacement are `39D1:02E4` and `02ED`, which write 0, and
`39D1:042A` and `0442`, in the setters `39D1:0418` and `39D1:0430`, which store their far argument.

The callers of each setter, and of the setters and forwarders named below, are the far calls
`far_pointer_words.py` lists with a relocated segment word, and for an overlay routine the far
calls through its trampolines and the near calls in its code. `callback_args.py` reads the
argument each passes, and checks that every pushed segment word is in its overlay's fixup list or
carries an MZ relocation:

| Setter | Calls | Argument | Values passed |
|---|---|---|---|
| `39D1:0430` (`A0F5`) | 18 | `[bp+6]` | `05F0:0057` (overlay 190's trampoline 11, `190+0092`), or 0 |
| `39D1:0418` (`A0F1`) | 7 | `[bp+6]` | `184+0DA7`, `199+0053`, `199+0D75`, `199+102C` through trampolines, or overlay 190 `+11A1`'s argument |
| overlay 190 `+11A1` | 23 | `[bp+6]` | `171+03C5`, `172+04C5`, `175+0B61`, `181+0000`, `189+0559`, `192+0785`, `194+0000`, `201+0000`, `202+05CA`, `203+00EE`, `209+169C`, `211+081B`, `212+0967`, `28C9:0CFF` (fixup word `00B0`, descriptor 22), 0, or a value read back from 190 `+11C1` |
| `3A8E:0CDC` (record `+F5`) | 7 | `[bp+0A]` | `199+0D75`, `202+05CA`, `211+081B`, `212+0967`, 0, or overlay 182 `+016B`'s argument |
| overlay 182 `+016B` | 31 | `[bp+0C]` | 25 routines: the 13 of 190 `+11A1`'s 14 other than `172+04C5`, and `172+0816`, `172+0C21`, `178+06EA`, `183+103D`, `183+109D`, `184+0DA7`, `199+0053`, `199+0D75`, `199+102C`, `209+0622`, `209+0E2D`, `213+034F`, or 0 |
| `3A8E:0D0C` (record `+F9`) | 17 | `[bp+0A]` | `172+1085`, `175+0020`, `189+001E`, `199+0E51`, `199+1601`, `202+0000`, `203+0605`, `209+00C2`, `209+0C84`, `211+0000`, `212+0000`, `213+14B9`, or 0 |
| `3A8E:0D3C` (record `+FD`) | 18 | `[bp+0A]` | `171+0000`, `172+0000`, `172+1252`, `175+0000`, `178+0020`, `189+0000`, `190+361C`, `190+363B`, `192+022B`, `199+0000`, `199+0020`, `213+16CE` |

190 `+11A1` stores its argument at DGROUP `61A2` before passing it to `39D1:0418`, and 190 `+11C1`
returns `61A2`, which is 0 at load and has no other store. The values read back are pushed at
overlay 172 `+0479` (from `[bp-4]`, which the routine set from 190 `+11C1` at `+03AB`) and at overlay
172 `+0775` and overlay 209 `+178E` (from the word pair at `4C4D:000F`, which overlay 209 sets from 190
`+11C1` at `+12B4`; ES is loaded with the fixup word `0300`, descriptor 96, at each of these three). 182 `+016B` passes its own argument on to `3A8E:0CDC` unchanged.

**The window record fields.** `3A8E:0707` and `08B7` call far through `+FD` and `+F9` of the record
the routine at `3A8E:060D` takes as its argument, and `39D1:0802` through `+F5` of the record at
DGROUP `A0FD`; each call is skipped when its field is 0. The only instruction starts of those ranges
that store through a base register to displacements `F5` to `100` are `3A8E:05CF..05DF`, in the
record setup, which copies DGROUP `A0F1` to `+F5` and writes 0 to `+F9` and `+FD`, and the three
setters `3A8E:0CDC`, `0D0C` and `0D3C`, which store their far argument at `+F5`, `+F9` and `+FD`.

**Overlay 190's callback arguments.** Declared to the walk, those calls reach three overlay 190
routines that call far through their argument `[bp+1E]`, which none of them writes: `+0802` (at
`+0AD4`, `+0AEE`, `+0C69`), `+0D89` (at `+0EB8`, `+0EDC`, `+0F00`, `+0FE8`, `+116B`) and `+139B` (at
`+13F3`, `+15AA`, `+17FE`, `+1831`, `+1918`, `+1961`, `+1A22`, `+1A39`, `+1CC5`). Their callers, 4, 4
and 8 far calls through overlay 190's trampolines, each copy a 24-byte structure argument onto the
stack through `1000:0699` and push a relocated trampoline before it: `189+042F`, `202+0547`,
`211+073D` and `212+08E3` for the first two, and those and `175+0C05`, `181+0612` and `203+0529` for
`+139B`.

**The walk with these targets.** The query `q007_dsun_gui_census.json` is FND-EXE-586's query with
exhaustive declarations for the eight calls (the `A0F1` calls naming the 18 values, the `+F5` call
those and the eight more of the `+F5` setters, the `+F9` and `+FD` calls the 12 values of their
setters) and the 17 overlay 190 calls. The walk reaches 2,142 routines and decodes 157,812
instructions in 2,332 ranges. It reaches no interrupt instruction that FND-EXE-586's walk did not,
so every interrupt site keeps that finding's reading, and leaves 50 transfers unresolved, the 58 less
the eight.

## Interpretation

The game's window hooks and window record callbacks go to overlay routines and to `28C9:0CFF`,
which callers register as constants. A hook is otherwise only cleared or restored to a value
registered earlier. None of these calls leaves the file.

## Alternatives

- The store searches decode linearly from range starts, so a store to the hooks or the record
  fields through another base displacement, or a block copy of a record, is not excluded. A record
  `3A8E:060D` is given that the setup at `3A8E:05AA` did not write could hold other values in those fields.
- `4C4D:000F` was checked only at the store the read-back path uses; another store to it is not
  excluded.
- The argument reading follows the pushes back from each call within one straight run of code, so
  a value pushed before a branch that joins the call is not read; every one of the 121 setter and
  forwarder calls and 16 overlay 190 calls read here ended at the pushes it needed.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, in `tools/research/exec-census/`: run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> q007_dsun_gui_census.json`,
then `python -I -m scientific_method_engine reach <config> > <report>`, then
`dsun_interrupt_census.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <report> <config>`.
`far_pointer_words.py <dsun> 39D1:0418 39D1:0430 3A8E:0CDC 3A8E:0D0C 3A8E:0D3C` lists the resident
setters' callers. `callback_args.py <dsun> <label> <argument offset> <call sites>` reads each call's
argument, with offset `6` for the two `39D1` setters and 190 `+11A1`, `A` for the `3A8E` setters,
`C` for 182 `+016B` and `1E` for the three overlay 190 routines.
