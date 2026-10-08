---
id: FND-EXE-043
title: Shared-record initialization verifies an encoded allocation before publishing target-field pointers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006005B0..0x006007F3
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00602540..0x00602546
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00602550..0x00602556
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00358FCA..0x00358FD3
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00358F06..0x00358F0E
tool: Ghidra 12.1.3 PUBLIC and bounded physical PE import-table reading
environment: null
---

## Observation

The initializer at `0x006005B0` reads the word at `0x0242F640`. Nonzero
returns immediately without refreshing either related pointer. Zero continues
with that zero saved. It builds a local lookup buffer: thirty-two bytes each
65, followed by eight full words copied from `0x00754E50` through
`0x00754E6C` and a final two-byte copy from `0x00754E70`. The copied tail's
contents and terminator are not interpreted in this finding.

It calls `0x00602540` with that buffer and tests only the low sixteen return
bits. The thunk's slot `0x02431804` is FindAtomA from KERNEL32.dll by bounded
physical import reading. Nonzero masks the returned word to sixteen bits,
passes it in the return register to FND-EXE-042's reader, then saves the reader
result as the selected base and joins publication. There is no local result-null
check on the reader result. Import names include NUL in the cited file ranges.

A zero low-half result requests sixty bytes through the malloc thunk verified
in FND-EXE-024. A null allocation calls FND-EXE-041's abort thunk; continuation
past that call is not established. A non-null result is saved and used for a
CLD, fifteen-dword repeated store of the saved zero through ES. Where ES
addresses the same storage as later ordinary accesses, that initializes all
sixty bytes. Segment identity and external effects remain conditional.

The ordinary stores then initialize the following offsets, in the shown order:

| Offset | 32-bit value or source |
| --- | --- |
| 4 | `0x006020C0`, FND-EXE-041's verified abort thunk |
| 8 | `0x00600520`, contents and behavior unread here |
| 0 | 60 |
| 20 | word read at `0x0242C920` |
| 24 | word read at `0x0242C924` |
| 28 | word read at `0x0071B200` |
| 32 | word read at `0x0071B204` |
| 44 | word read at `0x0242C930` |
| 48 | all ones |
| 40 | zero |
| 56 | word read at `0x0071B20C` |
| 52 | word read at `0x0071B208` |

The body encodes the saved allocation pointer into thirty-two local bytes,
writing index 31 from its low bit through index zero from its high bit.
A set bit produces 65 and a clear bit 97. The mask doubles at 32-bit width;
its full initial value is one because the preceding repeated store leaves
its count zero before the low-byte assignment of one. It copies the same
thirty-four-byte tail after these encoded bytes and calls `0x00602550`.
That thunk's slot `0x024317D8` is AddAtomA from KERNEL32.dll, independently
mapped with malloc/free controls matching FND-EXE-024.

The low sixteen return bits are tested. Zero selects fallback. Nonzero is
masked and passed in the return register to FND-EXE-042's reader. Its full
returned pointer is compared with the saved allocation. Equality retains
the nonzero masked identifier and selects publication with that allocation.
Inequality selects fallback regardless of the reader's returned word.

Fallback passes the saved allocation unchanged to the verified free thunk,
then calls FindAtomA again with the original lookup buffer. Its low sixteen
bits are passed to the reader without another zero-result guard. The reader's
normal result becomes the selected base. Shared-pointer publication occurs
only after these calls complete normally: first selected base to
`0x0242F640`, then base plus four to `0x0242F630`, then base plus eight to
`0x0242F650`, each at 32-bit width. The body then restores its frame and
returns. There is no local selected-base-null check before publication.

## Interpretation

This supplies one direct producer for FND-EXE-041's target-storage pointer.
On the verified fresh-allocation path, its initial stored target is the abort
thunk. The existing-record and fallback paths instead use the decoded record
and do not rewrite its target field. Publication is later than registration,
verification and any fallback release; it is not a transaction over those
external operations. Q-EXE-009 retains copied-tail identity/termination,
shared-source producers, atom API effects, segment assumptions, callers,
replacement paths and lifetime before a complete initialization contract exists.

## Alternatives

Publishing before the verification comparison, testing the full atom return
instead of its low half, keeping a mismatching allocation, guarding the second
lookup's zero result locally, or resetting an existing record's target to the
abort thunk are ruled out by the bounded body. Inferring the atom API's complete
sharing or concurrency behavior from these calls alone is not justified.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Read one hundred instructions
from `0x006005B0`, fifty-five from `0x00600734`, two from `0x006007EE`, and
one each from `0x00602540` and `0x00602550`. Restrict claims to the cited
ranges and exclude later functions. Independently map both slots through
bounded PE descriptors and terminated lookup thunks with malloc/free controls.
Track the nonzero entry exit, both local buffers, allocation/null branch,
repeated-store count and segment, all field stores, mask initialization and
encoding loop, low-half guards, reader comparison, release before fallback
lookup, combined outgoing cleanup and publication order. Use FND-EXE-042 for
the register-input reader and the cited import findings. Retain unread tail,
external and lifetime effects as conditional. Keep rich reports local and
execute no interpreter or game.
