---
id: FND-EXE-075
title: Pool initialization publishes an all-ones counter and unchecked semaphore return before once completion
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FCD50..0x005FCD65
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601970..0x00601996
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006025B0..0x006025B6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00358F7A..0x00358F8A
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00319D70..0x00319D77
tool: Ghidra 12.1.3 PUBLIC and bounded physical PE import-table and initialization-word reading
environment: null
---

## Observation

FND-EXE-025's malloc-fallback path conditionally supplies flag address
`0x0071B170` and callback `0x005FCD50` to FND-EXE-068's shared once wrapper
at `0x00601840`. The condition uses shared word `0x0242C910`; this finding
does not establish that word's producer or the actual runtime admission.
The two callback-related addresses are nonzero, so the wrapper's direct
null-argument rejection is not this caller's supplied-argument case.

The callback at `0x005FCD50` makes a conventional frame, reserves twenty
bytes and pushes `0x02427E50` as one full-word argument to `0x00601970`.
After normal return it removes sixteen outgoing bytes, restores its frame
and returns with the callee's full return register unchanged. It does not
test or convert that result, write the once flag itself, or pass a second
explicit argument. The unfilled outgoing reserve is not additional input
consumed by the initializer's local body.

The initializer reads its first original full-word argument as its input
address and directly writes all ones to the full word at that address.
This store precedes every local import call. There is no null, current-count,
existing-handle or ownership test before it. It then pushes four words,
in temporal order: zero, 65535, zero, zero. The callee-facing order is:

| Slot | Value |
|---|---|
| First | zero |
| Second | zero |
| Third | 65535 |
| Fourth | zero |

It calls thunk `0x006025B0`, whose only instruction jumps through PE import
slot `0x024317F4`. Physical descriptor and lookup-thunk reading identifies
KERNEL32.dll's CreateSemaphoreA at the cited shipped-file name range,
including its terminating NUL. FND-EXE-074's increment, decrement, wait and
release imports and FND-EXE-024's malloc/free imports provide known positive
controls for that same mapping procedure. The slot is read from this thunk,
not guessed from the names or positions of neighboring imports.

Under that imported 32-bit API calling contract the four supplied values
request null security attributes, initial count zero, maximum count 65535
and null name. The caller requests these values; no API was executed and
no successful creation, actual OS object or handle identity is observed.
After normal return, the initializer directly stores the complete import
return into the word at input plus four. It has no intervening result test,
error query, retry or finalization branch. Zero is stored just as a nonzero
word is. The prior all-ones counter store is not locally reversed on zero.
It restores its saved register and frame and returns the unchanged full
creation result. It does not read, close or preserve a previous handle,
clear the pool bitmap, or initialize pool payloads in this body.

For this callback's supplied input, the two ordered writes are therefore
all ones at `0x02427E50`, then the creation result at `0x02427E54`.
These are exactly the first-word import input and input-plus-four local
read of FND-EXE-074's wrappers. That connection establishes a direct producer
path, conditional on the callback being admitted and completing normally;
it is not a claim that every later wrapper call sees this state unchanged.

### Shipped once words and completion ordering

Physical PE section mapping places `0x0071B170..0x0071B177` at shipped-file
offsets `0x00319D70..0x00319D77`. Two little-endian four-byte reads show:

| Preferred-image address | Shipped initialization word |
|---|---|
| `0x0071B170` | zero |
| `0x0071B174` | `0xFFFFFFFF` |

They are physically backed values in this verified file, not inferred zeros
from an unmapped or virtual-only range. The addresses match this caller's
flag address and its plus-four increment input in FND-EXE-068. Prior writes,
loader/runtime effects and concurrent users can change them; they are not
a memory observation at first fallback admission.

FND-EXE-068's once wrapper tests the flag, requests InterlockedIncrement
on the adjacent word only when the flag is zero, and calls the callback
only when that full increment result is zero. Thus, if the admitted storage
still has these file initialization values and the increment import follows
its ordinary counter contract, the increment from all ones supplies the
wrapper's zero-result callback branch. This is a conditional composition,
not an executed first-call trace or proof of no preceding writers.

After normal callback return the once wrapper discards its return, writes
one to the flag and returns zero. The callback retains the creation return,
so a zero CreateSemaphoreA return still reaches that same completion store
if every intervening call returns normally. Consequently the pool input
counter and handle-result stores precede the once completion flag; a
completion value of one is not itself evidence that creation succeeded.
The fallback caller then rereads its separate shared guard before deciding
whether to call FND-EXE-074's first wrapper. Its later bitmap operations
have no direct callback-result test.

Local controls distinguish creation result zero from one: both locally write the
counter as all ones and store the respective result in input plus four,
with unchanged return through the callback and the same once-completion
store on normal return. A nonzero once flag bypasses this callback in the
shared wrapper, and a nonzero increment return takes its polling path
instead. These are branch/composition controls, not OS observations or
proof that the real initializer runs once, succeeds, or cannot be repeated.

## Interpretation

The pool's previously unread initializer now has a direct counter writer,
exact creation import and argument order, unchecked handle-result writer,
raw return and callback-to-once completion contract. The shipped once words
support one conditional admission path. Q-EXE-009 retains the shared guard
producer, other writes and lifetime, import errors/interleaving, stored
handlers and concrete callbacks. No complete synchronized pool lifecycle,
successful creation claim or shell outcome follows from this reading.

## Alternatives

Initializing the counter to zero, writing the creation result before the
counter, skipping its store on zero, retrying failed creation, marking once
completion only for a nonzero callback result, or treating the callback's
reserve as extra initializer parameters is ruled out. A completion flag may
indicate normal callback completion without successful semaphore creation;
its success meaning cannot be strengthened by the API name or the shipped
initialization words alone.

## How to reproduce

Use FND-EXE-011's verified source length, XXH3 identity and preferred PE image
base. Read fourteen instructions from `0x005FCD50` and twenty-two from
`0x00601970`, restricting claims to their cited bodies and excluding following
functions. Read one instruction from `0x006025B0` and resolve its actual
slot `0x024317F4` through physical import descriptors and lookup thunks.
Require descriptor termination inside the declared import-directory size,
thunk termination within 4096 entries, physical section mapping and names
terminated within 128 bytes. Check positive slots `0x024318AC`,
`0x02431854`, `0x02431868`, `0x02431858`, `0x024319D0`, `0x024319A0`
against FND-EXE-024/074. Map exactly the two four-byte words at
`0x0071B170` and `0x0071B174` to physical section bytes and read them
little-endian, rejecting absent mappings or reads beyond the file.
Use FND-EXE-025 for the actual flag/callback and later guard reread,
FND-EXE-068 for once admission/completion, and FND-EXE-074 for pool-input
consumption. Track last argument writers, write order, full return preservation,
frame cleanup, dropped callback result and conditional file-state admission.
Keep reports local and execute no original program or API.
