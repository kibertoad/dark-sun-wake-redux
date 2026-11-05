---
id: FND-ITEM-091
title: Backing-transfer validator checks only offset arithmetic and zero-count placement
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:012E..15F3:014A
tool: tools/research/exec-census/resident_listing.py at f5d27c55; Python 3.12.10, Capstone 5.0.7; composition with FND-ITEM-090 and FND-EXE-409/414
environment: null
---

## Observation

Q-PARTY-116 retains backing-store validator and selector obligations.
FND-ITEM-090 identifies the raw word count and ordinary buffer passed
to 15F3:012E. This reads that helper's complete body and reuses existing
selector findings rather than recording duplicate selector-body findings.
Addresses and offsets are hexadecimal; arithmetic counts below are decimal.

### Offset acceptance

The helper takes the supplied pointer's offset word o at BP+6 and raw
count word n at BP+0A. It never reads the segment word or pointed-to
data. It starts AX at zero, computes the word sum o+n, and increments
AX on carry. It also increments AX when the low sum equals o and that
sum is nonzero. It restores BX/BP and returns AX. It has no child call,
interrupt, port access, pointer normalization or data-memory write apart
from its ordinary stack use.

For n nonzero, low16(o+n) cannot equal o. AX zero therefore means
the integer sum o+n is at most 65535. Overflow returns one. For n zero,
there is no carry: offset zero returns zero and any nonzero offset returns
one. These cases cover all word inputs; no data-validity predicate is added.

| Offset | Raw count | Returned AX | Meaning in FND-ITEM-090's caller |
| ---: | ---: | ---: | --- |
| 0 | 0 | 0 | Admits effective count 65536. |
| 1 | 0 | 1 | Rejects zero-word transfer at a nonzero offset. |
| 65534 | 1 | 0 | Admits one byte without offset carry. |
| 65535 | 1 | 1 | Rejects a sum of 65536. |

The table is arithmetic derived from the complete static body, not an
executed original or synthetic experiment. A null segment:offset can pass
when its offset/count satisfy these rules; memory validity remains separate.

### Composition with transfer cursors

FND-ITEM-090's positive raw counts that pass this gate cannot overflow
the buffer offset during their total admitted progress, provided count,
buffer and local cursor survive child calls unchanged. Zero raw count can
pass only at offset zero and requests 65536 bytes. Its final offset update
wraps to zero; the loop has no further transfer after that final chunk.
This explains the caller's offset-only advance without turning the helper
into a segment-span or physical-memory validator.

### Existing selector contracts

FND-EXE-414 already records 1425:0FA3: six-record age updates precede
identity selection; a miss can publish identity, age and output index before
the later 0C04 result is returned. FND-EXE-409 already records 1425:060B:
the low-six-bit output and age updates precede possible failure, and a
returning backend read is followed by tag/age/output publication even for
a nonzero result. Its callback preservation and output aliases remain open.

These findings supply the local selector logic for FND-ITEM-090. They do
not prove that an identity or tag hit supplies complete backing metadata.
Native selector/chain producers and lifetimes, 0C54/0C04/0591 effects and
installed backend targets/preservation still require their own evidence.
No rereading or confidence promotion is needed to reuse those bounded facts.

## Interpretation

This removes 15F3:012E's local acceptance and effects, and the already
recorded selectors' local selection/publication logic from Q-PARTY-116.
Actual readable/writable buffers, handle/chain identity, backend contents,
child preservation and lifetime remain. Offset admission is not content
admission; returned selector success is not a global coherence invariant.

## Alternatives

Treating the validator as a memory probe invents a dereference. Treating
zero count uniformly as no work misses the caller's expansion. Treating
all nonzero-count offset wraps as admitted ignores the carry rejection.
Treating matching selector tags as proof of successful prior fill ignores
the retained failure-publication findings. No native defect, complete-reading
declaration or parity promotion follows.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and project-local evidence Python -I
with resident_listing.py on the half-open location range. Under
GAME_DIR/analysis/item-removal-20261104/,
effect-transfer-offset-validator.txt and its -repeat pair both have SHA-256
e7950ca5143403279957a2b2eee15214e75176945e8b6976afd91b5fa32a74ac.
Reuse FND-EXE-409/414's retained selector contracts and reproduction steps.
Keep reports local. No original game or isolated function was executed.
