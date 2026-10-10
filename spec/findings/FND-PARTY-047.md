---
id: FND-PARTY-047
title: The placement helper 31E0:0EFF returns -1 at once while DS:265B is 0, and start-up sets that byte through overlay 180 +0141 and overlay 200 +02BE before the start window
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0EFF..31E0:0F18
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:1262..31E0:128C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067321..0x000676D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008941E..0x000894E6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00089725..0x0008976C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004F65B..0x0004F65C
    kind: file-data
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py, immediate_search.py, direct_callers.py); scientific-method-engine 15.3.0 `reach` (tools/research/exec-census/reach_config.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`.

**The helper's first test.** `31E0:0EFF` (FND-ACTOR-003) compares the byte at `DS:265B` with 0
at `31E0:0F0E` and, when it is 0, jumps to `31E0:1285`, which loads -1 into `ax` and returns.
The placement routine `31E0:0121` returns -1 without its increment at `31E0:01E3` whenever
`31E0:0EFF` returns -1 (FND-PARTY-046). The other paths to a return of -1 for a slot below 319
go through `31E0:1262`: a nonzero result of the `OJFF` request `38FF:04AB` (`31E0:0F45`), -2
from the `RDFF` load `2D40:000A` (`31E0:103D`), and the word at `0x67BC` plus 0x25 times the slot
holding -1 after that load (`31E0:104A`).

**The byte's stores.** In the load image the byte at `DS:265B` (file `0x0004F65B`) is 0.
`immediate_search.py` finds 25 resident and 3 overlay 200 compares of it and two stores, both in
overlay 200: 1 at `+02CB` and 0 at `+0607`.

- `+02CB` is in overlay 200 `+02BE` (trampoline `5773:0020`). While the byte is not already 1
  it stores 1, clears the words at `DS:656F` and `DS:6571`, allocates 1,500 entries of 8 bytes
  through `444C:0008` and keeps the far pointer at `DS:67B7`, fills three tables through
  `1000:3FA2`, and calls `4842:077F` with `31E0:31A7`, keeping the result at `DS:2656`. When the
  allocation returns a null pointer (`+02F2`) or that call returns -1 (`+035D`) it calls `+05C5`
  and returns 0; otherwise it calls `4842:0992` and `4842:08B1` with the result and returns 1.
- `+0607` is in overlay 200 `+05C5`, which `+02BE`'s failure path calls at `+037F` and
  overlay 180 `+077C` calls through trampoline `5773:002F`. `+077C` lies in overlay 180 `+0762`,
  which has no direct caller other than its trampoline `56B2:0025`.

`direct_callers.py` finds one call of `+02BE` other than through its trampoline: overlay 180
`+04EF`. That call is inside overlay 180 `+0141`, which `277D:0004` calls before it builds the
start window (FND-PARTY-031). `+0141` has no return before `+04EF`, and each of its 25 jumps up to
`+04E4` lands at or before `+04EF`. The one backward jump, `+0248` to `+0234`, continues with a
forward jump to `+024D`, so no loop.

**The runs.** For each of `q017_add_window_reach.json`, `q017_create_characters_reach.json`,
`q011_start_handler_reach.json` and `q011_main_window_handler_reach.json`, a copy with the
targets replaced by overlay 200 `+02CB` and `+0607` (and everything else as committed) was run
through `reach_config.py` and `reach` as FND-PARTY-040 gives. None reaches either store.

## Interpretation

Start-up sets `DS:265B` to 1 before the start window, unless the 12,000-byte allocation for the
placed-object table fails or `4842:077F` returns -1, and nothing on the start window, the
character screen or its `ADD` window clears it again on the runs' readings. With the byte at 1,
the first test of `31E0:0EFF` passes, so the reading of FND-PARTY-046, in which `ADD` raises the
count, depends on the helper's later tests: the `OJFF` request and the `RDFF` load for the
object number `ADD` passes, and the slot record they fill.

## Alternatives

- Start-up's allocation fails, leaving the byte at 0, so `ADD` places nothing: possible only
  when the game lacks 12,000 bytes of memory at that point, which then also leaves no
  placed-object table for play. Not read here: what `444C:0008` returns under the memory the
  game finds.
- Another store to `DS:265B` exists in a form the search does not find, such as a write through
  a segment other than DS that holds `57E0`, or a string instruction: not ruled out.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `resident_listing.py <dsun>
31E0:0EFF..31E0:12A0`, `immediate_search.py <dsun> 265B`, `overlay_listing.py <dsun> 200 0x8941E
0x894E6` and `200 0x89680 0x8976C`, `overlay_listing.py <dsun> 180 0x67321 0x67720`, and
`direct_callers.py <dsun> 200+02BE 200+05C5 180+0762`. Read the byte at file `0x0004F65B`. Then
make each copy of the four queries named in the runs with the targets `200+02CB` and
`200+0607`, and run `reach_config.py` and `python -I -m scientific_method_engine reach` on it in
the repository root as FND-PARTY-040 gives.
