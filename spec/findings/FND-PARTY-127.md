---
id: FND-PARTY-127
title: Entering display mode 2 unregisters the main view's key handler 28C9:0CFF, which only overlay 182 +1167, +15D7 and +19F8 and overlay 199 +0BC1 register
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000794E1..0x0007950D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A1F6..0x0006A20E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069A18..0x00069A44
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000884C1..0x00088521
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/trampoline_target.py, direct_callers.py, immediate_search.py, overlay_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. **Overlay 190 `+11A1`** (trampoline `571F:0034`) stores its far pointer argument in
the dword at `DS:61A2` and passes it to `39D1:0418`; **`+11C1`** returns that dword
(`+11A1..+11CD`). `direct_callers.py <dsun> 190+11A1` finds one near caller and 23 far callers in
overlays 171, 172, 175, 181, 182, 192, 194, 199, 201, 202, 203, 209, 211 and 212.

**Overlay 182 `+1856`** stores 0 in `DS:0D9C` and then calls `+11A1` with a null pointer, before it
stores 2 in `DS:0DAB` (`+19A6..+19BE`, FND-PARTY-124). `immediate_search.py <dsun> 0CFF` finds the
offset of the main view's key handler `28C9:0CFF` pushed five times with segment `28C9`. Four
pushes come before a call of `571F:0034`: in overlay 182 at file `0x00069A3C` (in `+1167`),
`0x00069F9E` (in `+15D7`) and `0x0006A32E` (in `+19F8`), and in overlay 199 at `0x000884EB`. The
fifth, at `0x00069A1B` in `+1167`, passes the pointer to overlay 182's own `+016B`, whose result
`+1167` stores at `4E28:00D7` (`+11C8..+11F4`).

**Overlay 199 `+0BC1`**, which overlay 199 `+0C21` calls first (FND-PARTY-123), frees a nonzero
dword at `DS:6554` through `444C:0092` and stores the result back, stores 0 in `DS:143D`, registers
`28C9:0CFF` through `571F:0034`, calls `45B9:0034` until its result has bit 0 clear, calls
`4611:03A5`, and, when the byte at `DS:14E3` is not 0, calls `4611:0051` until it returns 0
(`+0BC1..+0C21`).

## Interpretation

While a window that overlay 182 `+1856` opened is up, the main view's key handler is not
registered unless the window itself or one of the four routines registers it; a direction key
that runs the fight step `28C9:305F` through it (FND-EXPLORE-004) has no effect in that mode. The
level gain's first step, overlay 199 `+0BC1`, registers that handler again whatever the mode, and
then empties the event queue.

## Alternatives

- A window may save the pointer through overlay 190 `+11C1` and register it again when it closes;
  the 23 far callers were not each read for the value they pass, and a window opened from mode 1
  before `+1856` runs would save `28C9:0CFF`.
- Whether `45B9:0034` hands the events it takes to the registered handler, so that a direction
  key pending when overlay 199 `+0BC1` runs is handled by `28C9:0CFF` before the gain, was not
  read.
- The other callers of `28C9:305F` (`28C9:02B1`, `28C9:0428` and `28C9:0C7B`, FND-PARTY-125) were
  not traced to the key handler.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `trampoline_target.py <dsun> 571F:0034`;
`direct_callers.py <dsun> 190+11A1`; `immediate_search.py <dsun> 0CFF`; and `overlay_listing.py`
over the ranges above.
