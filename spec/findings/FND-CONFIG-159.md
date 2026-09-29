---
id: FND-CONFIG-159
title: The nonzero-mode pre-setup helper stores a relocated video-reset pointer and retains external dependencies
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0057
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 49DE:00A7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0048
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared FBOV fixup mapping
environment: null
---

## Observation

FND-CONFIG-149's nonzero DS:13F7 branch calls overlay
180 entry 56B2:0057. Its declared trampoline resolves to
code target 0026, file offset `0x00067206`. The complete
local body ends with far return at `0x000672F0`. It
rechecks DS:13F7 and returns immediately when zero.
FND-SOUND-011 records this body's sound setup context;
this finding bounds its local branches and pointer store
for the archive-state question.

On its nonzero branch it calls 4734:0063 and retains the
returned far pointer in its local stack buffer. FND-CONFIG-005
identifies that callee's SOUND.CFG acquisition. A zero
pointer calls the local termination helper in FND-CONFIG-062;
if that helper returns, this body continues with the retained
zero pointer rather than returning success or failure here.

It calls 4AAB:0008 with first double-word argument 25000
and second double-word argument 12000 when DS:14E3 is
zero, or zero when it is nonzero. It then calls 49D2:0006
and passes far pointer 56B2:0048 to 49DE:00A7. The
pushed segment operand at `0x00067256` is a declared
fixup: raw 05A0 names descriptor 180, mapped 56B2.
It is a segment operand, not an additional numeric input.
The call's operand at `0x0006725E` maps descriptor 77
raw 0268 to resident 49DE.

The complete setter 49DE:00A7,
`0x0003F087..0x0003F094`, copies its double-word input
to DS:3475 and far-returns. It makes no call and has no
other direct store. Thus the concrete pointer field at
this site is 56B2:0048, assuming the shared DS convention.
When or whether another routine invokes that pointer is
not established by the store alone.

Trampoline 0048 targets code offset zero, file offset
`0x000671E0`. Its complete body through far return at
`0x00067205` compares word 4E71:0001 with FFFF. Equal
returns without a call. Different calls 1BF3:2973 with
word three, then writes FFFF to 4E71:0001 if that call
returns. Its segment loads and call are declared fixups,
resolved to 4E71 and 1BF3. FND-VIDEO-003 identifies the
mode-three request; FND-CONFIG-158 reads the setter's
local stores and external BIOS/port dependencies. This
pointer target contains no direct archive operation or
DS:193E write.

The helper next branches again on DS:14E3. Nonzero calls
2660:01C4, then 4734:0128 with word zero; zero calls
4734:0128 with word one. Both paths call 2834:043B and
join before the zero-result termination-helper call or
its bypass. FND-SOUND-011 reads that callee's DJ.DAT
acquisition separately. A returning termination helper
again permits the remaining local path.

It passes the retained SOUND.CFG pointer to 4734:00B0,
then calls 4602:000F when DS:14E3 is zero. Next, only
when both DS:13F7 and DS:1435 are nonzero, it passes
word zero and a word whose low byte comes from DS:26B5
to 45E1:0097, and calls 49E9:0142 when the AL result
of 49E9:00FD is nonzero. The local code does not clear
AH before pushing the DS:26B5 input; this observation
does not assign the receiving callee's parameter width.
Finally, a zero DS:14E3 calls 4A32:0011 with word two.
All local routes converge at the return. The unprefixed
98 instructions at `0x000672AB` and `0x000672E2`
sign-extend AL to AX; the decoder's CWDE label must not
be read as a 32-bit conversion for these bytes.

This helper's own memory writes are to its local stack.
Its read gates can be changed by intervening callees, so
repeated tests of DS:13F7 or DS:14E3 are not replaced
with one cached branch decision. Apart from the bounded
pointer setter and pointer target, external callee effects
remain open. No complete startup invariant is claimed.

## Interpretation

The nonzero pre-setup branch has a concrete stored function
pointer and verified local target. The pointer store is
neither an invocation nor evidence of an archive registration.
The helper's local code supplies no direct setup-gate or
FNFO producer, while its unread external callees remain
possible state-changing dependencies. Local failure calls
also cannot be silently treated as ordinary returns or as
observed process termination.

## Alternatives

Q-CONFIG-008 retains the external callees' effects, pointer
consumers, later replacement and actual branch inputs.
One reading keeps archive and gate state unchanged through
this sound setup; another changes them through an external
callee or later pointer invocation. The local reading does
not choose between those transitive states. An external
callee could also change a field between its repeated tests.
The bounded setter and target remove their own direct
archive-write candidates, not all possible effects of the
larger call sequence.

## How to reproduce

Validate overlay 180's header, trampoline, code and fixup
bounds. Resolve trampoline 0057, read from its target to
its sole return and follow each conditional branch. Resolve
all declared call segments and the pushed pointer segment.
Read 49DE:00A7 through its return, then resolve trampoline
0048 and read that target separately through both branches.
Check the pointer-store width, repeated gates, zero-result
failure continuations and the unprefixed conversion bytes.
Retain the remaining callees, pointer consumers and hardware
outcomes as dependencies rather than inventing their effects.
