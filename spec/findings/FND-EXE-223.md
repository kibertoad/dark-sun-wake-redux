---
id: FND-EXE-223
title: Outer descriptor-198 candidate retains three additional unresolved computed transfers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087459..0x00087478
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000874B2..0x000874C6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000874D3..0x000874E7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087504..0x0008753C
tool: executable-reader 2.5.0 and engine 13.6.0
environment: null
---

## Observation

Starting the conditional source traversal at the outer procedure candidate
`0x00087459`, rather than FND-EXE-222's internal targets, reaches 52
instructions and 127 bytes in the four intervals above. It retains unresolved
computed jumps at `0x00087473`, `0x000874C1` and `0x000874E2`.
The last is FND-EXE-221's eighteen-slot consumer, which is not declared in
this query. The other two need their own consumer and bound readings.

The separately declared eleven-slot continuation at `0x00087515` reaches
the far return at `0x0008753B`, with zero additional argument cleanup.
No calls are listed. The report is incomplete, with three explicit traversal
gaps; its eleven-slot table-consumption assumption remains visible.

## Interpretation

Closed internal tails do not close the outer procedure's discovery. Its
three additional computed transfers must be resolved independently before
using this traversal as a whole-function inventory body. Native entry, CS,
input and frame admission remain unresolved under Q-EXE-010. No analyzer
database or committed inventory was changed.

## Alternatives

Treating FND-EXE-222's complete conditional tail reports as a complete outer
body is ruled out by these explicit additional stops. The present omissions
are query exclusions, not evidence that the original dispatches nowhere.

## How to reproduce

At revision `280b6fd`, use FND-EXE-222's x86-bounds configuration and
limits, changing entry and the region's sole entries value to `0x00087459`.
Retain its source hash, full descriptor-198 region, analysis segment binding,
format controls and eleven-slot declaration at `0x00087515`, with table
start `0x0008753C`, count eleven, stride two, width two and fieldOffset zero.
Declare no other indirect jumps and supply no seeds or callee summaries.
Preserve the report's gaps, assumedContinuations and incomplete result.
Configs and reports remain in GAME_DIR and are not committed.
