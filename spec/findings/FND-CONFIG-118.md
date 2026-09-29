---
id: FND-CONFIG-118
title: Overlay 179 wraps a result-producing helper and pending-record drain with state-dependent return calls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56A7:0066
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56A7:006B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56A7:0070
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; FBOV trampoline and fixup mapping
environment: null
---

## Observation

The intervening helper in FND-CONFIG-117 is overlay 179 entry
56A7:0066, beginning at file offset `0x00064F5F` and returning
at `0x0006500E`. It retains its second and third word arguments,
then conditionally calls selector-adjacent helper 573B:0057 twice
with its retained second word and literal 53, then 54. Those calls
require either a derived byte test to be nonzero or unsigned word
BP+0E to be nonzero. The derived test admits a returned record's
low two bits equal to zero or two; third argument minus one instead
uses literal four and does not satisfy that test by itself.

Entry 0066 then forwards thirteen word-sized argument slots,
including byte values and a far pointer, to local 56A7:006B at
`0x00064FFA`. It saves that helper's byte result locally, calls
local 56A7:0070 at `0x00065005`, and returns the saved byte.
No local branch skips those two calls after the preliminary tests;
normal return of their callees is still required.

Entry 006B begins at `0x0006500F` and returns at
`0x000654BB`. Its bounded return-region path compares DS:0DAB
with one at `0x0006545E`. If equal, additional guards require
local BP-0E not minus one and bit `0x80` clear in a selected
37-byte record's byte field. That branch calls overlay 206 entry
0043 with the local word, two record words and a signed-index-below-
four indicator. A non-one state instead compares DS:0DAB with
five at `0x000654A7`; five bypasses the alternative overlay 206
004D call, while other values call it with the retained first word.
Earlier paths can skip this return-region logic entirely.

Entry 0070 begins at `0x000654BC` and returns at
`0x00065590`. Its loop tests signed byte DS:0915 against minus
one before reading a 14-byte record selected by that byte from
`0330:0000`. A word selected from `0330:0054` chooses a
call to selector 573B:0084 for value zero, or another call to
local 006B for value one. Other values skip those calls. The loop
decrements DS:0915 and repeats its signed test. Callee effects on
the byte remain unread, so this does not establish a fixed iteration
count or termination for all states.

## Interpretation

The temporary-five assignment in FND-CONFIG-117 surrounds both
the initial result-producing call and pending-record processing.
If state remains five at 006B's return-region checks, it bypasses
both of the listed overlay 206 calls. Its saved byte result is
returned after pending processing, not recomputed from that loop.
Neither the preliminary calls nor the nested processing establish
transitive preservation of DS:0DAB.

## Alternatives

The preliminary helpers, earlier 006B branches, overlay 206 effects,
pending-record producers and nested selector/helper state writes
remain unread (Q-CONFIG-008). A callee may change the state before
its later comparison or modify pending records during the drain.
These local paths do not establish gameplay roles, live completion,
feedback-window acquisition or the event handler's provenance.

## How to reproduce

Resolve overlay 179 trampolines 0066, 006B and 0070 using
FMT-EXE-002 through FMT-EXE-004. Read the complete wrapper
`0x00064F5F..0x0006500F`, retaining byte versus word argument
slots. Inspect 006B's prologue and return ownership separately,
then its state tests and guarded calls in
`0x00065442..0x000654BC`. Read 0070's loop from
`0x000654BC` through `0x00065591`, including its initial
jump to the signed byte test, word-selected branches and decrement.
Keep the original saved return byte separate from subsequent helper
returns and distinguish state reads from direct state assignments.
