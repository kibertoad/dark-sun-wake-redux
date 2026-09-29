---
id: FND-CONFIG-171
title: Following resident helpers reload callback targets and perform bounded fixed-segment word writes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0D83
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0609
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:05E1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4328:0B4C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0182
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4464:0083
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared MZ relocation mapping
environment: null
---

## Observation

FND-CONFIG-161 calls resident 3D72:0D83 and 2C5F:0182
in order after 56BD:00BB. Neither returned AX is tested
by that caller. The complete local 0D83 body occupies
file span `0x000336A3..0x00033705`, ending with far
return at `0x00033704`. It saves SI and has a stack-limit
guard through 1000:2E48 before its main branch.

If current DS:A119 is nonzero, it calls 39D1:0609,
then calls the far pointer freshly read from current
DS:A119 with word zero. The earlier nonnull test does
not snapshot this target. After the indirect call it
copies AX to SI and calls 39D1:05E1. A nonzero SI at
the subsequent test returns FFFF; zero continues. The
bracketing helper's transitive SI preservation remains
a condition on interpreting that test as the callback's
original result. A zero initial pointer skips all these
calls and the result test.

The continuation writes word current DS:2FB8 to one
and calls 4328:0B4C. It then tests DS:A119 again. A
nonzero value calls 39D1:0609, then the freshly read
pointer with word one, retains AX in SI and calls
39D1:05E1. Nonzero SI returns FFFF; zero returns zero.
A zero pointer at this later test returns zero without
the second bracketed call. Different values can be read
at the gates and indirect calls; target identity and
preservation across intervening calls remain open.

The complete 39D1:0609 and 05E1 spans are
`0x0002F519..0x0002F542` and
`0x0002F4F1..0x0002F519`. Each has a stack guard.
0609 passes current DS:A05B by far address to 4328:0118,
then passes the far pointer stored at current DS:A057
to 1BF3:5945. 05E1 passes the same A05B address to
4328:00E0, calls 1BF3:7AAB and stores returned DX:AX
at current DS:A057/A059. Their complete external effects
are not replaced with assumed save/restore semantics.

The complete 4328:0B4C span is
`0x00038FCC..0x00039055`. After its stack guard, it
continues only when current word DS:2FB8 is nonzero,
words DS:33B6 and 33B8 differ, and word DS:33BA is
nonzero. Otherwise it returns without its later calls
or local flag clear. The continuing branch passes
pointer DS:A17F to 409B:1675. Nonzero DS:3330 selects
3D72:0DE5 with word DS:33B8; zero selects 3D72:0B84.
It next passes word FFFF to 1BF3:5814. Nonzero word
DS:A2B2 additionally passes words DS:33B6 and 33B8
to 4400:000E. It then clears word DS:2FB8. A fresh
DS:3330 test selects 3D72:0E10 with DS:33B8 when
nonzero, or 3D72:0942 when zero. These branches do not
establish successful rendering or stable earlier fields.

The complete 2C5F:0182 span is
`0x00021972..0x0002197C`. It calls 4464:0083 without
arguments and returns far. The complete 4464:0083 body
occupies `0x000398C3..0x000398E2`. It saves DS, AX
and flags, disables interrupts, loads declared MZ segment
57E0 into DS, copies word 57E0:33C0 to 33C2 and clears
words 33C4 and 33C6. It restores the saved flags, AX
and DS before returning. There is no local timer wait,
input request or result normalization. The restoration
of interrupt flags is separate from any external timing
or interrupt outcome.

All named resident far-call and fixed segment operands
were verified through their declared MZ relocations.
DS-relative fields outside 4464:0083 refer to DS at the
corresponding instruction; their callees' preservation
is not assumed.

## Interpretation

The following calls include two conditional, freshly
loaded indirect targets around an intervening gated
helper. Earlier validation and the original callback's
result remain conditional on state/register preservation.
The caller ignores the local zero/FFFF result and proceeds
after a returning call. The final wrapper has bounded
fixed-segment word writes under saved interrupt flags;
its return does not establish a visible or timing effect.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain DS:A119 producers,
target identity, registrations, all callers, DS/SI and
field preservation, complete external callees, aliases,
valid graph/state, and interrupt or hardware outcomes.
One reading preserves the tested target and callback
result; another changes them across a bracketed call.
The local rereads allow that distinction, but do not
prove that an ordinary invocation changes them. A gated
0B4C bypass can leave 2FB8 equal to one; continuation
can clear it. Actual state selects those paths.

Q-SCRIPT-007 retains resident-only cases after supported
layouts and the harness exist. They can exercise the
local gates and fixed word copies without establishing
native callbacks, hardware effects or overlay return.
No native or emulated observation is claimed.

## How to reproduce

Read 3D72:0D83 through 0DE4 from its entry. Follow both
nonnull tests, the intervening bracketed callees, fresh
indirect target reads and retained-SI result tests. Read
39D1:0609 through 0631 and 05E1 through 0608, resolving
their callees without inferring semantics from their
positions. Read 4328:0B4C through 0BD4 and retain its
three early gates, ordered arguments, fresh selector
and local clear. Read 2C5F:0182 through 018B and
4464:0083 through 00A1, checking the declared fixed
segment and saved DS/AX/flags. Compare the ignored
results and next branches in FND-CONFIG-161.
