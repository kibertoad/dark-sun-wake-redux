---
id: FND-ITEM-027
title: Outer hit returns are overwritten on several direct routes but can pass through a cleanup skip and resident forwarding wrapper
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F61F..0x0007F63D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F6D9..0x0007F6DC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F08B..0x0007F0B7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F2AE..0x0007F2C3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F31F..0x0007F344
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F3BA..0x0007F409
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0303..31E0:0332
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:2D84..31E0:2D91
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:04CA..371D:04DF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008185C..0x00081874
tool: tools/evidence/report.mjs x86-incoming; tools/research/exec-census/overlay_listing.py and resident_listing.py at ea2dee7d; locked evidence reader
environment: null
---

## Observation

This replaces FND-ITEM-026: its forwarding call site was correct, but
it incorrectly named the adjacent function +04B3 as the containing entry.
The inventory and exact body reading establish +04CA..+04DF instead.
The caller-count and other continuation observations remain unchanged.

FND-ITEM-021 leaves two outer functions forwarding the hit byte. Controlled
incoming queries qualify their known direct callers and subsequent wrappers:

| Target | Confirmed direct call sites |
|---|---|
| Overlay 193 +1119 | Overlay 193 +132F |
| Overlay 193 +1249 | Overlay 193 +1348 |
| Overlay 193 +12A8 | Overlay 193 +0D9B, +0FBE, +102F, +10CA |
| Overlay 193 +1054 | Overlay 193 +104A |
| Overlay 193 +0DC7 | 31E0:0303, 31E0:2D84, 371D:04D5, overlay 195 +00EC |

Both first-level callers clean their arguments and join overlay 193 +13E9,
a restore-and-return ending. Thus +12A8 can forward the low byte on those
two paths. No normalization of AH follows.

Three of +12A8's four direct continuations replace the low byte before
using it. At +0D9B and +0FBE they load AL from their own locals before
requesting append +1519. At +102F they increment a local counter and load
that word into AX before testing the loop bound. Those local values are
not saved hit flags merely because they follow the call.

At +10CA in +1054, the caller instead rereads the shared count DS:2102
and compares SI with it. When SI is below count, AX is overwritten from
count before copy preparation. Otherwise it skips that assignment, decrements
count and tests the loop bound again. If that test exits, the epilogue
returns without replacing AX. This is a conditional register path, not
proof that a native call shrinks count sufficiently to take it. Producer,
lifetime and alias admission remain Q-ITEM-011.

The sole qualified direct +1054 caller, +104A in +0DC7, cleans arguments
and returns without replacing AX. Of +0DC7's four qualified callers:

- 31E0:0303 tests DI; its nonmatching branch overwrites AX at +0311,
  while the matching branch overwrites AL with one at +0330.
- 31E0:2D84 overwrites AX for metadata setup at +2D8C.
- Overlay 195 +00EC requests 3143:00A8 with its own SI/code arguments;
  that helper replaces the low byte and returns its separate lookup value
  as recorded by FND-ITEM-023.
- 371D:04D5 only cleans arguments and returns. Its containing wrapper
  begins at 371D:04CA. This remains a possible incidental low-byte
  forwarding route whose later consumers are not read here.

## Interpretation

Several outer routes discard the low-byte register value, but the cleanup
skip cannot be treated as an unconditional overwrite. The finite remaining
direct-consumer obligation is the possible return through 371D:04CA.
FND-COMBAT-054's caller walk was deliberately restricted to its starts;
the wider query adds direct parent callers without contradicting that scope.
No native feasibility, whole-word return meaning, complete reading or
confidence promotion follows. Other incoming mechanisms remain Q-ITEM-010.

## Alternatives

- A shared epilogue can preserve AL even when other branches overwrite it.
- A count reread after a child need not equal its earlier value.
- The copy branch does not prove its skip branch overwrites AX.
- A restricted reachability walk is not a full direct-caller census.

## How to reproduce

Reuse FND-ITEM-020's controlled reader mapping and FND-ITEM-025's eight
call controls. Set targets 7F409, 7F539, 7F598, 7F344 and 7F0B7 in
separate x86-incoming queries. Each retains the same verified source,
205 regions, 2376 roots, 588250 bytes, format controls and scan/instruction/
result limits. All five reports are untruncated with partialSearch false,
zero additional matching raw or contested rows and 896 unresolved calls.
The computed-transfer and undecoded exclusions remain FND-ITEM-020's;
this is no proof of universal caller completeness.

Inputs/reports remain in GAME_DIR/analysis/hit-incoming-20261104/ with
stems outer-193-1119, outer-193-1249, outer-193-12a8,
outer-193-1054 and outer-193-0dc7, suffixes .json and .report.json.
Use the named bounded readers on the locations above. Retained listings
are outer-1119-consumer.txt, outer-shared-return.txt,
dispatcher-consumer-7F08B.txt, dispatcher-consumer-7F2AE.txt,
dispatcher-consumer-7F31F.txt, dispatcher-consumer-7F3BA.txt,
dispatcher-last-skip-tail.txt, dispatcher-resident-parent.txt,
dispatcher-resident-parent-tail.txt, dispatcher-parent-2d84.txt,
dispatcher-parent-371d.txt and dispatcher-parent-195.txt. Some retained
windows extend into adjacent code; claims use only the stated paths.
Readers verify installed DSUN.EXE size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. No original content is committed.

For the corrected boundary, read resident 371D:04B3..371D:04DF and
compare the adjacent inventory rows: +04B3..+04CA and +04CA..+04DF.
The retained resident-forwarding-boundaries.txt covers both complete
functions. The first returns at +04C9; the second owns call +04D5.
