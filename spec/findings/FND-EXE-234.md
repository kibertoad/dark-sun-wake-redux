---
id: FND-EXE-234
title: Tail helper follows incoming BP words through SS and conditionally replaces matching words
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:075F..4AE5:0785
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0753..4AE5:075F
tool: Ghidra 12.1.3 PUBLIC and executable-reader 2.5.0
environment: null
---

## Observation

FND-EXE-233's tail callee at `4AE5:075F` contains twenty instructions and
38 bytes, ending exclusively at `4AE5:0785`. It has no calls and returns
near at `4AE5:0784` with no extra argument cleanup.

It clears BX, saves CX and BP, then jumps directly to a word read through
SS at incoming BP plus zero. It does not initialize BP from SP. It logically
shifts that word right by one into CX. A zero shifted result exits; both
original words zero and one therefore terminate this traversal. A nonzero
result whose original low bit was set skips the comparison. Otherwise it
compares DX against SS-relative word at BP plus four, at sixteen-bit width.
A match stores AX into that word. If BX is still zero, it also copies BP
into BX. All continuing paths double CX at sixteen-bit width, load BP
from it and repeat the SS-relative read. Thus the next BP is the previous
link word with its low bit cleared, including on the matching-store path.

On exit it pops BP, pops CX and returns. No explicit instruction writes
AX, DX, DS or ES. Those register effects do not prove preservation of saved
BP, CX or the return address: the SS-relative store's destination has not
been excluded from those stack slots. The input word graph has no admitted
termination or write-count bound. A matching BP of zero also leaves the
zero BX sentinel unchanged, so a claim that BX always identifies the first
match requires admission of nonzero matching offsets.

The adjacent wrapper at `4AE5:0753` calls this helper, tests returned BX,
and returns directly if it is zero. Otherwise it exchanges CX with the
SS-relative word at BX plus two, then returns. This uses both the reported
offset and the restored CX, whose stack-slot preservation remains conditional.
FND-EXE-233's other caller instead reloads its rewrite count and uses AX for
stores; this helper's explicit instructions do not change AX or ES. That
caller does not test BX. Successful return and unchanged saved storage remain
separate obligations from these explicit register effects.

## Interpretation

This supplies the tail callee's explicit reads, writes, link transformation,
sentinel behavior and the two known callers' result consumption. SS-relative
access is not evidence of DS/SS equality or an admitted stack-frame layout.
Neither the caller's link origin nor safe destinations follow merely from
the absence of direct register writes or from balanced pushes and pops.

Q-EXE-001 and Q-EXE-010 retain native incoming BP/SS and link admission,
all link and comparison-word writers, effective-address aliases, stack-slot
preservation, traversal/output bounds, other callers and interrupt-enabled
changes. No complete_reading or replacement inventory is established.

## Alternatives

A helper that first creates its own BP frame is contradicted by its initial
use of incoming BP. Treating an empty shifted result as only an original
zero word ignores the terminating word one. Guaranteed first-match reporting
ignores the zero-offset sentinel case. Guaranteed saved-register preservation
from balanced pushes and pops ignores the unresolved SS-relative stores.

## How to reproduce

At revision `db769c0`, use FND-EXE-226's original-source region, hash and
default x86-bounds limits. Set entry and sole entries value to `0x000407AF`
(`4AE5:075F`), with no seeds or summaries. Check all 38 covered bytes,
twenty instructions, the near return and absence of calls; CFG completion
does not establish termination or safe stores.

In the resident Ghidra snapshot, read-only with analysis disabled, run
ReportInstructionWindow at `4AE5:0735`, count 38, restricting the helper and
wrapper claims to the intervals above. Read FND-EXE-233's caller tail at
`4AE5:071A`, count nine, for result consumption. Check the default SS segment
on BP accesses and the explicit SS override on the wrapper exchange rather
than assuming DS-relative storage. Sources and reports remain in GAME_DIR.
