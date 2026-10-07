---
id: FND-EXE-024
title: Compiled allocation wrapper substitutes zero requests and retries through a callback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F7E10..0x005F7E8C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F7E90..0x005F7EAA
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F7EE0..0x005F7F0A
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F9910..0x005F9922
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601CF0..0x00601CF6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601D00..0x00601D06
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x0035953E..0x00359543
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x003595B6..0x003595BD
tool: Ghidra 12.1.3 PUBLIC and bounded physical PE import-table reading
environment: null
---

## Observation

FND-EXE-023 leaves the record-growth allocation and release boundaries
conditional. The thunk at `0x00601CF0` jumps through import slot
`0x024319D0`; the thunk at `0x00601D00` jumps through `0x024319A0`.
Physical PE import descriptors and lookup thunks identify these as malloc
and free from msvcrt.dll respectively. The cited shipped-file name ranges
include their terminating NUL. This establishes imported identities, not
the version or implementation of the CRT loaded by a host.

The allocation wrapper at `0x005F7E10` builds a local record and passes its
address to `0x006008F0` before reading its 32-bit request argument. If that
argument is zero, it overwrites the argument slot with one. A nonzero request
is passed unchanged to the malloc thunk; the direct body adds no header or
further size multiplication. On each attempt it saves the returned 32-bit
value in a local word, then tests that same value for zero.

A nonzero result enters a path that calls `0x00600990` with the local record
address. After that call returns, the wrapper reloads the saved allocation
result and returns it normally. This is conditional on those unread helpers
returning without changing the saved result; their record and exceptional
behavior are not established here.

A zero allocation result instead reads the full stored call target at
`0x0242BE90`. If nonzero it calls that target without an explicit new stack
argument. It does not test the callback's return value. After normal callback
completion it rereads the request argument and retries the same malloc call,
including saving and testing the new result. The direct retry loop has no
attempt bound and reloads the callback after each zero result. Its writers,
initialization, target and side effects remain unknown; no termination or
unchanged-request guarantee across the callback is claimed.

If the stored target is zero, the wrapper passes four to `0x005FCD70`, writes
`0x0075A470` into the first word reached through that returned value, and
passes that pointer with `0x00756050` and `0x005FDCC0` to `0x005FAED0`.
The first call's result is not locally tested before the store. The analyzer
ends this branch at the second call, marked without fall-through. These
callees and the local record's separately stored handler target remain
unread. This finding does not classify that branch as a throw, a process
exit, a returning null result or a particular cleanup path.

The release wrapper at `0x005F9910` reads its original full 32-bit pointer
argument. Zero returns directly. Nonzero removes its own frame and jumps
to the free thunk with the original argument still in place. There is no
local subtraction, header access or pointer transformation before that
transfer. CRT release effects remain outside this direct reading.

## Interpretation

The record-growth caller's computed request from FND-EXE-023 reaches a
wrapper that substitutes one for zero and otherwise passes the full value
to the named malloc import. It has a normal nonzero-result return path,
a callback retry path and a separate unresolved zero-result failure path.
Absence of a null check in the caller therefore does not by itself imply
that allocation failure returns null to it. This does not prove the host
allocator's units, bounds, metadata or failure behavior, nor a complete
exceptional reading. Object construction and caller record invariants remain
open under Q-EXE-009.

## Alternatives

Passing zero unchanged on the first normal attempt, adding a local header
to the request, using a callback result as the allocation result, imposing
a local retry-count limit, or transforming a released pointer are ruled out
by the bounded direct instructions. Whether the failure branch returns,
unwinds or terminates, and whether supported inputs reach it, require the
unread callees and handler state rather than an inferred C++ runtime name.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x005F7E10` and
`0x005F9910`; read 65 instructions from the former and 35 from the latter.
Restrict claims to the cited bodies, excluding following-function windows
and instructions past the no-fall-through failure call. Read three
instructions from each thunk but use only the first instruction of each.
Independently map the PE import directory and lookup thunks through bounded
physical sections to verify both slots and their names. Query references to
`0x0242BE90` with ReportReferences' fixed 200-reference cap as a lead, not
as proof of a complete writer set. Track the frame-based argument through
the zero substitution and retry, the saved allocation result across cleanup,
the unchanged release argument after frame removal, and the deferred stack
cleanup between the failure calls. Enumerate zero/nonzero request, result,
callback and release branches conditional on normal callees. Keep rich
reports local and execute no interpreter or game.
