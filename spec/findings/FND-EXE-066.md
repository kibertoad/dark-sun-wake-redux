---
id: FND-EXE-066
title: Second terminal wrapper calls a shared-field target whose initial helper tail-jumps through the current finalizer field
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD140..0x005FD14C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD150..0x005FD162
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600520..0x0060052D
tool: Ghidra 12.1.3 PUBLIC, bounded instruction reading
environment: null
---

## Observation

FND-EXE-065's alternate suffix with a negative saved state reaches the
call to `0x005FD150`. This helper creates its conventional frame, reserves
twenty stack bytes, loads the full pointer at `0x0242F650`, and reads the
full target word through that pointer. It pushes that target as one full-word
argument to `0x005FD140`. There is no local pointer or target-null guard,
status test, cleanup or established return beyond this call. The following
mapped alignment instructions do not establish a normal continuation.

The receiving wrapper creates its frame and reserves eight bytes with two
register pushes. It reads the target from its first original stack argument
and calls it indirectly. Those two pushes precede the call but their values
are not proved consumed parameters of the unresolved target. The wrapper
supplies no separately populated outgoing parameter or locally prepared
object. If that target returns normally, the next instruction calls
FND-EXE-041's finalizer at `0x005FD120`, without testing the target's return
register, flags or a status local. There is no local return path or null guard
before this finalizer boundary. External and exceptional effects remain
conditional; analyzer no-return labels are not proof of target behavior.

FND-EXE-043 records one producer for these shared values: after selecting a
record it publishes the record base to `0x0242F640`, record-plus-four to
`0x0242F630`, and record-plus-eight to `0x0242F650`. On its verified fresh
allocation path the word at record offset eight is initialized to
`0x00600520`. Existing-record and fallback selection do not rewrite that
field, so the new target body is not an unconditional target claim.

The body at that initial target reads the current full pointer at
`0x0242F640` into the accumulator register before its frame prologue. It
then establishes and immediately restores its frame, reads a full target
word at that saved pointer plus four into the count register, and jumps
indirectly to it. It reads no original stack-argument slot, makes no call,
checks no null pointer, and supplies no status or intervening cleanup.
This is a tail jump: after restoring the frame it leaves the wrapper's
original return address as the target's return address, rather than creating
another one. Its retained accumulator holds the record pointer at dispatch;
what the target consumes remains conditional.

With valid, unchanged publication from the fresh allocation, this word is
FND-EXE-043's offset-four abort thunk, whose import slot is established by
FND-EXE-041. This establishes a conditional direct route to that import, not
an execution of the import or its library. The target read is from the
current shared base, not automatically from the record whose offset-eight
word the outer helper previously read. Mutation between these reads, aliasing,
existing-record selection and lifetime can make the bases or targets differ.
FND-EXE-041's replacement routine supplies a concrete mutation path for the
other target field; neither field is assumed immutable.

If the initial helper's chosen target returns normally, it returns to the
wrapper's next finalizer call. That finalizer freshly reads its separate
shared target-storage pointer, calls through FND-EXE-041's nested helper,
and reaches its abort import after normal callback return. Thus the wrapper
has a normal-return fallback dispatch even when its first indirect target is
replaced; it does not equate any returned zero/nonzero value with successful
recovery. Whether a target instead transfers control or raises an exception
requires its concrete contract and handler admission.

## Interpretation

The callback's second terminal boundary now has two complete local pointer
loads, its receiving wrapper's normal-return continuation, and the recorded
fresh target's tail-dispatch contract. Q-EXE-009 retains target replacements,
shared-record identity/lifetime, existing-record field provenance, concrete
callee effects, callback stored-handler frame mapping, and classification-one
helper contracts. No universal termination, exception lifecycle or full shell
outcome is established or implemented.

## Alternatives

Calling the shared storage address directly, treating the initial helper as
a constant result, selecting the finalizer according to a returned status,
guarding zero locally, or adding a new return address on the initial helper's
jump is ruled out. An initialization store does not prove later field
immutability or that the outer slot and inner shared-base read select one
record. A mapped gap or no-return annotation does not prove how an unresolved
callee exits.

## How to reproduce

Use FND-EXE-011's length and XXH3-verified PE and image base. Read thirty-five
instructions from `0x005FD120` and eight from `0x00600520`, restricting new
claims to the three cited bodies and excluding alignment and later entries.
Use FND-EXE-041 for finalizer dispatch, replacement and exact abort import,
FND-EXE-043 for allocation stores and publication, and FND-EXE-065 for the
callback's negative-state caller. Track both outer pointer words, the full
stack target, frame and outgoing reservation, untested normal-return call,
current-base reload, restored-frame tail jump and possible changed identities.
Keep targets, aliases, lifetime, library and exceptional effects conditional.
Keep rich reports local and execute no interpreter or game.
