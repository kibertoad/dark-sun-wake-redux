---
id: FND-EXE-071
title: Head cleanup callee guards an offset-eight indirect target and returns its result unchanged
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601110..0x0060112C
tool: Ghidra 12.1.3 PUBLIC, bounded instruction reading
environment: null
---

## Observation

FND-EXE-070's signature-mismatch and counter-one cleanup arms prepare the
saved head address plus 48 as one outgoing full-word argument to this callee.
The body creates a conventional frame, reserves eight stack bytes through
two register pushes, reads its first original stack argument as an input
address, and reads the full target word at input offset eight. There is no
input-null guard before that read. For these callers the target location is
head offset 56, with addition at 32-bit width; its writer and concrete target
remain unresolved.

A zero target returns directly through frame restoration. The target's zero
is still in the full return register, so this branch returns zero. It performs
no additional read, call, direct record write, allocation or release operation.

A nonzero target prepares four full outgoing stack words in callee-facing
order: one, the input address, and two copies of the current count-register
word. It calls the loaded full target indirectly, removes sixteen outgoing
bytes after normal return, restores its frame and returns without changing
the full return register. The callee's return is therefore passed through,
not converted to a Boolean or tested for failure. The two auxiliary words
are prepared values; no consumption contract or semantic argument names
are established for the unresolved target. Stack restoration depends on
ordinary target completion and the local continuation's cleanup contract.

There is no direct field clear, free import or cleanup-result-dependent
branch in this bounded body. The indirect target may have effects or transfer
control, so the body does not prove that the operation is effect-free or that
resources are retained. The target is guarded only after its containing
record has already been read.

Both studied callers first change the context head as recorded in
FND-EXE-070: mismatch writes zero; counter one writes the saved head's
previous-link word. Only then does this callee read the target at head offset
56. The context-head store is therefore earlier than target selection, and
possible aliases can affect the word selected. Neither caller tests the
returned zero or passed-through result before its normal frame restoration.
A returned zero cannot distinguish a null target from a nonnull target that
returned zero, and it does not reverse the caller's prior context store.

## Interpretation

The previously unread head-associated callee now has a bounded input read,
target guard, outgoing-slot and normal-return contract. Q-EXE-009 retains
head-offset-56 producers, concrete indirect targets, their argument consumption,
resource effects, aliases and stored-handler/dispatcher admission. No release
operation, complete head lifecycle or shell outcome is established or implemented.

## Alternatives

Treating the input address itself as the call target, checking input null
before its first read, calling a zero target, converting every returned value
to success/failure, or inferring a direct release from the caller's cleanup
role is ruled out. The input-plus-eight read follows the context-head change;
it is not a target snapshot from before that change. Prepared auxiliary words
do not prove what an indirect target reads.

## How to reproduce

Use FND-EXE-011's length and XXH3-verified PE and image base. Read thirty-five
instructions from `0x00601110`, restricting claims to the cited body and
excluding the gap and later entry. Use FND-EXE-070 for input preparation,
prior ordered stores and ignored returns. Track the full input/target reads,
guard placement, zero-return writer, four outgoing slots, indirect call,
normal cleanup and unchanged return register. Keep target provenance, slot
consumption, aliases and exceptional effects conditional. Keep rich reports
local and execute no interpreter or game.
