---
id: FND-PARTY-128
title: The three windows that read the registered key handler through overlay 190 +11C1 register their own and put the saved one back, and 45B9:0034 reads the mouse buttons through INT 33h function 3
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 45B9:0034..45B9:0059
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000597D6..0x000597EF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000598A5..0x000598B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059A8E..0x00059AA1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059B9A..0x00059BAD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009440A..0x0009442B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000948E3..0x000948F6
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/direct_callers.py, resident_listing.py, overlay_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. FND-PARTY-127 shows that overlay 190 `+11A1` (trampoline `571F:0034`) registers a key
handler and `+11C1` (trampoline `571F:0039`) returns the registered one.

**`45B9:0034`** loads 3 into `AX`, runs `INT 33h`, stores `CX` and `DX` through its two far pointer
arguments and returns `BX` in `AX` (`45B9:0034..45B9:0059`). `direct_callers.py <dsun> 45B9:0034`
finds six far callers, in overlays 190, 199 and 201, and two paragraph references in the load
image.

**`direct_callers.py <dsun> 190+11C1`** finds three far callers, each followed by a call of
`+11A1`:

- Overlay 172 `+03A6` stores the result at `bp-4` and registers `566A:0052`
  (`+03A6..+03BF`); `+0475` passes the dword at `bp-4` to `+11A1` (`+0475..+0481`).
- Overlay 172 `+065E`, in the message window `+05DB` after its call of overlay 182 `+1856`
  (FND-PARTY-126), stores the result in the dword at `4C4D:000F` (`+065E..+0671`). `+076F`, in
  its close routine `+0746`, passes that dword to `+11A1` (`+076A..+077D`).
- Overlay 209 `+12AA` stores the result in the dword at `4C4D:000F` and registers `57B0:0048`
  (`+12AA..+12CB`); `+1788` passes that dword to `+11A1` (`+1783..+1796`).

## Interpretation

`45B9:0034` reads the mouse position into its arguments and returns the button state, so the
loops in overlay 199 `+0BC1` and `+0C21` (FND-PARTY-123, FND-PARTY-127) wait until the left button
is up and take no key or mouse event to a handler. A window that reads the handler through
`+11C1` puts back the one it found, so these three windows leave `28C9:0CFF` registered only if it
was registered when they opened; the message window opened from mode 1 saves the null pointer that
`+1856` registered, and overlay 182 `+19F8` registers `28C9:0CFF` after it.

## Alternatives

- The 23 far callers of `+11A1` that FND-PARTY-127 counts include windows that register a handler
  without reading the old one; whether any of them registers `28C9:0CFF` through a value it read
  from memory was not checked beyond the five pushes of the immediate `0x0CFF`.
- The branches of overlay 172 `+03A6`'s routine and of overlay 209 `+12AA`'s routine that return
  without reaching `+0475` or `+1788` were not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `direct_callers.py <dsun> 190+11C1 45B9:0034`;
`resident_listing.py <dsun> 45B9:0034..45B9:0059`; `overlay_listing.py <dsun> 172 597B0 59A0B`,
`overlay_listing.py <dsun> 172 59A6E 59C50` and `overlay_listing.py <dsun> 209 93160 94C00`.
