---
id: FND-EXE-412
title: Linked-record extension publishes accounting before reconnecting a newly allocated node
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:1198..1425:12F0
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-408's remaining literal link writer at 1296 belongs to
this 344-byte body, decoded as 136 instructions. It saves BP, SI and DI
and allocates six local bytes. Incoming word SS:BP+6 becomes SI.
Current DS:3EEC plus SI times 000E supplies a head word; no local
bound admits the incoming selector. Index arithmetic uses word width.
Incoming SS:BP+8 is a writable doubleword quantity in the caller's
argument area, distinct from the six local bytes.

### Initial head

If the indexed head is zero, the body calls 08E4 with its current
DS far address and local far output pointers SS:BP-2 and SS:BP-4.
It removes twelve outgoing bytes and tests returned AX through full-word
DX. Nonzero returns that result. Zero adds 00004000 to the current
indexed DS:3EF0 doubleword and sets indexed DS:3EF8 to one.
These writes precede the quantity test, including when the quantity does
not require traversal. FND-EXE-408 describes the called writer's own
publication and failure prefixes.

If the head was nonzero, it instead calls 060B with that head word
and the same two local output pointers, removes ten outgoing bytes and
tests full returned AX. Nonzero returns it. After either successful
route it re-reads the current indexed head into DI.

### Extension and traversal

The loop enters only while the doubleword at SS:BP+8 compares signed
greater than 00004000. At entry it reads the current DS:39CE word
selected by local BP-2 times 0108 plus local BP-4 times four.
If that word is nonzero, it proceeds directly to following the link;
this body does not reject link one, enforce a link range or detect cycles.

A zero link sets local BP-6 to zero, then calls 08E4 with a far
pointer to that local head and the same two output pointers. Nonzero
returned AX exits immediately. On zero it adds 00004000 to current
indexed DS:3EF0 and sets indexed DS:3EF8 to one before recovering
the prior node. It calls 060B with current DI and the local output
pointers. A failure returns after allocation and accounting writes, without
connecting the new node or undoing those earlier writes.

On recovery success it writes local BP-6 to the current selected
DS:39CE word at 1296 and sets DS:39C4 indexed by BP-2 times
0108 to one. It then re-reads the selected link into DI, calls 060B
with DI and the two local output pointers, and tests full returned AX.
A failure here likewise returns without rolling back the preceding link,
flag or accounting writes. Only success subtracts 00004000 from
the doubleword argument at SS:BP+8 and re-enters the signed test.
Ordinary completion returns AX zero and restores saved DI, SI and BP.

### Widths, ordering and admission

The doubleword additions and subtraction have wrapping 32-bit arithmetic;
no local overflow check accompanies them. With stable argument storage and
preserved caller state, an initial quantity no greater than signed 00004000
performs no traversal, whereas a larger positive signed quantity can require
multiple successful link steps. Values with bit 31 set do not enter merely
because their unsigned magnitude is large. This comparison is separate from
whether an initially empty head has already been allocated.

The body does not save or restore DS. SI and DI are saved for its final
return, but are not locally saved around the called helpers. Current DS,
SI, DI, local outputs and the quantity storage are consumed after calls;
callback preservation and writable aliases must therefore be admitted before
using the local arithmetic as an unconditional traversal or storage contract.
FND-EXE-409 describes 060B's early output, selection, transfer and tag
stores. Neither a zero result here nor its signed quantity test independently
establishes successful native effects or admitted record extents.

## Interpretation

This reads the remaining literal writer candidate named by FND-EXE-408.
Q-EXE-007 retains complete callers, computed and cross-region writers,
segment/selector/input provenance, stable link structure, admitted extents and
callback/native contracts. The listed local body is not a complete reading
of those dependencies. No original run, observed bug, atomic extension or
successful native transfer is claimed.

## Alternatives

Treating the quantity comparison as unsigned contradicts its signed branch.
Treating all small quantities as side-effect free ignores initial empty-head
allocation. Treating allocation and reconnection as atomic discards the
accounting writes and possible failed recovery between them. Treating each
nonzero link as a checked node adds absent local range and cycle tests.

## How to reproduce

At revision f35a5871 require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode file base
0x9450 plus 1198..12F0 in sixteen-bit mode. Require 344 bytes and
136 instructions. Follow both initial head routes, every 08E4 and 060B
argument/output binding, full-word result test, accounting store, link
publication and signed doubleword loop test in execution order. Use
FND-EXE-408 for candidate provenance and the allocation helper and
FND-EXE-409 for lookup dependencies. Preserve current-segment,
register, alias and native-contract gaps. Licensed bytes remain outside Git;
no game, DOSBox or emulated call runs.
