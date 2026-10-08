---
id: FND-EXE-090
title: Handler helper retains two low-byte fallback gates after a direct stored-target call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F55E0..0x005F575F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD140..0x005FD14D
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD060..0x005FD079
tool: Ghidra 12.1.3 PUBLIC bounded instruction reading
environment: null
---

## Observation

FND-EXE-089's stored handler supplies a saved frame word to `0x005F55E0`.
FND-EXE-051 and FND-EXE-088 record other stored-handler callers without
establishing native frame admission. This helper sets up a record at frame
-124, with callback `0x005F50A0`, metadata `0x006EE834` and stored handler
`0x005F5655`, through `0x006008F0` (FND-EXE-167). It calls `0x005FABA0`
with its first input and then freshly reloads that stack argument, subtracts
48 at 32-bit width, and reads words from this prefix. Offset 32 is saved
at local -128, offset 24 at -132, offset 12 at -136, and offset 36 at -64.
No initial pointer/prefix validity check precedes these reads.

It supplies prefix offset eight to `0x005FD140` after writing two to state
-120. That direct helper calls the target held in its first stack argument,
without a null test or newly pushed semantic argument. Two reserved slots
contain incoming ECX, whose meaning is not established. If the indirect
call returns normally, its EAX is not tested before calling `0x005FD120`
(FND-EXE-041). FND-EXE-066 independently records this same direct wrapper.
This caller's callback target, its effects and native return remain
unresolved. This direct helper has no decoded ordinary return before the
following function at `0x005FD150`.

The physically stored handler `0x005F5655` adds twenty-four to incoming
EBP, copies adjusted-frame word -116 to local -140, and compares full state
-120 with one. A state other than one writes zero to state -120 and calls
`0x005FACB0` twice, each preceded by that zero store, before writing all
ones and forwarding saved local -140 to `0x00600EB0`. FND-EXE-070 and
FND-EXE-052 record the separate callee effects and transfer boundaries.
Exact state one, or the forwarder's unexpected normal return, instead
supplies saved local -140 to `0x005FABA0`, then calls `0x005FD220`.

After normal returns it reads the returned context's head word without a
local null guard, saves head at -144 and head plus 80 at -148, writes one
to state -120, and calls `0x005F4EA0` with EAX zero, EDX local -128 and
ECX addressing local -72. FND-EXE-060 records this reader's separate marker
and output contracts; admission of this saved input remains conditional. It then
calls `0x005F5040` with EAX addressing local -72, ECX saved head plus 80,
EDX freshly loaded through saved head, and one explicit stack word from
local -132. FND-EXE-214 records the matching helper's separate scan and
indirect-call contracts. Only low byte AL is tested on normal return. Nonzero calls
`0x005FAF40` (FND-EXE-087), rather than testing a full-word success value.

Zero AL selects a second call to `0x005F5040`, with EAX addressing the
same local -72, ECX zero, EDX `0x00755E18` and stack word from local -132,
again after writing one to state -120. The second low-byte result has a
different gate: zero proceeds to the final route; nonzero first requests
four through `0x005FCD70` (FND-EXE-025). The returned pointer is immediately
written with full word `0x00759D60`, without a local null check, and supplied
to `0x005FAED0` with metadata `0x00755E18` and callback `0x005FD060`.
Its allocation includes the separately proved eighty-byte augmentation;
four is not the full storage contract. An unexpected normal publication
return physically reaches the `0x005FAF40` call, whose unexpected normal
return would in turn reach the final route.

That final route writes one to state -120 and supplies saved local -136
to `0x005FD0C0` (FND-EXE-041's optional-callback/finalization boundary).
It does not use either selector's entire EAX as a final result. No decoded
ordinary return follows before `0x005F5760`. Native handler frame validity,
local initializer effects and selected target identity are not established.

The supplied callback `0x005FD060` independently reads its first input,
writes full word `0x00759D60` through it, calls `0x005FD000` with the same
pointer, and returns that helper's raw EAX. FND-EXE-089 proves that callee
overwrites the first word with `0x0075A498`. These are ordered full-word
writes, not a direct freeing operation; actual callback invocation and
pointer validity remain conditional.

## Interpretation

The helper makes two separate low-byte decisions, with different terminal
and construction routes, after direct stored-target dispatch and a handler
state test. Q-EXE-009 retains concrete target producers, initializer and
selector contracts, aliases, mode/context writers, frame admission and
native finalization. This bounded reading does not establish complete
exception behavior or the meaning of the local selector structure.

## Alternatives

Treating all nonzero full-word results as equivalent, reusing the first
selector's arguments for its second call, skipping construction after the
second nonzero AL, guarding the stored-target call against zero, or naming
`0x005FD060` as a direct free is ruled out locally. An all-ones upper word
with AL zero takes the zero route. A physically adjacent handler is not
proof that the ordinary callee reaches it on native execution.

## How to reproduce

Verify FND-EXE-011's executable identity and the stored-handler caller in
FND-EXE-089. Read fifty instructions at `005F55E0` and sixty-five at
`005F5695`, retaining only the cited helper/handler region before
`005F5760`. Read thirty-five at `005FD140`, retaining only through
`005FD14C`; read thirty-five at `005FD000` to include the separately bounded
`005FD060` callback through its return. Track each prefix field's saved
local, both state stores before the repeated cleanup calls, full-state
versus low-byte gates, explicit register inputs, reserved versus consumed
slots, allocation/store order and unexpected normal continuations. Do not
infer the unresolved selector paths' contracts from their call-site register values.
Keep reports local and do not invoke a stored target or run the original.
