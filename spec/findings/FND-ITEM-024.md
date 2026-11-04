---
id: FND-ITEM-024
title: Random and refresh child entries replace incidental hit AX before consuming it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:391D..28C9:3963
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0822..1000:0848
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D37D..0x0005D3BA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005CDE7..0x0005CE64
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at ea793d60; locked evidence Python and Capstone
environment: null
---

## Observation

FND-ITEM-023 leaves implicit child-register inputs open on two loop
continuations. This reading follows only the first consumption of incoming
AX on those paths. It does not establish all child effects or argument and
memory admission, which remain separate obligations.

Resident repeated-roll helper 28C9:391D initializes its own total and
tests its count argument. A count below one explicitly clears AX before
returning. A positive count calls generator 1000:0822 before reading AX
as a draw. The generator reads its state into CX and BX, sets DX and AX
to multiplication operands, and only then calls its arithmetic helper.
Incoming hit AX is therefore replaced before any register-dependent
arithmetic or child call. FND-RNG-013 records the complete repeated-roll
arithmetic; this is its application to the continuation's implicit input.

Overlay 173 +2A5D similarly clears its own total and index, tests its
count argument and either calls the same generator or skips to its ending.
That ending explicitly assigns AX from its total and adds its own word
argument. Neither route consumes incoming AX as a hit result. The
overlay-204 loop's following use of AX is a use of this separate result.

Overlay 173 +24C7 saves its frame and SI, then jumps to +24EE. That
instruction overwrites AX for metadata segment setup, before the first
comparison or child call in the reached body. This also handles the
31E0:05BB invalid-index route identified by FND-ITEM-023: even if that
child returns with the hit value still in AX, +24C7 replaces it before
using it. The later caller assignment from local BP-19 remains the
separate Boolean described by FND-ITEM-023.

## Interpretation

The named random and refresh child paths do not consume incoming hit AX.
Together with FND-ITEM-021/022/023, this removes the local register-result
handling obligations for the sixteen known direct wrapper call sites in
FND-PARTY-102. Two sites forward the wrapper byte through their outer
returns, whose later consumers are still Q-ITEM-009. Other incoming routes
remain Q-ITEM-010. This is not a census of all computed callers, a claim
that memory effects are discarded, or a complete reading of the rules.

## Alternatives

- A child that preserves AX on one path can still be followed by a proved
  overwrite; the intervening call alone was insufficient evidence.
- Replacement of AX does not establish preservation of the caller's frame
  or exclude hit-induced local-memory changes; those remain Q-ITEM-011.
- An independent random result or local Boolean is not the hit flag byte.

## How to reproduce

With locked evidence Python and the installed DSUN.EXE resolved through
tools/game-dir.mjs, run the named readers:

```text
resident_listing.py <dsun> 28C9:391D..28C9:3963 1000:0822..1000:0848
overlay_listing.py <dsun> 173 5D37D 5D3BA
overlay_listing.py <dsun> 173 5CDE7 5CE64
```

The readers verify size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Reports are retained only in
GAME_DIR/analysis/hit-incoming-20261104/: child-random-inputs.txt,
child-2a5d-body.txt and child-24c7-exact.txt. The first two use the full
inventoried helper bounds; the last ends after the reached function's
return rather than including the adjacent function. No original listing
or bytes are committed.
