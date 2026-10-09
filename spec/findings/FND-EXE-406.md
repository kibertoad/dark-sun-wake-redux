---
id: FND-EXE-406
title: Registration manager validates a terminated record sequence before publishing a gate and consuming mutable quantities
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:13FE..1425:167A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0000AACA..0x0000AAD4
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-403 supplies this manager an incoming word at SS:BP+6
and a record far pointer at +8. The manager saves BP, SI
and DI and allocates six local bytes. Current DS:00C4 nonzero or
incoming word zero returns AX one before record processing.

Otherwise it walks record type words at offsets SI times 0044,
starting SI zero. Types above four select a local rejection marker;
types one through four increment SI. Type zero sets marker 0012
when encountered first or 0011 after a nonzero record. The unsigned
SI-below-sixteen test controls continued scanning. Sixteen nonzero records or
an invalid type returns AX two; a first zero returns one. A
zero after one through fifteen nonzero records permits initialization.

The selector is bounded unsigned at most four, doubled and used through
CS words at 167A. The five shipped offsets are 1439, 1436,
1436, 1436 and 1436. Thus zero reaches the terminator branch
and one through four share the increment branch. These reachable branches
occupy 1436..1447, omitted from the inventory's split body ranges.
The local jump at 1520..1523 is omitted there too, but this local
reading establishes no entry-path edge to that jump. A contiguous instruction
reading through 1679 contains 636 bytes and 233 instructions; that span
count is not a reached-byte count.
The inventory's additional cross-segment ranges do not establish their connection
to this body's control flow; no inventory repair is claimed here.

The admitted initialization writes DS:00C4 one before calling any slot
producer. It clears sixteen current DS argument words indexed from 3F4E
with stride fourteen, then zeroes 39CA, 3AD2 and 00C6.
It starts record index SI, local slot-index word BP-2 and DI at zero.

Each producer pass snapshots BP-2 into BP-4 and the incoming quantity
word into BP-6. It increments SI while forming the current record
far pointer and passes that pointer, SS:BP-2 and SS:BP+6 to
1425:04FF. Thus the dispatcher's record pointer is at its BP+6,
slot-index far pointer at +0A and quantity far pointer at +0E.
FND-EXE-391/396/398 read the corresponding producer paths. The manager
removes twelve outgoing bytes and keeps returned AX in DI.

If the current slot index equals the previous index plus one, it passes
the consumed quantity difference, previous index and current DS:00C6 far
pointer to local far 076B. That return is not tested. Every loop
test reloads the next record's type and incoming quantity; it continues only
while type is nonzero, quantity is positive unsigned and DI is zero.
Producer effects can change those fields, SI, DS and the records. The
earlier validation does not admit immutable record types or a preserved index.

A nonzero producer result or exhausted records with remaining quantity calls
cleanup 1684. A zero DI on that latter route becomes four; otherwise
DI is retained. The shared failure exit copies current DI into AX.
FND-EXE-559 reads cleanup's mutable targets and gate clear. This is not
a guarantee that the initialization or earlier producer effects are undone.

On quantity exhaustion with DI zero, the manager has further state setup.
When DS:39CA is zero it copies 3ACE/3AD0 to 39C6/39C8,
sets 39C4 zero and 39CA one, and copies 64 pairs of words
from indexed 3AD4/3AD6 to 39CC/39CE. Otherwise it calls 0591
with one and tests the full AX through DI. It derives words
00C8/00CA/00CC from 00C6 shifted right twelve and increments,
zeroes 00CE, calls 0591 with zero and tests that result too.
Nonzero on either helper path calls cleanup and returns current DI.

The continuing path initializes fields of records indexed one through four
with stride 0108, conditionally invalidates 3ACE/3ACC after comparing
the pair with 39C6/39C8, and initializes six fourteen-byte records
at 3EEC..3EF8. These are local writes, not admitted storage extents.

It then calls the far pair at current DS:3F46/3F48 with the
current word 3F4E, current DS:3EEC far pointer, word zero,
offset SI shifted left three and word eight. Returning AX goes into
DI. Nonzero calls cleanup and returns current DI. Zero increments SI
and repeats while unsigned SI is below 0800; no local SI or
DS save surrounds the indirect call. Under preserved SI, the nominal
indices are zero through 2047. The word eight is an outgoing input,
not a bound on native effects or proof of eight-byte output. The
success continuation sets DS:00D0 one and returns AX zero.

## Interpretation

This supplies actual manager input bindings and a writer of the cleanup
gate and slot argument initialization, connected to FND-EXE-403's caller.
It leaves current DS storage identity, callback preservation, aliases, complete
callers/writers, 076B and 0591 effects, and native contracts unresolved
under Q-EXE-007. Earlier validation alone does not establish the later
record sequence or unconditional loop bounds. No complete reading is declared.

## Alternatives

Accepting an empty record sequence contradicts its first-zero return. Accepting
sixteen nonzero records contradicts its bounded validation. Treating cleared slot
arguments as cleared targets invents stores. Treating cleanup as rollback ignores
untested mutable callbacks. Equating nominal callback iterations with an unconditional
output bound ignores preservation and native effects. Inventory ownership cannot
replace the dispatch table's explicit targets and bounds.

## How to reproduce

At revision 8b9a9585 require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode header 0x5200
plus relative segment 0x0425 times sixteen plus 0x13FE through
exclusive 0x167A in sixteen-bit mode, modeled segment 0x1425.
Require 636-byte coverage and 233 instructions. Read five little-endian words
at shipped-file 0xAACA and trace the unsigned selector guard, index
transformation, all targets and loop markers. Compare with the recorded
inventory split ranges without assigning unsupported cross-segment ownership.
Follow every gate, slot write, outgoing word, quantity reload, return test,
cleanup path and indirect call. Use FND-EXE-391/396/398/403/559
for connected producers and cleanup. Preserve unresolved call effects and
storage assumptions. Licensed bytes remain outside Git; no original execution occurs.
