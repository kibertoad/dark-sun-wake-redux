---
id: FND-EXE-023
title: Compiled record append delegates full storage to a width-sensitive insertion helper
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A8540..0x004A856C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A8570..0x004A8588
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006E5930..0x006E599B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006E59A0..0x006E5A48
tool: Ghidra 12.1.3 PUBLIC, pinned bounded instruction and reference reporters
environment: null
---

## Observation

FND-EXE-022 reads base, end and index words from 16-byte records beginning
at `0x01BA4AA0`. The routine at `0x004A8540` takes a 32-bit slot argument
and a supplied 32-bit word. It forms the record address by shifting the slot
left by four at 32-bit width, with no local slot bound. It compares the
record's end word at offset four with the word at offset eight. When they
differ, a nonzero end receives the supplied word; a zero end skips that
store. Both arms then write end plus four, at 32-bit width, back to offset
four and return. No ordering comparison proves that differing pointers
describe spare allocated storage.

When the words compare equal, the routine calls `0x006E5930` with the record
address, the old end as insertion position, and the address of its supplied
word argument. It returns after normal completion without testing a return
value. Its direct body neither constructs the supplied object nor writes
the published pointer array from FND-EXE-022.

The insertion helper takes a record, insertion position and address of a
word to insert. It independently compares end with offset eight. In its
unequal arm, a nonzero end first receives the preceding four-byte word; a
zero end skips this copy. It then reads the supplied word and publishes end
plus four into the record before calling the memmove thunk identified by
FND-EXE-021. The copy length is the 32-bit difference end minus four minus
insertion position, with its low two bits cleared. The copy destination is
end minus that length and the source is the insertion position. After normal
copy completion it writes the saved supplied word at the insertion position.
There is no local admission check for position alignment, ordering or extent.
This arm is not the path selected by a stable full record in the append
caller; its other callers and their inputs remain unread.

In the helper's equal arm it subtracts base from end at 32-bit width and
arithmetically shifts right by two. If that computed count is zero, the
requested word count is one; otherwise it doubles the computed count at
32-bit width. It then shifts the requested count left by two at that width
and passes the resulting value to `0x005F7E10`. Neither overflow nor a
negative nonzero count has a separate local rejection. This finding names
the value passed, not that callee's allocation unit, header, failure policy
or admitted size.

After that call returns, its return value is used as the new base without a
local success test. The helper copies insertion-position minus the then-read
old base bytes from the old base to the new base through memmove. The
insertion destination is new base plus that difference. Only if that
destination is nonzero does it read the supplied word and store it there.
It next reads the record's end and copies end minus insertion-position bytes
from the insertion position to insertion destination plus four. All pointer
arithmetic and differences here are 32-bit operations.

It then reads the old base again. If nonzero it passes it to `0x005F9910`;
if zero it skips that call. After normal completion, it publishes the new
end first, new base second and new base plus the saved requested value third,
at record offsets four, zero and eight respectively. The new end uses the
end-minus-position length saved before the second copy plus its destination.
No local rollback or alternative failure branch is present. The two
unread callees and CRT effects remain conditional; their exceptional exits,
allocation units and possible shared-state effects are not established.
The record's index at offset 12 is not changed by these direct bodies.

## Interpretation

Offset eight participates as a capacity endpoint in append and insertion,
while offset four supplies the count endpoint read in FND-EXE-022. These
paths establish one way words enter those records, not the provenance or
storage bounds of their objects. The full-storage path requests a value
computed with wrapping 32-bit arithmetic and publishes replacement fields
after copying and the conditional old-base call. A valid, aligned record
with an admitted position might make these operations ordinary insertion;
that invariant needs producer and caller evidence and is not inferred from
the decompiler's apparent container type.

## Alternatives

A strict end-less-than-capacity guard, a checked growth multiplication, a
locally tested allocation return, publication before both growth copies, or
an index increment by the append routine are ruled out by the bounded direct
branches. Whether the allocation call throws rather than returns on failure,
and whether all reachable callers exclude invalid records, remain open under
Q-EXE-009. Neither a missing local guard nor a conditional null store proves
a reachable failure in the declared wrapper.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Query references separately to
`0x01BA4AA0`, `0x01BA4AA4`, `0x01BA4AA8` and `0x01BA4AAC`, retaining DATA
references as well as WRITE references. ReportReferences has a fixed cap
of 200 per address; its arguments are addresses, not a configurable cap.
Summarize `0x004A8540` and `0x006E5930`. Read 28 instructions from the
former and 90 from the latter, stopping at the cited bodies' ends and
excluding following functions. Verify every equal/unequal and null/non-null
arm, arithmetic width, the arithmetic rather than logical count shift,
request value, copy lengths and destinations, field-publication order and
conditional old-base call. Track the append frame's 12-byte reservation
and helper frame's 44-byte reservation when identifying the original
arguments and the address passed for the supplied word. Treat allocator,
release and CRT calls as conditional boundaries; no complete caller census
or object construction is claimed. Keep rich reports local and execute no
interpreter or game.
