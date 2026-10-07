---
id: FND-EXE-047
title: Mode resource initialization publishes a saved index and a zero-helper-derived mode
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600800..0x00600856
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00602630..0x00602636
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00602560..0x00602565
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00602490..0x00602495
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x0035926E..0x00359276
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x003590AA..0x003590B6
tool: Ghidra 12.1.3 PUBLIC and bounded physical PE import-table reading
environment: null
---

## Observation

FND-EXE-046 calls `0x00600800` before publishing its flag. This direct
body saves the pointer read at `0x0242F640`, calls `0x00602560` and compares
the full returned word with all ones. The thunk's PE slot `0x02431898`
is TlsAlloc from KERNEL32.dll by bounded import reading.

For any result other than all ones, it writes that result at saved base
offset 44. It then calls `0x00602630` with the reread offset-44 word, zero,
and two register words in four outgoing slots. That local callee's entire
direct body clears its return register to zero, restores its frame and
returns; it reads no original argument and performs no additional call or
memory access. The caller tests the returned word for zero, constructs
exactly zero or one at 32-bit width, and stores it at saved base offset 48.
Under this callee's normal direct behavior that stored mode is one. It
then restores its frame and returns. No imported operation is hidden inside
that particular local helper's direct body.

For an all-ones result, it does not locally overwrite offset 44. Instead it
calls `0x00602490`, tests that full return for zero, constructs zero or one
and stores it at saved base offset 48. The thunk's PE slot `0x02431838`
is GetLastError from KERNEL32.dll by bounded import reading. A zero return
there stores mode one; any nonzero return stores mode zero. It returns after
normal frame restoration. Imported return values and their external meanings
are not inferred from this local branch alone.

Both mode stores use the initially saved base rather than a new shared-pointer
read. There is no local base-null check, previous-index release, rollback or
explicit status calculation beyond the mode stores. FND-EXE-046 subsequently
writes its completion flag and rereads the shared pointer, so helper publication
and the caller's later mode selection remain distinct events.

## Interpretation

This narrows the initialization dependency to a verified imported call,
branch-specific offset-44 publication and two sources for the mode result.
The ordinary non-all-ones branch has a known constant-zero local helper;
its outgoing values are not read by that body. Q-EXE-009 retains imported
execution effects, shared storage provenance, index lifetime, callers and
interleaving. No runtime success, thread-local lifecycle or complete failure
contract is established by these instructions alone.

## Alternatives

Always overwriting offset 44, reading the outgoing index inside the zero helper,
storing an imported return directly as the mode, or rereading the shared base
before each store are ruled out by the bounded bodies. Treating the all-ones
branch as necessarily mode zero is also ruled out: its mode depends on the
following full return test. A semantic label for the unused helper arguments
is not justified by their presence in outgoing slots.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x00600800` and
`0x00602630`. Read thirty instructions from the initializer, one from
`0x00600856`, twenty-five from `0x00602630`, and one each from
`0x00602560` and `0x00602490`. Restrict claims to the cited bodies, excluding
later functions. Independently map both import slots through bounded PE
descriptors and terminated lookup thunks, with malloc/free controls matching
FND-EXE-024. Follow the saved base, all-ones guard, branch-specific index store,
helper argument consumption, full return tests, boolean width and mode stores.
Keep imported and concurrent effects conditional. Keep rich reports local and
execute no interpreter or game.
