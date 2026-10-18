---
id: FND-PARTY-249
title: Two graph walkers pack item word 0x0E from separate bytes when an ancestor query returns 16 and word 0x10 is 313
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00086A93..0x00086CC7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D6F0..0x0005D73B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062AC5..0x00062B1E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:391D..28C9:3963
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at commit 31017697; Python 3.14.7, Capstone 5.0.7
environment: null
---

## Observation

These are local static readings of installed DSUN.EXE. No original code
was executed. Overlay offsets below are relative to the named descriptor.
FND-PARTY-218 supplies the ten-byte graph entry layout and keeps the graph
builder's DS/SS storage admission qualified.

### Shared selection

Overlay 197 functions `+3693` and `+37A5` each ask `2D40:2196` to build a
graph for their first word argument in an 800-byte stack buffer. Each
walks indices from zero while the signed index is below the returned
count. A candidate must have entry word +2 equal to kind 1. Each passes
that entry index and the graph pointer to overlay 173 `+2DD0` and keeps
the entry only when the returned word is 16.

`+2DD0` starts at the supplied index. While it is positive and its
parent entry's word +2 is not kind 2, it follows the current entry's
signed parent byte +1. It returns word +6 of the resulting entry.
Thus this is the selector at the ancestor immediately under a kind-2
parent on a well-formed terminating descendant path, rather than a test
of each item's own immediate field alone. For index zero it returns
the root entry's word +6, which is the root object number, not a field
selector (FND-PARTY-218). Thus a kind-1 root object numbered 16 can
satisfy this local gate too. Invalid indices, cycles and buffer
identity are not admitted by this local reading.

For retained kind-1 entries, both functions use entry word +4 as the
23-byte record index at DS:19C1. They subtract one from record word
`0x10` at 16-bit width and compare with 312, so only a source word of
313 enters the payload update. Neither compares a bit mask here.

### Writer at +3792

The function at `+3693` saves the record's low byte `0x0E` and starts
a separate local byte at zero. For a nonzero saved low byte it calls
overlay 177 `+09C5` with its second word argument, adds the returned low
byte to the zero local byte and clamps that local byte to the saved
low byte using unsigned comparison. It sets its returned-change byte
to one on this branch, even when the helper returns zero. For a zero
saved byte it skips that call and leaves the change byte unchanged.

It stores a complete word at `0x0E`: saved low byte plus the local
byte in the high position. In particular it does not preserve the
record's incoming high byte. There is one helper call per qualifying
nonzero-low record, not a loop accumulating repeated helper results.

The helper `177 +09C5` returns 3 for a signed input from 269 through
304. For inputs 0 through 114 it selects the ten threshold bytes at
DS:0656; for 115 through 234 it selects those at DS:0661. It descends
an index from ten until the preceding unsigned threshold is no greater
than the input, or the index reaches zero, then returns the index in AL.
Other signed inputs return zero. Therefore its local result is always
from zero through ten regardless of threshold contents. On stable
admitted inputs, this writer's high byte is the lesser of that result
and the saved low byte. Threshold provenance and gameplay naming remain
separate obligations.

### Writer at +38B3

The function at `+37A5` likewise saves the low byte and starts its other
local byte at zero. It first calls the same helper with its second word
argument. A nonzero unsigned AL skips the intervening change branch.
Otherwise it calls the helper again with the same argument, subtracts
that AL from the zero local byte, sets its change byte to one, then
calls `28C9:391D` with word arguments 1 and 100. This is RULE-RNG-001's
`roll_sum(1, 100)`, not the inclusive-range helper. A signed returned
word below 10 decrements the saved low byte modulo 256; another result
leaves it alone. There is no zero guard on that decrement.

It finally packs the saved low byte and local byte into word `0x0E`.
With the helper's unchanged DS/threshold inputs, its two calls return
the same AL: the second call on this branch is also zero, leaving the
stored high byte zero. With a nonzero first result, the high byte is
also zero and no random draw occurs for that entry. The incoming high
byte is never loaded by either path. This establishes local arithmetic
and draw order; it does not establish that a particular gameplay route
admits those inputs or that a zero low byte occurs on that route.

Both functions start their change byte at zero, retain it across later
entries and return it in AL after the graph walk.

## Interpretation

The two remaining direct stores listed by Q-PARTY-020 write packed byte
values, rather than uniformly decrementing a word counter. Their shared
selection also provides a concrete whole-word use of item `0x10` as
313. It does not assign one global meaning to that word: FND-PARTY-248
already shows a different type-dependent copy operation. Field-16
gameplay naming remains Q-PARTY-082; the root-object exception and
graph admission remain qualified under Q-PARTY-099, and
these packed-byte effects' caller/consumer meaning Q-PARTY-106.

## Alternatives

- Treating the incoming high byte as an accumulated value is ruled out
  locally: neither function loads it, and both initialize their own byte
  to zero before the complete-word store.
- A persistent uses-left label for every word `0x0E` is not established.
  These branches select one exact source word and independently pack
  two bytes; their user-action and subsequent-consumer meanings remain open.
- These are not complete readings of the graph builder or the functions'
  callers. The graph's count, parent termination, capacity, DS/SS identity,
  current record/table identity and threshold initialization stay qualified.
- Repeated helper equality assumes stable DS and threshold storage; the
  helper itself has no calls or stores. Asynchronous writers and other
  graph/runtime bindings are not ruled out by these bodies.

## How to reproduce

With the locked evidence Python and installed executable `<dsun>`:
`overlay_listing.py <dsun> 197 86A93 86CC7`,
`overlay_listing.py <dsun> 173 5D6F0 5D73B`,
`overlay_listing.py <dsun> 177 62AC5 62B1E`, and
`resident_listing.py <dsun> 28C9:391D..28C9:3963`, all under
`tools/research/exec-census/`. The scripts verify the source length and
XXH3 before decoding. Compare the complete inventory ranges starting
at file offsets 86A93, 86BA5, 5D6F0 and 62AC5. Follow the zero/nonzero
helper-result branches separately and the exact argument order of the
roll helper; do not infer a percentage or a gameplay effect from its name.
