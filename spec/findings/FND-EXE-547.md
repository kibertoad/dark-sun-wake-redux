---
id: FND-EXE-547
title: Game resident block producers handle alignment results differently before publishing segment headers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:14C4..1000:1582
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:14C4..1000:1582
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-531's initial-block path calls near 14C4 with transformed
quantity AX. It saves AX, loads DS from CS:1361 and calls
near 1831 with two zero words, removing four argument bytes. It
masks returned AX with 000F. Nonzero forms DX as 0010 minus
that nibble, reloads DS from CS:1361 and calls 1831 with push
order zero, DX. It removes four bytes and ignores that result. A
preliminary FFFF result therefore selects a one-unit padding request instead
of itself causing the helper's failure return.

It restores the original quantity into AX and holds it again. It forms
BX from zero-extended original AH shifted right by four and shifts AX
left by four at word width. Together BX:AX represent the original
word quantity multiplied by sixteen. It reloads DS from CS:1361,
pushes BX then AX into 1831, removes four argument bytes and
restores the held original quantity into BX. Only returned AX exactly
FFFF takes its failure path, which clears AX and sign-extends zero
into DX using word opcode 99 before returning near without cleanup.

Every other returned AX stores DX into both CS:135B and
CS:135D, replaces DS with DX, stores held original quantity BX
at that segment's word zero and DX at word two, then sets AX
to four and returns near. Zero and other AX values are not rejected
here. Preliminary request effects are not locally reversed on final failure;
header publication does not itself prove valid segment storage.

The adjacent helper 1528 holds incoming AX, forms the same multiply-
by-sixteen pair, reloads DS from CS:1361 and calls 1831 with
that pair. It removes four argument bytes and restores the original quantity
into BX. Returned AX FFFF clears AX/DX and returns near.
Otherwise it masks AX with 000F. Zero proceeds directly to publication.

Nonzero instead saves BX and returned DX, negates the nibble in
AX at word width and adds 0010, yielding 0010 minus that
nibble. It pushes zero then AX into 1831, without a local DS
reload before this extra request. It removes four bytes, restores the
earlier DX and BX, and tests this second returned AX for FFFF.
Failure returns zero AX/DX without reversing the first request. Otherwise
it increments the restored earlier DX at word width and continues; it
does not use the second request's returned DX for publication.

Publication reads current CS:135D into CX, replaces that shared word
with DX, loads DS from DX, stores held quantity BX at word zero
and old shared CX at word two, then sets AX to four and returns
near. The initial helper's word-two writer and this helper's writer have
different sources. The saved quantity and segment require intact frame and
callee preservation; no local extent, alias, ownership or wrap check admits
the destination storage.

Both helpers make no interrupts themselves, do not locally change SS,
and share identical instructions in both editions. They change DS during
requests/publication and rely on FND-EXE-531's shared-word restoration
after returning. The wrapper passes their AX/DX pair to the startup
caller in FND-EXE-529, whose zero-pair test and high-word arithmetic
consume it. These bodies do not establish 1831's units, effects or
native/backing-storage contract.

## Interpretation

This resolves the two remaining immediate block-producer bodies of the resident
request wrapper. It distinguishes preliminary result disposal from tested padding,
and identifies which returned segment and held quantity reach header stores.
Q-EXE-007 retains 1831 and its callees, shared code-word and record
producers, units, actual segments, extents, aliases and lifetime, other callers
and remaining startup/native dependencies. No complete allocator or launch
contract is claimed.

## Alternatives

Treating initial padding failure as immediately fatal ignores its discarded
result. Treating later padding as equally unchecked ignores its FFFF test.
Publishing the later padding's returned DX contradicts the saved-segment restore.
Calling failure transactional ignores earlier requests and absent rollback.
Treating both word-two stores as the same header meaning ignores their
distinct source values and unresolved producers.

## How to reproduce

At revision 7c9eb6d require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
66C4..6782 in sixteen-bit mode. Track held quantity words, high/low
request formation, each DS reload, outgoing pushes, cleanup, sentinel test,
saved DX restoration and ordered code-word/header stores. Use FND-EXE-531
for wrapper return handling and FND-EXE-529 for the startup pair consumer.
Keep 1831 and storage admission explicit. Licensed bytes remain outside
Git; no original process, DOSBox or emulated call runs.
