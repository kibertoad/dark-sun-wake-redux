---
id: FND-PARTY-189
title: Generation's window handler, overlay 184 +0DA7, is registered only by +07D8 after it points DS:1429 at 4E4F:0029, and reaches a routine that points DS:1429 at a stored record only in +1029, after +1029's sheet draw and before it unregisters the handler
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
tool: Python 3.14.7 with Capstone 5.0.7 (tools/research/exec-census/overlay_listing.py, direct_callers.py, reach_config.py) and scientific-method-engine 19.7.0 `reach` with the query tools/research/exec-census/q076_generation_writers_reach.json
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D952..0x0006D978
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006DE37..0x0006DF05
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E0B9..0x0006E4D6
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`, where overlay 183's code starts at `0x6AFE0` and overlay 184's at `0x6D090`.

**Registration.** A search of the file for a push of the far address `56DD:0043` (the trampoline
that enters overlay 184 `+0DA7`) as `push 0x05C0` then `push 0x0043`, the reverse order, or one
`push` of the dword `0x05C00043`, finds two, both in `+07D8`: at `+08C2` it is passed to overlay
182 `+016B` with `0x4C2F`, whose result goes to `DS:1431`, and at `+08DD` to `39D1:0418`, which
stores it at `DS:A0F1` (FND-PARTY-130). Both come after `+07E4..+07FB`, where `+07D8` sets
`DS:1429` to `4E4F:0029` (FND-PARTY-073). `direct_callers.py` finds no other call of `+0DA7`.

**The handler.** `+0DA7` takes an event number. For event 6 with `[bp + 0x12]` equal to `0x11B`
it calls `+1029` with the word at `4E71:0B44` and 0 (`+0DCD..+0DEC`); for event 2 it branches
through the table at `+0F3B` to the button routines, among them overlay 183 `+0013`, `+026D`,
`+02AC`, `+02EB` and `+032A`, overlay 183 `+0A3E` with codes 3, 4, 5 and 8 (`+0E62`) and overlay
183 `+10FF` (`+0E6D`), and to `+0E75`, which calls `+1029` for DONE and its other button
(FND-PARTY-070). `direct_callers.py` finds `+0DE6` the only call of `+1029`.

**`+1029`.** With its flag nonzero it calls overlay 183 `+0A3E` at `+1067` when the first class
byte of the record at `DS:1429` is 0, and then at `+10CF` overlay 184 `+1648`, which stores the
character (FND-PARTY-073). On both flags it then frees the dwords at `DS:1134`, `DS:1138` and
`DS:1431` (`+1155..+1194`), passes 0 to `39D1:0418`, `39D1:0448` and `39D1:0430` (`+1232..+1250`),
and, when the flag is nonzero and `DS:112E` is -1, calls by the word at `DS:4265` plus twice the
slot one of overlay 212 `+061C` (`+13CE`), overlay 190 `+36D2` (`+13D9`), overlay 211 `+02B5`
(`+1404`) and overlay 202 `+0287` (`+142C`), each of which points `DS:1429` at the slot's stored
details record (FND-PARTY-188). It then sets `DS:112E` to -1, calls `27E5:0283` and returns
(`+1434..+1445`). `+0DA7` jumps to its end after `+1029` returns (`+0DEC`).

**Reach.** `reach` from overlay 183 `+0013`, `+026D`, `+02AC`, `+02EB` and `+032A` and overlay
184 `+07D8`, `+0DA7` and `+1029`, with the seven `DS:1429` stores of FND-PARTY-188 outside `+07D8`
as targets (file `0x78972`, `0x7BA49`, `0x8A72F`, `0x933A2`, `0x93C0E`, `0x96139` and
`0x98831`), the sheet call at `0x6D4F2` as the control and `1000:0388` as a leaf (FND-PARTY-039,
FND-PARTY-043), reaches the control and six targets, all through the calls at `+13CE`, `+13D9`,
`+1404` and `+142C` of `+1029`; `0x78972` is not reached. Without `+1029` as a start, every chain
still passes through `+0DE6`. The report has 1079 routines, 34 unresolved calls and
`negativeUsable` false.

## Interpretation

While the generation screen is up, the routine that answers its events reaches nothing that
points `DS:1429` away from `4E4F:0029` except through `+1029`, which closes the screen: it frees
the window, drops the handler and only then opens the screen it came from. Its own sheet draw at
`+1067` comes before those calls. So the generation screen's sheet, the third caller of the
experience line (FND-PARTY-188), prints a record holding codes 1 to 8, unless something other than
`+0DA7` runs while the screen is up.

## Alternatives

- `+07D8` replaces only the handler at `DS:A0F1`. Whatever the screen before generation left at
  `DS:A0F5` and `DS:A0F9` stays there until `+1029` clears them, and the main loop's calls through
  those slots (FND-PARTY-130) are among the 34 unresolved calls. Whether a handler there can run
  during generation and point `DS:1429` at a stored record was not read.
- The 34 unresolved calls are treated as reaching nothing, so the negative is not usable on its
  own. The search for the handler's address covers immediate pushes only.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `direct_callers.py <dsun> 184+0DA7
184+1029`; `overlay_listing.py <dsun> 184 6D930 6D990`, `184 6DE37 6DF20` and `184 6E0B9
6E4D6`. Search the file for `68 C0 05 68 43 00`, `68 43 00 68 C0 05` and `66 68 43 00 C0 05`.
In the repository root, run `reach_config.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv
<config> tools/research/exec-census/q076_generation_writers_reach.json` and `python -I -m
scientific_method_engine reach <config>`; then the same with `184+1029` removed from `starts`.
