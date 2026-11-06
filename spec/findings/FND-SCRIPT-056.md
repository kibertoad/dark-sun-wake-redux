---
id: FND-SCRIPT-056
title: The twelve script setup requests fit the recorded heap arithmetic without establishing live capacity
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 444C:00FA..444C:0162
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:15A5..1000:1622
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00057580..0x000579A2
tool: existing-evidence composition at fd5dd3b2; FND-EXE-293/294, FND-CONFIG-223/211/212 and FND-SCRIPT-055; integer request-domain calculation
environment: null
---

## Observation

This assesses the complete named setup-request domain against already
read allocation interfaces, rather than reopening the lower heap reading
for each buffer. It uses DSUN.EXE size634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and the caller arguments 200 and10000
from FND-CONFIG-156. FND-SCRIPT-055 records the twelve requests and
their four failure checkpoints. Different callers or changed input/state
bindings are outside this domain assessment.

### The wrapper adds one before the heap's paragraph conversion

FND-SCRIPT-053 binds the setup allocation helper to 444C:00FA.
FND-EXE-293 reads that complete wrapper: it increments its incoming
double-word request by one, calls 1000:15A5, and returns zero without
header access if the returned far pointer is zero. A nonzero pointer
instead causes a preceding-header read and a header-derived marker write;
the pointer returned to the setup caller is unchanged.

FND-EXE-272 and FND-CONFIG-223 read the shared heap entry. For a
nonzero request it adds19, rejects addition carry or an adjusted high word
with upper twelve bits set, and converts the total to paragraphs by a
four-bit right shift. The byte-request admission maximum is1,048,556.
For a setup input N, the heap therefore receives N+1 and computes
P=floor((N+20)/16), if that arithmetic is admitted. None of the twelve
named inputs wraps the increment or selects the heap's arithmetic rejection.
The largest forwarded request is10002, well below that limit.

FND-EXE-294 and FND-CONFIG-211 connect the recorded ordinary return
paths to offset four and a selected/produced paragraph-count header.
With count P retained in that header, the wrapper's marker address is
segment offset16P-1, or displacement16P-5 from the returned pointer.
The nominal byte span after the four-byte header is16P-4. These formulas
are conditional address relationships, not evidence that DOS acquired
the segment or that its bytes remain writable.

| Setup allocation | N | P | Nominal bytes after header | Marker displacement from returned pointer |
| --- | --- | --- | --- | --- |
| Trigger pool | 2600 | 163 | 2604 | 2603 |
| Global flags | 101 | 7 | 108 | 107 |
| Global numbers | 400 | 26 | 412 | 411 |
| Global big numbers | 80 | 6 | 92 | 91 |
| Global strings | 420 | 27 | 428 | 427 |
| Working buffer | 300 | 20 | 316 | 315 |
| Local flags | 9 | 1 | 12 | 11 |
| Local numbers | 64 | 5 | 76 | 75 |
| Local big numbers | 40 | 3 | 44 | 43 |
| Local strings | 420 | 27 | 428 | 427 |
| Working allocation | 32 | 3 | 44 | 43 |
| Script buffer | 10001 | 626 | 10012 | 10011 |

Every P in this table is at most626. Its shift by four, offset additions
and marker displacement fit the wrapper's word arithmetic when the
returned offset is the recorded four and the header retains P. Thus
the named request arithmetic does not itself require an offset-wrap
case. Changed returned offsets or headers retain their separate admission
obligations; they are not covered merely because the requested N is small.

For the trigger pool specifically, the allocator wrapper forwards2601,
not2600. The nominal block is2608 bytes including its four-byte header.
The 200 thirteen-byte records occupy returned displacements0..2599;
the nominal marker at displacement2603 is outside those records. The
initial link loop's last two-byte store at displacement2598 therefore
does not reach that marker under the stated bindings. This is an address
calculation, not a native pool-capacity observation.

### Reuse the shared interface without assuming a live heap invariant

FND-CONFIG-211 supplies distinct exact-block, tail-split and new-memory
header producers. FND-CONFIG-212 retains request/bound arithmetic and
runtime conditions. Their active shared heap obligations remain within
Q-CONFIG-008; historical FND-EXE-293/294 references to Q-EXE-010 do
not create a new heap owner. That old item was answered for its overlay
entry-position question by FND-EXE-520, not by proving every allocation.

Q-SCRIPT-019 still needs the actual admitted header/root/link/bound and
service state, source/returned pointer preservation, independent storage
and lifetime at trigger uses. Those are different from the now-classified
request arithmetic. The 444C:00FA wrapper has no own user-buffer fill.
Setup's explicit variable clears and link writes, followed by
later reset/restore effects, remain FND-SCRIPT-026/031/055 rather than
an assumed allocator initialization guarantee.

## Interpretation

All twelve recorded setup requests admit the local size conversion and
have finite conditional payload/marker relationships. Reusing the shared
heap findings removes the need to reread the same allocator arithmetic
for these callers. It does not establish actual acquisition, global heap
validity, initialized trigger records, or a whole-entry complete reading.
No queue item is closed or status promoted by this domain assessment.

## Alternatives

Calling2600 the unchanged lower-heap request drops the wrapper increment.
Placing the marker immediately after exactly2600 payload bytes drops
paragraph rounding and the header. Treating the nominal span as acquired
capacity or initialized bytes drops heap/service admission and lifetime.
Conversely, generic large-request overflow cases do not describe this
fixed twelve-request domain under its retained bindings.

## How to reproduce

Reuse FND-SCRIPT-055's caller/request-stage reports and FND-EXE-293's
complete wrapper reading. Reuse FND-CONFIG-223's current heap-entry
arithmetic, FND-EXE-294's selected DS/DX relation, and FND-CONFIG-211/212
for header producers and remaining runtime bounds. For each N in the
table calculate N+1, P=floor((N+20)/16),16P-4 and16P-5; require the
trigger control N2600 giving forwarded2601/P163/marker2603 and the
largest control N10001 giving forwarded10002/P626/marker10011.

The arithmetic controls are calculations from the recorded interfaces,
not executed heap fixtures. The old Q-EXE-010 closure is commitafcaa542
with its Queue trailer and FND-EXE-520; do not revive its historical
references as a live allocation question. No new reader, original-game
run, emulated call or original-derived output is needed for this assessment.
