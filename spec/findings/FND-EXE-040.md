---
id: FND-EXE-040
title: Conditional payload release forwards the raw prefix pointer except for one fixed address
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D4DF0..0x006D4E0E
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-036 and FND-EXE-039 call this helper on their non-special prefix
paths when the signed pre-decrement word is zero or negative. Their first
outgoing slot is the saved payload-minus-twelve pointer; later slots contain
a local address and two old-value copies.

The direct body of `0x006D4DF0` reads only its first original 32-bit argument.
It compares that pointer with `0x0071B270`. Equality skips the call and returns
after normal frame restoration. Inequality calls `0x005F9910` with the original
pointer unchanged, then restores its frame and returns after normal completion.
It performs no direct storage dereference, field clearing, pointer adjustment,
reference-word test or decrement. It does not directly read the later outgoing
slots or compute an explicit status value.

FND-EXE-024 identifies `0x005F9910` as a zero-pointer bypass followed by the
free import for nonzero input. Consequently this wrapper does not subtract
another twelve or eighty before forwarding the prefix pointer. Zero is not
its fixed-address bypass, but the nested helper handles zero. Nonzero pointers
other than the fixed address reach the imported release boundary, conditional
on normal helper execution. Valid allocation provenance remains a caller contract.

## Interpretation

This supplies the previously unread release boundary in the two failure routes:
the saved prefix pointer is forwarded unchanged, with a local fixed-address
bypass. The earlier decrement is the caller's operation, not repeated here.
The local source field is not cleared by this direct body. Q-EXE-009 retains
allocation provenance, aliases, later reads, exceptional handlers and shared
failure-consumer effects. This is not a complete ownership or lifetime proof.

## Alternatives

Reading the old-value slots, decrementing twice, releasing the payload pointer
instead of the supplied prefix, or clearing a source field directly are ruled
out by the bounded instructions. Interpreting the fixed address as an owned or
immortal object requires its producers and consumers, not this comparison alone.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x006D4DF0` and
read twenty instructions from its entry, restricting claims to the cited body
and excluding later functions. Follow its first argument read, fixed-address
comparison, unchanged outgoing pointer and both frame-restoration paths.
Use FND-EXE-036 and FND-EXE-039 for caller writers and FND-EXE-024 for the
nested release/import contract. Keep allocation provenance and exceptional
behavior conditional. Keep rich reports local and execute no interpreter or game.
