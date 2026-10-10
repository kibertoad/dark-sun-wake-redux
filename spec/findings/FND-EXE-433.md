---
id: FND-EXE-433
title: Quantity query selects a record before admission checks and publishes one doubleword
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:18F6..1425:1954
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

The 94-byte body at 18F6 decodes as 36 instructions. It saves
BP and allocates two local bytes. Current DS:00C4 zero returns
AX three without later selection or output. Otherwise it calls
0FA3 with incoming word SS:BP+6 and far local output pointer
SS:BP-2, removes six outgoing bytes and tests full returned AX
through DX. Nonzero returns that AX directly.

On zero, current DS:3EEE indexed by local BP-2 times
000E must equal incoming word SS:BP+8. The similarly
indexed DS:3EEC head word must not equal one. A scalar
mismatch or head one returns AX eleven without the caller-output
store. FND-EXE-414 describes selection's preceding age, identity,
output and possible native effects; the later rejection does not
undo them.

### Quantity publication

After those checks, it re-reads the local selector and loads current
indexed DS:3EF0 as a doubleword into EAX. It loads ES:BX
from incoming far pointer SS:BP+0A and writes EAX through
that pointer as one doubleword store. It then clears AX, restores
BP and returns far. The success result is a zero word in AX,
not a separate established 32-bit return-value contract.

There is no nested quantity adjustment, arithmetic, snapshot output
or explicit quantity/dirty store after selection. This differs from
FND-EXE-432's addition wrapper, which computes a total and calls
the adjustment wrapper before publishing its saved previous quantity.
The query's lack of later adjustment does not make the whole call
side-effect free: selection has already run.

### Preservation and admission

The body does not save or restore DS or ES. Current DS, the
local selector, scalar input and output pointer are consumed after
selection; their preservation, aliases and extents remain unadmitted
by a zero selector result. The quantity is loaded after scalar/head
checks and before the far output pointer. No subsequent source-field
reload occurs between that pointer load and the doubleword store.
The body adds no local destination extent or alias check.

## Interpretation

This supplies another scalar/head-checking consumer of the selected
record quantity. Q-EXE-007 retains complete callers, input/output
admission, selector/scalar/head/quantity writers, current-segment
preservation, destination aliases/extents and native contracts. No
complete reading, side-effect-free query, admitted output capacity
or observed original bug is declared.

## Alternatives

Treating this as quantity addition invents arithmetic and a nested
adjustment absent from the body. Treating a scalar/head rejection
as leaving state unchanged ignores prior selection. Treating AX
zero as a doubleword return contract confuses result width with
the separately loaded and stored quantity. Treating one doubleword
store as proof of admitted destination capacity goes beyond the
pointer checks read here.

## How to reproduce

At revision c7536505 require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode file base
0x9450 plus 18F6..1954 in sixteen-bit mode. Require 94 bytes
and 36 instructions. Follow the 00C4 gate, selector argument/output,
full-word result, scalar/head tests, doubleword source load, far pointer
and store, and AX-only success result. Use FND-EXE-414/432
for dependencies and comparison. Preserve caller, alias, extent,
state-producer and native-effect gaps. Licensed bytes remain outside
Git; no game, DOSBox or emulated call runs.
