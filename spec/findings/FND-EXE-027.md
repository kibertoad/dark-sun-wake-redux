---
id: FND-EXE-027
title: A compiled list producer inserts its allocation result before dispatching an output word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006B0A5B..0x006B0B6F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006B0B95..0x006B0BB5
tool: Ghidra 12.1.3 PUBLIC, pinned bounded instruction and reference reporters
environment: null
---

## Observation

FND-EXE-026's append block is reached by the unsigned exit at
`0x006B0A75` of an earlier loop. The bounded producer sequence here is not
a complete parent-function or caller reading. Stack offsets are relative
to its unchanged local stack pointer, conditional on normal callees; earlier
frame construction, input admission and aliases remain unread.

Each test reloads end and base words at stack offsets 2868 and 2864,
subtracts base from end at 32-bit width, arithmetically shifts right by two,
and compares that count unsigned with the index at offset 824. Count less
than or equal to index exits to the block in FND-EXE-026. The index's original
initialization is outside this reading. On the admitted arm it initializes
the word at offset 1048 to `0xFFFFFFFF`, reads the indexed 32-bit source word,
and saves it at offset 816. It passes 262580 to the allocation wrapper from
FND-EXE-024, saving the returned value at offset 812. There is no local
success test of that return before the subsequent callee.

The call to `0x004CD2B0` receives five explicitly supplied 32-bit arguments:
the saved allocation result, the sign-extended byte at offset 1027, the
saved source word, the zero-extended byte at offset 1003, and the address
of the word at offset 1048. A local state word at offset 1072 is set to 130
before the call. The callee's direct output, object extent, virtual
targets, storage writes and exceptional behavior are not read here; its
name is kept neutral. In particular, the initialized output word is not
evidence of the value retained after that call.

After normal completion the caller reloads the saved allocation result,
not the callee's return value, and saves it at offset 1044 for insertion.
It compares the local list's end at offset 1140 with the word at offset
1144. When unequal, a nonzero end receives that saved pointer, while zero
skips the store. Both arms increment the end word by four at 32-bit width.
When equal, it calls `0x006E5930` with the record address at offset 1136,
the old end and the address of the saved pointer at offset 1044. It writes
131 to the local state word before this call and joins the same continuation
after normal return. FND-EXE-023 reads that insertion helper's local branches;
FND-EXE-024 describes its conditional allocation boundary.

Only after either insertion arm does it read the word at offset 1048.
It compares that full word unsigned with six. A larger value branches to
`0x006B0C05`; otherwise it jumps through the four-byte indexed table at
`0x007288D8`. The table's concrete targets and their paths are not read here,
and the values are not assigned success or error meanings. This is a
bounded table-index gate, not a complete dispatch-table reading.

A separately read continuation at `0x006B0B95` calls `0x0058C430`, then
reloads the word at offset 1048. Nonzero branches to `0x006B0EAD`; zero
increments the source index at offset 824 and jumps to the earlier loop
test. No claim that every dispatch arm reaches this continuation is made.
The helper might change state before the reread. Source end/base and output
word producers and all exceptional cleanup remain conditional.

## Interpretation

This supplies a bounded producer for the local pointer list consumed by
FND-EXE-026, with explicit allocation and following-call arguments. Normal
insertion precedes the local status dispatch, so the pointer is not withheld
by a status test in this sequence. Whether a later path removes it, releases
storage or changes either local list is still unread. No object type,
object size contract, admitted selector range or complete mount outcome
is inferred from the request value or argument order. Q-EXE-009 remains open
for those dependencies and the concrete table paths.

## Alternatives

Using the following callee's return value as the inserted pointer,
checking the output status before insertion, zero-extending the selector
argument, sign-extending the second byte, or admitting unsigned status seven
to the local table are ruled out by the bounded instructions. A nonzero
status might represent failure, a special outcome or an intermediate state;
its producer and consumers must decide that rather than this initialized
all-bits-set word alone.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Query references to
`0x006B0C0E` using ReportReferences' fixed 200-reference cap as a lead,
not as a complete predecessor census. Read 65 instructions from
`0x006B0A5B` and 40 from `0x006B0B95`. Restrict claims to the cited ranges;
exclude later unrelated branches and the already separately recorded append
block. Track full-width subtraction and arithmetic shift separately from
unsigned guards. Verify the allocation request, each argument's last writer,
the saved allocation result across the following call, both insertion
arms, publication before output testing and the table's local index bound.
Read neither inferred parent-frame types nor table targets into these
windows. Keep callees and dispatch reachability conditional. Keep rich
reports local and execute no interpreter or game.
