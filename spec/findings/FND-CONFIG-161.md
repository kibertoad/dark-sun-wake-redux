---
id: FND-CONFIG-161
title: A shared helper clears three supplied pointer fields and polls before returning
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 576C:0039
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared FBOV fixup mapping
environment: null
---

## Observation

Descriptor 199's resident entry 576C:0039 selects code
0C21, file offset `0x00088521`. The complete local body
ends with far return at `0x00088674`, so its half-open
file span is `0x00088521..0x00088675`. FND-CONFIG-135's
resident callback and FND-SCRIPT-023's interpreter error
entry are known callers; this is not an exhaustive incoming
inventory.

The body stores zero at current DS:0DA4, calls local far
entry 0BC1 with a push-CS/near-call pair, and then stores
one at current DS:25BB. That local callee's effects and
DS preservation are not read here.

It next tests the double-word fields at 52A1:0008, 0000
and 0004, in that order. Each nonzero field is passed to
56BD:0043 with a word zero. When that call returns, the
field is set to zero. A zero field skips both its call
and its store. These segment operands are declared FBOV
fixups to descriptor 144. FND-CONFIG-128 and
FND-CONFIG-129 read consumers of these pointer fields.
Their object identities and the callee's complete effects
are not assigned by this helper.

A nonzero double word at current DS:6558 is passed to
444C:0092. Its returned DX:AX is stored back into that
field. A zero field skips this call. The body then calls
56BD:00BB, resident 3D72:0D83 and resident 2C5F:0182
unconditionally in that order. None of these calls is
replaced by an assumption about freeing, closing or
redrawing.

The following optional branch requires current DS:13F7
and DS:14E3 to be nonzero. Within it, nonzero DS:14E8
reads byte 4E71:0033, adds 41 to its zero-extended value,
and passes that word, DS:2623 and DS:43FB to 3150:000E.
It then calls 44DE:0086 with word one and DS:43FB.
A returned word other than FFFF is passed to 44DE:003A.
Zero DS:14E8 or a FFFF result bypasses that latter call.

Still within the outer optional branch, word 4C10:0019
selects word two when zero and word three otherwise,
which is passed to 56EF:00F2. A zero outer gate skips
this entire branch. These are local argument and call
contracts, not evidence that a file is opened, closed
or a video mode is changed successfully.

All paths then pass the same SS:BP-2 local far pointer
twice to 45B9:0034. They repeat this call while bit zero
of returned AX is set. Only a result with that bit clear
reaches the store of zero at current DS:25BC and the far
return. There is no own iteration bound or timer/input
instruction in this polling loop. The external callee's
return sequence determines whether this continuation is
reached.

The body does not establish that DS stays 57E0 across its
callees. Its DS-relative fields and arguments above mean
DS at the corresponding instruction. Under maintained
ordinary DS they correspond to 57E0; callee preservation
and field provenance remain separate conditions.

## Interpretation

This first helper of the interpreter error path has
multiple state-changing dependencies before returning.
Three fields are locally cleared after successful call
returns, a fourth takes a callee's returned pointer, and
a final poll can repeat. The subsequent diagnostic and
stop-byte writes in FND-SCRIPT-023 are therefore not
unconditional consequences of entering the error routine.

## Alternatives

FND-CONFIG-162 subsequently reads the first local callee and its two
polls. FND-CONFIG-163 bounds the callback setter's guard route. The
remaining external effects, input and return dependencies below still apply.

Q-CONFIG-008 and Q-SCRIPT-003 retain the local 0BC1
callee, external effects, DS preservation, pointer and
gate producers, and polling outcomes. One reading uses
valid state and a returned poll result with bit zero
clear; another reaches failed, changed or repeatedly
set return state. Complete callees and input provenance
would distinguish their reachability and effects.

The local body contains a far return, but that does not
rule out failure or non-return in an earlier callee or
poll. No native termination or visible diagnostic is
observed. Q-SCRIPT-007's resident-only harness cannot
execute this FBOV overlay or confirm its external outcomes.

## How to reproduce

Resolve descriptor 199, resident trampoline 0039 and code
0C21 using the declared FBOV header and fixup table. Read
0C21 through 0D74 from its entry, following every zero
pointer test, the nested three-byte gates, FFFF return
branch, word selector and common polling loop. Resolve
all declared segment operands before assigning field or
callee locations. List call returns and DS preservation
as conditions on later stores. Compare the two named
callers and the supplied-pointer consumers in
FND-CONFIG-128 and FND-CONFIG-129 without importing their
unread semantic identities.
