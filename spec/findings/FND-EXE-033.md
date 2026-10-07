---
id: FND-EXE-033
title: Two preceding-word helpers differ in whether they return the value before addition
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5760..0x005F576F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5770..0x005F577E
tool: Ghidra 12.1.3 PUBLIC, pinned bounded instruction and function reporters
environment: null
---

## Observation

FND-EXE-032's nonnegative, non-fixed-address path passes payload minus
four and one to `0x005F5770`. This helper reads only its first two original
stack arguments, both at 32-bit width: an address and an addend. It performs
a LOCK-prefixed 32-bit memory addition through that address, restores its
frame and returns. Its direct body has no other call, branch, memory guard
or explicit status calculation. The caller discards its return before
publishing the saved payload pointer. Later outgoing words recorded in
FND-EXE-032 are not read by this direct body.

The neighboring helper at `0x005F5760` likewise reads a 32-bit address and
addend. It uses LOCK-prefixed 32-bit exchange-add: memory receives its
previous value plus the addend at that width, while the original memory
value becomes the normal return value. It restores its frame and returns
with that original value unchanged. Its direct body has no additional call,
branch, guard or explicit overflow handling. It does not return the updated
memory word. No hardware execution or concurrent observation is made here;
the prefix and instruction operations are static evidence.

## Interpretation

The first helper narrows the auxiliary boundary in FND-EXE-032: under
valid writable storage and normal execution, the supplied one adds to the
full word at payload minus four before the caller publishes its saved
pointer. This does not establish a reference count, ownership, allocation
header, lifetime or concurrency policy. The second helper's previous-value
return is a distinct contract that any later cleanup reading must trace
through its own caller tests. Q-EXE-009 remains open for those producers,
callers, cleanup paths and the negative field-helper path.

## Alternatives

Reading extra outgoing slots in the first helper, returning the updated
word from the exchange-add helper, changing only a byte, or imposing a
local positive-value or overflow guard are ruled out by the complete direct
bodies. A reference-count interpretation is consistent with some caller
shapes but remains unsupported until producers, decrements, release paths
and admitted values are read. LOCK prefixes alone do not establish the
collection's complete concurrent behavior.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x005F5770` and
`0x005F5760`. Read 25 instructions from the former and eight from the latter,
restricting claims to the cited bodies and excluding following functions.
Use FND-EXE-032's outgoing writers for the add-one call. Verify both original
argument widths and order, memory operand width, LOCK prefixes, addition
versus exchange-add, preserved previous-value return and frame restoration.
Distinguish zero, positive, negative and wrapping addends as arithmetic
operations, without claiming that callers admit every case. Keep rich reports
local and execute no interpreter or game.
