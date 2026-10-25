---
id: FND-AI-006
title: The key routine 28C9:0CFF is registered only by the main view's set-up in overlay 182 and by overlay 199 putting the main view back, as the main view window's handler and as the global key hook
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069A1B..0x00069A1E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069A39..0x00069A44
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069F9B..0x00069FA6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A32B..0x0006A336
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000884E8..0x000884F3
tool: Capstone 5.0.9 over overlay code; tools/research/exec-census/immediate_search.py and overlay_listing.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

`immediate_search.py` finds the word `0x0CFF` as an immediate five times in the load image and
the overlays, each a `push 0xcff` after a `push 0xb0`, the segment word of descriptor 22,
segment `28C9`:

| Push (file) | Overlay | Passed to |
|---|---|---|
| `0x69A1B` | 182 `+1167` | overlay 182 `+016B`, as the handler of `WIND/19501` |
| `0x69A3C` | 182 `+1167` | overlay 190 `+11A1`, through the far call to offset `0x0034` of segment word `0x05F0` |
| `0x69F9E` | 182 `+15D7` | overlay 190 `+11A1` |
| `0x6A32E` | 182 `+19F8` | overlay 190 `+11A1` |
| `0x884EB` | 199 `+0BC1` | overlay 190 `+11A1` |

Overlay 190 `+11A1` stores its argument as the global key hook `DS:A0F1` (FND-UI-065,
FND-EXE-587). Overlay 172 `+0475` and `+076F` and overlay 209 `+1788` pass `+11A1` the hook they
saved when they opened (FND-PARTY-128); every other caller of `+11A1` in FND-EXE-587's list passes
its own window's handler, or 0.

## Interpretation

Nothing calls `28C9:0CFF` directly (FND-AI-004), so it runs only as the main view's handler or as
the hook while the main view is up: after the view's set-up on Start Game and after a load
(`+1167`), after a region load (`+15D7`, FND-PARTY-125), on the way back from display mode 2
(`+19F8`, FND-PARTY-127), and when overlay 199's windows close (FND-PARTY-127). A key reaches it
as a kind-6 event when no menu, accelerator or mnemonic of the current window takes it
(FND-UI-065). While another window is open the hook holds that window's handler, and windows that
forward keys pass them to overlay 190 `+139B`, where Space does not reach this routine
(FND-COMBAT-025). So Space turns computer control off only in the main view, in or out of a fight.

## Alternatives

`immediate_search.py` reads instructions only; a far pointer to `28C9:0CFF` kept as data was not
searched for. Whether a menu, accelerator or mnemonic of `WIND/19501` takes Space before it
becomes a kind-6 event was not checked; FND-UI-025 lists only `APFM/19200` and `APFM/19201` in
that window.

## How to reproduce

`immediate_search.py <dsun> 0CFF`; `overlay_listing.py <dsun> 182 69A14 69A44`, `... 182 69F98
69FA8`, `... 182 6A328 6A338` and `... 199 884E4 884F4`.
