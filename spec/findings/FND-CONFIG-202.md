---
id: FND-CONFIG-202
title: An overlay text-wrapper caller uses formatted frame storage and strict screen-edge gates
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5799:0DBE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5799:0B81
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:09F3
tool: Python 3.14.7 and Capstone 5.0.7 bounded overlay entry/call-site reading with declared FBOV fixup mapping
environment: null
---

## Observation

A query for declared relocated/fixed-up far calls to 2C5F:09F3
found no MZ resident operand and 21 FBOV overlay operands: six
in descriptor 178, four in 190, one in 206 and ten in 213.
This is an encoded-call inventory, not complete incoming coverage.
The descriptor-206 site is code 0DBE, file 0x0008F70E.
Its header segment is 5799 and code base is file 0x0008E950.

The containing entry 5799:0B81 reserves 24 frame bytes and saves
SI/DI. A zero far pointer at mapped 4E28:00D7 bypasses its active
body. Later paths depend on setup calls, a cached image at current
DS:2904, caller words, record geometry, frame dimensions and an
accepted reference request. This reading follows the admitted text
call continuation without establishing all upstream state or external
callee effects.

At code 0D3A, file 0x0008F68A, it passes its first word argument,
current DS:291E by far address and SS:BP-18 by far address to
3150:000E. The destination starts in a six-byte frame area before
the word at BP-12. The caller supplies no own output-capacity argument.
The formatter's actual output length, termination and safety remain
open; the frame reservation alone does not establish them.

It then passes SS:BP-18 to 2C5F:03B7 and retains AX as a width
in BX. A signed half-width adjustment, together with the retained
image-width word BP-04, changes local BP-0E. The subsequent word
arithmetic and signed comparisons admit the text-wrapper call only
when all four conditions hold at their respective instructions:

| Quantity | Required comparison |
|---|---|
| SI plus BP-0E | Greater than zero |
| SI plus BP-0E plus current BX | Less than 320 |
| DI plus BP-10 | Greater than zero |
| DI plus BP-10 plus six | Less than 200 |

Each quantity is computed with word arithmetic. The width and
coordinates are not independently range-validated before those sums.
Failed conditions skip the wrapper but continue to following cleanup.

The admitted call forwards mapped 4E28:00D7's current far pointer,
SS:BP-18's far pointer, the two coordinate sums, and current words
DS:2D14/2D16 to 2C5F:09F3, in that parameter order. The caller
removes sixteen bytes and does not test returned AX. FND-CONFIG-201
maps these six inputs into the fixed text routine's argument shape.
Neither the earlier nonnull gate nor this call proves the record
pointer remained unchanged across the intervening services.

The later local path conditionally releases its BP-12 handle through
2D40:3BD1, calls 1BF3:5814 with FFFF and snapshots current
DS:145E into a double-word local. It repeatedly compares that saved
value plus 40 decimal, with double-word wrap, unsigned against fresh
DS:145E, looping while the sum is greater. There is no local update
of that shared counter in the wait. Actual progress, counter wrap,
interrupt/timing effects and native duration are not established.
Further calls precede the normal return. This is not evidence that
text display completed or that this caller always terminates.

The format-call segment, width-call segment, fixed pointer segment
and wrapper-call segment were checked at declared overlay fixups.
They resolve respectively to 3150, 2C5F, 4E28 and 2C5F.
DS-relative fields mean DS at each instruction; their provenance
and transitive preservation are not assumed.

## Interpretation

The fixed wrapper has a concrete formatted-frame caller with strict
screen-edge gates and a later counter-dependent wait. Its supplied
text pointer has an explicit stack segment, but capacity and zero
termination require the formatting callee's contract. Encoded bounds
and an ignored return do not establish native text or rectangle-transfer
success. Counter dependence also prevents a complete timing claim
from this static reading alone.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain the other encoded caller
paths, format/source and width contracts, record and coordinate
producers, reference state, aliases and counter writers. One reading
supplies bounded terminated output and reaches the admitted screen
region; another skips the wrapper or encounters unsupported storage
or counter state. Complete caller/callee and producer readings would
separate code-decided conditions; owner evidence is needed where
hardware or timing decides the outcome.

The reading that equality with an outer screen edge admits this
wrapper is ruled out by the strict comparisons. A reading that the
caller checks text-wrapper success is ruled out by its untested
continuation. No native or emulated result is claimed.

## How to reproduce

Enumerate declared MZ relocation and FBOV fixup far-call operands
for 09F3 with resolved resident segment 2C5F, retaining only bounded
counts and call addresses. Resolve descriptor 206 and read its entry
and the text continuation at code 0D3A through 0DC3. Map every
push through FND-CONFIG-201, checking SS on the formatted pointer,
the four signed gates and untested return. Check the following
counter snapshot and unsigned wrapped-sum comparison. Verify the
format, width, pointer and wrapper fixups through their descriptors;
keep formatting capacity, upstream state and timing unestablished.
