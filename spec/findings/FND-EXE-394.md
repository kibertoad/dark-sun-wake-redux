---
id: FND-EXE-394
title: Game type-two queries round a native record value and condition a firmware request on an installation probe
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:03B3..15F3:0423
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 15F3:03B3..15F3:0423
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:0371..15F3:03AE
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 15F3:0371..15F3:03AE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:00BC..15F3:00E9
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 15F3:00BC..15F3:00E9
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-393's first query, 15F3:03B3, saves BX, CX, DX,
SI, DI, DS and ES. It requests interrupt 21 with AX 3000,
copies returned AX into BX, sets AX 0040 and tests BL unsigned.
BL outside two through three goes directly to register restoration and a far
return. That path returns 0040 without a local record walk or direction-flag
assignment; native effects other than the restored registers remain unadmitted.

BL two or three selects another interrupt 21 request, AX 5200.
The helper loads ES:BX from the far pointer at returned ES:BX, clears the
direction flag and initializes AX:DX to 0010:0000. For each current record it
loads DS:SI from the far pointer at ES:BX+0012. It saves the original SI,
adds 0012 at word width and compares five successive bytes against CS:03AE,
using CX five and a saved/restored ES while ES temporarily equals CS. It
restores SI before testing the comparison result.

For a matching record, the byte at DS:SI+002E and word at DS:SI+002C
form the high eight and low sixteen bits of a candidate value. The helper
compares that byte against AL first. A smaller candidate byte is ignored;
a larger one replaces AL and DX. Equal bytes replace DX only when the
candidate word is unsigned greater than current DX. AH remains zero in this
loop. Nonmatching records leave the accumulated value unchanged.

It loads the next ES:BX from the far pointer at current ES:BX+0018.
Only the newly loaded offset BX equal to FFFF ends the walk; its segment is
not tested. The initial record is processed before this terminator test.
There is no local iteration cap, cycle check, mapped-extent validation or
native-error branch. Wrapped offsets and malformed native records therefore
remain outside any admitted traversal contract.

On termination, two consecutive word-width shifts of DX, each rotating the
carry into AX, select the high word of the accumulated value multiplied by
four. Nonzero remaining DX increments AX once. For an admitted terminating
walk this returns the ceiling of the selected value divided by 4000, with
the initial value 100000 supplying the lower bound. Thus the local result
ranges from 0040 through 0400. Values 100000, 100001 and FFFFFF give
0040, 0041 and 0400 respectively. These are arithmetic cases, not claims
about native record contents. It restores all seven saved registers and
returns far without incoming argument cleanup. The enumerating path leaves
the direction flag clear; no earlier flag state is saved.

The second query, 15F3:0371, first calls near 15F3:00BC. That helper saves
DS, ES and BX, assigns DS from CS and writes CS's word to offsets 00A6
and 00B0. It requests interrupt 2F with AX 4300 and tests returned AL
against 80. It sets AX zero without changing the comparison flags. A
nonmatch restores the saved registers and returns near with AX zero.

A match requests interrupt 2F with AX 4310, writes returned BX and ES
to offsets 00B8 and 00BA through the then-current DS, sets AX one and
restores BX, ES and DS before returning. DS equals CS for the two initial
stores; the later pointer stores require the native DS-preservation contract
to identify them as that same storage. No local check verifies that contract.
The second request's result and flags do not gate the AX-one return.

The outer query decrements AX and immediately returns far if it becomes
zero. Therefore the probe's AX-one path produces outer AX zero and bypasses
the firmware-byte read and interrupt 15. On the probe's AX-zero path it
saves ES, reads the byte at F000:FFFE and compares it with FC, restores ES
and sets AX zero without changing comparison flags. A nonmatch returns zero.

A matching firmware byte selects a path which saves CX and computes a
three-byte address from current CS: rotate CS left four, retain the low
nibble in CL, clear AX's low nibble, add 0034 to AX and add that carry to
CL. It writes AX to CS:0036 and CL to CS:0038, then restores CX. With
modeled CS 15F3 these stores are 5F64 and 01. It requests interrupt 15
with AX 8800 and returns far with the interrupt's AX unchanged. No local
carry, error or size check qualifies that result. SI and DI are not saved
by this query or its near probe; their preservation across the reached
interrupts cannot be assumed by FND-EXE-393's caller.

Both editions have identical bytes across these three bounded bodies. The
near probe contains 21 instructions, the second query 26 and the first query
58, with their final returns at 00E8, 03AD and 0422 respectively.

## Interpretation

This replaces the unread-body dependency with the local return arithmetic,
record traversal, state stores and native selectors for both type-two queries.
The first query's arithmetic bounds require a valid terminating native walk;
they are not bounds on available game storage. The second can bypass its
firmware request, and its reached interrupt result remains unchecked locally.
Q-EXE-007 retains native return/register contracts, record origins and bounds,
firmware admission, storage writers, complete caller coverage and other slot
producers/targets. No complete reading or game launch exclusion is claimed.

## Alternatives

A fixed native capacity would ignore the returned record pointers, their
selection and the firmware branch. A bounded walk would invent a guard absent
from the next-offset loop. Treating the initial record as already terminated
would move the FFFF check before its actual position. Treating the probe's
second request as checked success would add a result test. Assigning its later
pointer stores to CS unconditionally would assume DS survives that request.
Assuming SI/DI preservation would supply saves or native contracts not shown
by the second query's body.

## How to reproduce

At revision 395982a9 require both identities in FND-EXE-350. Use MZ header
size 5200, relative segment 05F3 and modeled load segment 1000, placing
these code ranges at shipped-file base B130 plus their offsets. Decode each
Locations range separately in sixteen-bit mode and check full byte coverage,
instruction counts and matching bytes between editions. Do not decode the
five-byte comparison key at 03AE..03B3 as instructions.

Follow every local branch, saved register, segment assignment, far pointer
load, native selector and result test. Keep the initial current-record read
separate from the newly loaded next-offset terminator. Reconstruct the
three-byte comparison and word-width rounding for the three stated arithmetic
cases, and the CS-derived address stores for modeled CS 15F3. Use
FND-EXE-393 for the caller's query order and AX consumption. No negative
caller/writer search is claimed. Licensed bytes remain outside Git; no game,
DOSBox or emulated call runs.
