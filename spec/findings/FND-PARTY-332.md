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
tool: tools/research/exec-census/overlay_listing.py at 0db4c879; locked Python 3.12.10 with Capstone distribution 5.0.9; connected interfaces in FND-PARTY-288/328, FND-MAGIC-019/020 and FND-CONFIG-131
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
