---
id: FND-EXE-046
title: Record-mode admission distinguishes direct clearing from initialization and a flag wait loop
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600860..0x006008E8
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-045 calls `0x00600860` when its shared record mode is negative.
This body saves the pointer at `0x0242F640`, reads the full word at
`0x0242C910` and forms saved base plus 52 at 32-bit width. If the shared
word is zero or that derived address is zero, it writes zero at saved base
offset 48 and returns normally. The derived-address check is not a check of
the base itself. No local base-null guard precedes the later accesses.

Otherwise it reads saved base offset 52. Nonzero reads offset 48 as signed
32-bit: negative joins the zero-mode write, nonnegative returns without that
write. Zero at offset 52 calls `0x00602570` with saved base plus 56. Its full
return is tested. The import identity, argument reads and effects are not
established here.

A zero return calls `0x00600800` and, after normal completion, stores one at
the saved base offset 52. It then rereads the shared base pointer and joins
the signed mode test at offset 48. The initializer's writes and failure effects
remain unread. This path's flag store precedes the shared-pointer reread,
so different saved and current bases remain possible until producer/alias
coverage establishes otherwise.

A nonzero return rereads offset 52 through the saved base. If now nonzero,
it rereads the shared base and joins the signed mode test. Otherwise it enters
a loop: call `0x006023B0` with zero, read the word through the saved base-plus-52
address, and repeat while that word is zero. Once nonzero it rereads the shared
base and joins the same mode test. The loop watches the saved address rather
than recomputing it from every shared-pointer read. No local iteration bound
or guaranteed nonzero writer is established by this body.

After the shared-base reread, a negative mode clears offset 48 of that reread
base; a nonnegative mode returns unchanged. Thus the pointer used for the final
mode decision can differ from the pointer used for initialization or waiting.
The direct clearing path before any external call uses the initially saved base.

## Interpretation

This narrows FND-EXE-045's negative-mode boundary to explicit guards, an
initialization branch, a flag wait and a fresh mode decision. It does not prove
termination, concurrency safety, thread-local semantics or external helper
behavior. Q-EXE-009 retains shared-word/pointer producers, initializer and import
contracts, flag writers and lifecycle. Timing and interleaving claims cannot be
established by this bounded static sequence alone.

## Alternatives

Testing the base rather than its adjusted address, writing the flag before
initialization, polling a freshly reloaded base every iteration, or clearing a
nonnegative mode unconditionally are ruled out by the bounded instructions.
Treating a zero-return path as no-op is also ruled out: it calls another helper
and publishes the flag. A lock or thread-local interpretation remains a lead
until the external boundaries and writers are read.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x00600860`; read
forty instructions from its entry and eighteen from `0x006008C0`, restricting
claims to the cited body and excluding following functions. Track the saved
base, derived-address guard, shared word, flag reads, full return test, initializer
call before flag publication, wait-loop argument and saved polling address,
shared-pointer reread and signed mode decision. Use FND-EXE-045 for caller
admission. Keep unread external effects and possible pointer changes conditional.
Keep rich reports local and execute no interpreter or game.
