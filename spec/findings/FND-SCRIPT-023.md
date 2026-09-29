---
id: FND-SCRIPT-023
title: The interpreter error entry calls a shared helper before an optional message and stop assignment
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5702:00B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:00EE
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared FBOV/MZ relocation mapping
environment: null
---

## Observation

Descriptor 188's resident entry 5702:00B1 selects code
1901, file offset `0x000747A1`. The complete local body
ends with far return at `0x000747CD`, so its half-open
file span is `0x000747A1..0x000747CE`. Declared FBOV
fixups resolve its three far callees and final field
segment before any semantic assignment.

It first calls 576C:0039. FND-CONFIG-161 reads that
helper's local branches, pointer-field clears, external
calls and final polling loop. Only after that call returns
does the error entry test byte current DS:143C. Nonzero
passes current DS:19A0 as a far text pointer to
566A:002A; zero skips the message. FND-CONFIG-056 records
this diagnostic site and FND-CONFIG-018 bounds the shared
message's acquisition and wait gates. This new reading
does not re-read or copy the diagnostic writing.

Both message paths then call resident 172C:00EE. Its
complete body `0x0000C5AE..0x0000C5BE` writes one to
byte 4C13:0326 and returns far at `0x0000C5BD`, with
no other call, interrupt or field write. The state segment
is resolved by its declared MZ relocation. This confirms
the stop-byte setter already read in FND-SCRIPT-005.

When that call returns, the error entry writes zero to
byte 4F49:000A and returns far. FND-CONFIG-135 reads
that iterator-flag clear independently. There is no own
cache-bound, identity, age or buffer rollback in the error
entry. Its first helper and optional message retain their
separate effects; the stop write does not undo earlier
loader or replacement writes.

The entry does not normalize AX to a success/failure
result. Its known loader and allocator callers retain
their own local failure encoding (FND-SCRIPT-019,
FND-SCRIPT-022). Under ordinary unchanged state, a
returning error call has set the stop byte before the
allocator's caller checks it. This does not prove that
every earlier helper or message call returns.

## Interpretation

The error entry has a concrete conditional returning path:
shared helper, optional message, stop setter, iterator
clear and far return. It is neither a proven terminating
fatal exit nor a transactional failed-load path. The
shared helper's polling outcome and all intervening
callee effects precede the stop assignment.

## Alternatives

Q-SCRIPT-003 and Q-CONFIG-008 retain full shared-helper
and message effects, actual pointer/gate inputs, DS
preservation and successful returns. One reading reaches
the stop setter and caller continuation; another fails,
changes state or does not return in an earlier dependency.
FND-CONFIG-161 separates those dependencies from the
error entry's own stores. The existence of its final far
return alone does not decide the native outcome.

Q-SCRIPT-007 can check resident callers' local branches
only once the harness exists. The overlay error entry and
its external calls remain outside that harness; substituting
a return value would not establish their effects.

## How to reproduce

Resolve the descriptor 188 trampoline at 00B1 to code
1901 and read through 192D. Map the declared fixups to
576C, 566A, 172C and 4F49. Follow the first call return,
DS-relative gate, optional text pointer and both common
continuations. Independently read resident 00EE through
its far return and resolve the MZ state-segment operand.
Compare FND-CONFIG-056, FND-CONFIG-135 and
FND-CONFIG-161, keeping caller encodings and external
return conditions separate from this local body.
