---
id: FND-PARTY-283
title: Three graphics alias stores depend on FONT pointer bytes and an allocator segment, not resource selector values
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0009..39D1:0316
tool: tools/research/exec-census/resident_listing.py at 1399b7ec; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; reuse FND-CONFIG-207, FND-EXE-292/293 and FND-RNG-093
environment: null
---

## Observation

Q-PARTY-124 retains fourteen unclassified writer values after
FND-PARTY-282. Three are in graphics initializer 39D1:0009:
0105 and 0132 overlap selected word DS:A0EC; 0154 writes selected
word DS:9E76. Installed DSUN.EXE is 634416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Offsets and byte conditions
below are hexadecimal; decimal resource number 1001 is word 03E9.

### The FONT request's returned pointer

FND-CONFIG-207 already identifies the double-word FONT tag,
selector 100 decimal and zero third argument passed to 38FF:0438.
At 0105 the initializer stores returned DX to DS:A0EB, then returned
AX to DS:A0E9. The subsequent AX OR DX zero test precedes the
copy at 0132, but follows the store at 0105.

Selected word A0EC starts at the high byte of the stored segment.
At 0105 its low byte becomes the returned DX high byte, while its
high byte at A0ED is not written by that instruction. Thus this store
leaves selected word 03E9 exactly when returned DX is E900..E9FF
and the contemporaneous byte at A0ED is 03. A null return forces
the selected low byte to zero at this store; it cannot produce 1001.

The nonzero continuation at 012E reloads the stored far pointer
from A0E9 and at 0132 copies its double word to A0ED. This store
changes selected A0EC's high byte to the reloaded pointer's offset
low byte, while leaving A0EC's low byte untouched. Its exact local
1001 condition is that byte A0EC is E9 and the reloaded offset's
low byte is 03.

Under retained DS and pointer storage from return through copy,
the pair composes to a single joint condition: the FONT pointer's
segment is E900..E9FF and its offset low byte is 03. This is not
the FONT selector's numeric domain. The nonnull check rejects zero
only; it does not independently exclude either byte condition.
Pointer acquisition/representation and the named storage preservation
remain obligations. No new reading of the shared service is required
to recover these already recorded local producer identities.

### The allocation segment store

FND-EXE-292 identifies the next request: at 0146 the initializer
pushes double-word 00000400 and calls 444C:00FA. FND-EXE-293
establishes that wrapper's increment to request 00000401 and its
unchanged return of the allocator's saved DX:AX, subject to its
frame, metadata and shared DS conditions. At 0154 the initializer
stores DX to DS:9E76, followed by AX to 9E74 and the whole-pointer
zero test. The segment store therefore occurs even on null return.

At that store selected word 9E76 is 03E9 exactly when returned DX
is 03E9. A null return writes zero; a nonnull test alone does not
exclude segment 03E9. This differs from the FONT overlap's E9
high-byte condition and must not be merged with it.

FND-RNG-093 and FND-CONFIG-223/211/212 already give the conditional
startup/valid-heap separation above resident data. With the modeled
DS=57E0, admitted startup base, preserved bounds/roots/links, valid
block extents and nonwrapping segment arithmetic, that allocation
provenance excludes segment 03E9. Those conditions are required;
neither the requested size nor a nonzero returned pointer establishes
them. The graphics caller supplies no independent segment-range test.

## Interpretation

The three immediate writer expressions and their 1001 conditions are
answered. Eleven other writer values remain unclassified. These three
sites retain explicit obligations: FONT return-segment/offset provenance,
A0ED's prior byte for the first publication and pointer/DS preservation,
plus the allocation's returned-segment/valid-heap provenance. They are
not added to the 36 unconditional literal-store exclusions or
FND-PARTY-282's separate canonical segment-store exclusion.

FND-PARTY-279/281/282's other state/storage conditions remain, as do
83 other direct getter sites and repeated-hook preservation.
FMT-PARTY-007 stays supported. No queue item closes or complete
reading is declared.

## Alternatives

- A literal FONT selector does not constrain the pointer bytes later
  reused by an unrelated wrapped lookup.
- A stored segment's high byte and its complete word are different
  domains: the FONT overlap needs E900..E9FF, the allocation store 03E9.
- The two FONT stores compose only with retained DS and stored pointer
  bytes. Their own instruction-point conditions remain valid separately.
- Reusing the lower allocator reading preserves its admission conditions;
  it does not turn startup placement into an all-lifetimes invariant.

## How to reproduce

Run resident_listing.py on the fingerprinted DSUN.EXE with the
half-open range 39D1:0009..39D1:0316 from its established entry.
Retained report:
GAME_DIR/analysis/class-table-closure-20261106/graphics-alias-producers.txt.
Read 0105/0109, 010C..012E, 0132 and 0146..015B together.
Intersect each store's byte interval with A0EC..A0EE or 9E76..9E78
as appropriate, rather than treating a pointer or selector as one value.
Reuse FND-CONFIG-207 and FND-EXE-292/293 for the producer bindings,
and FND-RNG-093 with FND-CONFIG-223/211/212 for conditional heap
admission. No original game ran.
