---
id: FND-EXE-069
title: Context initialization derives its guard from a converted allocation result without always writing the index
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FD1F0..0x005FD218
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006018B0..0x006018DE
tool: Ghidra 12.1.3 PUBLIC, bounded instruction reading
environment: null
---

## Observation

FND-EXE-068's initialization wrapper prepares `0x005FD1F0` as its indirect
callback target. This callback creates a conventional frame, reserves sixteen
stack bytes, and pushes two full-word values in callee-facing order:
`0x0242BE60` and `0x005FD1B0`. It calls `0x006018B0`, removes sixteen bytes
of outgoing space, tests the full returned word for zero, writes the resulting
zero/one predicate to the low accumulator byte, then masks the full word to
255 before storing it at `0x0071B180`. The mask establishes all upper bytes:
wrapper return zero stores full-word one; any nonzero return stores full-word
zero. It then restores its frame and returns that same predicate word.

The receiving wrapper reserves eight bytes before calling FND-EXE-047's
exact TlsAlloc import. It saves the full return and compares it with all ones
at 32-bit width. Every other return, including zero, loads its first original
stack argument as a destination address and stores the returned index there
at full width. There is no local destination-null check or prior-index read
or release. This store precedes the next helper call.

It prepares four full outgoing words for `0x00602630`: the saved index,
its second original stack argument, and two copies of the destination address.
FND-EXE-047 establishes that this direct local body returns a full zero,
reads no argument, and makes no further call or memory access beyond its
frame. Thus the callback-shaped value `0x005FD1B0` is prepared but is not
invoked or registered by this local callee. The wrapper removes sixteen
outgoing bytes, restores its frame and returns the callee's zero unchanged.
Under ordinary calls, that path writes the index before the callback stores
guard one.

An all-ones TlsAlloc return skips the index destination load and store and
skips the zero helper. It calls FND-EXE-047's exact GetLastError import,
restores its frame and returns that full word unchanged. Consequently the
outer callback's resulting guard has these branch-specific writers:

| Allocation return | Later local return source | Index store | Guard |
|---|---|---|---|
| Any word except all ones | Constant-zero helper | Store allocation return | One |
| All ones, error query zero | Zero error word | No direct store | One |
| All ones, error query nonzero | Nonzero error word | No direct store | Zero |

These are control-flow and full-width return contracts, not claims that the
external APIs produce all listed combinations during a particular run. A
zero error word in the all-ones branch can set guard one while retaining
whatever previously occupied the index destination. Guard one therefore
cannot prove that this invocation wrote a new index. Guard zero likewise
says nothing about prior index validity or its lifetime.

The direct callback supplies the same index-storage address that
FND-EXE-068's lookup and setter later read. After this callback returns
normally, that wrapper independently stores one to its completion flag,
discarding the callback's predicate return. Its completion flag and the
context guard have distinct writers and can differ. The provider then
rereads the context guard as recorded in FND-EXE-068. No local rollback or
synchronization proof makes these stores one atomic publication.

This callback reads no original stack argument and performs no direct
context-head/counter initialization. FND-EXE-068's two allocation zero stores
remain a separate later path. Prior index/guard/flag values, callback target
replacement, imported effects and interleaving remain conditional; no
original process or API was executed.

## Interpretation

The previously unread initialization callback now has a direct index writer,
converted guard writer and two normal-return sources. The prepared callback
value is not evidence of registration by the studied zero helper. Q-EXE-009
retains initial/static storage producers, index lifetime, imported effects,
interleaving and stored-handler admission. No universal successful allocation,
initialized context or thread-local lifecycle is established or implemented.

## Alternatives

Always writing the index, treating all ones as zero guard unconditionally,
storing a raw allocation result as guard, interpreting an unmasked byte as a
full predicate, calling the prepared callback value, or equating completion
flag one with a newly written valid index is ruled out by the local bodies.
An error-query zero is a control-flow input, not proof of successful allocation.

## How to reproduce

Use FND-EXE-011's length and XXH3-verified PE and image base. Read seventy
instructions from `0x005FD1F0` and twenty-three from `0x006018B0`, restricting
claims to the two cited bodies and excluding later functions. Use FND-EXE-047
for exact TlsAlloc/GetLastError bindings and the zero helper's argument-free
contract, and FND-EXE-068 for callback preparation, completion-flag publication
and subsequent guard/index consumers. Track all-ones comparison, index store
before the ignored outgoing values, full return sources, byte predicate and
upper-byte mask, retained index on both error-query branches and the separate
completion flag. Keep prior storage, imports, lifetime, aliases and interleaving
conditional. Keep rich reports local and execute no interpreter or game.
