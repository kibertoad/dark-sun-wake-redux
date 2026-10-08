---
id: FND-EXE-096
title: PATH preparation helper computes replacement spans and publishes length after optional copy and release
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D69A0..0x006D6ACF
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D6AE6..0x006D6B6A
tool: Ghidra 12.1.3 PUBLIC bounded instruction reading
environment: null
---

## Observation

FND-EXE-093's initial output call and FND-EXE-095's replacement route
both reach `0x006D69A0` with four full-word inputs. Denote them object,
A, B and C without assigning unproved semantic types. The helper saves
the object at frame -96 and sets up a local record at -92, storing
callback `0x005F50A0`, metadata `0x006EE8B3` and handler `0x006D6AD0`,
through `0x006008F0` (FND-EXE-167). The handler is undisassembled in
the current bounded report and is excluded; native frame admission and
exceptional behavior remain unresolved.

After normal setup it reloads object and payload P, reads full L at P
minus twelve, and computes T = L + C - B and U = L - A - B, all modulo
32 bits. T and U are saved separately. It reads full capacity-like word
at P minus eight and compares it unsigned with T. A value below T selects
the new-prefix route. Otherwise it reads signed word at P minus four;
positive also selects that route, while zero or negative selects in-place
handling. These observed field positions and tests do not independently
establish ownership, actual capacity or a valid input range for A and B.

The new-prefix route writes one to record state -88 and calls
`0x006D50A0` with T, the preceding capacity-like word, address of local
-40 and old prefix P minus twelve in four outgoing slots. Its allocation
and result contract remain unread here. The raw returned word N is saved
at local -108, without a local null/result guard. If freshly reloaded A
is nonzero, memcpy receives N plus twelve, the object's freshly read
payload and A, plus an auxiliary saved-register word. Its return is not
admitted; normal completion proceeds to the separate saved U test.

If U is nonzero, the helper freshly reads object/payload and A, B and C.
It supplies destination N + 12 + A + C, source current payload + A + B,
and U to memcpy. Before that call it replaces its own incoming A slot
with current payload plus A; this is an actual full-word frame store,
not proof that the original caller's A remains intact. Arithmetic is
32-bit throughout. The copy result is ignored before the old-payload
release route. U zero skips this tail copy.

That route freshly reloads old payload and saves old prefix at local
-112. Equality to `0x0071B270` bypasses decrement and release. Otherwise
it supplies old payload minus four and addend all ones to `0x005F5760`
after writing one to state -88. FND-EXE-033 proves the returned word is
the old exchange-add value. Signed old value greater than zero bypasses
release; zero or negative supplies saved prefix to `0x006D4DF0`, with
local -40 in the next slot and two old-value auxiliary slots.
FND-EXE-040 records that release helper's first-argument consumption.
On normal completion all new-prefix routes reload N, add twelve and
store that payload pointer through the freshly reloaded object before
joining common publication. No local rollback reverses prior copies or
the old-word decrement.

In-place handling selects memmove only when saved U is nonzero and B
differs from C. Its first three outgoing slots are current payload + A + C,
current payload + A + B, and U. Otherwise it skips the move. The move's
return is not tested. FND-EXE-021/034 identify the actual memmove/memcpy
slots used in these paths; prepared arguments do not prove CRT execution
or an alias-safe full object contract.

Common publication freshly reloads object and its current payload, writes
full zero at payload minus four, saved T at payload minus twelve, and
one terminating zero byte at payload plus T, in that order. It then cleans
the local record through `0x00600990` (FND-EXE-049) and returns that
helper's raw EAX after normal frame restoration. There is no final reload
of the original object pointer as a normalized return and no local T-bound
check before the terminator. The record helper's effects remain separate.

FND-EXE-093 supplies A zero, B captured from its output payload's preceding
length before this setup, and C zero. If the freshly read L is unchanged
from that captured B, T and U are both zero. Under that invariant the
in-place route skips memmove but still publishes zero length, preceding
word and terminator; the new-prefix route skips both copies but still
decrements/releases as admitted and publishes its new pointer before
the same zero stores. This is conditional on that exact length invariant,
not a universal empty-output claim: intervening setup writes or aliases
can change L and saved inputs.

## Interpretation

The helper separates requested resulting span T from retained tail span U,
and publishes pointer/length state after distinct new-prefix or in-place
routes. Its caller's ignored return does not remove these earlier writes.
Q-EXE-009 retains the new-prefix producer, stored handler, original field
writers, input ranges, object aliases, releases and native helper outcomes.
The conditional zero-input PATH specialization does not establish that
every failed lookup leaves an empty string or that allocation is transactional.

## Alternatives

Conflating T and U, comparing capacity signed, testing the updated decrement
value, publishing a new pointer before old-prefix release, skipping the
in-place terminator store, or returning the saved object pointer is ruled
out locally. A and B are not independently bounded by the subtraction;
wrapped U can be nonzero. Captured caller length is not automatically the
fresh length read after record setup.

## How to reproduce

Verify FND-EXE-011's identity and the callers in FND-EXE-093/095. Read
eighty-five instructions at `006D69A0`, ninety at `006D6A43`, five at
`006D6A3E` and twenty-two at `006D6B53`, restricting to the cited ordinary
regions. The attempted `006D6B4F` window is inside the six-byte conditional
jump at `006D6B4D`, not an independent instruction start; use its actual
continuation `006D6B53`. The `006D6AD0` query reports missing disassembly,
not absent executable bytes or a proved unreachable handler. Read two at
each copy/move thunk and use the cited exact import findings. Trace saved
T/U/N, fresh inputs, incoming-slot mutation, old-value release admission,
pointer publication, final stores and raw cleanup return. Keep reports local,
assume no complete allocation or handler contract, and execute no original.
