---
id: FND-CONFIG-170
title: An error collector copies and clears near state while restoring the retained consumer result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0460
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:37B6
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared MZ relocation mapping
environment: null
---

## Observation

FND-CONFIG-168's local 56BD:022D error helper passes
words 4542 and 4592 to resident 39D1:0460, then loads
AX from current DS:4592. The complete resident body
occupies file span `0x0002F370..0x0002F3AC`, ending
with far return at `0x0002F3AB`. It saves SI before
its stack-limit guard through 1000:2E48. After that
returns or is bypassed, SI holds the first word argument.
It copies current word DS:9DB0 to current DS at that
near offset. For the named call, that destination is
DS:4592.

It next passes near source offset 9DB2 and the second
argument to 1000:37B6. For the named call, the destination
is near offset 4542. After that call returns, it clears
word current DS:9DB0, then passes near source offset
32C6 and destination offset 9DB2 to the same runtime
helper. Finally it restores saved SI and returns far.
No caller-provided destination capacity is passed.
Its two runtime-helper operands and stack guard are
all declared MZ relocations mapping raw zero to 1000.

The complete 1000:37B6 body occupies
`0x000089B6..0x000089D8`. It saves SI and DI, sets ES
to incoming DS and clears the direction flag. It scans
from the near source offset for a zero byte using an
initial word count FFFF. The count consumed by that
scan becomes the copy count. It then copies that many
bytes to the near destination and returns its offset
in AX. With a terminator reached, this includes the
terminator. If the entire FFFF-byte scan finds none,
it still copies FFFF bytes; there is no separate
unterminated-input failure branch or destination bound.
Both source and destination offset advancement are
word operations in that supplied segment. The body
restores SI and DI, does not write DS and leaves ES
equal to DS and the direction flag clear.

Thus the resident collector restores its incoming SI
on its normal far-return path. The overlay helper
022D does not otherwise change SI. The outer 56BD:0043
wrapper's retained consumer result is copied from SI
to AX afterward, rather than replaced with 022D's
loaded DS:4592 word. In particular, a returning
3A8E:0A26 FFFF result remains FFFF through this local
collector path with ordinary balanced returns. The
caller in FND-CONFIG-161 still ignores AX and clears
its field when the wrapper returns.

## Interpretation

The collector has concrete near-state output and clear
ordering, and the copy primitive locally preserves the
register holding the consumer result. This resolves
FND-CONFIG-168's local SI-preservation dependency on
this normal-return path. It does not prove the guard
returns, the source bytes terminate within valid buffers,
the destinations have room, DS has the expected value,
or that the named failing child is reached natively.
The returned output word is distinct from the result
retained by the outer wrapper.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain producers and bounds
of near state at 9DB0, 9DB2 and 32C6 and the two named
outputs, other callers, aliases, expected DS and runtime
guard outcomes. One reading supplies ordinary bounded
terminated input and distinct output regions; another
changes these inputs or aliases the destinations. The
local copy and clear order is known, but full input
provenance is needed to distinguish reachable effects.
The absence of a destination-capacity argument is not
evidence that an ordinary invocation overflows one.

Resident cases could check the bounded collector/copy
branches after the harness and supported layouts exist
(Q-SCRIPT-007), but not the overlay wrapper or a native
guard/operating-system outcome. No native or emulated
result is claimed.

## How to reproduce

Read 39D1:0460 through 049B from its entry, verifying
the three MZ far-call operands. Retain the stack guard,
argument order, word output, first copy, word clear,
second copy and saved SI restoration. Read 1000:37B6
through 37D7; follow the scan's consumed count into the
copy, both termination possibilities and the restored
registers. Compare 56BD:022D's near outputs and
56BD:0043's final SI-to-AX copy in FND-CONFIG-168.
Keep buffer provenance, aliasing and actual returns
separate from the local instruction contract.
