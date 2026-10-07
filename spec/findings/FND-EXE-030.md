---
id: FND-EXE-030
title: Node comparison uses a stored payload length and unsigned byte ordering
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006BCC40..0x006BCC9F
tool: Ghidra 12.1.3 PUBLIC, pinned bounded instruction and function reporters
environment: null
---

## Observation

FND-EXE-029 passes node plus eight and its original input to `0x006BCC40`.
The callee reads both arguments at 32-bit width. It dereferences the first
argument to obtain a payload pointer, then reads a 32-bit word twelve bytes
before that payload pointer. It saves both pointer and word locally before
calling the strlen thunk identified in FND-EXE-015 with the second argument.
The stored word's producer, header allocation and string lifetime remain
unread; calling it the stored length describes its use here, not a complete
object layout.

After normal strlen completion it saves that full 32-bit result and compares
it unsigned with the saved stored length. A smaller input length selects
the input length as comparison count; otherwise it selects stored length.
It reloads the saved payload pointer, clears the direction flag and seeds
the comparison flags with an equal comparison before repeated byte comparison.
The payload side is addressed through DS and the original input side through
ES in the string instruction. Their segment views and the host CRT remain
conditions on interpreting this as two ordinary flat byte strings.

With nonzero count, comparison advances forward and repeats while bytes
compare equal and count remains. A differing byte produces unsigned ordering
flags, converted to a signed byte result: plus one for payload-side greater,
minus one for payload-side less. The result is sign-extended to 32 bits and
returned unchanged when nonzero. No case folding, locale conversion or
additional normalization occurs in this direct comparison body.

When compared bytes remain equal, including a zero initial count using the
seeded flags, the callee instead returns stored length minus the saved
strlen result at 32-bit width. That subtraction has no separate overflow
or signed-range check. Its normal paths restore the saved registers and
stack frame. It has no local null, payload capacity or terminator guard,
and it does not locally publish a node or change its fields.

## Interpretation

Under valid readable segment views, a truthful stored length and normal
strlen behavior, the full-width zero test in FND-EXE-029 selects equal-length
payload/input byte sequences. This is narrower than establishing the
collection's admitted strings or lifetime: an embedded NUL, stale length,
invalid capacity or different segment view cannot be assigned a supported
outcome from these local operations alone. Q-EXE-009 still needs producers,
caller contracts, aliases and downstream effects. This is not a complete
caller or storage-layout reading and does not promote FMT-EXE-006.

## Alternatives

Comparing only until the payload's NUL, selecting the larger length, using
signed byte ordering, relying on incoming direction flags, returning a byte
difference magnitude, or testing equality without considering lengths are
ruled out by the bounded instructions. A header with truthful length and
valid flat byte storage is consistent with these operations; its construction
and allocation bounds require direct producer evidence. No normalization
inside this routine excludes normalization by an earlier producer.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x006BCC40` and
read 45 instructions from its entry, stopping at `0x006BCC9F` and excluding
following functions. Follow FND-EXE-029's argument writers into the pointer
dereference and preceding length word. Use FND-EXE-015's import-slot evidence
for strlen rather than an inferred name. Verify unsigned minimum selection,
explicit forward direction, flag seeding for zero count, repeated comparison
segment operands, unsigned ordering and signed-byte extension, then the
full-width length subtraction and caller zero test. Cover either length
smaller, equal lengths, zero count, first differing bytes and equal prefix
conditional on valid producers and normal CRT behavior. Keep rich reports
local and execute no interpreter or game.
