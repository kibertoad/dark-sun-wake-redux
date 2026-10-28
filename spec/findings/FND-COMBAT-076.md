---
id: FND-COMBAT-076
title: Script instruction 0x25 repeats its child by the second parameter and sums byte results; MAS 67 passes a decreasing twice-difficulty counter
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0FC2..172C:1012
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:2DB9..172C:2DF8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073A8D..0x00073B77
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x001514B6..0x00151500
tool: tools/research/exec-census/resident_listing.py, overlay_listing.py and script_decode.py at 231d857b; Capstone 5.0.9
environment: null
---

## Observation

The six-parameter handler at 172C:0FC2 reads all six expressions before calling
172C:2DB9. It forwards the first four as words and the last two as bytes in
word stack slots. It zero-extends the helper's byte result into the script
accumulator.

The near helper takes parameter 1, the second script parameter, as a signed
word loop limit. A signed word index starts at zero. For every index below
the limit it calls overlay 188 +0BED through 5702:0043, forwarding parameter
0, parameters 2 and 3 as words and parameters 4 and 5 as bytes. It adds the
returned byte to a byte total and increments the index. A nonpositive limit
makes no child call. A positive limit n makes n calls if they return, without
stopping when a child returns zero. The accumulated result wraps modulo 256.

Overlay 188 +0BED first loads the negation of parameter 0 as an RDFF resource
through 2D40:000A into slot 319. It conditionally reads details byte +0x37,
unloads that temporary slot when occupied, and continues only when the load
result was zero. It makes at most two local placement attempts through
+0AA8. After a nonzero placement result it calls 31E0:0121 with the negated
object parameter and the chosen coordinates. A nonnegative result is stored
at 4C13:0363 and returns byte 1. Otherwise it retries with its local flag
cleared, then returns byte 0. This reading identifies success/failure branches,
not the full admission or effects of those children.

MAS/67 begins at file offset 0x1513AB and has 1494 shipped bytes. At script
0x010B it assigns difficulty to local word 0, then at 0x0118 doubles that
word. At 0x0120 it tests the word above zero; 0x0126 leaves the loop when
false. The body reads two random expressions into locals 1 and 2, then at
0x0141 calls instruction 0x25 with object -205, local word 0 as the count,
locals 1 and 2 as coordinates, and 6 and 0. At 0x014F it decrements local
word 0 and at 0x0152 jumps back to 0x0120. These local opcode meanings are
RULE-SCRIPT-003 and RULE-SCRIPT-011; the difficulty query is FND-COMBAT-075.

## Interpretation

A count passed to instruction 0x25 is a number of child calls, not a promise
that that many objects appear. Failure does not terminate the count loop.

If its children preserve MAS/67's local word 0 and return normally, the outer
loop requests group sizes 2d through 1. For difficulty d of 0, 1, 2 or 3 that
is respectively 0, 3, 10 or 21 child calls, distributed over 0, 2, 4 or 6
outer iterations. This is a conditional arithmetic consequence, not an
established native creature count or a complete reading of the script.

## Alternatives

The child graph's preservation of the local-word backing store at the far
pointer 4C13:0337 is not proved here. Neither are the coordinate random
reader's effects, placement admission, resource load effects, campaign entry
conditions or visibility. Q-COMBAT-007 retains these obligations. A fixed
number of successful placements cannot be inferred from the requested count.

## How to reproduce

With the licensed installed DSUN.EXE, run resident_listing.py for
172C:0FC2..172C:1012 and 172C:2DB9..172C:2DF8. Run overlay_listing.py with
descriptor 188 and file range 73A8D 73B77. Run script_decode.py with the
installed GPLDATA.GFF and OBJEX.GFF, MAS 67, and read script 0x010B..0x0155.
The GFF directory's indexed MAS entry 67 gives offset 0x1513AB and length
1494; adding those script bounds gives the cited physical range.