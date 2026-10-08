---
id: FND-EXE-237
title: Replacement cleanup callback prioritizes one state bit before a second dispatch path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0D11..4AE5:0D27
tool: executable-reader 2.5.0, Capstone 5.0.7 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

Under FND-EXE-236's candidate CS binding, the replacement offset `0x0D11`
decodes from the shipped source as eight instructions and 22 bytes, ending
exclusively at `4AE5:0D27`. The saved resident listing has no instruction at
that start; its window request reports an undisassembled start, not absent
source bytes.

The candidate first tests bit zero of current DS-relative byte `0x0010`.
If set, it calls `4AE5:0BCF` at `4AE5:0D18` and returns near at
`4AE5:0D1B`, without extra argument cleanup. This path does not test bit
one after the call. If bit zero was clear, it separately tests bit one of
the current DS-relative byte at the same offset. When set, it calls
`4AE5:09CB` at `4AE5:0D23`; whether set or clear, that path returns near
at `4AE5:0D26`, without extra argument cleanup.

Both bits being set at the first test therefore selects the first call,
not both calls, assuming its return. With neither bit set at the respective
tests, no callee is called. These are ordered memory tests rather than one
retained byte snapshot; changes between them have not been excluded. The
bounded CFG includes both direct targets and assumes their returns.
The candidate itself has no explicit memory stores or segment-register
writes; its callees' effects and interrupt-time changes remain unread.

## Interpretation

This supplies the replacement callback candidate's priority and concrete
callee dependencies. It does not admit the cleanup caller's live target,
the callback's current DS identity, the state bits' meaning or preservation
of ES and stack storage before FND-EXE-235's later segment clear.

Q-EXE-001 and Q-EXE-010 retain native caller/CS and writer DS admission,
all writers of the state byte and callback word, both callee effects,
storage aliases and interrupt-enabled changes. No complete_reading or
replacement inventory is established.

## Alternatives

Independent invocation of both helpers when both bits are set is contradicted
by the first path's return before the second test. A no-op replacement is
contradicted by the conditional direct calls. An immutable byte snapshot is
not supplied by two separate memory tests. A missing analyzer window cannot
establish absent source code.

## How to reproduce

At revision `332c766`, use FND-EXE-236's original-source region, source hash
and default x86-bounds settings, setting entry and sole entries value to
`0x00040D61` (`4AE5:0D11`), with no seeds or summaries. Check the 22 covered
bytes, eight instructions, two near returns and both returning-call assumptions.
CFG completion does not establish a Standard complete reading.

Independently hash-check the shipped source against XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Using Capstone 5.0.7 in x86 sixteen-bit
mode, decode only file interval `0x00040D61..0x00040D77` with initial IP
`0x0D11`; inspect both byte tests, branches, near calls and returns above.
The resident Ghidra snapshot, read-only with analysis disabled, reports
an undisassembled start for ReportInstructionWindow at `4AE5:0D11`, count
45. Keep that limitation separate from source decoding. Sources, listings,
configurations and reports remain in GAME_DIR.
