---
id: FND-ITEM-031
title: The effect-block builder clears fifteen bytes but its code-byte bit store needs a separate destination bound
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006500F..0x00065025
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000650C3..0x000650F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FE93..0x0007FF07
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3F7E..1000:3FC1
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at 3a3e8b5e; locked evidence Python and Capstone
environment: null
---

## Observation

FND-ITEM-030 classifies the switch's explicit block dereferences as reads.
The block's producer is a different write dependency of Q-ITEM-013.
FND-MAGIC-021 records the builder's bitmap meaning; this reading checks
the exact destination footprint and caller frame relationship.
Frame offsets below are hexadecimal; code values and lengths are decimal.

The hit wrapper reserves 30 bytes in hexadecimal and initializes local
BP-7 to zero at +0D41. On its ordinary block-building route it calls
overlay 193 +1BA3 twice: at +0DFC with its second slot and near offset
BP-30, then at +0E09 with target DI and near offset BP-20. Neither
destination has an explicit SS segment argument at these calls.

The builder uses its current DS as the segment for the supplied near
destination. It first requests fill byte zero and count fifteen through
1000:3FA2. The complete fill contract in FND-CONFIG-144, checked here,
writes exactly that many consecutive bytes for valid nonwrapping storage.
This clear occurs before the builder's unsigned slot-below-48 gate;
rejecting the slot does not undo or skip the clear.

For an admitted slot the builder requests its first effect-row index
through 3143:000D. At each iteration it reads the row's owner word before
testing the signed index against DS:2100. Matching owner and an index
below that count enter the body. There is no local nonnegative-index test.
The loop increments the index after processing each row and stops at the
first owner mismatch or failed signed count test. Row-index and table
admission remain necessary; this is not a proof that every lookup result
is safe to read.

The body reads the row's code byte, zero-extends it to a word and divides
by eight. It adds the quotient to the supplied destination offset and
ORs mask one shifted by the code's low three bits into that DS-relative
byte. No local test limits the code to the fifteen-byte clear span.
For the byte's full numeric domain, the written offset is zero through
31. Codes zero through 119 stay within the fifteen bytes; codes 120
through 255 can address later bytes. This is local arithmetic, not a
claim that the original admits every possible byte as an effect code.

For maintained DS=SS and valid nonwrapping caller storage, the first
destination BP-30 plus offset 31 reaches BP-11, still below flags BP-7.
The second destination BP-20 can reach BP-1. In particular, code values
200 through 207 select offset 25, the flags byte BP-7. This is a static
conditional overlap demonstrating the required input bound, not evidence
that such a row occurred in native play or that an observed flag was
changed by the builder.

## Interpretation

The fifteen-byte clear and per-row bit store have different bounds.
Treating the former as the latter's capacity check would hide a frame
admission requirement. Q-ITEM-013 now has a concrete producer obligation:
establish admitted code values and row/index/owner lifetimes at these
calls, or account for their larger write footprint. Other frame aliases
and child effects remain. Segment identity is Q-ITEM-011; block semantics
also serve the existing Q-MAGIC-023 without settling that whole question.
No native defect, complete reading or confidence promotion is claimed.

## Alternatives

- Clearing fifteen bytes does not bound later indexed stores to fifteen.
- The builder's near DS destination is not automatically the caller's SS.
- A code-byte arithmetic example does not establish a native input state.
- An owner match does not prove a negative row index was admitted safely.
- Explicit reads in the consumer do not make its earlier producer read-only.

## How to reproduce

Resolve installed DSUN.EXE through tools/game-dir.mjs and use the locked
evidence Python. The readers verify size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run:

```text
overlay_listing.py <dsun> 179 6500F 65025
overlay_listing.py <dsun> 179 650C3 650F1
overlay_listing.py <dsun> 193 7FE93 7FF07
resident_listing.py <dsun> 1000:3F7E..1000:3FC1
```

Keep original-derived reports only in GAME_DIR/analysis/hit-incoming-20261104/:
hit-flags-initialization.txt, hit-block-producer-call.txt,
hit-block-builder.txt and block-fill-contract.txt. The first two retained
windows extend beyond the reproduced bounds; the claims use the stated
paths. The exact builder return and every local loop branch are included.
No original listing, bytes or extracted records are committed.
