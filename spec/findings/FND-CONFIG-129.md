---
id: FND-CONFIG-129
title: Resident value-64 states two and three require a later state change before mode dispatch
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:1261
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:05CF
tool: Python 3.14.7 and Capstone 5.0.7 bounded disassembly; verified MZ relocation and FBOV fixup segment mapping
environment: null
---

## Observation

This entry corrects the raw segment labels in FND-CONFIG-115.
MZ relocations at 0x0001F1CE, 0x0001F213, 0x0001F267, 0x0001F298, 0x0001F2C9, 0x0001F2DC and 0x0001F31D establish the corrected loaded-segment labels below.
The bounded control-flow description is retained with mapped addresses.

In the event-five/value-64 branch of FND-CONFIG-128, DS:0DAB
equal to two or three selects the common block at file offset
`0x0001F1CD`. This reading covers that block through its join at
`0x0001F31C`, independently of the entry's still-unknown producer
(FND-CONFIG-114).

A nonzero far pointer at `52A1:0008` takes three helper calls,
then the local helper beginning `0x00020911` with argument one,
and exits the event handler. It bypasses the mode-dispatch call.
With that pointer zero, a nonzero pointer at `52A1:0000` instead
exits directly.

The remaining path conditionally calls helpers for nonzero stored
pointers at DS:4214, `4DA0:0000`, `55CB:0000`,
`52A1:0004` and `4E46:0000`. Four sites forward the 24-byte
input record with helper 1000:0699 to the local routine beginning
`0x0001E3F8`. The DS:4214 path uses a signed result in zero
through three for one subsequent helper, or takes its alternative.
The 3DA0 and 45CB paths compare the local result with minus one
and can call another helper before continuing. Those comparisons
do not themselves exit this handler.

For nonzero `4E46:0000`, a result of minus one from the local
record-taking routine instead takes the local helper at
`0x00020911` with argument one and exits. A different result,
or zero pointer, joins `0x0001F31C`.

The join applies the gates recorded in FND-CONFIG-128: word
`4C10:0019` must be zero or signed DS:426D below four, and
DS:0DAB must then equal one. Only after that equality does the
handler forward the event record to mode dispatcher 28C9:05CF
at `0x0001F34C`. No instruction in the read state-two/three block
assigns DS:0DAB directly; its helper calls can have unread effects.

## Interpretation

Entering with stored state two or three does not alone establish the
later mode-dispatch call. The early pointer exits bypass it entirely.
A path joining the final gate can call the dispatcher only if state
has become one by that later read. If all intervening helpers preserve
DS:0DAB, these two initial states fail that final equality.

## Alternatives

Some helper can change DS:0DAB to one, or all can preserve it.
FND-CONFIG-130 reads the record-taking wrapper and its conditional
index remapping, leaving its transitive callee effects open.
FND-CONFIG-117 identifies a getter/setter and one temporary-five
assignment with saved-word restoration elsewhere. The state-two/three
helpers' state-writing and dispatch effects remain unread
(Q-CONFIG-008). The conditional paths are not an established player
action, and the local routine at `0x0001E3F8` is not assigned a
role from its return values alone. Event provenance remains open in
FND-CONFIG-114; this local gate does not settle it.

## How to reproduce

Inspect `0x0001F1CD..0x0001F31C` from its verified branch target,
tracking pointer tests, calls, result comparisons and exits. Compare
the state selector at `0x0001F1B2..0x0001F1CD` and final
join at `0x0001F31C..0x0001F34F`. Verify that the local block
has no direct DS:0DAB assignment while retaining every helper call
as an unresolved possible writer. Compare argument forwarding with
1000:0699 in FND-CONFIG-128. Do not treat helper presence or a
non-exiting return comparison as proof that the final state is one.

For the segment-label correction, select each named operand from the
MZ relocation list or its overlay's declared FBOV fixup list. Apply the
recorded load segment to MZ operands; decode an overlay operand's shifted
descriptor index and resolve its segment-table entry before labelling an
address. Raw segment operands and mapped addresses are different forms.
