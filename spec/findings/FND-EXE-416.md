---
id: FND-EXE-416
title: Linked-node initialization publishes the count before repeated template requests
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0CDD..1425:0DE9
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

The 268-byte body at 0CDD decodes as 106 instructions. It saves
BP, SI and DI and allocates fourteen local bytes. Current DS:00D0
unsigned at least 0020 returns AX twelve before later calls or stores.
Otherwise local BP-4 and BP-6 both start at zero and SI starts
at one. The body begins by testing current DS:39CE selected by
BP-4 times 0108 plus BP-6 times four, using word arithmetic.

### Link traversal and allocation

A nonzero selected link is re-read and passed to 060B with far
output pointers SS:BP-4 and SS:BP-6. Ten outgoing bytes are
removed and full returned AX is tested through DX. Nonzero returns
that result. Zero increments SI; if unsigned SI is now above 001F,
AX twelve returns. Otherwise it repeats the selected-link test from
the new local coordinates. This routine adds no local range or cycle
test for each link; its counter bound depends on SI surviving the helper.

When the link is zero, it calls 08E4 with a far pointer to the
current selected DS:39CE word and the same local output pointers.
Twelve outgoing bytes are removed and full returned AX tested through
DX. Nonzero returns immediately. Zero writes SI plus one, at word
width, into current DS:00D0 before preparing or invoking the slot target.
FND-EXE-408 describes 08E4's allocation/publication effects and
FND-EXE-409 describes 060B's state and callback dependencies.

### Template requests

The body prepares eight local bytes: word one at SS:BP-0E,
word zero at BP-0C and doubleword zero at BP-0A. It reads
current selected DS:39CC into DI, stores that word's high four
bits in local BP-2 and retains its low twelve bits in DI.
SI becomes zero.

Each iteration pushes word eight, current SI, current DI, far pointer
SS:BP-0E and current DS:3F4E indexed by local BP-2 times
000E. It calls the current far pair at similarly indexed
DS:3F46/3F48, removes twelve outgoing bytes and tests full
returned AX through DX. Nonzero returns immediately without undoing
the allocation, count publication or previous target calls. Zero adds
eight to SI at word width and repeats while unsigned SI is below 4000.
Ordinary completion returns AX zero and restores saved DI, SI and BP.

With preserved SI and local/segment state, the request positions are
0000, 0008, through 3FF8, giving 2048 calls. This is a local
call-count bound under those conditions, not proof of 2048 successful
native writes, a particular destination extent or an eight-byte output
bound for each target invocation. The template's first eight bytes are
explicitly initialized, but its lifetime across calls and possible mutation
through aliases remain unadmitted.

### Preservation and admission

The body does not save or restore DS, and does not locally save SI
or DI around either nested helper or the slot target. These registers,
local coordinates, encoded word, current DS and local slot index are
re-used after calls. A bounded traversal and repeated-request count
therefore require their preservation and writable-alias contracts, not
only the initial input/count comparison. DS:00D0 is re-published from
the traversal counter, not incremented from the value that passed the
initial gate. The encoded-word producer and storage extent remain open.

## Interpretation

This supplies another record-zero link consumer and a template request
producer using the second slot pair. Q-EXE-007 retains complete callers,
record-zero/link/encoded-word and count writers, template/destination aliases,
slot target/argument producers, segment/register preservation and native
effects. The adjacent 0DE9 consumer remains a separate reading.
No complete reading, admitted allocation/storage, atomic initialization or
observed original bug is declared.

## Alternatives

Treating the initial count gate as admitting the linked structure ignores
the separate traversal and its helper dependencies. Treating DS:00D0
as an increment of its old value contradicts the SI-based store.
Treating initialization failure as restoring the previous count discards
publication before the callback loop. Treating a request word eight or
2048 nominal iterations as proof of native writes adds effects not read here.

## How to reproduce

At revision d5f167f6 require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode file base
0x9450 plus 0CDD..0DE9 in sixteen-bit mode. Require 268 bytes
and 106 instructions. Follow the unsigned count gate, zero coordinates,
link re-reads, helper outputs/results, counter rejection, allocation and
count publication, all eight local template bytes, encoded-word split,
slot argument order and SI loop. Use FND-EXE-408/409 for helper
dependencies and FND-EXE-415 for the distinct callback layouts.
Preserve writer, alias, preservation and native-effect gaps. Licensed bytes
remain outside Git; no game, DOSBox or emulated call runs.
