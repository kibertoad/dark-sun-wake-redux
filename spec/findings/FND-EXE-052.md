---
id: FND-EXE-052
title: Handler forwarding publishes a selected record before restoring frame and stack for an indirect jump
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600EB0..0x00600FE4
tool: Ghidra 12.1.3 PUBLIC, bounded instruction and function reporters
environment: null
---

## Observation

FND-EXE-051's handlers call this forwarding body with a saved word in the
first outgoing slot. The body saves its first original 32-bit argument,
reads the shared pointer at `0x0242F640`, and selects an initial record.
A zero shared pointer calls FND-EXE-043's initializer and rereads the
pointer. A negative signed offset-48 mode calls FND-EXE-046's admission
helper and rereads the pointer. The later mode read is distinct from the
initial signed guard.

A later zero mode obtains the word at shared offset 40. A later nonzero
mode reads offset 44, saves GetLastError's return, calls TlsGetValue with
that index, saves its returned word, then calls SetLastError with the first
saved return. These exact imports are established by FND-EXE-047 and
FND-EXE-048. On normal return it uses the saved TlsGetValue result as the
initial record. It stores that initial record in two separate local words,
at frame offsets minus 16 and minus 20.

It reads the first argument's word at offset 12 without a local argument
null or range guard. A zero word selects `0x00600AD0`; a nonzero word
selects `0x00600CC0`. Before either call the return register contains the
saved original argument and the auxiliary register contains the address
of the minus-20 local. This records the prepared register values, not a
complete callee parameter contract. Both callees remain unread and may
change that local. After normal return the body compares the full returned
word with seven. Any other result calls `0x006020C0`, identified as the
abort import in FND-EXE-041; its exceptional or nonreturning effects are
not reconstructed here.

For a result of seven, it rereads the shared pointer and reads the selected
record from the minus-20 local. This selected record need not equal the
initial record without the selector's writer contract. A zero shared
pointer invokes initialization and rereads it. A negative signed mode
invokes admission and rereads it. It then reads the current mode again.
The selected record is retained across these calls.

A current zero mode writes that selected record to shared offset 40. A
current nonzero mode prepares four outgoing stack words: the current
shared offset-44 index, the selected record, and two auxiliary-register
words. It calls TlsSetValue and tests the full returned word. Nonzero
proceeds to control transfer. Zero calls GetLastError and, on normal
return, also proceeds to the same transfer. There is no local rollback
or alternate failure branch before that join. The GetLastError result
is replaced by the subsequent selected-record load and is not locally
tested or saved. Import success and concurrent effects remain conditional.

At the transfer join it rereads the minus-20 local as a record address.
It forms record plus 32 at 32-bit width, reads the jump target at record
offset 36, loads the frame register from record offset 32, loads the
stack register from record offset 40, then jumps through the saved target.
These are full 32-bit reads. The instruction is an indirect jump, not a
call, and does not push a new return address. The target was saved before
the frame and stack replacement. There is no local record-null, target-null
or bounds guard between this local load and those reads.

The supplied argument, initial record, selected local record and latest
shared base are distinct value sources. No local reading establishes their
identity, stability or lack of aliasing. In particular, the saved frame
word does not yet prove how the incoming frame in FND-EXE-051 relates to
its constructor: record-field writers and selector/dispatcher admission
remain necessary.

## Interpretation

This body supplies a selected-record publication and explicit saved-state
control transfer for the handler chain. It does not normally return through
an ordinary epilogue on the seven-result transfer path. It also does not
prove that the indirect destination is a particular stored handler, or that
publication and the restored frame are valid. Q-EXE-009 retains both
selectors, selected-record field writers, dispatcher admission, target
provenance, first-argument producers, shared lifetime and exceptional effects.
No complete exception lifecycle or runtime success is established.

## Alternatives

Always using the initial record, always using shared offset 40, accepting
any truthy selector result, or treating the transfer as an ordinary call
are ruled out by the local instructions. A failed TLS result does not
locally stop transfer: the zero branch queries the error and joins it.
Inferring four consumed import parameters from prepared words is unjustified.
The decompiler's indirect-call presentation does not override the final
jump or its explicit frame and stack replacement.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x00600EB0`,
read sixty-five instructions from that entry and fifty from `0x00600F83`,
and restrict claims to the cited range, excluding the following function.
Track both local record words, the first-argument offset-12 guard, prepared
selector registers, full-width seven test, later pointer/mode reads, retained
selected record, TLS zero/nonzero join and exact order of target/frame/stack
loads. Use FND-EXE-043 and FND-EXE-046 for local helpers and FND-EXE-041,
FND-EXE-047 and FND-EXE-048 for exact imported targets. Keep unread selectors,
record layouts and exceptional effects conditional. Keep rich reports local
and execute no interpreter or game.
