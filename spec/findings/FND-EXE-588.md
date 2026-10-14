---
id: FND-EXE-588
title: DSUN.EXE's application frame callbacks in segment 3D72 go to overlay routines and two resident routines, or are never set, and declaring the one that is set reaches no further interrupt site
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0078..3D72:012E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:01C8..3D72:01F9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:02B2..3D72:0302
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:038B..3D72:03BE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00032E11..0x00032E35
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0515..3D72:059E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0635..3D72:06D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:08E1..3D72:08FB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0942..3D72:09CB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0B84..3D72:0BDB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0D83..3D72:0DD4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:0211..3F96:02F8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:0603..3F96:0650
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0414..3A8E:0440
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000300CF..0x000300ED
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:0008..3EBE:042B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:0991..3EBE:0A7B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068A3F..0x00068A7D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00057119..0x0005711D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0005713D..0x0005713F
tool: scientific-method-engine 19.3.0 (reach, Capstone 5.0.9, pypcode 4.0.1) and xxhash 4.0.1 (tools/research/exec-census/reach_config.py with q007_dsun_apfm_census.json, dsun_interrupt_census.py, far_pointer_words.py, callback_args.py); Capstone 5.0.9 for the store and caller searches
environment: null
---

## Observation

Addresses, regions and the walk are those of FND-EXE-587, whose walk leaves 50 computed transfers
unresolved. This reads the six of them in segment `3D72`.

**The records.** Five of the calls go through a field of an application frame record, which
starts with the tag `APFM` and has the layout of an `APFM` resource (FMT-UI-004):

| Call | Field | Record | Guard |
|---|---|---|---|
| `3D72:012A` | `+70` | the record the hit test at `3D72:0EB8` returns, once `3D72:0091` finds `APFM` at its `+0` | the call is skipped when the word at `+70` is 0 (`3D72:010C`) |
| `3D72:0590` | `+62` | the routine's argument `[bp+0A]`, when its argument `[bp+6]` is `APFM` (`3D72:0529`) | the call is skipped when the doubleword at `+62` is 0 (`3D72:0545`) |
| `3D72:09C7` | `+5E` | the record at DGROUP (`57E0`) `A15B` | the call is made only when DGROUP `A13D` is 2 |
| `3D72:0BD7` | `+5A` | the record at DGROUP `A15B` | the call is skipped when the doubleword at `+5A` is 0 (`3D72:0BC8`) |

The seven calls of `3D72:0515`, all in `3D72:0009`, pass the record at DGROUP `A125` and a tag.
At `3D72:0256` and `0278` they are the copies of `A125` and `A129` made at `3D72:0049` and
`0051`, and at `3D72:0292` `A125` and `A129` themselves; `A129` is written only at `3D72:01ED`,
from the `+0` of the record at `A125`, and set to 0 at `3D72:01F3`, `3A8E:0A19` and `3A8E:0C02`,
next to the stores that set `A125` to 0. At `3D72:033C`, `035E`, `0493` and `04B5` the tag is
the `+0` of the record at `A125`, and the calls lie in the `APFM` cases of two dispatches on that
same tag (`3D72:02E6` with the tags at `3D72:0509`, `APFM` going to `031F`, and `3D72:03A1` with
the tags at `3D72:04F1`, `APFM` going to `0476`).
`A15B` is written only at `3D72:06AC`, which copies `A125` when `A129` is `APFM`, and at
`3F96:079B`, which stores the record `39D1:049C` returns for the tag `APFM`; `39D1:049C` returns the
record of a window entry whose tag word matches the one it is given. `A13D` is 0 at load and is
written only at `39D1:0236` (`FFFF`), `39D1:06A1` (0), `3D72:06A2` (3), `3D72:06CD` (0) and
`3D72:06C5` (2). `3D72:06C5` runs only when the doublewords at `+5A` and `+5E` of the record at
`A125` are both nonzero.

**Where the fields are written.** In all 97 `APFM` resources the bytes `0x5A..0x74` are 0
(FND-UI-005). The instruction starts in the walk's decoded ranges and the inventory's ranges that
store through a base register to displacements `57` to `73` and touch those fields' bytes are:

- the four setters in `3F96`, each of which looks up the record with `39D1:049C` and the tag `APFM`
  and stores its far argument `[bp+0E]`: `3F96:0211` at `+5A`, `3F96:025E` at `+5E`, `3F96:02AB` at
  `+62` and `3F96:0603` at `+70`;
- word stores at `+5C`, `+5E`, `+60` and `+62` in `3EBE:0008`, and a doubleword store at `+64` in
  `3EBE:0991`. `3EBE:0008`'s only caller is `3A8E:04CE`, in the case for `BUTN` of the tag dispatch
  at `3A8E:0414`, which compares a window entry's tag with the five tags at `3A8E:05EF` (`ACCL`,
  `APFM`, `BUTN`, `MENU`, `EBOX`) and jumps through the five words after them (`0537`, `0514`,
  `04B3`, `0440`, `0543`). `3EBE:0991` stores into the record `39D1:049C` returns for the tag `BUTN`;
- stores through ES loaded with an overlay fixup word for a data descriptor: `0308` at overlay 173
  `+0FEA` and `+207A` and overlay 195 `+0926`, and `0360` and `0368` at overlay 184 `+0A2C`, `+11BC`
  and `+1D8A`.

`3F96:0211`, `3F96:025E` and `3F96:0603` have no caller: `far_pointer_words.py` lists no far
pointer to them, and a search of segment `3F96`'s 64 KiB frame for `E8` or `E9` naming them finds
none.

**The callbacks set at `+62`.** `callback_args.py` reads the argument at each of the 28 far calls of
`3F96:02AB` and at the 17 calls of overlay 182 `+01EF`, which one of them (182 `+020B`) uses to pass
its own argument `[bp+0C]` on unchanged; every pushed segment word is in its overlay's fixup list or
carries an MZ relocation:

| Routine | Calls | Values passed |
|---|---|---|
| `3F96:02AB` | 28 | `172+0C21`, `175+0231`, `175+0B61`, `178+06EA`, `181+0648`, `189+0559`, `189+0BB0`, `190+0092`, `203+0706`, `203+09B3`, `207+0000`, `212+0A57`, `213+034F` through trampolines, 0, or overlay 182 `+01EF`'s argument |
| overlay 182 `+01EF` | 17 | `202+0673`, `209+03DC`, `209+0EF0`, `211+10F4`, `211+175E`, `213+034F` through trampolines, `28C9:0061` and `28C9:1261` (fixup word `00B0`, descriptor 22), or 0 |

**The hook at DGROUP `A119`.** `3D72:0DA1` and `0DD0` call far through `A119` when it is not 0. It is
0 at load and written only at `39D1:0272` (0) and at `3D72:08F3`, in the routine at `3D72:08E1`, which
stores its argument there. `3D72:08E1` has no caller by the same two searches.

**The walk with these targets.** The query `q007_dsun_apfm_census.json` is FND-EXE-587's query with
an exhaustive declaration for `3D72:0590` naming the 20 routines above. The walk reaches 2,195
routines and decodes 167,106 instructions in 2,397 ranges. It reaches no interrupt instruction that
FND-EXE-587's walk did not, so every interrupt site keeps that finding's reading, and leaves 49
transfers unresolved: the 50 less `3D72:0590`. No transfer in the code it newly reaches is
unresolved.

## Interpretation

`3D72:0590` calls the routine a caller registered for an application frame: one of 18 overlay
routines or `28C9:0061` or `28C9:1261`. The other five calls never run: `+5A`, `+5E` and `+70` of an
application frame and the hook at `A119` stay 0, because the only routines that would set them are
never called, and `3D72:09C7` needs `+5A` and `+5E` both set. None of the six calls leaves the file.
Of the 49 transfers still unresolved, 44 have a classification in FND-EXE-575 and FND-EXE-578 to
FND-EXE-588, and these five never run.

## Alternatives

- The fields start as the resource's bytes only if an application frame record is the loaded `APFM`
  resource with its fields at the resource's offsets; a record built some other way, or a block copy
  into one, could carry other values. The store searches decode linearly from range starts, so a
  store through another base displacement is not excluded either.
- That a record whose `+0` is `APFM` is an application frame rests on the resource loader giving
  each record its resource's tag; a record of another kind carrying that tag is not excluded.
- The caller searches would miss a far pointer to `3F96:0211`, `025E`, `0603` or `3D72:08E1` built at
  run time from separate words.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, in `tools/research/exec-census/`: run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> q007_dsun_apfm_census.json`,
then `python -I -m scientific_method_engine reach <config> > <report>`, then
`dsun_interrupt_census.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <report> <config>`.
`far_pointer_words.py <dsun> 3F96:0211 3F96:025E 3F96:02AB 3F96:0603 3D72:08E1` lists the far
callers. `callback_args.py <dsun> <label> E <call sites>` reads the argument at the calls of
`3F96:02AB`, and `callback_args.py <dsun> <label> C <call sites>` at the calls of overlay 182 `+01EF`.
The store search decodes each of the report's decoded ranges and each inventory range linearly and
keeps each `mov`, `pop`, `xchg` or arithmetic instruction whose memory destination has a
displacement from `57` to `73` (for the record fields) or names DGROUP `A119..A11C`, `A13D` or
`A15B..A15E`.
