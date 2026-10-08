---
id: FND-EXE-252
title: Cleanup gate skips only the vector initializer and both paths continue through two callbacks
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0193..4AE5:01B5
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

The cleanup candidate at `4AE5:0193` contains sixteen instructions and
thirty-four contiguous bytes. It saves BP, forms its frame and saves DS,
then loads DS through CS-relative word five. It compares DS-relative word
`0x0128` against zero. Equality branches to `4AE5:01A8`, the CS push
immediately before the first computed callback, not to final restoration.
Only the nonzero arm executes the CS push and near call at `4AE5:01A5`
to FND-EXE-251's initializer at `4AE5:0140`.

Both arms therefore reach the near call through current DS-relative word
`0x0084` at `4AE5:01A9`. If that call returns, both continue to the near
call through current DS-relative word `0x0082` at `4AE5:01AE`. Each near
call is immediately preceded by a CS push. That extra segment word and
the call's return offset form a far-return frame if the target consumes
them that way; a near return alone would leave the extra word on the
stack. Native return behavior and saved-slot integrity remain obligations,
not guarantees from the caller's pushes.

There is no carry test after the initializer or either callback. No DS
reload separates the callbacks: the second offset is read through the
DS that exists after the first callback. Likewise the nonzero gate arm
reads the first offset through DS after the initializer. The source does
not prove that either callee preserves this state binding. A zero handle
on entry therefore does not establish that no cleanup callback runs,
and the initializer's handle clear does not cancel the already selected
continuation to callbacks.

After the second callback, the explicit continuation restores saved DS
and BP and returns far at `4AE5:01B4`, without additional argument cleanup.
The bounded CFG lists three calls and one far-return exit, assuming each
call returns. The computed offsets and current CS are separate parts of
each target's admission. FND-EXE-178's shipped defaults and replacement
writers remain leads rather than resolved live targets.

## Interpretation

This replaces FND-EXE-177's cleanup reading. The state gate controls only
the initializer; both local arms include both callback continuations.
Reading the gate as a complete cleanup bypass would omit callbacks on the
zero arm and miss their separate state/return contracts.

The correction does not change FMT-EXE-005's supported layout evidence,
which is FND-EXE-003 and FND-EXE-005. Its Open questions and parity note
retain unresolved cleanup admission using this corrected reading.
FND-EXE-178's default values, stub decode and writer observations do not
depend on callbacks being gated, so they remain recorded and unchanged.
Its reference to FND-EXE-177 remains historical context for unresolved
targets, not evidence of a zero-arm bypass. No complete_reading declaration
or glossary claim depended on the mistaken bypass.

Q-EXE-001 and Q-EXE-010 retain native incoming frames and CS, gate and
callback writers, effective state identity after each call, complete
replacement bodies, saved-storage aliases and external effects. No
complete_reading or replacement inventory is established.

## Alternatives

FND-EXE-177 incorrectly said the zero word bypassed all three calls.
Its recorded target `4AE5:01A8` instead precedes the first callback's
segment push. That whole finding is superseded; its historical text is
preserved. The gate test, nonzero initializer call, callback order,
conditional CFG size and far-return observations are retained here with
the corrected branch interpretation.

Assuming both callback words use the entry state segment ignores the
absence of a DS reload after each callee. Treating stored offset defaults
as resolved native targets ignores live writers and the current CS.
Balanced pushes and pops do not establish intact saved words under aliases.

## How to reproduce

At revision `22a87dc`, use FND-EXE-236's source identity, original-source
region and default x86-bounds limits. Set entry and sole entries to
`0x000401E3`, with no seeds or summaries. Check interval
`0x000401E3..0x00040205`, sixteen instructions, direct call site
`4AE5:01A5` and computed sites `4AE5:01A9` and `4AE5:01AE`.
The complete field assumes all calls return and does not establish a
Standard complete reading or resolve their targets.

Decode that interval directly from the shipped source in sixteen-bit
mode with Capstone. Follow the equality target `4AE5:01A8` through both
callback calls and compare the nonzero path separately. Track each CS
push, near call, post-call DS use and final far return. Search committed
spec, glossary, queue and parity references to FND-EXE-177; review each
claim's actual dependency before moving citations. Keep source, reports
and configurations in GAME_DIR.
