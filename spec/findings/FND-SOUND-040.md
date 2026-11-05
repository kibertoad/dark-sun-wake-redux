---
id: FND-SOUND-040
title: Setup group allocation size is stored in the first prefix word before the offset-four payload
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1A6C..1000:1A95
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1AF5..1000:1C5D
tool: tools/research/exec-census/resident_listing.py at c5c359c4; Python 3.12.10, Capstone 5.0.7
environment: null
---

## Observation

FND-SOUND-026 establishes the setup allocator's request rounding and
normal offset-four return. Q-CONFIG-006/011 and Q-SOUND-010 still need
native record-byte/extent coherence beyond FND-SOUND-039's pointers.
This binds the prefix size field used by a bounded observation without
assuming DSUN's different heap markers or following a free-list pointer.
Addresses and offsets are hexadecimal; sizes are decimal.

The common request path at 1000:1BE9 rounds its positive double-word
request to paragraphs as floor((request+19)/16). For the one-word group
request 1BD6 first clears the high word. A normal result returns the
selected block's segment in DX and payload offset four in AX.

On initial heap allocation, 1AF5 retains the paragraph request across
its break/alignment calls, then stores that size as word zero of the
returned block segment before returning offset four. On heap growth,
1B59 likewise retains the requested paragraphs; both its aligned and
adjusted successful branches store that word at segment offset zero.
The split path 1BB3 subtracts the requested paragraphs from the free
block, selects the resulting tail segment, and stores the requested
paragraphs as its word zero. The exact-size free-block path compares
word zero with the paragraph request before unlinking through 1A6C;
that unlink helper does not change word zero. Normal exact-size return
therefore retains the matching size word.

Under the existing valid-heap, separate-storage and child-result
conditions, all these normal paths supply the same local binding:

| Relative to returned block segment | Size | Meaning |
| --- | ---: | --- |
| 0000 | 2 | Block size in paragraphs, including its four-byte prefix |
| 0002 | 2 | Remaining prefix word; not interpreted or followed here |
| 0004 | Requested bytes | Returned payload |

The block's half-open physical extent is segment*16 through
segment*16+16*paragraphs. Its modeled payload capacity is
16*paragraphs-4. Neither offset-four alone nor an arbitrary size word
proves live ownership; this binds the size convention under the normal
allocator contract, not all heap states.

For the observed counts of FND-SOUND-039, the group reader's low-word
count-times-254 requests yield:

| Group profile | Requested bytes | Expected size word, paragraphs | Modeled payload capacity |
| --- | ---: | ---: | ---: |
| Positive count 13 | 3302 | 207 | 3308 |
| Signed count -1 | 65282 | 4081 | 65292 |

Reading only word zero of each returned segment can check this necessary
extent condition without following the second word, walking the heap,
reading rounded slack or copying the negative-count group's unused
payload. A populated group's 3302-byte record span is inside the modeled
capacity. That span and the headers still need native observation.

## Interpretation

The location/width of the normal allocation-size field is removed from
the remaining extent uncertainty. The native value and record contents
remain Q-CONFIG-006/011 and Q-SOUND-010. Header validation can bound a
specific observation; it cannot establish universal heap lifetime or
replace pointer, stop, source and disjointness checks.

## Alternatives

Treating the second prefix word as a payload size, reusing a DSUN heap
marker or scanning free-list links would not follow this binding. A stale
or aliased pointer, modified size word, invalid heap or failed service
can violate the normal preconditions. No native header was read by this
static batch and no complete-reading or parity promotion is claimed.

## How to reproduce

Use approved installed SOUND_DS.EXE length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. Run resident_listing.py with -I and
the two code ranges above in order. Retain setup-group-prefix-size.txt
and setup-group-prefix-size-repeat.txt under
GAME_DIR/analysis/disc-adv-survey-20261104/. Both have SHA-256
5fc143a550c5700869ff3c0da89ecaf1bd4d31d5648f5c007a502f266357128a.
Track the requested paragraphs across each normal constructor/split
branch and the exact-size compare/unlink return, without interpreting
data near 198C as instructions. Reuse FND-SOUND-026's caller/request
binding and keep native success separate from the local header convention.
