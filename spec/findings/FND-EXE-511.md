---
id: FND-EXE-511
title: Game startup target clears conditional diagnostic flags and forwards freshly tested modes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0886..1000:092B
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:0886..1000:092B
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-510's priority-two near target 0886 starts DX at five and
compares it unsigned with current DS:37C2 installed or DS:3736 on
disc. Values at or above the limit skip the loop. Each admitted iteration
clears the indexed flag word at DS:37C4 installed or DS:3738
on disc using doubled DX at word width. It also stores FF at
record byte four and stores the record's own numerical offset at word
fourteen. Record addressing uses DX shifted left four plus base 3682
installed or 35F6 on disc, all at word width. It increments DX
and reloads the limit for the next unsigned comparison. There is no
allocation or extent check, segment adjustment or captured immutable limit.
Under admitted unchanged limit 0014 from FND-EXE-508 and nonaliasing
storage, indices five through nineteen are selected. Lower indices are not
selected by that conditional loop; wrapped addressing and aliases remain open.

Afterward it sign-extends byte four of the base record and passes that
word to local far-returning 0705, removing two argument bytes. Full AX
zero clears flag 0200 in the base record's word two. Nonzero leaves
that word unchanged locally. It then pushes quantity 0200, freshly tests
current flag 0200 and pushes one when set or zero when clear,
then zero and the base record offset, calling local far-returning 364E.
It removes eight argument bytes and ignores the returned AX.

It repeats this sequence for the next record: offset 3692 installed or
3606 on disc, FND-EXE-508's diagnostic record. The sign-extended
incoming byte comes from DS:3696 installed or DS:360A on disc.
A zero result from 0705 clears bit 0200 at DS:3694 installed
or DS:3608 on disc. The later flag test selects two when set
or zero when clear. The 364E outgoing stack consists, in push order,
of quantity 0200, selected word two or zero, zero, and the diagnostic
record offset. It removes eight argument bytes, ignores returned AX and
returns near without incoming cleanup.

The mode-producing flag test happens after the 0705 call and conditional
clear, rather than reusing the shipped flag or the helper's returned word.
All bytes of those outgoing words come from constants or the listed
conditional selection. The next record's byte four is reloaded after the
first 364E call. Thus call effects and aliases can change later inputs.
The body contains no local interrupt, does not locally write SI or DI
and saves neither around its callees; returned SI/DI preservation still
depends on those callees. Both editions share local control flow with the
distinct record and table offsets above.

## Interpretation

This supplies a concrete startup writer for the failure-bypass flag whose
shipped value FND-EXE-508 records. The diagnostic cannot be assumed to
retain flag 0200 merely because its shipped record contains it. Q-EXE-007
retains 0705's return producer, 364E's argument meaning and record writes,
callee preservation, actual segments and extents, aliasing and lifetime, the
earlier priority-one target and remaining startup/callers, and broader launch
coverage. No native flag outcome, complete initialization contract or whole-game
launch exclusion is established by this caller reading.

## Alternatives

Treating shipped flags as unchanged ignores the conditional clear. Treating
the 364E selector as the 0705 result ignores its separate fresh flag
test. Treating the iteration limit as captured ignores its per-iteration reload.
Treating the ignored 364E return as proof of successful initialization ignores
the absent result test. Treating untouched local SI/DI as preserved across
the target ignores its callees.

## How to reproduce

At revision 23274c3 require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
5A86..5B2B in sixteen-bit mode. Follow unsigned limit tests, word-width
indexed writes and per-iteration reload; verify opcode 98 at 08BD and
08F5 as byte-to-word sign extension. Track full AX zero tests, conditional
flag clears, fresh flag tests, every pushed argument, discarded returns and
exact cleanup. Keep edition-specific record offsets and actual segment admission
separate. Licensed bytes stay outside Git; no game process, DOSBox or
emulated call runs.
