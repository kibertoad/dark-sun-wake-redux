---
id: FND-PARTY-332
title: Reflected publication returns directly to the hit wrapper's first queue drain
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065591..0x000655E4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084C0F..0x00084C1C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084CCA..0x00084CD6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084E90..0x00084E94
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006517E..0x0006518D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000654B5..0x0006557F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00064FFD..0x00065008
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E32C..0x0007E33B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006500F..0x0006501D
tool: tools/research/exec-census/overlay_listing.py at 0db4c879 and 0f4e3da5; locked Python 3.12.10 with Capstone distribution 5.0.9; connected interfaces in FND-PARTY-288/328, FND-MAGIC-019/020 and FND-CONFIG-131
environment: null
---

## Observation

Q-PARTY-137 asks whether a reflected number survives publication through
the queue's selected handoff. This reading selects the known root through
overlay 179 +0C7F: it calls hit +0D2F, which calls overlay 197 +1760.
The reflection producer's +18D3 branch rejoins its shared enqueue call at
+180F (FND-MAGIC-020, FND-PARTY-288). Other hit callers and later drain
iterations are not equated with this root.

### Publication and direct return chain

For the admitted non-full enqueue path, 179+12B1 increments the queue
index, constructs the selected 4DA1 record, and stores its number word at
+4. It then reads its level byte and stores that at +0C before its ordinary
return. With index 0..5 and preserved record/frame binding, those two
payload fields are disjoint. FND-PARTY-328 retains index admission and the
full-queue branch, which cannot be treated as successful publication.

The shared reflection caller then discards its four argument words,
assigns AL0 and jumps from 197+1819 to +1A90. That epilogue only restores
DI, SI and its frame and far-returns. It does not continue through the
later reflection helpers at +18D6 and beyond. The hit caller at 179+0EA6
tests AL and, for zero, jumps from +0EAA directly to +11D5. Its epilogue
loads the separately saved result byte, restores DI/SI and returns.
This branch bypasses the sound and overlay-206 return-region calls
recorded in FND-CONFIG-131; they are not intervening game helpers on
this selected reflection-to-return path.

The immediate 179+0C7F wrapper continuation at +0D1D removes the hit
arguments and stores the returned byte at its SS:BP-1. It then performs
its known near/far-style local call to runner +11DC at +0D25. There is no
additional encoded game-helper call before the runner. Runner entry
reserves its local frame and saves SI/DI, tests signed DS:0915 against
-1, and, when the byte is nonnegative, constructs the record pointer and
loads its number word at +1206 into SS:BP-2 at +120A.

After successful publication and before this first read, the selected
game instructions do not explicitly update DS:0915 or issue another
queue-payload store. They perform returns, frame/stack operations and
the wrapper's saved-result/local-pointer stores. Those stores need actual
physical backing: an arbitrary SS or aliased return frame is not excluded
merely by calling them stack locals. Overlay residency/return machinery,
normal rejoin and asynchronous writers are also not established by a
source-level far-return list. No VECT/SPST/SAVE request or unrelated GUI
writer is an encoded synchronous game child in this selected interval.

### The first selected handoff

The runner reads the dispatch-kind word after saving the number. For
kind zero it pushes SS:BP-2 at +1255 and calls 193+003C. For kind one
it pushes the same SS:BP-2 at +1291 and calls hit +0D2F. Other kind
values skip both calls. There is no encoded child call between the
number load and either selected push (FND-PARTY-288).

The runner's own other local stores use BP-0A, BP-8, BP-6 and BP-4,
not its saved number at BP-2. Its outgoing pushes are below the allocated
frame. With the normal same-frame/return binding, those own writes do
not replace the saved word before either push. External or aliased writes
remain distinct admission conditions. The kind can choose a different
client or no client without changing which saved word either encoded
handoff uses. A universal zero-kind invariant is not required for this
local value-identity conclusion.

### Saved word, outgoing argument and actual transfer forms

Let B be the admitted runner BP. Its prologue allocates 0A bytes and
pushes SI/DI, leaving SP at B-0E before argument setup. The number
is saved at B-2. Stack offsets and outgoing byte counts below are hexadecimal; require the actual
same SS/BP, readable arguments and nonwrapping stack accesses.

| Dispatch kind | Outgoing bytes | SP F before transfer | Number word F+4 | Recipient BP after its initial PUSH BP |
|---|---|---|---|---|
| zero | 16 | B-24 | B-20 | B-2A |
| one | 1A | B-28 | B-24 | B-2E |

The kind-zero total includes its four-byte null pointer push. The
kind-one total includes both words of its far record pointer. Neither
argument block overlaps B-2 or the runner's other allocated locals.
From the number read through each number push, there is no encoded
call, interrupt or port instruction. Ordinary synchronous overlay
loading therefore is not an intervening writer in that prefix; a
changed transfer or asynchronous event needs its own evidence.

Kind one pushes CS then makes a near call at +1299 directly to hit
+0D2F in the same overlay. It does not transfer through a header
trampoline or synchronously request a callee overlay load. The hit
prologue saves BP, reserves its locals and saves SI/DI, then loads its
third word into SI at +0D3A. Under the stated stack conditions these
own writes are below the outgoing arguments and do not change that
number before the first load. Later hit-client effects remain separate.

Kind zero far-calls descriptor 193's trampoline +0084, entering +003C.
An already admitted jump-form trampoline introduces no handler path.
For the ordinary trap form, let H be the INT3F handler BP after its
initial PUSH BP. Far return and interrupt-frame construction give
H=F-0C=B-30. The nonzero trampoline offset selects FND-PARTY-333's
six-byte adjustment. After the documented exchange, the exposed first
walk node is H+6=B-2A, with its next BP link equal to runner B.
Its repair fields are H+8 (return offset) and H+0A (return segment).
The outgoing number is H+10=B-20: distinct from both fields.

If the subsequent runner/ancestor graph satisfies FND-PARTY-333's
upward-chain contract, writes at runner B+4 and later ancestor +4,
and the possible first-match +2 exchange, do not overlap B-2 or the
outgoing number below B. The synthetic first node's +2/+4 fields are
below the outgoing argument block. Thus the frame repair's own writes
have a concrete exclusion for the saved/outgoing number on this
admitted trampoline chain. This is not a claim about arbitrary linked
SS words, changed BP across loading or a chain with nodes in that block.

On successful rejoin, +003C's prologue establishes BP=B-2A, reserves
its locals and saves SI/DI, then reads its third word into DI at +0048.
There is no child call before that number read. Its own stack writes
are below the argument block under the nonwrapping-frame condition.
The argument at its BP+0A is exactly F+4. Native loader/copy effects,
actual backing, restored register/return bindings and asynchronous
writers still require evidence; balanced stack arithmetic is not their
proof. The argument is followed only to this first recipient number
load, not through the recipient's later use or return.

## Interpretation

For successful reflected publication through this root, the normal game-
code continuation goes directly to its first drain. This finite interval
removes unrelated synchronous hit-tail, GUI and resource-reader helpers
from that particular handoff audit. To insert those helpers requires a
different caller/continuation, re-entry, asynchronous transfer or changed
frame/routing, which must be identified rather than presumed from their
existence elsewhere in the program.

If the published index/record binding survives the return chain, frame
writes are disjoint from that record/index, and the return/overlay and
asynchronous conditions hold, the runner's first number read obtains the
published word. If kind zero or one selects a client, that read word is
forwarded under the saved-local conditions above. This is a conditional
connected contract, not an observed live reflection, a proof of all stack
backing or a whole-entry complete reading.

Remaining work is the actual backing and transfer admission for this
finite interval, other roots and later iterations. Kind history remains
material to client reachability under Q-PARTY-134, but establishing an
unchanging zero kind is unnecessary to show number preservation for both
encoded handoffs. Q-PARTY-137 remains open.

## Alternatives

- Reflection's zero return runs the normal hit tail before draining:
  contradicted by the direct branch to +11D5.
- Every global SPST/VECT/SAVE writer must run between this publication
  and first read: none is an encoded game child on this selected path.
  Other roots and asynchronous/reentrant execution are not excluded.
- Only a zero-kind record forwards the saved number: kind one forwards
  it too; other kinds perform no client handoff.
- A list of source returns proves live overlay/stack preservation:
  it omits the actual return frames, residency and asynchronous effects.

## How to reproduce

Use licensed DSUN.EXE size 634416, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`, and the locked evidence Python with
`tools/research/exec-census/overlay_listing.py`. Read descriptor 179
ranges `65591 655E4`, `6517E 6518D`, `654B5 6557F` and `64FFD 65008`;
read descriptor 197 ranges `84C0F 84C1C`, `84CCA 84CD6` and
`84E90 84E94`. Bounds are hexadecimal and half-open. The existing reader
checks source identity and resolves far-call fixups. Follow actual branch
targets, not fallthrough through the skipped tail or reflection suffix.

Reuse FND-MAGIC-019/020 and FND-CONFIG-131 for the known root and calls,
and FND-PARTY-288/328 for both handoffs and index/publication interfaces.
Keep successful publication, first read and selected push distinct. Check
both kind branches and the no-handoff branch, retaining actual stack,
index, residency and asynchronous conditions. No original game ran and no
original-derived listing or runtime state is committed.

For the two concrete handoffs, read descriptor 179 `654BC 65591`,
193 `7E32C 7E33B` and 179 `6500F 6501D` using overlay_listing.py
at 0f4e3da5. Count the four-byte null push and two-word record pointer
at their actual widths. Distinguish the local PUSH CS/near CALL from
the far trampoline call. Apply FND-PARTY-333's actual handler frame
layout and sufficient chain contract, retaining native and asynchronous
conditions. No original game or emulated call was run for this reading.
