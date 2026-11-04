---
id: FND-ITEM-025
title: The two qualified direct queue callers restore saved low-byte results
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E2F0..0x0007E32C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065005..0x0006500F
tool: tools/evidence/report.mjs x86-incoming; tools/research/exec-census/overlay_listing.py at e5bbccc2; locked evidence reader
environment: null
---

## Observation

Q-ITEM-009 asks whether a caller consumes the queue runner's incidental
hit return. FND-ITEM-018 reads its near caller in overlay 179. A controlled
incoming query for canonical target 654BC, overlay 179 +11DC, confirms
that near call at file 65005 and one far call at file 7E322, overlay 193
+0032. There are no additional matching raw candidates or contested calls
in this declared domain. This does not settle computed incoming routes.

The overlay-193 wrapper starts at +0000. It prepares its arguments, calls
its inner +003C routine, cleans 22 argument bytes and saves AL in local
BP-1. It then calls the queue runner at +0032. On return it reloads AL
from that local and returns. Thus the caller returns its saved inner result
rather than the queue's low byte, assuming its frame survives the child.
FND-ITEM-018 establishes the analogous save/reload in overlay 179.
Neither reading proves that AH is normalized or that the queue preserves
arbitrarily aliased storage; frame admission remains Q-ITEM-011.

## Interpretation

The two qualified direct queue callers overwrite the runner's AL before
returning. Their outer results must not be described as its incidental
last-hit byte. The direct low-byte consumer obligation is settled; indirect
incoming routes and any meaningful high-byte contract remain unproved.
No complete-reading, rule promotion or native observation follows.

## Alternatives

- Reloading a saved low byte does not clear AH.
- A nonpartial scan is not proof that every computed caller was found.
- A saved stack byte still needs the frame-preservation qualification.

## How to reproduce

Reuse FND-ITEM-020's reader configuration, changing only target to 0x654BC.
The retained queue-179-11dc.json uses the wrapper query's additional known
call controls, giving eight controls total: 64FFA, 65579, 7E7CE, 0D46B,
71463, 5D1C9, 7F74E and 8D0CE. Keep the same source fingerprint,
205 regions, 2376 roots, 588250 searched bytes, source-table controls,
scan limit 1048576, instruction limit 588250 and result limit 10000.
Run node tools/evidence/report.mjs x86-incoming with that configuration.

The report is untruncated and partialSearch is false. It retains 896
unresolved calls; FND-ITEM-020's mapping and computed-transfer exclusions
still apply. Controls are general call-decoding controls, not an assertion
that their targets equal this query's target. No new negative proof is made.

With the locked evidence Python, run overlay_listing.py on the installed
DSUN.EXE with descriptor/start/end 193 7E2F0 7E32C. The reader verifies
size 634416 and XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.
Inputs and reports stay in GAME_DIR/analysis/hit-incoming-20261104/:
queue-179-11dc.json, queue-179-11dc.report.json and queue-outer-body.txt.
Reuse FND-ITEM-018's retained near-wrapper report. No original content is
committed.
