---
id: FND-EXE-038
title: Temporary production distinguishes null input from equal endpoints before payload copying
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D6BE0..0x006D6C1B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D4AA0..0x006D4B08
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-036 calls `0x006D6BE0` with a destination-word address, an input
pointer and a local address. This direct body reads those three original
arguments at 32-bit width. It initializes the prospective end to all ones.
For nonzero input it calls `0x00601CD0`, identified as the strlen import
in FND-EXE-015, and adds its result to the saved input at 32-bit width.
For zero input it skips that call and retains the all-ones end.

It calls `0x006D4AA0` with saved input, computed end, the third original
word and zero in four outgoing slots. The immediately preceding stack
reservation is two bytes, not a decompiler-implied aligned word. After normal
return it reads the original destination argument, removes sixteen outgoing
bytes and stores the full returned word through that destination. Its frame
restoration discards the earlier local reservation. There is no direct null
or result-status test at this publication point.

The range helper initially selects `0x0071B27C` as its return word. It
reads the first two original 32-bit arguments and returns that selected word
immediately when they are equal, without a copy or allocation. Consequently
zero/zero endpoints take this branch before any null check.

For unequal endpoints it tests the first word for zero. A zero first word
calls `0x005F7C80` with `0x00754504`; if that unread callee returns normally,
the path rejoins the unequal-endpoint allocation sequence. The body does not
repeat the null guard. A nonzero first word enters that sequence directly.

The sequence computes end minus start at 32-bit width without a local ordered
range guard. It calls `0x006D50A0` with that wrapped difference, zero, the
third original argument and start in four outgoing slots. FND-EXE-035 reads
the storage helper's first two inputs and capacity rounding; its third is not
directly read. This range body's fourth original argument is not directly read.

After normal storage return it saves the base and forms base plus twelve.
It calls the memcpy thunk `0x00601CB0`, identified in FND-EXE-034, with
that payload destination, a fresh read of the original start argument and
the saved difference. After normal copying it writes the saved difference
as the base's first 32-bit word, writes a zero byte at base plus twelve plus
that difference, and returns the saved payload pointer. It does not use the
imported copy result. No local destination-null, capacity or overlap guard
precedes these accesses. The start argument reread can differ if unread
callees or aliases mutate its storage; the count is the earlier saved difference.

## Interpretation

Under valid non-null terminated input and normal helper behavior, temporary
production supplies a copied payload for a nonempty input and the fixed word
for equal endpoints. A null source instead passes zero/all-ones endpoints,
reaching the unresolved null-input helper rather than the equality branch.
This narrows temporary production for FND-EXE-036 and the collection path in
FND-EXE-031, without establishing every caller, input range, fixed storage,
exception, ownership or lifetime. Q-EXE-009 retains those dependencies.

## Alternatives

Treating null input as an empty input, checking null before endpoint equality,
reading the fourth original range argument directly, returning the allocation
base, or publishing memcpy's return are ruled out by the bounded bodies.
Guaranteed null rejection requires reading `0x005F7C80`; its analyzer label
or the surrounding shape is insufficient. The fixed return word is not called
an empty-string object until its storage and consumers are read.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x006D6BE0` and
`0x006D4AA0`, reading fifty instructions from the former and eighty-five from
the latter. Restrict claims to the cited bodies, excluding later functions.
Track each original argument read, null versus non-null end production,
two-byte reservation, four outgoing writers, return publication, equality
before null checking, normal-return failure join, subtraction width, allocation
slots, saved base/count, start reread, copy slots, header/terminator writes and
payload return. Use the cited import and storage findings; retain unread callee,
caller-range and alias effects as conditional. Keep rich reports local and
execute no interpreter or game.
