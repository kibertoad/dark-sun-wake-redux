---
id: FND-EXE-184
title: Shipped provider setup changes state before its builder result is admitted
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00417020..0x004170F6
tool: executable-reader 2.4.0, scientific-method-engine 13.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

The setup candidate saves EDI, ESI and EBX and reserves sixteen stack
bytes. Its first five incoming dword arguments are then at current
`ESP + 0x20`, `+0x24`, `+0x28`, `+0x2C` and `+0x30`. FND-EXE-183
records one caller's explicit outgoing writers; the first argument's
object admission remains unread.

The candidate tests the byte at the first argument's address. A nonzero
byte branches directly to a call of `0x0058F890` at `0x004170F1`, with
outgoing argument `0x0071F337`. That helper's continuation is unresolved.
For a zero byte, the candidate first writes byte one there and dword one
at object offset eight. It then searches indexes one through `0x7F` in
the dword table at `0x0075B460`, selecting the first value equal to
`0x00415E30`. The index increments before the next unsigned bound test;
an index above `0x7F` branches to a call of `0x0058F890` at
`0x004170DA`, with outgoing argument `0x0071F228`.

On a selected slot, the candidate stores the index at object offset four
and clears the selected table dword. It calls `0x004162C0` at
`0x00417099` with four outgoing dword arguments: the index, incoming
argument three, incoming argument four, and a zero-or-one value formed
from whether incoming argument two is nonzero. It tests the returned EAX
at full dword width. A zero branches to shared cleanup at `0x00417074`.
There is no local undo of the earlier object and table stores on this
path; the callee's own writes are not established by this observation.

On a nonzero builder result, the next local store places EDI in the
indexed dword table at `0x0075B460`. EDI initially holds incoming
argument two; its survival through the builder, and EBX's index survival,
require that callee's contract. ESI initially holds incoming argument
five. If the tested ESI is zero, a local path clears the indexed dword
at `0x0075B260` and goes to shared cleanup. Otherwise, the candidate
calls `0x00601CD0` with ESI, increments the returned EAX at dword width,
passes that value to `0x005F9940`, stores its returned EAX in the indexed
table at `0x0075B260`, then calls `0x00601CC0` with that EAX and ESI.
These returns are not treated as admitted lengths, allocations or copies
until their callees are read. Their failure effects and register
preservation are unresolved.

Shared cleanup releases sixteen reserved stack bytes, restores EBX, ESI
and EDI, and returns near at `0x0041707A`, without extra argument cleanup.
It does not normalize EAX into a common success indicator. Under the
reporter's explicit assumption that the slot-exhaustion helper returns,
its continuation reaches the indexed-table clear at `0x004170DF`; this
is conditional traversal, not established failure behavior.

The original-source bounded traversal reaches 210 bytes in
`0x00417020..0x0041704C` and `0x00417050..0x004170F6`. It lists the
shared return, six calls and an unresolved edge at the exclusive region
end following the already-used-object helper call. It remains incomplete.

## Interpretation

The caller's local ordering shows that admission failure can follow state
changes. Neither a zero builder return nor a failure-helper call proves
transactional rollback. This resolves part of FND-EXE-183's setup
consumer, while Q-EXE-001 and Q-EXE-010 retain object and table producers,
callee register/stack preservation, builder effects and outputs, diagnostic
continuations, metadata allocation/copy extents and subsequent dispatch.
The tables' runtime identities and every writer remain unread. No complete
reading or guest callback admission follows.

## Alternatives

A reading that assumes all setup state is written only after the builder
succeeds is contradicted by the local store ordering. A stronger claim that
failure leaves the earlier values unchanged would still need every callee's
effects and aliases. The bounded traversal's continuation assumptions
cannot establish that either diagnostic call returns.

## How to reproduce

Use the shipped interpreter with XXH3-128
`09861838aa3018346f9f15c9a4f5925c`. Run the committed wrapper's
`x86-bounds` command with sourceKind `pe32`, entry file offset `0x00016420`
(91168), and one named region start 91168, exclusive end `0x000164F6`
(91382), entries `[91168]`. Describe it as bounded setup paths with callee
effects and failure continuations unresolved. Omit segment/ip, seeds and
callee summaries. The reader derives the PE mapping from section virtual
start `0x00401000` and raw offset `0x400`. Preserve both reached intervals,
the four-byte hole, every call continuation assumption and incomplete result.

In the saved shipped-PE project, read-only with analysis disabled, run
ReportInstructionWindow at `0x00417020`, count 75. Restrict the reading to
the candidate's reached intervals and exclude the neighboring procedure
beginning at `0x00417100`. Follow each incoming argument from the entry
pushes and reservation, and each branch to the shared cleanup, without
assuming a return register or saved register survives an unread callee.
Rich listings and reports stay in the local licensed-source store, outside Git.
