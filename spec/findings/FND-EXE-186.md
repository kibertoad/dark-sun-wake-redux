---
id: FND-EXE-186
title: Shipped callback consumer admits signed indexes and preserves a nonzero callback result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004017C0..0x00401830
tool: executable-reader 2.4.0, scientific-method-engine 13.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

The consumer at `0x004017C0` reserves twelve stack bytes. At each loop
entry it calls `0x004F26E0` and tests only AL. For nonzero AL, it calls
through the dword at `0x0075B0D0` and tests the returned EAX at dword
width with signed branches. A negative result returns dword one. Zero
re-enters the loop. A positive result above `0x7F` returns dword zero.
The remaining values, one through `0x7F`, select a dword call target
at `0x0075B460 + EAX * 4`, called at `0x004017EA`.

The table is the one written by FND-EXE-184's setup candidate. After the
table call, the consumer tests returned EAX at full dword width. Zero
re-enters the loop; nonzero releases the twelve reserved bytes and returns
near at `0x004017F8`, preserving that EAX value. It does not normalize this
callback result to one. The targets' own stack cleanup, effects and return
contracts, and the table's other writers, remain unread.

For zero AL at the first test, the consumer calls `0x00520810`, then
reads the dword at `0x0075B040` and tests it against zero. A nonzero dword
calls `0x004F28B0`, then decrements the stored dword and re-enters the loop.
The decrement operates on the stored value after the callee, not necessarily
the value previously tested. For zero, it calls `0x00401280` and then
returns dword zero. The negative-index and above-bound returns are at
`0x0040182F` and `0x00401826` respectively. All three local near returns
release twelve reserved bytes and have no extra argument cleanup.

The fingerprinted source traversal covers 105 instruction bytes at
`0x004017C0..0x004017F9` and `0x00401800..0x00401830`. It lists the
three returns and six call sites: `0x004017D0`, `0x004017D9`,
`0x004017EA`, `0x00401800`, `0x0040180F` and `0x0040181C`. It lists no
decoding gap. Computed targets remain unresolved. Its complete flag covers
local control flow under an explicit return-to-next-instruction assumption
for every call, not a complete reading of those callees or the consumer.

## Interpretation

This supplies a bounded dispatch and return-consumption contract for
FND-EXE-184's callback table. The signed index gate and full-width callback
result distinguish admission from the result returned to the consumer's
own callers. It does not identify the active provider, prove table-slot
initialization or stability, or establish native loop termination.
Q-EXE-001 and Q-EXE-010 retain root callers, every table and selector writer,
all callee effects, callback inputs, backing memory and lifetime. No formal
complete reading follows.

## Alternatives

A reading that the table consumer truncates the callback result to a byte,
or converts every nonzero result to one, is contradicted by its full-dword
test and unchanged return. A prior nonzero state test does not prove which
value the later decrement consumes across an unread callee. The two explicit
constant returns cannot replace the callback-return path's separate contract.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
Run the committed wrapper's `x86-bounds` command with sourceKind `pe32`,
entry file offset `0x00000BC0` (3008), and one named region start 3008,
exclusive end `0x00000C30` (3120), entries `[3008]`. Describe it as the
bounded table consumer with callee and target producers unresolved. Omit
segment/ip, seeds and callee summaries. The reader derives PE mapping from
section virtual start `0x00401000` and raw offset `0x400`. Preserve both
reached intervals, their seven-byte hole, unresolved computed transfers,
and every call-continuation assumption.

In the saved shipped-PE project, read-only with analysis disabled, run
ReportInstructionWindow at `0x004017C0`, count 26, and at `0x00401821`,
count eight. Exclude the neighboring routine beginning at `0x00401850`.
Run ReportReferences at `0x0075B460` only for positive consumer/writer
leads; its analyzer types and results are not an exhaustive access search.
Rich listings and reports stay in the local licensed-source store, outside Git.
