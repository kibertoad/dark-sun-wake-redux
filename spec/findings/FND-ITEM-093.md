---
id: FND-ITEM-093
title: A zero-count constructor searching a retained zero key returns the uncounted boundary for nonzero coordinates
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:00F0..289B:0154
tool: tools/research/exec-census/resident_listing.py at ce4b1c77; Python 3.12.10, Capstone 5.0.7; composition with FND-ITEM-065/066/068
environment: null
---

## Observation

Q-PARTY-116 retains constructor count/key/extra-row admission and returned
K relative to persistent N. FND-ITEM-065 gives the complete constructor
and insertion composition; FND-ITEM-066 gives fresh allocation clearing;
FND-ITEM-068 binds the selected caller's key byte to zero. This resolves
the single-row zero-key search case, not an actual native constructor call.
Addresses and field offsets below are hexadecimal; counts are decimal.

### Preconditions and unchanged key

Assume entry count n=0, valid table storage and a retained zero key in
row zero: coordinate words +0/+2 and byte +4 are zero. Fresh ordinary
allocation under FND-ITEM-066's conditions supplies those bytes initially.
That finding does not show that they remain zero at constructor use.

The constructor makes persistent N=1 and searches that one-row prefix.
Its pre-child stores include byte-zero mask two in the object's separate
37-byte record and mask 40 in byte +5 of its old selected table row.
With admitted separate storage, even an old selected index zero changes
only flags +5 and leaves the search key's bytes unchanged. Aliasing,
other writers, valid old indices and child/frame preservation remain
preconditions, not conclusions from the count value.

### Complete one-row search cases

For supplied search count one, 289B:00F0 initializes both search bounds
and the midpoint to zero. It compares the key's unsigned y word, then
unsigned x word, then its signed byte +4 with the zero row key. A greater
comparison returns position one at the upper boundary; equal returns zero.
A smaller signed byte at tied zero coordinates returns zero at the lower
boundary. Coordinate words cannot be unsigned below zero.

| Prepared key against zero row | Returned position |
| --- | ---: |
| y nonzero, any x/byte | 1 |
| y zero, x nonzero, any byte | 1 |
| x and y zero, signed byte positive | 1 |
| x and y zero, signed byte zero or negative | 0 |

These cases cover all key bit patterns under the stated one-row inputs.
They are derived statically, not a synthetic or original-run experiment.
Flags and slot fields do not participate in the comparisons. Wrapped
negative coordinate bit patterns are still unsigned nonzero words here.

FND-ITEM-065's wrapper searches the same one row twice without a selected
existing-entry removal. With stable inputs both positions agree. Its leaf
increments a local count, while persistent N stays one under admitted
separated storage. Position zero shifts the old row to index one and copies
the prepared row to index zero. Position one copies the prepared row to
index one without a shift. Storage through index one is supplied by the
modeled fresh allocation, independently of which row is persistently counted.

### Selected placement caller

FND-ITEM-068's admitted caller uses byte +4 zero. For this caller and a
retained zero row key, x=y=0 therefore gives K=0<N=1. Every other pair
of prepared coordinate words gives K=1=N. In the latter case the prepared
flagged row is outside the persistent counted prefix even though the caller
accepts its returned index and can make immediate cell/effect requests.

The origin-key case supplies a counted prepared row only conditionally;
it does not prove a real map admission, target or surviving effect. The
nonzero-key case is likewise not evidence that a native game invocation
starts with this count and retained key. Existing installation/map loads,
restores, prior insertions and independent writers can change those inputs.
The final counted pass and later removal still require their own lifetime
and identity evidence.

## Interpretation

This removes uncertainty about K for the complete zero-count/zero-key
input domain from Q-PARTY-116. Fresh clearing alone is not use-time
admission: the remaining question is which native call, if any, retains
that state and what writes occur before both searches. Other counts/keys,
row ordering, actual slot/base/sample inputs and intervening row lifetime
remain open. No native lost effect or game defect is claimed.

## Alternatives

Treating an initially empty table as a zero-length search ignores the
constructor's prior persistent increment. Treating nonnegative K as counted
ignores K=N. Treating fresh allocation as retained zero content erases
later producers. Conversely, the conditional boundary case does not prove
that a shipped native path reaches it. No complete-reading declaration,
rule implementation or parity promotion follows.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and project-local evidence Python -I
with resident_listing.py on the half-open location range. Under
GAME_DIR/analysis/item-removal-20261104/,
effect-first-row-search-contract.txt and its -repeat pair both have SHA-256
144cb06616d574ecf1830b337d6cc1e82238167c766997e6b69c78f3a60fa672.
Compose FND-ITEM-065/066/068 without discarding their alias and lifetime
conditions. Keep reports local. No original or isolated function was run.
