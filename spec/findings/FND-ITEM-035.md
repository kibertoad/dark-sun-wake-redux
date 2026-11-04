---
id: FND-ITEM-035
title: The handler's local bitmap has distinct bounds for its cleared span and neighboring dispatch and result fields
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082269..0x0008228E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000823CE..0x000823F8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082720..0x00082724
tool: tools/research/exec-census/overlay_listing.py and operand_range.py at d08a0fa2; locked evidence Python and Capstone
environment: null
---

## Observation

Q-ITEM-013 retains handler-local frame preservation after FND-ITEM-034's
explicit output-pointer reading. This applies FND-ITEM-031's already-read
bitmap builder to the handler's different local destination. Frame offsets
below are hexadecimal; code ranges and lengths are decimal.

Overlay 195 +0AF9 reserves 68 bytes in hexadecimal, saves SI and DI,
and initializes local byte BP-5 to one at +0B07. It then passes near
offset BP-18 and its first slot to overlay 193 +1BA3 at +0B10.
After that call returns, it reads its code argument at BP+0A and saves
the word into local BP-8 at +0B1B for the dispatch comparisons.

The builder clears fifteen bytes and ORs each admitted row's code bit
at destination plus code divided by eight, as FND-ITEM-031 records.
For maintained DS=SS, a preserved pointer and a nonwrapping frame, its
clear span is [BP-18,BP-9). A code set bounded by 119 stays in that span.
The conditional 1..127 domain from FND-ITEM-032 permits one more byte,
at BP-9, but still stops before the dispatch word [BP-8,BP-6) and result
byte BP-5. The dispatch word is assigned after the builder returns,
whereas the result byte is initialized before it.

The builder itself accepts a full code byte without a local upper bound.
Consequently the signed API's current request code is insufficient to
admit this call: the builder scans existing rows for the slot. Their
stored code values, count, owners and indices need admission separately.
As a static arithmetic illustration, code values 152..159 select byte
offset 19 from BP-18, which is BP-5. This is not an observation of such
a row in native play or a claim that the handler's result was changed.

The local byte is a real result dependency on one handler route. Stores
at +0C5E and +0C7E clear it; +0C82 loads it into AL and jumps to the
shared restore-and-return ending at +0FB0. Other paths assign their
own result, so this is not a claim that every handler return reloads
BP-5. Child effects between these operations remain unproved.

## Interpretation

The prologue's bitmap call and neighboring fields are no longer unread
frame-layout dependencies. A fifteen-byte clear bound, a neighboring-field
separation argument and global code admission are different claims.
Q-ITEM-013 still needs the existing slot rows' admitted code set and
storage/lifetime, plus other child writes and aliases. Q-ITEM-011 retains
the near DS destination's relationship to the SS frame. No native defect,
whole-handler preservation, complete reading or status promotion follows.

## Alternatives

- The current request's positive signed code does not bound all preexisting
  effect rows scanned by the builder.
- A store outside the fifteen-byte clear span need not reach either named
  neighboring field; the 1..127 arithmetic illustrates the distinction.
- Assigning the dispatch word after the builder does not initialize the
  result byte again or prove that another child preserves it.
- The local result route does not describe every handler return.

## How to reproduce

Use installed DSUN.EXE through tools/game-dir.mjs with locked evidence
Python; the readers verify size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run overlay_listing.py for descriptor
195 with ranges 82269 8228E, 823CE 823F8 and 82720 82724.
Reuse FND-ITEM-031's complete builder and fill-helper reading.

For the local-field candidate locator, use FND-ITEM-034's metadata-only
effect-handler-row.tsv and run operand_range.py with bounds FFE8 FFFC.
The initialized byte, bitmap-address producer, saved dispatch word and
local-byte reload are independently reviewed instruction controls.
Inventory linear decoding and its restart behavior establish no universal
absence of other field access or alias.

Keep reports only in GAME_DIR/analysis/hit-incoming-20261104/:
effect-handler-prefix.txt, effect-handler-bitmap-locals.txt,
effect-handler-local-result.txt and effect-handler-ending.txt.
The retained ending covers only the verified shared epilogue 82720..82724.
The other retained windows extend beyond some reproduced bounds.
No original listing, bytes or extracted rows are committed.
