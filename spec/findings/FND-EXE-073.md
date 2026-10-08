---
id: FND-EXE-073
title: Payload tail helper clears a floored pool bitmap bit or frees the adjusted prefix by unsigned address range
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FCEF0..0x005FCFC8
tool: Ghidra 12.1.3 PUBLIC, bounded instruction reading
environment: null
---

## Observation

FND-EXE-072 restores its frame and tail-jumps here after replacing its first
original argument with the adjusted payload address. This helper creates
its own conventional frame and nested record at frame offset minus 64.
The record stores FND-EXE-165's callback entry, unread metadata `0x006EF274`,
a frame-minus-twelve local address, stored handler `0x005FCFC9`, and the
stack pointer before outgoing setup space. It calls FND-EXE-167's setup
with that record and then reads its first original full-word argument.
The tail jump retained the original caller's return address; no mode word
is supplied in that first slot after FND-EXE-072's rewrite.

Two unsigned full-word comparisons select the payload address P:

| Input address | Local storage branch |
|---|---|
| `0x02427E60 <= P < 0x0242BE60` | Pool bitmap update |
| Any other word | Prefix-adjusted free import |

The pool window spans thirty-two 512-byte slots, matching FND-EXE-025's
bitmap allocation addresses. Selection uses P itself, not P minus 80.
There is no local alignment, allocation-ownership, original-request-size
or null-input guard. It does not prove that every address in the window
is a live pool payload.

For an in-range input it subtracts the lower bound and shifts right
logically by nine, saving the full slot index. The unsigned range bounds
that index to zero through thirty-one. It reads the full word at
`0x0242C910`; a nonzero word writes nested state one and calls `0x006019A0`
with `0x02427E50` in one outgoing full-word slot, then removes sixteen
outgoing bytes. Zero skips this call. The callee remains unread.

It forms the full mask `0xFFFFFFFE`, rotates it left by the saved index's
low byte, reads bitmap `0x02427E40`, ANDs it with the mask and stores the
result back. The rotate count uses x86's low-five-bit masking, with the
locally bounded index already within that range. Exactly the selected bit
is cleared; other bits retain their read values. This is a read/modify/store
sequence, not a locally atomic bitmap operation or a claim about concurrent
writers. Clearing an already-clear bit is not rejected.

After calculating the new bitmap it freshly reads `0x0242C910`, publishes
the new bitmap, and tests that fresh saved guard. Nonzero writes nested
state one and calls `0x006019F0` with the same address, removing sixteen
outgoing bytes after normal return. Zero skips this second call. The fresh
guard is separate from the first: the local branches permit only the first
call, only the second, both, or neither when the word changes between reads.
Neither callee is given a lock/unlock meaning by its position. Bitmap
publication precedes the second call, and no direct rollback restores the
bit if a later callee fails or transfers control.

For an out-of-range input the helper subtracts 80 at 32-bit width and passes
that resulting prefix address to FND-EXE-024's exact free import thunk.
After normal import return it removes sixteen outgoing bytes and joins
common cleanup. This route does not directly inspect or change the bitmap,
or make the two pool-associated calls. A null input is not passed as null
to free: the local subtraction produces `0xFFFFFFB0`. That arithmetic
control describes the instructions, not evidence that a null payload is
admitted by its real callers or a claim about free's external behavior.

Other boundary controls distinguish the predicate: the lower-bound address
selects bit zero despite lacking the usual payload offset; the last address
below the upper bound selects bit thirty-one despite being unaligned; the
upper bound itself takes the free branch, whose adjusted address lies below
that bound. These are local arithmetic checks, not shipped allocation
provenance. FND-EXE-025's ordinary pool payloads add 80 to a selected slot's
base, so those documented producers fit inside their slots and window.

Both storage arms pass the nested record to FND-EXE-049's cleanup, remove
outgoing space and restore saved registers/frame before returning. The body
does not save a separate status or reload the payload after cleanup; its
return register retains cleanup's path-dependent result. FND-EXE-071's
outer caller does not test the passed-through result. The bitmap change
or free call precedes record cleanup, with no local reversal. Handler
admission, shared-storage lifetime, import and alias effects remain conditional.
No original process or API was executed.

## Interpretation

The payload tail helper now has a complete local unsigned selection,
slot-mask, guarded-call ordering, prefix adjustment and ordinary cleanup
contract. Q-EXE-009 retains pool/static storage producers, the two unread
pool-associated callees, concurrent and alias effects, stored-handler
admission and the optional concrete callback from FND-EXE-072. This does
not establish a synchronized pool lifecycle or validate arbitrary payloads.
No replacement allocator or complete shell outcome is implemented.

## Alternatives

Classifying the prefix address instead of the payload, requiring slot alignment,
rejecting an already-clear bit, shifting the clearing mask instead of rotating,
always pairing the two guarded calls, passing the payload itself to free,
returning a saved Boolean, or assuming cleanup rolls back publication is
ruled out. A pool-range comparison is not ownership validation, and separate
guard reads do not establish matched synchronization operations.

## How to reproduce

Use FND-EXE-011's length and XXH3-verified PE and image base. Read ninety
instructions from `0x005FCEF0`, restricting claims to the cited body and
excluding later entries and stored handlers. Use FND-EXE-025 for bitmap
allocation/prefix relations, FND-EXE-024 for the exact free import,
FND-EXE-167/049 for setup/cleanup, and FND-EXE-071/072 for the incoming
tail argument and ignored return. Track unsigned half-open bounds, floor
index and rotate width, before/after guard reads, publication order, free
input wrap, common cleanup and return-register preservation. Keep callee,
initializer, concurrency, alias and exceptional effects conditional. Keep
rich reports local and execute no interpreter or game.
