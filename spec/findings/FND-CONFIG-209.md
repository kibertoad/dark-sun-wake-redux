---
id: FND-CONFIG-209
title: The shared reader allocation wrapper requests a wrapped product plus one and marks runtime header extent
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 444C:0008
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:041A
tool: Python 3.14.7 and Capstone 5.0.7 bounded resident readings with explicit operand widths
environment: null
---

## Observation

FND-CONFIG-151's shared reader calls 444C:0008 only when the
far destination pointer it reads is zero. It supplies first double-word
argument one and second double-word argument the selected transfer
count. FND-CONFIG-208 reaches that reader with a zero local pointer.

The allocation wrapper occupies file 0x000396C8..0x00039751.
Its normal paths end at far returns 0083 and 008D; the trailing
stack-restoration return at 0091 is unreachable from those paths.
The wrapper loads its two double-word arguments into register pairs
and calls 1000:041A. That helper, file 0x0000561A..0x00005630,
computes their product modulo 2^32: the low-word product supplies
the low result and the cross products contribute to the high word.
It does not reject overflow. With the shared reader's first argument
one, the returned product equals its count bit pattern.

The wrapper adds one to that product using low-word addition and
high-word carry, with no overflow rejection. It calls 1000:18D0
with first double-word argument the wrapped product plus one and
second double-word argument one. It stores the returned DX:AX far
pointer. A zero combined pointer returns DX:AX zero without a header
access or marker write. The runtime allocator's own admission,
allocation and initialization contract is not established here.

For a nonzero pointer, it subtracts four from the offset only,
retaining the segment without a borrow adjustment. It reads a word
at that derived far address, shifts the word left by four with word
truncation, and adds that extent to the derived offset with word
wrap. It writes byte 77 hexadecimal at one byte before the resulting
address in the same segment. Thus the marker location depends on
a runtime header word, not directly on the caller's requested count.
It returns the originally stored allocator pointer unchanged.
There is no own proof that the header read or marker write is within
allocated storage. FND-CONFIG-165 records the corresponding release
guard's marker comparison and FND-CONFIG-184 its cleanup context.

## Interpretation

The reader's allocation branch requests count plus one through this
wrapper, subject to double-word wrap. This is a local request shape,
not evidence of available resource capacity or a successfully filled
FONT object. The wrapper's header-derived marker and unchanged return
connect acquisition to the already recorded release guard. Runtime
header layout and allocated extent remain necessary dependencies.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain the runtime allocation contract,
header provenance, admitted counts, offset-wrap conditions, DS/storage
aliases and resource lifetime. A valid runtime allocation can provide
the expected header and extent; a null allocation returns before those
accesses. A nonzero pointer alone does not distinguish valid capacity
from incompatible header or wrap conditions. Reading 1000:18D0 and
its input/header producers would settle that distinction. No native
or emulated result is claimed.

The reading that the marker always follows exactly the requested
payload is unsupported: its position is derived from the runtime
header word. The reading that this wrapper bounds allocation arithmetic
is ruled out by its unchecked product and increment operations.

## How to reproduce

Read 444C:0008 through both reachable far returns, retaining the
trailing unreachable epilogue separately. Track argument widths,
1000:041A's complete low and cross-product reading, the carry-bearing
increment, 1000:18D0 argument order, null branch, offset-only header
adjustment, word-sized extent arithmetic and unchanged pointer return.
Compare FND-CONFIG-151's destination test and allocation arguments,
FND-CONFIG-208's local pointer, and FND-CONFIG-165's release guard.
Do not infer runtime capacity from a nonzero pointer or marker write.
