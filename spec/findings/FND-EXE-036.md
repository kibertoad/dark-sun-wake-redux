---
id: FND-EXE-036
title: Capacity-limit helper constructs a local value and decrements its preceding word before failure publication
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F77D0..0x005F7884
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F78E8..0x005F7920
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-035 calls `0x005F77D0` for its initial unsigned capacity-limit
branch. This bounded direct path first sets up a local record through
`0x006008F0`, storing `0x005F7885` as a handler target. The handler body
and its exceptional entries are outside this finding's scope.

It reads the first original 32-bit argument and calls `0x006D6BE0` with
an address of a local word, that argument, and a second local address in
three outgoing slots. The local record state is four before this call.
The local word is later used as a payload pointer, but the construction
helper's writes and failure effects remain unread.

After normal completion it overwrites the current first outgoing slot with
eight and calls `0x005FCD70`. FND-EXE-025 records the latter helper's
80-byte prefix, malloc boundary and bitmap fallback. Thus the eight-byte
request is not the full malloc request: its ordinary augmented request is
88 bytes. The returned pointer is saved, and the local record state becomes
three. It calls `0x005FE730` with that saved pointer and the address of the
local word. There is no local result-null guard before this call, and the
callee's accesses and result are not interpreted here.

After normal completion it rereads the local payload pointer, forms payload
minus twelve, and compares that address with `0x0071B270`. Equality skips
the preceding-word operation and joins the final publication call. Otherwise
it changes its record state to one and calls `0x005F5760` with payload minus
four and minus one in the first two outgoing slots; the later two slots
both hold the saved payload-minus-twelve pointer. FND-EXE-033 establishes
that helper's 32-bit locked addition and return of the pre-addition word.
Its own direct body reads only the first two slots.

The caller tests that old 32-bit value as signed. A value greater than zero
joins publication without the next call. Zero or a negative value calls
`0x006D4DF0` with the saved payload-minus-twelve address, another local
address, and two outgoing copies of the old value, then joins publication
after normal completion. This branch tests the old value, not the decremented
stored value. Release semantics and the latter helper's argument reads,
mutations and exceptional exits remain unread.

At the common publication point it reloads the saved allocated pointer,
sets its record state to minus one, and calls `0x005FAED0` with that pointer,
`0x00755E00` and `0x006D8BD0` in the first three outgoing slots; a later
slot holds the current preserved register word. FND-EXE-025 reads the
consumer's first three inputs and prefix writes without establishing a
complete exception contract. The analyzer has no fall-through for this call.
Bytes following it are also the stored handler target; that coincidence is
not evidence of a normal return or an ordinary cleanup continuation.

## Interpretation

This narrows the unresolved capacity-limit boundary to temporary construction,
an augmented failure-object request, a pre-addition signed cleanup decision and
the previously studied consumer. All steps after calls are conditional on their
normal completion and valid local storage. The special prefix address bypasses
the decrement; it is not assigned an ownership meaning here. Q-EXE-009 still
needs constructor effects, handler admission, cleanup and consumer contracts
before this route can establish rejection, exception or termination behavior.

## Alternatives

A plain eight-byte malloc request, testing the newly decremented value,
always calling the later helper after decrement, or applying the decrement
to the special prefix address are ruled out by the bounded reading. Treating
the analyzer's no-return flag as proof of an exception, or reading the stored
handler as normal fall-through, is not justified by these instructions.
The later helper may release storage, but its semantics cannot be inferred
solely from its location after a decrement.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x005F77D0`, read
65 instructions from it and forty from `0x005F78B7`. Limit this finding to
the two cited body ranges; the windows also show stored-handler code outside
that scope. Follow the initial argument and local-address writers, outgoing
slot overwrite with eight, saved allocation pointer, local payload reread,
subtractions by twelve and four, special-address guard, first two decrement
slots, signed old-value test, later helper slots and common consumer slots.
Use FND-EXE-025 for the prefix allocation and consumer boundary and
FND-EXE-033 for the locked helper's return contract. Keep unread callee and
handler effects conditional. Keep rich reports local and execute no interpreter
or game.
