---
id: FND-EXE-411
title: Linked-record release clears its caller head before mutable-node updates and can fail after publication
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:09AD..1425:09FD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:09FF..1425:0B26
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-408's link writer candidates 0AA5 and 0B0B are in
the local body starting 09AD. It saves BP and allocates four
local bytes. It reads a word through incoming far pointer SS:BP+6;
zero returns AX zero without later calls or node stores. Nonzero must
be unsigned above current DS:00C8 and below current DS:00C6.
Either failed comparison returns AX nine. The input word is re-read
between comparisons and for the subsequent lookup argument.

### Preflight link walk

It calls 060B with that input word and far output addresses
SS:BP-2 and SS:BP-4, removes ten outgoing bytes and tests the
full returned AX through DX. Nonzero returns that word. Zero reads
the current DS:39CE word indexed by BP-2 times 0108 plus
BP-4 times four, using word arithmetic.

A nonzero link must not equal one and must be unsigned below
current DS:00C6; rejection returns nine. There is no repeated lower
bound against 00C8 and no local cycle or visited-node check. The
body writes FFFF to current DS:39CA indexed by BP-2 before
calling 060B with that link. A nonzero result returns immediately;
zero repeats the link test from the new local outputs. Age-field writes
can therefore precede an eventual error and remain unrolled back.

A zero link ends this phase. The body again writes FFFF to
the current record's 39CA, then re-reads the caller head word and
calls 060B for it. A failure returns without clearing the head
locally. A zero result compares the re-read head unsigned with current
DS:00CA and lowers 00CA when smaller, then clears the pointed-to
head word. It proceeds using current local outputs from that last call.

### Release and failure prefixes

It reads the selected current link into DX. Nonzero enters a release
step that lowers 00CA when the link is smaller, writes one to
the current indexed 39CE link, decrements current DS:00CC and
sets indexed 39C4 one. It writes FFFF to indexed 39CA
and calls 060B with DX, the link captured before those stores.
Nonzero returned AX exits through DX; zero repeats from current outputs.
This phase has no local link-one or upper-bound check before lookup.

A zero link selects the final step: it writes one to that
indexed 39CE, decrements 00CC, sets indexed 39C4 one and
returns AX zero. Counter updates are word-width and have no local
underflow guard. The head has already been cleared before either release
step. Failure while looking up a later node therefore follows the head
clear and earlier node/counter/flag updates; no local undo occurs.

FND-EXE-409 supplies 060B's early output, record selection, mutable
callback and post-transfer tag publication. This body re-reads output locals,
far input pointers and current DS fields; neither stable links nor disjoint
head/output/frame storage is checked. Earlier validation does not establish
later input validity or native preservation. A cycle in preflight links has
no local cycle-based exit, even when every individual link passes its bounds.

All local returns discard the frame and return far without incoming cleanup.
The two decoded spans contain 32 and 106 instructions and cover
80 and 295 bytes respectively. The two intervening bytes at 09FD..09FF
are outside these inventory spans; this reading makes no claim that they
are reached or owned. Every branch described here lands in the listed spans.

## Interpretation

This supplies two further link-field writers and their different failure
prefixes. Q-EXE-007 retains complete callers/writers, the candidate at
1296, stable/admitted link structure, DS/input/alias/extent provenance and
slot-target preservation/native effects. Returning zero does not independently prove
valid head storage or successful native transfer. No complete reading, rollback
contract or observed original bug is declared.

## Alternatives

Treating an empty head as invalid contradicts its zero return. Treating
preflight bounds as repeated release bounds adds absent checks. Treating the
release as atomic erases the head clear and node writes before a
later failure. Treating word decrements as checked accounting invents an underflow
guard. A valid bound on each link is not a termination bound
on a cyclic traversal.

## How to reproduce

At revision 6fba11d4 require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode file base
0x9450 plus each Locations span separately in sixteen-bit mode. Require
80/295-byte coverage and 32/106 instructions. Follow the initial unsigned
head tests, every 060B input/output binding, full-word results, indexed
link reads, head reload/clear, preflight bounds and release stores in order.
Retain the unproved two-byte gap rather than adding it to reached coverage.
Use FND-EXE-408 for candidate provenance and FND-EXE-409 for the
callee; preserve current-segment and writable-alias gaps. Licensed bytes remain
outside Git. No game, DOSBox or emulated call runs.
