---
id: FND-EXE-067
title: Classification-one helper preserves counter ordering and returns a saved payload after cleanup before its caller ignores it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FABA0..0x005FAC76
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD2F0..0x005FD37B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5596..0x005F55BE
tool: Ghidra 12.1.3 PUBLIC, bounded instruction reading
environment: null
---

## Observation

FND-EXE-057's classification-one route prepares the saved fifth-argument-minus-48
base plus 48 as one outgoing full-word argument to `0x005FABA0`. This helper
creates a conventional frame with saved registers and sixty local stack bytes.
It prepares a nested record at frame offset minus 64: offset 24 receives
FND-EXE-055's callback entry, offset 28 receives `0x006EF064`, offset 32 receives
its local frame-minus-twelve address, offset 36 receives stored handler
`0x005FAC77`, and offset 40 receives the stack pointer before outgoing setup
space. It passes that record to FND-EXE-045's setup helper. The metadata and
handler are unread; this is not a complete record layout.

After normal setup it removes sixteen outgoing bytes and calls
`0x005FD2F0` without preparing a new explicit argument. It computes its first
original argument minus 48 at 32-bit width as a base, saves the returned
context address separately, and reads that context's first full word as a
saved old head. No local null guard precedes the context read. It then reads
full words at base offsets 48 and 52 and compares them with FND-EXE-055's
signature pair. The saved old head is not refreshed after these reads.

On signature mismatch, a nonzero saved old head sets the nested state word
to one and calls FND-EXE-041's finalizer. There is no ordinary return or
cleanup established beyond that boundary. A zero saved old head instead
writes the base to the context's first word, saves return value zero, calls
FND-EXE-049's cleanup with the nested record, reloads the saved zero and
returns through frame restoration. This direct mismatch arm does not write
the base's link at offset sixteen or either counter described below.
Indirect callee writes and aliases remain conditional.

On signature equality it reads the full word at base offset twenty and
tests it at signed width. The branches have different counter effects:

| Original base-offset-twenty word | Ordered direct counter stores |
|---|---|
| Nonnegative | Decrement the context-offset-four word by one, store it there; then store original base word plus one at base offset twenty |
| Negative | Store one minus the original base word at base offset twenty; no direct context-offset-four decrement |

All arithmetic and stores use 32-bit width. The negative branch is not an
absolute-value conversion with a guarantee of a positive result. For example,
all ones maps to two, while the minimum signed word maps to the full-word
representation of negative 2147483647. The maximum nonnegative signed word
increments to the minimum signed word. These are instruction-arithmetic
controls, not shipped-state admission or an implementation rule.

After the counter stores it compares the base address with the saved old
head. Equality skips link publication. Inequality first writes that old
head to base offset sixteen, then writes the base to the context's first
word. The comparison uses the saved value even if earlier stores or aliases
changed the current context head. Counter stores precede both link stores;
these operations are not a transaction or an identity proof.

Both equality arms then read the full word at base offset forty and save
it in a dedicated return local. They call cleanup with the nested record,
reload that saved payload into the return register, remove the outgoing
space, restore registers/frame and return. The payload is read after the
counter and possible link stores, so aliasing can affect the value read.
Cleanup's return is discarded and is not a success test. Preservation of the
saved payload and normal callee completion remain conditional.

The context provider itself constructs a similar nested record with callback
entry, unread metadata `0x006EF2A4` and stored handler `0x005FD380`. After
setup it initializes a saved result address to `0x0242BE70`, then compares
the full word at `0x0071B180` with zero at signed width. Zero reaches its
common cleanup directly, reloads that saved address and returns through
complete frame restoration. Thus this branch provides an address, not the
contents of that address. It does not establish that the context's fields
are initialized or identify a stable context lifetime.

A positive guard instead reads the word at `0x0242BE60`, prepares it in one
outgoing stack slot and calls `0x00601910`. A nonzero full return replaces
the provider's saved result before common cleanup. A zero return goes to
`0x005FD409`; a negative guard goes to `0x005FD3A0`. Those branches, the
callee and its actual parameter consumption remain unread. The bounded
zero-guard and positive/nonzero join do not establish a universal context
contract. The provider's setup return is overwritten, and its cleanup
return is replaced by its separately saved address.

At the studied classification-one caller, normal helper return is immediately
overwritten by the saved base address. The caller removes the previous
argument word, reads the full word at that base plus twelve, pushes it into
the same outgoing position, writes its nested state to all ones and calls
FND-EXE-041's indirect-target finalization helper at `0x005FD0C0`. There is
no helper-return truth test, no branch on its zero versus payload result,
and no local seven-return join here. The twelve reserved outgoing bytes
remain across the two calls: one popped argument is replaced by the new
one. The helper may still have changed counters and links before its return
is discarded; unused return does not mean an effect-free call.

## Interpretation

This supplies the classification-one helper's local branch, update order,
link and saved-return contracts and its caller's distinct finalization
continuation. It also bounds one concrete context-address producer without
promoting unread context initialization or guarded acquisition paths.
Q-EXE-009 retains those provider branches, concrete imported/indirect callees,
record aliases and lifetime, stored-helper/callback handler frame admission,
and higher caller contracts. No complete exception lifecycle or shell outcome
is established or implemented.

## Alternatives

Always decrementing the context counter, turning every negative base word
positive, reloading the old head before comparing, publishing the link before
counter stores, returning cleanup's status, testing the helper's return in
this caller, or treating classification one as the seven-return route is
ruled out. A prepared context address is not a proven initialized record,
and a zero-return mismatch arm still publishes the base before cleanup.

## How to reproduce

Use FND-EXE-011's length and XXH3-verified PE and image base. Read eighty
instructions from `0x005FABA0`, thirty-five from `0x005FD2F0`, ten from
`0x005FD362`, two from `0x005FD37A`, and twelve from `0x005F5596`; restrict
claims to the cited bodies and exclude later entries and stored handlers.
Use FND-EXE-045/049 for setup/cleanup, FND-EXE-055/057 for prior writers and
signature admission, and FND-EXE-041 for finalization. Track saved old head,
all counter widths and ordered stores, negative subtraction boundary,
conditional linking, payload last read before cleanup, address versus word
return, and the caller's overwritten return and shared outgoing stack space.
Keep provider/callee gaps, aliases, lifetime and exceptional effects conditional.
Keep rich reports local and execute no interpreter or game.
