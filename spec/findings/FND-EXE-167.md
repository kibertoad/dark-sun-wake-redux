---
id: FND-EXE-167
title: Record setup initializes missing shared storage before mode-dependent link publication
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006008F0..0x0060097C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F50A0..0x005F50D6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5128..0x005F513B
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

The bounded record-setup path at `0x006008F0`, called by FND-EXE-032 and
later field/object findings, reads the pointer at `0x0242F640` and saves
its first original 32-bit argument as a record address. If the shared pointer
is zero it calls FND-EXE-043's initializer, rereads the shared pointer, and
reads its word at offset 48 without a local null check. A nonzero initial
pointer reads offset 48 directly.

A negative signed 32-bit mode calls `0x00600860`, then rereads the shared
pointer and mode. The callee's effects remain unread. A nonnegative initial
mode reaches another mode read without that call. Thus the branch does not
freeze one mode value across the entire sequence.

When the current mode is zero, the body reads the shared word at offset 40,
writes it as the supplied record's first 32-bit word, then stores that record
address at shared offset 40. It restores its frame normally. These two writes
are ordered; no ownership, rollback or concurrency semantics are inferred.
FND-EXE-043's fresh-record path initially writes all ones at offset 48, so
its ordinary first setup reaches the unread negative-mode helper before it
can select the direct-link path.

For a current nonzero mode, the body reads shared offset 44, calls
`0x00602490` without a new explicit outgoing argument and saves its return.
It calls `0x00602580` with the saved offset-44 word, saving that return,
then calls `0x00602440` with the first saved return. After normal completion
it writes the second saved return as the supplied record's first word.
It rereads the shared pointer and offset 44, calls `0x00602590` with that
fresh word and the supplied record address in the first two outgoing slots,
then tests the full returned word. A nonzero result branches to the earlier
normal frame-restoration sequence; the zero path's later continuation is not
established in this bounded finding. No field write is reversed before the test.
The external helper identities, input reads and effects remain unverified here.

For FND-EXE-166's particular callback caller, let B be the callback's
conventional frame address. FND-EXE-165 directly forms the supplied setup
record at B minus 108, while its sixth incoming four-byte slot starts at
B plus 28. These numeric four-byte intervals are disjoint: their starts differ by
136 bytes at 32-bit width, including when the address arithmetic wraps.
This is local frame geometry, not admission of any aliased external base.
The callback's direct frame-relative fields and incoming slot use SS; the
setup store through its supplied pointer uses DS. The numeric comparison
proves storage separation for that pointer store only under equal DS/SS
bases, as in the flat-address model. Segment identity is not established
by this arithmetic or by the analyzer's preferred-base addresses.

Before setup, the callback saves three four-byte registers, reserves 172
bytes and then twelve outgoing bytes, pushes the record address and calls
setup. With B as above, the setup entry stack pointer is B minus 204;
its frame is B minus 208 and its first argument at that frame plus eight
is the record-address slot at B minus 200. Its own three saved registers
and twelve reserved local bytes lie below that frame. On the direct
nonnull-shared-base, zero-mode path there is no intervening call before
the two publications and normal return. Those local frame writes cannot
by their fixed offsets overwrite the callback's incoming sixth slot.

The first publication on this path writes four bytes through the supplied
record address, at numeric B minus 108, so under that shared-base segment
model this specific direct store cannot be the writer of the incoming
slot at B plus 28. The second publication
writes the supplied record address through the selected shared base at
offset 40. That destination has no local relation or disjointness test
against the callback frame. Its four-byte interval must be kept separate
from the incoming slot until the shared-base producers and segment identities
establish whether they overlap. Under the same flat model, in the
conditional exact-alias case where shared base plus
40 equals B plus 28, this direct publication would replace the slot with
the callback's own record address. No evidence here admits that equality
on a shipped caller path; it is an unresolved condition, not an observed
corruption or a new supported state.

Other setup routes invoke initialization, mode admission or imported
helpers before publication. Their writes, returned storage, register
preservation and possible aliases require the contracts retained in
FND-EXE-043, FND-EXE-046, FND-EXE-047 and FND-EXE-048. The nonzero-mode
local record store likewise targets the supplied address only if the
intervening calls preserve it. Neither that conditional store nor local
frame separation proves the incoming slot survives every route. The
callback's later reload in FND-EXE-166 therefore remains necessary.

## Interpretation

This supplies a direct lazy-initialization caller and a bounded record-link
sequence for the construction findings. It does not establish record machinery
as complete, nor prove what the mode helper or external path does. Q-EXE-009
retains mode producers, external mappings, remaining continuation, cleanup,
callers and lifetime. Fresh initialization alone does not make the subsequent
negative-mode call disappear.

## Alternatives

This replaces FND-EXE-045. Its original half-open prefix location ended
inside the final two-byte conditional jump at `0x0060097A`; the correct
exclusive end is `0x0060097C`. The old observations are preserved with
that corrected instruction span. The added caller-relative comparison
narrows input preservation without establishing the shared-base alias
condition or a complete setup/callback reading.


Always using shared offset 40, skipping initialization on a zero pointer,
using one cached mode through the negative helper, or linking before the external
calls on the nonzero path are ruled out by the bounded instructions. Assigning
thread-local or exception semantics from the call shape alone is not justified.
The indirect-target initializer is not merely an assumed startup event: this
record-setup body calls it explicitly when shared storage is missing.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Read forty-five instructions
from `0x006008F0` and twelve from `0x00600965`, restricting claims to the
cited range and excluding later continuation. Follow the original record
argument, both shared-pointer and mode reads, initializer call, negative-mode
callee, zero-mode write order, each nonzero-path saved return, later pointer
reread, outgoing slots and full-width result test. Use FND-EXE-043 for fresh
initialization values. Keep unread helpers and zero-result continuation
conditional. Keep rich reports local and execute no interpreter or game.

For the caller comparison, read ninety instructions from `0x005F50A0`
and seventy-eight from `0x006008F0`, keeping only the three cited spans.
Check the final conditional jump through its last byte; the zero-result
tail beyond the cited prefix is already separate in FND-EXE-048. Trace
every four-byte push from the callback's B, record-address formation,
setup argument consumption, both direct zero-mode destinations and the
callback's later incoming-slot reload. Do not treat a shared pointer as
heap storage or disjoint stack storage without its producer evidence.
Keep DS-based pointer stores separate from SS-based frame slots unless
their segment-base identity is established; the numeric comparison alone
is conditional on the flat-address model.
