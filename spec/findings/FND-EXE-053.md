---
id: FND-EXE-053
title: Register-input selector traverses a mutable record local and separates callback results from a saved match guard
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600AD0..0x00600B43
tool: Ghidra 12.1.3 PUBLIC, bounded instruction and function reporters
environment: null
---

## Observation

FND-EXE-052 prepares the original argument in the return register and the
selected-record local's address in an auxiliary register before calling this
selector. The selector copies those incoming registers into saved registers
and reads the selected record through the incoming local address. It does
not obtain these two inputs from original stack-argument slots. The local
address is dereferenced without a local null guard.

At each iteration it starts a callback target at zero and a status word at
five. A nonzero current record reads a full target word at record offset 24
and clears the low byte of that status word, yielding zero. It compares the
current record address with the argument's word at offset 16 and constructs
a saved match flag of exactly zero or four. The equality-result write changes
the return register's low byte; that register no longer necessarily contains
the unchanged current record pointer.

A zero current record returns exactly two through normal frame restoration.
This path precedes the later saved-match abort test even when the argument's
offset-16 word is also zero. A nonzero current record with a zero target
reaches the saved-match test without calling a callback.

With a nonzero callback target, the caller prepares eight outgoing 32-bit
words: one; the saved match flag combined with two; the argument's word at
offset zero; its word at offset four; the argument address; the selected-local
address; and two copies of the return-register word whose low byte was changed
by the equality result. It calls the previously loaded target indirectly and
then adjusts the outgoing stack by thirty-two bytes. These are prepared slots,
not proof of the target's parameter count or semantic names. The argument
fields are read after the target and match were obtained.

The full callback result seven returns immediately through normal frame
restoration. The selected local is not reread or advanced before that return;
callback changes to it therefore remain available to FND-EXE-052. Result
eight instead reaches the saved-match test. Any other result returns exactly
two. The match flag used after result eight was computed before the callback,
not recomputed from a callback-mutated local or argument field.

At the saved-match test, a nonzero match flag calls `0x006020C0`, identified
as the abort import by FND-EXE-041. A zero flag rereads the selected local's
current record address, dereferences that record's first word, stores the
obtained link back through the selected-local address, and repeats. The same
advance occurs for a nonzero record with a zero callback target and no match.
There is no local cycle counter or iteration bound. Because advancement
rereads the local after a callback, it is not necessarily the link of the
record whose target and match were loaded earlier.

No local store in this body directly addresses the saved frame, target or stack
fields at record offsets 32, 36 and 40 used by FND-EXE-052. This is not a
no-alias proof for stores through the supplied local address. A callback may
write them, but its body and input contract remain unread. This selector's
indirect target at offset 24 is distinct from the forwarding body's jump
target at offset 36; their equality is not established.

## Interpretation

The register-input contract and direct mutable-local traversal are now
bounded. Returning seven can expose callback changes to the selected local,
while result eight combines those possible changes with an older match guard.
Q-EXE-009 retains callback target provenance and effects, record-link and
field writers, cycle admission, the second selector, dispatcher frame mapping,
argument producers and exceptional behavior. No complete traversal bound,
record layout or handler lifecycle is established.

## Alternatives

Reading the two principal inputs as stack parameters, always following the
original current record's link, treating seven and eight identically, or
recomputing the match after a callback are ruled out by the instructions.
The zero-record case returns two before the saved-match abort test; it is
not equivalent to a nonzero matching record with no callback. Treating the
last two prepared slots as unchanged record pointers ignores the low-byte
write. Naming the offset-24 callback the offset-36 transfer target requires
separate evidence, and callback argument consumption remains conditional.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x00600AD0` and
read one hundred instructions there, restricting claims to the cited range
and excluding the later function also printed. Use FND-EXE-052 for incoming
register writers and FND-EXE-041 for the abort import. Track zero/nonzero
records, target-zero/nonzero arms, the exact match flag and low-byte write,
all eight outgoing slots, full-width seven/eight/other results, retained match
through callbacks and the later local reread before link advancement. Keep
callback and exceptional effects conditional. Keep rich reports local and
execute no interpreter or game.
