---
id: FND-EXE-049
title: Record cleanup restores a pre-helper saved link through a freshly selected mode
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600990..0x006009FC
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

The record-cleanup helper called by the field and object construction paths
in FND-EXE-032 and FND-EXE-037 reads its first original 32-bit argument as
an address and immediately saves that address's first word. This read precedes
the shared-pointer lookup and every helper call. There is no local null guard
on the supplied record. Later original arguments are not read in this body.

It reads the shared pointer at `0x0242F640`. A zero pointer calls
FND-EXE-043's initializer, then rereads the pointer and its word at offset 48
without a local null guard. A nonzero pointer reads offset 48 directly.
If that word is negative at signed 32-bit width, it calls FND-EXE-046's
mode-admission helper, rereads the shared pointer and reads offset 48 again.
A nonnegative initial mode also reaches a fresh offset-48 read. The initial
mode and the later mode are not one frozen value.

When the later mode is zero, it writes the saved record word to the selected
shared base at offset 40, restores its saved register and frame, and returns.
It does not locally reread the supplied record's first word or clear it.
It neither compares the current offset-40 word with the supplied record
address nor validates any link chain before this replacement.

When the later mode is nonzero, it prepares four outgoing stack words:
the selected base-offset-44 value, the saved record word, and two copies
of the current auxiliary register. The first two are passed to the thunk
`0x00602590`, identified as TlsSetValue by FND-EXE-048. The body tests the
full returned word after the call. A nonzero result joins the ordinary
saved-register/frame restoration and return. A zero result restores the
saved register and frame, then jumps to `0x00602490`, identified as
GetLastError by FND-EXE-047. The tail jump pushes no new return address:
on normal external return the later query returns to this helper's caller.
There is no local GetLastError/SetLastError save-and-restore pair before
TlsSetValue, and no second link store after its result test.

The external path uses the record word captured before initialization or
mode admission, alongside the index obtained from the later selected base.
A helper changing the record or shared pointer would therefore not imply
that both inputs came from one contemporaneous record. The external calls'
stack consumption and exceptional effects remain external contracts; the
frame restoration, rather than a decompiler's inferred parameter count,
bounds the local return paths.

## Interpretation

This describes the cleanup body's mode selection, saved-link provenance,
local replacement and zero-result tail return. It does not establish that
callers supply a current record, that initialization preserves the record,
or that the external operation succeeds. Q-EXE-009 retains caller admission,
return consumption, record lifetime, aliases, concurrent writers and
exceptional contracts. No complete exception or thread-local lifecycle is
claimed, and no game behavior is implemented.

## Alternatives

Reading the link after helper completion, always using shared offset 40,
using one cached mode, or requiring the current head to match the record
before replacement are ruled out by this body. Clearing the record on
cleanup is also unsupported: the local zero-mode write targets shared storage.
A plain return of the tested zero on the external zero-result path is ruled
out by the final tail jump. Inferring four consumed import parameters from
four prepared stack words is not justified by caller pushes alone.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x00600990` and
read sixty-five instructions there, restricting claims to the cited range
and excluding the later functions the window also prints. Track the first
argument's word before all helpers, the zero-pointer branch, signed initial
mode test, fresh mode and shared-pointer reads, zero-mode store, four
outgoing slots, full-width return test and both frame-restoration paths.
Use FND-EXE-043, FND-EXE-046, FND-EXE-047 and FND-EXE-048 for the helper
and exact import identities. Do not treat the analyzer's decompiled import
argument count as an external ABI proof. Keep rich reports local and execute
no interpreter or game.
