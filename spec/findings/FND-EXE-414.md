---
id: FND-EXE-414
title: Six-record selection publishes identity and output before loading the selected head
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0FA3..1425:1049
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0C54..1425:0CDD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0C04..1425:0C54
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

The selector called by FND-EXE-413 at 0FA3 saves BP and SI,
allocates two local bytes and takes incoming word SS:BP+6 into SI.
It first visits six records at indices zero through five, using a word
stride of 000E. Each current DS:3EF6 age below unsigned FFFF
increments by one; FFFF remains unchanged. These stores precede
any identity match or later call.

It then searches those six records in increasing index order for
current DS:3EF4 equal to SI. The first match clears that record's
age, writes its index through incoming far pointer SS:BP+8 and
returns AX zero. This route does not test the corresponding head
at DS:3EEC, dirty word at DS:3EF8 or native effects.

On no match, it calls 0C54 with a far pointer to local SS:BP-2,
removes four outgoing bytes and tests full returned AX through DX.
Nonzero returns that AX without publishing the local selector through
the caller's output pointer. Zero uses the local selector to write SI
to current indexed DS:3EF4 and zero to DS:3EF6, then writes
the selector through the caller's far output pointer. It subsequently
calls 0C04 with SI and a far pointer to the current indexed
DS:3EEC head. Six outgoing bytes are removed and its AX becomes
the final result without undoing the identity, age or output writes.

### Selection helper

At 0C54, SI starts at FFFF. A six-record scan assigns SI
whenever current DS:3EEC equals one, so the last matching index
is selected. With no such record, SI starts at zero and indices
one through five compete by unsigned DS:3EF6 age. A strictly
larger age replaces SI; equal ages retain the earlier selection.

DX starts at zero before the selected DS:3EF8 dirty word is
tested. Zero skips the following call. Nonzero calls 0BB4 with
the selected identity word at DS:3EF4 and a far pointer to
the selected DS:3EEC head. It removes six outgoing bytes,
copies returned AX to DX and tests it whole. Zero clears the
selected current DS:3EF8 word; nonzero leaves it uncleared.
Both routes then write current SI through incoming far pointer
SS:BP+6 and return AX equal to DX. Thus this helper publishes
its output even on a nonzero result, though 0FA3 does not then
forward that local output to its own caller.

### Head-loading helper

At 0C04, BP and SI are saved and four local bytes allocated.
It calls 0B26 with incoming identity word SS:BP+6 and local
far output pointers SS:BP-2 and SS:BP-4, removes ten outgoing
bytes and tests full returned AX through DX. Nonzero returns
that AX without the subsequent indirect call.

Zero derives SI from the high four bits of local BP-2 and
keeps the low twelve bits in that local word. It pushes word eight,
the incoming doubleword far pointer SS:BP+8, local BP-4,
the low-twelve-bit local word and current DS:3F4E indexed by
SI times 000E. It calls the current far pair at similarly indexed
DS:3F42/3F44, removes twelve outgoing bytes and returns its
AX. The word eight is a request argument, not independent proof
of an eight-byte output bound or successful native transfer.

### Preservation and storage

All three bodies use current DS without locally restoring it. Saving
SI for the final return does not save it around nested calls or the
slot target. The selector's local index, identity SI and current DS
are re-used after calls. The six-record loops bound their own initial
scans; they do not admit native preservation, the returned selector
after intervening calls, destination aliases or the extents of any
indirectly reached storage. A hit's output and age stores can also
alias other state unless the caller's pointer is independently admitted.

## Interpretation

This supplies the local producer needed by FND-EXE-413 and its
selection/head-loading paths. Q-EXE-007 retains 0B26 and 0BB4,
identity/head/age/dirty writers and startup provenance, complete callers,
input/output aliases, current-segment/register preservation and slot-target
native contracts. No complete reading, admitted caller storage, observed
original bug or atomic selection/loading contract is declared.

## Alternatives

Selecting the first unused record contradicts the scan's repeated SI
assignment. Selecting the latest record on equal age contradicts the
unsigned comparison that preserves the earlier one. Treating a matching
identity as proof of an admitted head adds a test absent from that route.
Treating a failed load as leaving identity and output untouched erases
their earlier publication. Treating the request word eight as a proved
native output count goes beyond the target contract read here.

## How to reproduce

At revision f4afb7fd require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode file base
0x9450 plus each Locations span separately in sixteen-bit mode.
Require 166/137/80 bytes and 61/57/34 instructions. Follow all six
age and identity tests, last-unused selection, unsigned age ties,
dirty-call results, local versus caller output pointers, identity publication,
head-load argument widths and the indirect target's current-segment
provenance. Use FND-EXE-413 for the quantity caller. Preserve callee,
alias, writer and native-contract gaps. Licensed bytes remain outside Git;
no game, DOSBox or emulated call runs.
