---
id: FND-EXE-025
title: Allocation failure-object storage has an eighty-byte prefix and a bitmap fallback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FCD70..0x005FCE3D
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FCE40..0x005FCEC4
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FAED0..0x005FAF38
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601CE0..0x00601CE6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x003595F2..0x003595F9
tool: Ghidra 12.1.3 PUBLIC and bounded physical PE import-table reading
environment: null
---

## Observation

FND-EXE-024's allocation failure branch passes four to `0x005FCD70`.
That callee first builds a local record and calls `0x006008F0` with its
address. It then adds 80 to its 32-bit request argument, overwrites the
argument slot with that result, and passes it to the malloc thunk identified
in FND-EXE-024. This addition has no local overflow rejection. A nonzero
returned pointer follows the initialization path described below; a zero
result enters a fallback path.

The fallback path first reads shared word `0x0242C910`. If nonzero it calls
`0x00601840` with `0x0071B170`, `0x005FCD50` and that shared value twice,
then rereads the shared word. If still nonzero it calls `0x006019A0` with
`0x02427E50`. A zero initial or reread value skips the corresponding call.
Only after those normal returns does it read bitmap word `0x02427E40`.
These callees, the shared word's producer and the object at the supplied
address remain unread; no synchronization semantics are assigned to them.

The size guard compares the already augmented request unsigned with 512.
A larger value bypasses the bitmap search. Otherwise a counter starts at
zero and a local bitmap copy is tested at its low bit. A set bit shifts the
local copy right logically and increments the counter; unsigned counters
through 31 inclusive are tested. The first zero bit selects a fallback
pointer at `0x02427E60` plus counter times 512. Before returning to the common
fallback continuation it rereads the shared bitmap, ORs in one shifted by
the counter, publishes that bitmap and saves the selected pointer locally.
It does not clear a bit in this direct body. If all 32 tested bits are set,
the pointer remains the zero malloc result.

The common continuation rereads `0x0242C910` and, if nonzero, calls
`0x006019F0` with `0x02427E50` before testing the saved pointer. A nonzero
saved pointer joins initialization; a zero one calls `0x005FD120` after a
local record-state write. The analyzer marks that call without fall-through.
This callee and the separately stored handler target are not read here,
so this is not a proven returning-null, throw or termination description.
The selected bitmap bit is published before that continuation call and the
initialization calls; their possible mutations and exceptional exits remain
conditional rather than transactional.

On the initialization path, the callee invokes the thunk at `0x00601CE0`
with the selected pointer, zero and 80. That thunk's import slot is
`0x024319E4`; bounded physical PE import reading identifies memset from
msvcrt.dll, with the cited shipped-file name range including NUL. It then
forms selected pointer plus 80 at 32-bit width, saves that value, calls
`0x00600990` with its local record address, reloads the saved value and
returns normally. This establishes the prefix initialization request and
returned pointer adjustment, conditional on the CRT and record helpers.
It does not prove the complete allocation lifetime or fallback area's extent.

The following consumer at `0x005FAED0` reads its pointer and two additional
32-bit arguments. It writes the latter into the prefix at pointer minus
80 and minus 76. It also writes two 32-bit marker values, `0x432B2B00` and
`0x474E5543`, at pointer minus 32 and minus 28; a stored target
`0x005FAE80` at pointer minus 24; and values read through shared pointers
`0x0242F650` and `0x0242F630` at pointer minus 72 and minus 68.
It calls `0x005FD2F0` and increments the 32-bit word at returned pointer
plus four. It then calls `0x00600B50` and `0x005FABA0` in order with the
original pointer minus 32, before calling `0x005FD120`. These callees and
shared-pointer producers remain unread. Neither the marker values nor the
analyzer's non-return annotation alone establishes exception semantics.

## Interpretation

The failure object's four-byte request normally becomes an 84-byte malloc
request with an 80-byte prefix, or a bitmap-selected 512-byte fallback block,
and returns the pointer after that prefix. This explains why the next
consumer can write before the supplied object pointer without treating the
object's four-byte request as the whole storage contract. It does not extend
this header contract to the record-growth allocation in FND-EXE-023, which
uses the distinct wrapper in FND-EXE-024. Q-EXE-009 still needs exceptional
callee behavior, bitmap lifetime, shared state and caller invariants.

## Alternatives

Testing the unaugmented size against 512, inspecting only 31 bitmap bits,
returning the malloc base unchanged, clearing the chosen bit in this direct
body or initializing the prefix before selecting fallback storage are ruled
out by the bounded instructions. A lock-protected pool and an exception
record are possible readings of the external helpers and marker values,
not established semantic names or complete outcomes. Exhausted fallback
storage reaches the unresolved final callee; what that callee does remains
open under Q-EXE-009.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x005FCD70` and
`0x005FAED0`. Read 45 instructions from `0x005FCD70`, 55 from `0x005FCDE9`,
eight from `0x005FCEA8` and 60 from `0x005FAED0`. Restrict every claim to
the cited bodies, excluding following functions and any inferred continuation
past the no-fall-through calls. Read one instruction from `0x00601CE0` and
verify its slot through bounded physical PE import descriptors and lookup
thunks. Track the augmented argument slot through the unsigned guard,
bitmap shifts and counter bound, shared-word rereads, publication before
calls, prefix initialization and saved return after record cleanup. Follow
the consumer's pointer adjustments and the extra arguments into their exact
prefix fields. Treat every unread callee as a conditional boundary. Keep
rich reports local and execute no interpreter or game.
