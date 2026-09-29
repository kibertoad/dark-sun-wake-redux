---
id: FND-CONFIG-166
title: A state clear can make the following status poll return zero under stable valid inputs
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4611:03A5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4611:0051
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4611:0407
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared MZ relocation mapping
environment: null
---

## Observation

FND-CONFIG-162's 0BC1 caller invokes resident 4611:03A5
before testing current DS:14E3 and, when that byte is
nonzero, polling 4611:0051 until AL is zero. This finding
bounds the state-clear body and the status function's
early return conditions. It does not assign the complete
active status branch or external device semantics.

The complete 03A5 body occupies file span
`0x0003B6B5..0x0003B717`, ending with far return at
`0x0003B716`. It reads a word at offset 14 from the far
pointer at current DS:3411. A zero word returns without
its later calls or state clear. FND-CONFIG-020 records a
producer of that SOUND.CFG buffer pointer; validity and
intervening writes remain separate conditions.

With a nonzero word, 03A5 next tests current DS:3434.
Zero again returns without later calls. Nonzero calls
4842:0C70 with current DS:3436 and word zero. If its
returned AX is not two, it calls the same entry with that
field and word one. If either return equals two, it calls
4842:0C8E with DS:3436. Both the bypass and call paths
then pass DS:349E to local far 0407. Its complete body,
`0x0003B717..0x0003B725`, merely forwards that word to
resident 44DE:003A and returns. These external callees
are not characterized as successful device or file actions.

After local 0407 returns, 03A5 writes zero to current
DS:3434 and returns. There is no own DS write, but its
callees' DS preservation and other effects remain open.
The word clear follows the calls; entering their branch
does not alone prove that it reaches this clear.

The complete 0051 body was inspected from file
`0x0003B361` through its far return at `0x0003B486`.
Its entry reads the same pointed word at offset 14, then
current DS:3434. Either zero reaches AX zero and the
common return with no external call. Only both nonzero
enter the active path. That path uses resident 4842
services and local 032A; it can call 03A5 and return
zero, or return AX one. Its device results, buffer/refill
inputs and complete state contract remain dependencies.
The early zero return is not an inference from those
unread active effects.

Under valid unchanged pointer/storage, preserved ordinary
DS and unchanged gate inputs between 03A5's return and
0051's entry, the named caller reaches a zero result on
its first 0051 invocation if it invokes it at all. If
03A5 took an early return, the corresponding zero gate
still blocks 0051. If it took the active branch and
returned normally, its final DS:3434 clear blocks 0051.
No additional 0051 service call is needed on these cases.

Changed pointer/record state, DS or DS:3434 between those
instructions can instead admit the active status branch;
the bodies do not establish a universal stable-state
invariant. No native immediate-return or loop duration is
observed. FND-CONFIG-164's preceding driver poll and the
outer shared helper's later driver poll remain separate.

## Interpretation

The second poll in 0BC1 is not necessarily an active
service-wait loop. Its preceding helper and matching entry
gates provide a conditional immediate-zero path across
all local state-clear outcomes, under valid stable inputs.
Those conditions do not establish actual startup or error
state, asynchronous input changes or successful return
through any earlier external dependency.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain pointed-buffer and
state-word writers, DS preservation, 4842:0C70/0C8E,
44DE:003A, local 032A and the active 0051 branch's
complete inputs/effects. One reading maintains the
post-clear or zero-gate state and returns zero at the
poll entry; another changes one of those inputs and
reaches active processing. Complete writers, callbacks
and callee effects distinguish actual reachability.

The pair's guarded local consequence is not a complete
sound or device rule and makes no successful-release or
timing claim. Q-SCRIPT-007's resident fixture cases may
check zero guards after the harness exists, but cannot
establish interrupt, timing or operating-system outcomes.

## How to reproduce

Read 03A5 through 0406 and local 0407 through 0414,
resolving the declared MZ segment operands of their
external calls. Track the two early zero gates and final
DS:3434 store after the last call. Read 0051 from its
entry through 0176, isolating the two early AX-zero
returns from its active dependencies. Compare 0BC1's
03A5 call, DS:14E3 gate and 0051 back edge. Derive the
first-zero cases from shared valid state; keep intervening
writes and external outcomes as separate conditions.
