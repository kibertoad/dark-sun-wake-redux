---
id: FND-EXE-041
title: Failure finalization reads a mutable indirect target before reaching an abort import
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD120..0x005FD133
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD0C0..0x005FD105
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD170..0x005FD182
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006020C0..0x006020C6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00359482..0x00359487
tool: Ghidra 12.1.3 PUBLIC and bounded physical PE import-table reading
environment: null
---

## Observation

FND-EXE-025's failure consumer calls `0x005FD120`. This bounded body loads
the 32-bit pointer stored at `0x0242F630`, reads the word through that pointer,
and passes that word to `0x005FD0C0`. It performs no local null guard at either
read. The analyzer supplies no fall-through for the latter call; no normal
return or frame restoration beyond it is established here.

The nested callee sets up a local record through `0x006008F0`, storing
`0x005FD105` as its handler target. After normal setup it sets local state
one, restores its outgoing setup space and calls the target read from its
first original 32-bit argument. No local target-null check precedes the
indirect call, and it supplies no new explicit outgoing argument. If the target
returns normally, it calls `0x006020C0` next. Handler admission and target
behavior remain unread; the callback's return is not tested as a status.

That thunk jumps through PE import slot `0x0243194C`. Bounded physical import
descriptors and lookup thunks identify abort from msvcrt.dll at that slot;
the cited shipped name range includes its terminating NUL. Independent malloc
and free slots match FND-EXE-024 as controls. This establishes which import is
called, not the execution of that library or the indirect target on this build.

A separate bounded routine at `0x005FD170` loads the pointer at `0x0242F630`,
reads the word through it into the return register, and writes its first original
32-bit argument through the same loaded pointer. It returns with the old word
unchanged. There is no direct guard, allocation or additional call. This is a
replacement path for the storage the finalizer reads; its callers and initial
contents are not established. The analyzer associates this instruction range
with an earlier function, so the claim follows the explicit entry and return,
not the analyzer's ownership label.

## Interpretation

Under valid shared storage and normal setup, finalization dispatches through a
mutable stored target and reaches the abort import if that target returns.
It is not a locally conditional choice based on the target's returned status.
This narrows one boundary in FND-EXE-025 but does not establish the complete
failure route: Q-EXE-009 retains target initialization, replacements, exceptional
handlers, earlier consumer calls and library effects. No native run is implied.

## Alternatives

Treating the shared pointer itself as the call target, guarding zero locally,
or testing the callback's return before choosing the import are ruled out by
the bounded bodies. An immutable fixed callback is not justified in the presence
of the replacement routine. The analyzer's no-return annotation alone does
not prove what an unresolved target or handler does.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x005FD120` and
`0x005FD0C0`; read thirty instructions from the former and twenty-five from
the latter. Read twelve instructions from `0x005FD170` and one from
`0x006020C0`. Restrict claims to the cited ranges and exclude handler and
following-function instructions. Follow both shared-pointer reads, the explicit
argument into the indirect call, local record state, normal-return import call
and the replacement routine's old-word return. Independently map the import
slot through bounded raw sections, descriptors and terminated lookup thunks,
with malloc/free controls. Keep target provenance, handler and library effects
conditional. Keep rich reports local and execute no interpreter or game.
