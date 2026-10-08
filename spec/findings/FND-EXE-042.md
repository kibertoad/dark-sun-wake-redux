---
id: FND-EXE-042
title: Shared-record reader decodes thirty-two atom-name bytes after a nonzero import result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600530..0x006005A8
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00602530..0x00602536
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x0035903A..0x00359046
tool: Ghidra 12.1.3 PUBLIC and bounded physical PE import-table reading
environment: null
---

## Observation

The bounded reader at `0x00600530` masks its incoming return-register word
to the low sixteen bits and passes that value, a local buffer address and 66
to `0x00602530`. The thunk jumps through slot `0x02431820`; bounded PE
import reading identifies GetAtomNameA from KERNEL32.dll. The cited shipped
name includes NUL. Independent malloc/free slots match FND-EXE-024 as controls.
This is a register input, not an inferred original stack argument.

Before the call the body clears its prospective decoded word. After normal
return it tests only whether the full returned word is zero. Zero selects
an unresolved call to `0x00601F80` with locally written outgoing values.
The body does not compare the returned length against 32 before decoding.

For nonzero return it initializes a byte index to 31 and a 32-bit mask to one.
At each index it compares the local byte with 65. Equality ORs the current
mask into the decoded word; inequality leaves that word unchanged. Both paths
double the mask at 32-bit width, decrement the index and continue while the
index is nonnegative. Thus indices 31 through zero are examined exactly once;
index 31 supplies the low bit, and index zero the high bit. Other byte values
are not rejected locally. The local buffer is not directly initialized before
the import call; unread import effects and actual caller inputs remain conditions.

After decoding it reads the first 32-bit word through the decoded address
and compares it with 60, without a local decoded-address-null check. Equality
returns that decoded address after normal frame restoration. Inequality calls
`0x00601F80` with a separately written set of outgoing values. Its behavior
and any normal-return continuation past either call remain unread. No claim
of rejection, process termination or malformed-input reachability is made.

## Interpretation

This supplies a bounded reader for the existing-record lead in the shared-target
initializer. The reader's acceptance test is a decoded record word equal to 60,
not a local validation of all 32 encoded bytes or a returned-length check.
Q-EXE-009 retains initializer branches, encoded-name production, admitted inputs,
import writes, record validity and failure-helper contracts. It does not establish
all shared-target producers or a complete storage lifetime.

## Alternatives

Decoding only the returned number of bytes, treating every non-65 byte as an
error, reversing the bit/index correspondence, or guarding a zero decoded address
before dereference are ruled out by the bounded instructions. A full-length
name supplied by all callers is possible but requires producer evidence; absence
of a local guard alone does not prove a reachable short-name defect.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x00600530` and
read forty instructions from its entry and eight from `0x0060059F`. Restrict
claims to the cited body, excluding subsequent functions. Read one instruction
from `0x00602530` and independently map its slot through bounded PE sections,
descriptors and terminated lookup thunks with malloc/free controls. Follow the
register mask, buffer/count slots, zero-result branch, index and mask updates,
both byte-test arms, decoded dereference, equality return and failure calls.
Keep unread import, caller and continuation effects conditional. Keep rich
reports local and execute no interpreter or game.
