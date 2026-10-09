---
id: FND-EXE-352
title: Existing allocation-wrapper evidence connects an independently controlled overlay caller
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 444C:0008..444C:008E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: code
    offset: 0x000894E6..0x00089530
tool: executable-reader 2.5.0, Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-CONFIG-209 already reads wrapper 444C:0008 and its request
shape: two double-word inputs are multiplied modulo 2^32, one is
added at double-word width without overflow rejection, and that adjusted
value and double-word one are passed to 1000:18D0. The declared
MZ operand at shipped 0x000396FD resolves to 1000:18D0, connecting
that existing entry-based reading to FND-EXE-299's incoming candidate.
The preceding operand at 0x000396DD resolves to 1000:041A,
the multiply helper already read in FND-CONFIG-209. No replacement
interpretation of that wrapper is required.

FND-CONFIG-183 independently grounds overlay 200 entry 0386 in
shipped code starting at 0x000894E6. A bounded decode from this
entry reaches its allocation call at shipped 0x0008951A. After testing
byte DS:265C against one, the active path writes one to that byte,
pushes the incoming double word from SS:BP+06 and double-word one,
and makes the far call. The segment operand at 0x0008951D is an
FBOV fixup with descriptor 58 resolving to 444C:0008. It removes
eight argument bytes, stores returned DX:AX into current DS:2654/2652,
then tests the combined pair; zero branches to its failure path. The
gate publication precedes this call and result test.

A relocation-aware incoming query for shipped wrapper target
0x000396C8 returned 35 declared call-byte candidates: seven MZ
relocations and 28 FBOV fixups. At limit 100 it was not truncated
and had no unresolved candidates. The independent known MZ control
0x00040DA9 resolved to 1000:15A5. The independently entry-read
overlay control 0x0008951A resolved to 444C:0008. Thus both
reported relocation kinds have controls here, unlike the earlier queries
that retained a missing overlay control.

## Interpretation

This connects an existing wrapper reading and one grounded overlay
instruction path to the product-request body. For FND-CONFIG-183's
caller argument 000092E0, the wrapper supplies 000092E1 and one
to FND-EXE-299. Its wrapped product is therefore 000092E1. This
identifies one concrete request bit pattern; it does not establish the
allocation extent, forward fill direction or native initialization outcome.

The query excludes near calls, computed calls, unrelocated pointers and
instruction-boundary verification. Controls validate the two selected
reference kinds, not every returned candidate's path or native CS.
Q-EXE-010 retains the remaining callers' arguments and admission,
direction provenance, aliases, initialized headers, extents and lifetime.
Other areas' entries are dependency evidence only; no status changes,
complete-reading promotion or general caller-absence claim follow.

## Alternatives

Treating the allocator request as the unadjusted incoming argument ignores
the existing product-plus-one reading. Treating a nonzero returned pair
as initialized capacity ignores allocator and fill-state obligations.
Treating query controls as every caller's reachability ignores the unverified
paths and excluded transfers. Treating the outer gate as published only
after successful allocation ignores its earlier store.

## How to reproduce

At revision 9265f7d require FND-EXE-176's installed DSUN.EXE identity,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Resolve
site/targetOffset pairs 0x000396DD/041A and 0x000396FD/18D0
with the committed operand reporter, loadSegment 1000. Decode shipped
half-open 0x000396C8..0x0003974E at IP 0008, modeled CS 444C,
and 0x000894E6..0x00089530 at overlay IP 0386, using locked
Capstone 5.0.7 in sixteen-bit mode. Use FND-CONFIG-183's independent
entry and FND-CONFIG-209's wrapper reading. Run the incoming reporter
with target 0x000396C8, limit 100, loadSegment 1000 and controls
[0x00040DA9, 0x0008951A]. Keep original bytes and reports outside
Git. No original execution or complete caller search is claimed.
