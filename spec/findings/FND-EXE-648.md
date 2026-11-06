---
id: FND-EXE-648
title: The slot-action window publishes allocation-derived returns in two stages and clears its pointer only after a returning close helper
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00042C00..0x00042C04
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00063450..0x000634CC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006355F..0x00063591
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00063752..0x0006379A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068914..0x00068946
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068978..0x00068A3F
tool: Pinned Python 3.12.10, Capstone distribution 5.0.9 (module 5.0.7), executable-reader 4.3.0; existing static/data readers at bf7e98aa and segment_displacement_writes.py --overlap at 23fa5697
environment: null
---

## Observation

This takes the concrete destination of FND-EXE-644's window callback
store, rather than assuming every unread helper can alias a header.
FND-EXE-604 retains the enclosing slot-action and event behavior.

**Initial slot and source census.** At modeled load segment 1000,
4DA0:0000 is file 42C00..42C04. Descriptor 101 has raw segment 3DA0,
flags zero and bounds zero through four. The shipped doubleword is zero;
no two-byte MZ relocation intersects these four bytes. This establishes
the unmodified loaded-image slot value, not its value at every call.

The relocation/fixup reference reporter lists 31 overlay references,
all in overlay 178, two resident reference operands at file 1EFE8 and
1F267, and the segment-table descriptor word at 4B3A8. These are
candidate contexts, not automatic instruction-boundary proofs. The
resident references test/read the slot in event routing, as retained in
FND-CONFIG-129. The descriptor word is data, not a decoded writer.

The writer reporter is used at every byte zero through three with
--overlap. Within its absolute ES and nearby literal-segment-load
mechanism, the union has five own store starts and one interior decode:

| Overlay 178 instruction | Own store |
| --- | --- |
| +040C | return segment DX into slot +2 |
| +0411 | return offset AX into slot +0 |
| +04D1 | return segment DX into slot +2 |
| +04D6 | return offset AX into slot +0 |
| +06D3 | zero doubleword into slot +0 |

The candidate at file 63784 starts inside the operand-size-prefixed
clear at 63783, rather than another instruction on that local path.
Both real publication pairs and the complete clear are positive controls
for the overlap scan. Legacy exact-start results are not an overlapping
write exclusion. Registers/offset aliases, remote segment loads,
strings, bulk copies and changed segment identities remain excluded.

**Acquisition and publication.** The +0040 routine checks the slot at
+03A5. Nonzero jumps to +066D and bypasses this acquisition block.
On its admitted new-window paths it calls overlay 182 +016B at +03FF
with WIND 15500 or at +04C4 with WIND 3020, position arguments and the
+06EA callback. FND-EXE-604 retains the state/slot branches that select
them. Each returning path stores DX at slot +2 before AX at slot +0,
then writes the same DX/AX words at current DS:1433/1431. No child call
intervenes between these four stores. There is no intervening returned
pointer null test. Normal execution of those own stores gives equal
slot/mirror pairs afterward, but not atomic publication or preservation
by asynchronous code or later children.

Overlay 182 +00C4 initializes its output local to zero and passes that
local's SS address, the zero-extended resource number and tag WIND to
38FF:04AB. It reloads that local as DX:AX without a conditional branch
on the reader's returned AX. FND-CONFIG-151 retains 04AB's zero-start,
complete-count request to 07B1. With the zero output and intact frame,
this selects allocation rather than reusing a supplied existing buffer.
The selected allocation's address/header/span domain is not established
merely by identifying this producer. FND-CONFIG-234 records how later
reader errors can still leave that allocation published.

Overlay 182 +016B stores the pointer returned by +00C4 in its local,
tests it for zero and passes the same saved pair through position/
registration, +F5 callback setup and activation. It returns that saved
pair when those local result checks succeed. A failing check enters the
shared fatal reporter and encodes a zero return only if that reporter
returns. FND-CONFIG-030/031, FND-EXE-587 and FND-CONFIG-234 retain the
child, guard and input conditions; none is an observed successful open.

**Cleanup and lifetime boundary.** Overlay 178 +06A2 tests the slot.
For nonzero P it calls overlay 182 +0128 with P and flag one. There
is no own pre-call clear of either slot; preservation by the callee
is not assumed. Only after the
helper returns does it clear current DS:1431, then 4DA0:0000. It does
not branch on the returned AX. For an initially zero slot it skips
both that helper and those two clears. Its own continuation resets
DS:08CA and calls 2C5F:0182; the latter's effects are not assumed absent.
Thus a zero slot does not itself prove that this body clears a stale
mirror.

FND-CONFIG-168 and FND-EXE-640 retain +0128's flag-one route: first
3A8E:08FB, then 3A8E:0A26 only after a zero first result, with a local
error helper on nonzero results. Removal changes its list/count before
some child failure paths and later requests releases; it is not a
transactional success test. FND-CONFIG-165/167 retain release results
that callers discard. The caller's later clear therefore proves neither
successful removal nor that the storage was already released. Child
callbacks/reentrancy and external effects before the caller's own
clear require their actual mechanisms and inputs.

## Interpretation

The concrete slot has named producer and clearing instructions; its
new-window path is allocation-derived, with explicit partial-publication
and close-before-clear intervals. Q-EXE-033 no longer needs an unnamed
producer caveat for this route. Its remaining relevant conditions are
the allocator/record address domain, concrete intervening writers or
callbacks, and admission of the saved frame/segment and close interval.
No header disjointness, complete lifetime or all-writer proof follows.

## Alternatives

- An admitted nonnull cached slot bypasses acquisition. Its prior history
  and storage lifetime still matter. The initial shipped zero is not a
  substitute for those histories.
- A partial publication can be observed by an admitted asynchronous or
  reentrant route. None was executed; merely naming the interval does
  not establish that such a reader runs there.
- Destination/header overlap requires the actual allocation and accepted
  record domain. It is not proved or disproved by a null test, callback
  value classification, or a returned pointer with ignored reader status.
- The narrow writer/reference censuses exclude computed/adjusted aliases,
  changed segments, strings and bulk writes. Reached callees' actual
  effects remain separate; their names alone do not prove aliasing.

## How to reproduce

Use the licensed installed DSUN.EXE, 634416 bytes and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, with the pinned readers. Run
segment_references.py with 4DA0. Run segment_displacement_writes.py
with 4DA0 and each displacement 0, 1, 2, 3, explicitly adding --overlap.
Keep every-byte candidates and all documented exclusions. Read overlay
178 ranges 63450..634CC, 6355F..63591 and 63752..6379A and overlay 182
ranges 68914..68946 and 68978..68A3F using overlay_listing.py.
Compare the acquisition, callback and cleanup interfaces with the cited
retained findings; do not extend into unrelated rendering.

Use report.mjs table for one four-byte unsigned field at file 42C00,
count one, stride four, with the exact source hash. The installed
readMz API resolves address(4DA0,0) to that offset; verify no relocation
word's interval intersects 42C00..42C04. Keep initial bytes and all
original-derived reports only in GAME_DIR.

Retained reports in GAME_DIR/analysis/ are window-4da0-segment-references-20261106.txt,
window-4da0-overlap-byte<0|1|2|3>-20261106.txt and
window-4da0-initial-config-20261106.json. No original-game run was made.
