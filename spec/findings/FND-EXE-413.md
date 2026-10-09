---
id: FND-EXE-413
title: Quantity adjustment dispatches unsigned comparisons to signed link helpers and publishes the caller quantity on success
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:17AE..1425:1879
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:12F0..1425:1383
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

The body at 17AE contains a direct local call to 1198, the extension
helper read in FND-EXE-412. Its 203 bytes decode as 74 instructions.
It saves BP and allocates two local bytes. Current DS:00C4 zero
returns AX three before later calls or stores. Otherwise it calls 0FA3
with incoming word SS:BP+6 and local far output pointer SS:BP-2,
removes six outgoing bytes and tests full returned AX through DX.
Nonzero returns that result; the producer and admission of the local
selector remain unproved here.

With a zero result, current DS:3EEE indexed by the local selector
times 000E must equal incoming word SS:BP+8, and similarly indexed
DS:3EEC must not equal one. Either failed test returns AX eleven.
The body then sets DX zero and tests incoming doubleword SS:BP+0A.

### Dispatch and publication

Quantity zero calls 09AD with a far pointer to current indexed
DS:3EEC, then tests returned AX through DX. FND-EXE-411 supplies
that release body's ordered effects. A nonzero quantity is compared
unsigned against current indexed DS:3EF0. A smaller current value
calls 1198; a larger current value calls 12F0; equality calls neither.
Each selected helper receives a pushed doubleword copy of SS:BP+0A
and a word copy of the local selector. Six outgoing bytes are removed
after either returns, before AX becomes DX for the result test.

Nonzero DX returns immediately. Zero publishes the re-read incoming
doubleword SS:BP+0A into current indexed DS:3EF0 and sets indexed
DS:3EF8 to one, then returns AX zero. This final value comes from
the caller's incoming slot, not the helper's reduced outgoing copy.
DS and the local selector are re-used after calls without independent
admission of their preservation, extents or writable aliases.

The unsigned dispatch differs from 1198's signed quantity test. An
unsigned-large request whose bit 31 is set can select extension while
that helper's signed loop performs no traversal. The caller can still
publish that request after a zero helper result. This is a conditional
local path, not proof that a real caller can supply such a request or
that preceding native effects preserve its state.

### Companion shrink body

The body at 12F0 has 147 bytes and 58 instructions. It saves BP
and allocates four local bytes. It calls 060B with current DS:3EEC
indexed by incoming word SS:BP+6 times 000E and far output pointers
SS:BP-2 and SS:BP-4. It removes ten outgoing bytes and tests full
returned AX through DX; nonzero returns it immediately.

While the incoming doubleword SS:BP+8 compares signed greater than
00004000, it reads the selected current DS:39CE link using local
BP-2 times 0108 plus BP-4 times four, calls 060B with that word
and the same output pointers, and tests full AX. Nonzero returns
before subtracting from the quantity. Zero subtracts 00004000 from
that argument slot with wrapping doubleword arithmetic and repeats.
The body adds no local link range, terminator or cycle check before
passing the link to the helper.

When the signed test ends, it sets current DS:39C4 indexed by local
BP-2 times 0108 to one. It then calls 09AD with a far pointer to
the current selected DS:39CE word, removes four outgoing bytes and
returns that full AX through DX. Thus a selected flag write can precede
a release failure. Small or negative signed quantities do not bypass the
initial lookup or this final flag/release sequence. FND-EXE-409 and
FND-EXE-411 describe those callees' effects and unresolved contracts.

## Interpretation

This supplies one extension caller and its companion shrink helper.
Q-EXE-007 retains 0FA3's selector production, broader callers and input
admission, segment/register preservation, computed/cross-region writers,
stable link structure, storage extents and native contracts. The listed
bodies do not establish a complete caller set, admitted oversized request,
observed original bug or atomic adjustment.

## Alternatives

Using one signedness for caller and helpers erases their different branches.
Treating the helper's reduced outgoing quantity as the final accounting value
contradicts the caller's re-read incoming slot. Treating equality as a no-op
ignores the final quantity and flag stores. Treating small shrink quantities
as side-effect free ignores the initial lookup and final release sequence.

## How to reproduce

At revision e567158d require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode file base
0x9450 plus each Locations span separately in sixteen-bit mode. Require
203/147 bytes and 74/58 instructions. Follow the 00C4 gate, 0FA3
output, identity/head tests, unsigned quantity dispatch, pushed argument
widths and cleanup, re-read incoming publication, and the shrink body's
signed loop and final flag/release ordering. Use FND-EXE-412,
FND-EXE-409 and FND-EXE-411 for callee dependencies. Do not infer
caller completeness from this direct call. Licensed bytes remain outside Git;
no game, DOSBox or emulated call runs.
