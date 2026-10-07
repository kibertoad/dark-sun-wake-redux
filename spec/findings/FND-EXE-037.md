---
id: FND-EXE-037
title: Capacity-limit object construction publishes a payload field before replacing its first word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FE730..0x005FE751
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FE630..0x005FE69D
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-036 passes its saved allocated pointer and local payload-word address
to `0x005FE730`. This wrapper reads its first two original arguments at
32-bit width, saves the first pointer, and calls `0x005FE630` with those
two values. After normal return it writes `0x00759D30` as the first 32-bit
word through the saved first pointer, then restores its frame and returns.
It has no direct null guard, additional conditional branch or explicit return
value calculation. The meanings and contents behind the stored word remain
unread; it is not identified as a dispatch table here.

The nested helper saves its first original 32-bit argument before calling
`0x006008F0` with a local record. That record stores `0x005FE6A0` as a
handler target, whose body is outside this reading. After normal setup it
reloads the saved destination pointer and reads its second original 32-bit
argument. It writes `0x00759CA8` at destination offset zero, forms destination
plus four at 32-bit width, sets its local record state to one, and calls
`0x006D6C70` with that adjusted destination and the second argument.

FND-EXE-032 reads the latter helper's signed preceding-word branches and
payload publication. FND-EXE-033 through FND-EXE-035 narrow its addition
and copying/storage boundaries. Thus, conditional on valid storage and normal
completion, the second argument is a source-field address, and the object
field at offset four receives either the saved original payload pointer or
the negative branch's copied-payload result. The first object word is already
written before this field helper can fail; neither constructor has a local
rollback on that boundary.

After normal field-helper completion, the nested helper calls `0x00600990`
with its local record, restores its frame and returns. Only then does the
outer wrapper replace offset zero with `0x00759D30`. Both first-word writes
are full 32-bit stores; the direct constructors write no byte beyond the
first word themselves, and delegate the offset-four field to the cited helper.
This is not a bound on indirect record/helper accesses or exceptional writes.

## Interpretation

The ordinary path initializes two words within FND-EXE-036's eight-byte
object request, with an intermediate first-word value before payload
publication and a different final first-word value after record cleanup.
The source local field remains available for FND-EXE-036's later reread and
decrement path; these constructors do not directly clear it. Construction
is not proven transactional, owned or exception-safe. Q-EXE-009 retains
handler behavior, temporary production, aliases, stored-word consumers and
failure-consumer contracts. No complete object type or lifetime is established.

## Alternatives

Writing the final first word before field publication, always storing the
source-field address itself at offset four, or directly clearing that source
field in these constructors are ruled out by the bounded bodies and cited
field helper. Calling the two stored words dispatch-table pointers requires
their bytes and consumers, not their values or decompiler labels. Unread
handlers may perform additional cleanup; the normal sequence alone does not
establish those effects.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x005FE730` and
`0x005FE630`; read 55 instructions from the former and 65 from the latter.
Restrict claims to the two cited bodies, excluding following functions and
the separately stored handler. Follow original argument reads, destination
preservation, local record setup, first-word stores, destination-plus-four
calculation, outgoing source-field address, field-helper call, record cleanup
and wrapper overwrite after normal return. Use FND-EXE-036 for the outgoing
writers and FND-EXE-032 through FND-EXE-035 for payload publication boundaries.
Keep unread record/helper effects and aliases conditional. Keep rich reports
local and execute no interpreter or game.
