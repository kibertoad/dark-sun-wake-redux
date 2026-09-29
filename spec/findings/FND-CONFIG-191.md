---
id: FND-CONFIG-191
title: The handle request writes metadata before coordinate rejection and the wrapper gates its graphics primitive
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:282D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3B1D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3B30
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3B67
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:42A3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:42B9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:42CF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:42E5
tool: Python 3.14.7 and Capstone 5.0.7 bounded complete resident 16-bit readings and header-derived MZ relocation checks
environment: null
---

## Observation

FND-CONFIG-186 and FND-CONFIG-189 call resident
1BF3:282D and pass returned words through wrappers
2D40:3B1D or 3B30. The complete 282D body spans
`0x0001395D..0x000139D9`, including its success and
FFFF returns. It saves DS/SI/DI, selects DS=CS and
calls local far 2707 through push-CS/near-call. The
complete slot scan in FND-CONFIG-183 returns a free
slot offset in SI or carry-set exhaustion. Exhaustion
returns FFFF before later metadata work.

An admitted slot doubles the first word argument as
its initial reference offset. While word reference+C04
has bit 0040 set, it follows the word at reference+4
as the next offset. There is no own first-argument range
check or chain-length/cycle bound. Once bit 0040 is
clear, it stores that resulting reference offset at selected
slot+4. It resets the reference offset to twice the
original word argument and writes zero at selected
slot+204 before performing coordinate checks.

The four remaining stacked words are checked in order:
second must be signed-at-least reference+404;
third signed-at-least reference+604;
fourth signed-at-most reference+804;
fifth signed-at-most reference+A04. Each accepted word
is written to the selected slot's corresponding field
before the next check. A failed comparison returns FFFF,
without rolling back the earlier reference, zero-count or
coordinate assignments. It does not set the success flag
on that path, so under unchanged CS and normal slot-scan
state its earlier free bit remains available to a later
scan despite partially changed metadata.

After all comparisons pass, it stores word 0040 at
selected slot+C04 and returns selected slot offset divided
by two. It does not compare the request's own first/last
coordinate pairs with each other. There is no later
callee in these validation/write paths; normal returns
restore saved DS/SI/DI. Invalid reference chains, aliases,
word offsets, capacities and native slot inputs remain open.
A returning FFFF therefore is not a guarantee of no
metadata writes, while an admitted reference does not
itself establish performed graphics or valid capacity.

2D40:3B1D's complete span is
`0x0002611D..0x00026130`. It forwards its two word
arguments to local far 3B30 and returns its AX without
an own result normalization. The complete 3B30 span is
`0x00026130..0x00026167`. It saves SI/DI and loads
the two words there. Either signed-negative word skips
all further work. Otherwise it calls local far 3B67
for the first, tests returned AL, then does the same
for the second only after a nonzero first AL. Any zero
AL skips the graphics primitive. Two nonzero bytes call
1BF3:43AE with the two handles. Its returned AX is
forwarded, not normalized to an overall success result.
The wrapper restores saved SI/DI at normal return.

Thus the unchecked 3B1D invocation with a FFFF
handle in FND-CONFIG-189 does not by itself invoke
43AE: the signed-negative wrapper gate rejects FFFF.
The caller's later state commit remains independent of
that wrapper gate and its returned result. Complete
43AE and actual graphics outcomes remain outside this
reading. A wrapper call is not evidence of primitive work.

3B67's complete span is
`0x00026167..0x000261D1`. It saves SI/DI, rejects
a signed-negative handle, then reads four coordinate words
through resident 1BF3:42A3, 42B9, 42CF and 42E5.
It requires signed 0 <= first <= last < 320 for the
first pair and signed 0 <= first <= last < 200 for
the second pair, with equality admitted. A failing check
returns AL zero; all admitted checks return AL one.
It does not independently bound a nonnegative handle index,
check slot flags or supply a normalized AH. The enclosing
3B30 checks AL only, avoiding an unsupported full-AX
success interpretation. Normal SI/DI are restored locally.

The four getters' complete consecutive spans are
`0x000153D3..0x000153E9`,
`0x000153E9..0x000153FF`,
`0x000153FF..0x00015415` and
`0x00015415..0x0001542B` respectively. Each saves
DS/SI/DI, selects DS=CS, doubles its word handle and
returns a word from offset 404, 804, 604 or A04
respectively. They restore those registers and contain no
callee, capacity, flag or index check. Valid slot storage
and the doubled index remain caller conditions rather than
a guarantee supplied by those screen-coordinate tests.

All external getter and primitive segment operands were
verified through header-derived MZ relocations; local calls
use far push-CS/near-call frames. The complete 2707
scan is already bounded in FND-CONFIG-183. No own
DS:0DAB, DS:1440 or caller cache field write occurs
in these bodies, and only the request's CS-relative slot
metadata is directly mutated here. Unread primitive effects,
reference producers, aliases and native graphics remain open.

FND-CONFIG-192 subsequently reads the complete local 43AE body,
including shared scratch, reference walks, direction and overlap gates,
port accesses and phased copies. Full caller/input, capacity/alias,
mask-table and hardware/presentation outcomes remain open.

## Interpretation

Failure and success contracts differ across the layers.
The handle request can return FFFF after partial metadata
writes. The forwarding graphics wrapper can be invoked with
that result yet reject it before its primitive. Coordinate
validation is a separate AL predicate, not a full-AX or
slot-capacity contract. None supplies a universal no-write
failure, completed presentation or rollback guarantee for
the outer service's later state assignments.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain all callers,
slot/reference/flag/coordinate and capacity producers,
chain termination, aliases and word wrap, complete 43AE
and VGA/graphics outcomes. One reading supplies valid
finite references and admitted coordinate fields; another
fails a later check after metadata writes or supplies a
negative handle that the forwarding wrapper rejects.
Complete inputs and primitive evidence would distinguish
native reachability and performed graphics.

A reading that FFFF request results leave all slot fields
unchanged is ruled out by the write-before-check ordering.
A reading that every unchecked outer wrapper call reaches
43AE is ruled out by its signed-handle and AL gates.
A reading that screen-coordinate validation bounds every
slot index is unsupported: the getters have no such
check. No native or emulated outcome is claimed.

## How to reproduce

Read 1BF3:282D through both returns ending 28A8.
Follow 2707's carry gate, unbounded bit-0040 reference
walk, pre-check reference/count writes, each signed coordinate
comparison and write, final flag and handle result. Do not
include neighboring 28A9 as part of this body. Read
2D40:3B1D through 3B2F, 3B30 through 3B66 and
3B67 through 3BD0. Verify local far frames and MZ
getter/primitive segments. Read each of the four coordinate
getters through its own return ending 42FA. Derive
negative/sentinel, coordinate endpoint/equality and later-failure
metadata cases under valid storage and chain conditions.
Keep AL predicates, primitive invocation, accepted slots,
capacity and actual graphics outcomes distinct.
