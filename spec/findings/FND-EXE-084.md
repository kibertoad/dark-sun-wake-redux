---
id: FND-EXE-084
title: Two cleanup callbacks share an old-value decrement gate and ordered indirect-resource calls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A7E90..0x004A7EF7
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A7F00..0x004A7F0B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A7F10..0x004A7F1B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004AD9C0..0x004ADA27
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004ADA30..0x004ADA3B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004ADA40..0x004ADA4B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F9AA0..0x005F9B6C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F9B70..0x005F9BC2
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D2100..0x006D2190
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D21C0..0x006D2210
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x002EB31C..0x002EB31F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x002EB324..0x002EB327
tool: Ghidra 12.1.3 PUBLIC bounded instruction reading with physical callback-table provenance
environment: null
---

## Observation

FND-EXE-082 physically identifies cleanup slots one and three as
`0x004A7F10` and `0x004ADA40`. Each sets EDX to 65535, clears EAX and
jumps to its own helper, `0x004A7E90` or `0x004AD9C0`, without a new
return address. The helpers compare full EDX with 65535 and full EAX
with one, retaining their equality results as bytes and saving incoming
EAX separately. Their second gate tests that saved full input for zero.
For each cleanup wrapper's fixed inputs, the first group is bypassed and
the zero-input group calls `0x005F9AA0`. The first helper supplies pointer
`0x01BA4A90`; the second supplies `0x01BA4C68` in its first outgoing slot.
Each then restores saved registers and returns without normalizing EAX.

Independently, FND-EXE-080's constructor slots three and five, at physical
`0x002EB31C` and `0x002EB324`, contain `0x004A7F00` and `0x004ADA30`.
Those wrappers supply EAX one and EDX 65535 to the respective helpers.
That path calls `0x005F9B70` with the same respective pointer, then tests
the preserved original input for zero. Under the callee-preserved register
contract, original input one skips the cleanup call after normal return.
Neither wrapper table establishes actual invocation or an all-callback
constructor/cleanup correspondence.

### Shared cleanup callee

The direct local body at `0x005F9AA0` never reads the incoming pointer slot
that distinguishes those two callers. Its nested callees and potential
aliases are not proved unable to access caller memory. It instead builds
a local record rooted at frame minus 64, storing callback `0x005F50A0`,
metadata pointer `0x006EEFF4`, and saved handler target `0x005F9B38`, then
passes the record to FND-EXE-045's setup helper `0x006008F0`.

After normal setup return it writes all ones at frame minus 60, and calls
FND-EXE-033's exchange-add helper `0x005F5760` with address `0x02427E00`
and addend all ones in the first two callee-facing slots. The operation
subtracts one modulo 32-bit width and returns the previous full word.
The caller tests that previous word for exact equality with two, after
the memory update. Previous two produces updated one and admits the next
calls. Previous three produces updated two but skips them; previous zero
wraps memory to all ones and also skips them. There is no local positive
value guard or rollback. These are conditional arithmetic cases, not
proof that the original admits each initial state.

Exact previous two writes record state one, then calls `0x006D2100` with
these first-slot pointers, in order, waiting for each normal return:

1. `0x0242C250`
2. `0x0242C1B0`
3. `0x0242C110`

No call result admits the next call or triggers a direct counter rollback.
All other old values bypass these calls. Both routes then pass the local
record to FND-EXE-049's cleanup helper `0x00600990`, restore the frame and
return its raw EAX result. They do not return the saved decrement value.
The outer callbacks likewise do not turn that result into a success flag;
FND-EXE-082's parent discards it on its fresh cursor read.

The separately stored handler at `0x005F9B38` adds twelve to EBP, then
uses the adjusted frame's minus-56 word as an argument to `0x005FABA0`
(FND-EXE-067). After normal return it writes all ones to adjusted frame
minus 60, calls `0x005FACB0` (FND-EXE-070), then cleans the record at
adjusted frame minus 64 through `0x00600990` and restores the frame.
The first two returns are not tested. Post-call saved-frame values and register preservation remain conditional
where a called body or alias effect is unresolved. The actual dispatcher frame binding,
handler admission and exceptional lifecycle remain unresolved; storing this
target does not prove that the runtime invokes it with that frame identity.

A bounded initialization prefix in `0x005F9B70` separately supplies the
same global address `0x02427E00` and addend one to `0x005F5760`. It tests
the old full word for zero, not the incremented word. The nonzero branch's
later continuation and the complete zero route are unread here. Thus a
producer is located, but initial state, all writers and lifecycle cardinality
are not established. The global counter and supplied object pointers fit
FND-EXE-078's virtual-only BSS; no shipped raw initializer value follows.

### Called resource helper's bounded ordinary paths

The helper `0x006D2100` saves its full incoming pointer at frame minus 68
and sets a local word at frame minus 72 to zero after record setup. It
loads the pointer's first word, reads an adjustment at that value minus
twelve, adds the adjustment to the saved pointer at 32-bit width, then
reads a nested pointer at adjusted offset 120. There is no local guard
before those first pointer and adjustment reads.

A zero nested pointer skips the indirect call. Nonzero reads its first
word, then a full target at offset 24, supplying the nested pointer in
the first outgoing slot. After normal return it increments EAX and tests
zero: exactly all ones before the increment writes one to the local
word. Every other returned word skips that write. This is not a generic
truthy/zero success predicate, and the concrete indirect target remains
unresolved for all three supplied pointers.

If the local word is nonzero, a later route recomputes the adjusted object
pointer, ORs the full word at adjusted offset 20 into the local word and
calls `0x006E7BF0` with adjusted pointer and that combined word in the first
two slots. The next two slots are populated with ESI, whose incoming value
is not established by this reading. The callee and its effects remain
unread. Both ordinary routes then clean the local record and reload the saved
pointer word after normal restoration. Its equality to the original incoming
value remains conditional on intervening callee/handler writes. An undecoded
registered-handler region at `0x006D2191` is excluded, so these paths do
not establish a complete helper or any successful resource operation.

## Interpretation

Two concretely admitted cleanup callbacks join a shared old-value decrement
gate and ordered callee sequence rather than demonstrably destroying their
separate outgoing objects. Q-EXE-009 retains initialization, all counter
writers, indirect targets, resource semantics, saved-handler admission and
exceptional/alias effects. Normal call order alone is not a cleanup guarantee.

## Alternatives

Testing the updated counter for two, decrementing only after the selected
calls, using different direct counters for the two incoming pointer arguments,
undoing the counter on a returning call error, retaining the decrement result
as the final return, or interpreting the indirect helper result as a Boolean
is ruled out locally. The provided object pointer is not proof that this
callee directly consumes it; the locked operation does not establish the
complete shared lifetime or thread behavior.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-082's first four cleanup
slots. Inspect FND-EXE-080's 29 constructor slots for full targets `0x004A7F00`
and `0x004ADA30`. Read three instructions at each wrapper; 50 at `004A7E90`
and 40 at `004AD9C0`, restricting claims to each cited helper body before
gaps. Read 65 at `005F9AA0`, seven at `005F9B65` to check its final return,
and 42 at `005F9B70`, retaining only its counter-call prefix through `005F9BC2`.
Use FND-EXE-033's complete exchange-add body and trace old versus updated
values through every caller test. Use FND-EXE-045/049 for record setup/cleanup.
Read 55 at `006D2100` and 38 at `006D21C0`; retain only the cited ordinary
regions, exclude the missing `006D2191` handler and following functions.
Trace pointer adjustments, result width, all-ones increment gate, fresh object
reads, outgoing slot provenance and raw restored return. Recover only the
physically stored wrappers, tail targets and explicit handler `005F9B38`;
export start and body size only. Keep reports local and execute no original.
