---
id: FND-EXE-045
title: Record setup initializes missing shared storage before mode-dependent link publication
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-EXE-167]
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006008F0..0x0060097B
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

The bounded record-setup path at `0x006008F0`, called by FND-EXE-032 and
later field/object findings, reads the pointer at `0x0242F640` and saves
its first original 32-bit argument as a record address. If the shared pointer
is zero it calls FND-EXE-043's initializer, rereads the shared pointer, and
reads its word at offset 48 without a local null check. A nonzero initial
pointer reads offset 48 directly.

A negative signed 32-bit mode calls `0x00600860`, then rereads the shared
pointer and mode. The callee's effects remain unread. A nonnegative initial
mode reaches another mode read without that call. Thus the branch does not
freeze one mode value across the entire sequence.

When the current mode is zero, the body reads the shared word at offset 40,
writes it as the supplied record's first 32-bit word, then stores that record
address at shared offset 40. It restores its frame normally. These two writes
are ordered; no ownership, rollback or concurrency semantics are inferred.
FND-EXE-043's fresh-record path initially writes all ones at offset 48, so
its ordinary first setup reaches the unread negative-mode helper before it
can select the direct-link path.

For a current nonzero mode, the body reads shared offset 44, calls
`0x00602490` without a new explicit outgoing argument and saves its return.
It calls `0x00602580` with the saved offset-44 word, saving that return,
then calls `0x00602440` with the first saved return. After normal completion
it writes the second saved return as the supplied record's first word.
It rereads the shared pointer and offset 44, calls `0x00602590` with that
fresh word and the supplied record address in the first two outgoing slots,
then tests the full returned word. A nonzero result branches to the earlier
normal frame-restoration sequence; the zero path's later continuation is not
established in this bounded finding. No field write is reversed before the test.
The external helper identities, input reads and effects remain unverified here.

## Interpretation

This supplies a direct lazy-initialization caller and a bounded record-link
sequence for the construction findings. It does not establish record machinery
as complete, nor prove what the mode helper or external path does. Q-EXE-009
retains mode producers, external mappings, remaining continuation, cleanup,
callers and lifetime. Fresh initialization alone does not make the subsequent
negative-mode call disappear.

## Alternatives

Always using shared offset 40, skipping initialization on a zero pointer,
using one cached mode through the negative helper, or linking before the external
calls on the nonzero path are ruled out by the bounded instructions. Assigning
thread-local or exception semantics from the call shape alone is not justified.
The indirect-target initializer is not merely an assumed startup event: this
record-setup body calls it explicitly when shared storage is missing.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Read forty-five instructions
from `0x006008F0` and twelve from `0x00600965`, restricting claims to the
cited range and excluding later continuation. Follow the original record
argument, both shared-pointer and mode reads, initializer call, negative-mode
callee, zero-mode write order, each nonzero-path saved return, later pointer
reread, outgoing slots and full-width result test. Use FND-EXE-043 for fresh
initialization values. Keep unread helpers and zero-result continuation
conditional. Keep rich reports local and execute no interpreter or game.
