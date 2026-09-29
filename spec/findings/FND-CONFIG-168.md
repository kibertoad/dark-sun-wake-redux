---
id: FND-CONFIG-168
title: A pointer consumer changes list state before a child failure and its caller ignores the result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0043
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0A26
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:022D
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared relocation/fixup mapping
environment: null
---

## Observation

FND-CONFIG-161 passes each nonzero field at 52A1:0008,
0000 and 0004 to overlay entry 56BD:0043 with a word
zero. Descriptor 182 maps that trampoline to code 0128,
file span `0x00068978..0x000689BB`, ending with far
return at `0x000689BA`. A null pointer returns zero
without either resident consumer. For a nonnull pointer,
the low byte of the second argument selects an optional
first call: nonzero calls 3A8E:08FB, and its nonzero AX
branches to local error helper 022D. A zero result from
that first call continues to 3A8E:0A26. A zero selector
bypasses 08FB and calls 0A26 directly. Thus all three
named calls use 0A26, without first using 08FB.

After 0A26, nonzero AX calls the same local error helper;
zero bypasses it. The consumer result is retained in SI
before that helper and copied to AX on return. This is
not a preservation guarantee across the helper's external
callee. Local 022D occupies `0x00068A7D..0x00068A93`:
it passes words 4542 and 4592 to 39D1:0460, then loads
AX from current DS:4592. The complete external effects,
SI preservation and return of 39D1:0460 remain open.
The caller in FND-CONFIG-161 does not branch on returned
AX: it clears each supplied field when 56BD:0043 returns.

The complete local 3A8E:0A26 body occupies
`0x00030506..0x000306F3`. It has the stack-limit guard
through 1000:2E48 before reading its pointer. A null
pointer returns FFFF. A nonnull pointer with bit 4000
set in its word at offset 9E first passes its word at
A0 to 1BF3:28C5. The callee's effects are not assigned
by this reading.

The body compares the target with current DS:A105. A
match replaces DS:A105 with the target's far link at EE.
Otherwise, DS:A103 equal to one skips its list search.
All other counts search from DS:A105, following far
links at EE until a pointer equals the target, then
replace the previous record's link with the target's
link. The search has no own null, membership, cycle or
iteration check before dereferencing the next pointer.
All these paths decrement word DS:A103 with word wrap,
then call local far 3A8E:0003 before the child scan.
Valid membership and count invariants are separate inputs;
the count-one bypass does not itself validate the target.

The child scan starts with index zero and re-reads the
unsigned word at target+F3 at each comparison. A count
at most the index ends the scan. Otherwise its child
record uses the target segment and this wrapped word
offset: target offset plus zero-extended byte at F2,
plus 30 times the index, plus 0105. The tag is the double
word at child+4; the child object's pointer is at child+0.
The four-entry tag table at file
`0x000306F3..0x00030703` and target table at
`0x00030703..0x0003070B` resolve the computed branches:

| Tag | Local target | Calls before the common continuation |
|---|---|---|
| APFM | 0B5C | 3CFA:0544 with the object's far address at offset +0C. |
| BUTN | 0B28 | 3CFA:0544 with object+0C; nonzero object+68 is then passed to 444C:0092. |
| MENU | 0B12 | 3BA6:0EC2 with the object pointer; nonzero AX returns FFFF immediately. |
| EBOX | 0B78 | 409B:0AFB with the object pointer. |

A continuing recognized branch, and an unrecognized tag,
both pass the child object pointer to 444C:0092 and then
increment the word index. Offset additions above do not
carry into the supplied segment. The scan's index/count
progress depends on callees preserving the relevant state;
the body has no independent iteration limit.

After the scan, it passes target+0A2 to 3CFA:0544 and the
target itself to 444C:0092. It clears current DS:A11D and
DS:A0FD separately if each equals the target. It always
clears DS:A17F and A183 on this continuation. Equality
of DS:A121 with the target additionally clears A129,
A125 and A121. It then returns zero. These later stores
are bypassed by the MENU nonzero-result return. That
failure path does not locally restore the earlier link
and count changes or already processed children.

All resident far-call segments above were verified at
their declared MZ relocation operands. The overlay's
consumer and error-helper segments were verified through
descriptors 40 and 39 respectively. DS-relative fields
mean current DS at the corresponding instruction; the
complete callees' preservation is not assumed.

## Interpretation

The named caller's field clear occurs after a returning
consumer, without a success test. The consumer can return
FFFF after prior list/count changes and before its later
pointer clears. The outer error helper adds another
return dependency. This bounds the local ordering, not
successful release, valid graph membership or native
reachability of a failing child. FND-CONFIG-165 and
FND-CONFIG-167 separately bound 444C:0092's returned zero
and the runtime result it discards.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain valid graph, links,
count and object provenance, aliases, child-tag inputs,
all callers, local 0003, external callees, DS/SI
preservation and return outcomes. One reading supplies
valid members with continuing child results; another
reaches changed state or a nonzero MENU result. The
local paths distinguish their ordering but do not prove
which ordinary input reaches them. The optional 08FB
consumer also remains unread for other selector values.

A returned field clear could accompany accepted runtime
operations, an ignored failure, or different effects
inside the error helper. Complete producer/callee readings
are needed to distinguish them. No native or emulated
result, visible dismissal, diagnostic or termination is
claimed. Q-SCRIPT-007 cannot execute the overlay wrapper.

## How to reproduce

Resolve descriptor 182's 0043 trampoline, code 0128 and
its declared segment fixups. Read 0128 through 016A and
022D through 0242. Follow both low-byte selector values
and both resident result tests; compare the zero selector
and ignored AX at the three calls in FND-CONFIG-161.
Read resident 0A26 through 0C12 from its entry. Follow
the head/count-one/list-search alternatives, count write,
local 0003 call, all child tags and the MENU failure edge.
Resolve the computed dispatch from both adjacent tables,
not their order alone. Check unsigned loop comparisons,
word offset arithmetic and which later stores failure
bypasses. Keep unknown callees and native outcomes separate.
