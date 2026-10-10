---
id: FND-EXE-431
title: Creation wrapper publishes two words using a reloaded offset and retained output segment
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:1720..1425:1769
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

The 73-byte body at 1720 decodes as 29 instructions. It saves
BP and allocates two local bytes. Current DS:00C4 zero returns
AX three without the later creation call or output stores.

Otherwise it calls 1049 with far local output pointer SS:BP-2,
removes four outgoing bytes and copies returned AX to DX. It
tests that full word; nonzero returns DX without its own caller-output
stores. FND-EXE-418 describes the creation helper's earlier dirty,
head, quantity, scalar and cursor effects. A failed wrapper result
does not independently imply those effects were absent or reversed.

### Ordered output stores

With DX zero, the body reads current DS:3EF4 indexed by
local BP-2 times 000E at word width. It loads ES:BX
from incoming far pointer SS:BP+6 and stores that identity word
through ES:BX. It then re-reads local BP-2, derives the
indexed current DS:3EEE word, and re-reads only the offset
word at SS:BP+6 into BX. It writes the scalar through
the retained ES at BX plus two. Final return copies DX to AX,
restores BP and returns far.

The segment word at SS:BP+8 is not reloaded for the second
store. The second table index and offset are reloaded after the
first store. If that first destination aliases the local selector or
incoming pointer storage, the later source or destination can differ
from a simple four-byte write through one immutable pointer. In
particular, a changed offset is consumed while ES still holds the
earlier segment. The body applies no local extent, wrap or alias
check to either destination.

With admitted stable selector, current DS and output pointer, the
two stores publish the selected identity followed by its scalar word.
That conditional layout does not establish immutable pointer storage,
admitted four-byte capacity, persistence or a broader record format.

### Preservation and admission

The body does not save or restore DS or ES. The creation helper
can reach native/time-dependent work as FND-EXE-418 records;
its zero result alone does not admit current DS, the local selector,
the caller's output pointer or their aliases. There are no further
calls between the two caller-output stores, and DX remains the
creation result throughout their local sequence. The source fields
are loaded separately, with the second load after the first store.

## Interpretation

This supplies one public creation wrapper for the chain read in
FND-EXE-418 and distinguishes its first-store pointer from the
second store's retained segment/reloaded offset. Q-EXE-007 retains
complete callers and pointer/input admission, selector/state producers,
current-segment preservation, destination extents and writable aliases,
and native/time-dependent contracts. No complete reading, admitted
output structure, atomic publication or observed original bug is declared.

## Alternatives

Treating both words as written through an immutable far pointer hides
the offset reload and retained ES. Treating both source fields as
captured before publication contradicts the second indexed load after
the first store. Treating a failed wrapper as side-effect free ignores
the creation helper. Treating the two local stores as an established
persisted four-byte schema adds lifetime and admission not read here.

## How to reproduce

At revision ff61b519 require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode file base
0x9450 plus 1720..1769 in sixteen-bit mode. Require 73 bytes
and 29 instructions. Follow the 00C4 gate, local creation-output
binding, full-word result, separate indexed source loads, first LES,
second offset-only reload, retained ES and DX return. Use
FND-EXE-418 for callee dependencies. Preserve caller, alias,
extent, state-producer and native-effect gaps. Licensed bytes remain
outside Git; no game, DOSBox or emulated call runs.
