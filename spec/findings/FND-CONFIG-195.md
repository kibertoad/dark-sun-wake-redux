---
id: FND-CONFIG-195
title: Declared resident release calls include a record-gated route outside the signed-handle wrapper
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0A26
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:28C5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3BD1
tool: Python 3.14.7 and Capstone 5.0.7 header-derived MZ relocation inventory and bounded resident instruction reading
environment: null
---

## Observation

The approved executable's MZ header has 4703 relocation entries and
5200 hexadecimal header bytes. Enumerating those entries, selecting a
far-call opcode three bytes before the relocated segment word, and
checking the segment and offset yields 16 encoded resident call sites
to 1BF3:28C5 and 21 to 2D40:3BD1. This query classifies declared
relocated operands, not every possible direct or indirect call.

The 16 service-call file offsets are:

| Call instruction in DSUN.EXE |
|---|
| 0x000261E1 |
| 0x0003053D |
| 0x00033290 |
| 0x000332A1 |
| 0x00033687 |
| 0x00033693 |
| 0x000337B7 |
| 0x00033DC8 |
| 0x00033DD4 |
| 0x000371FC |
| 0x00037299 |
| 0x000372AA |
| 0x00038F9E |
| 0x00038FAF |
| 0x000391DC |
| 0x000391EB |

The first is the signed-wrapper call read in FND-CONFIG-184. The
second belongs to FND-CONFIG-168's resident 3A8E:0A26 pointer
consumer. Its entry at file 0x00030506 has a stack-limit guard before
reading the stacked far pointer. A null pointer selects FFFF return.
For a nonnull pointer, the word at record+9E is tested for bit 4000.
If that bit is clear, the release call is bypassed. If it is set, the
record's word at +A0 is passed directly to 1BF3:28C5; the caller
removes the argument and continues without checking the returned AX.
There is no own signed-handle, FFFF-sentinel or root-index check on
this route. The call's segment word is an MZ relocation resolving
relative segment 0BF3 to loaded segment 1BF3.

FND-CONFIG-168 supplies a concrete incoming overlay route: all three
nonnull field clears in FND-CONFIG-161 pass a zero selector to the
wrapper that selects 0A26. This does not establish their native
record fields, pointer validity or the handle word actually supplied.
FND-CONFIG-194 supplies the callee's flag gates: it doubles the handle
without an own index bound, touches VGA ports even on a skipped slot
path, and may change slot flags, the pool cursor and following blocks.
Consequently the signed wrapper's root protection cannot be applied
to this different direct call merely because both use the same service.

## Interpretation

This reading connects an already located pointer-consumer route to the
release service's now-described effects. Its returning continuation is
independent of a local release-result test. The record bit admits the
call; it does not validate the handle or guarantee reclamation. The
other listed encoded calls are navigation evidence, not established
incoming reachability or accepted input contracts.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain record+9E/+A0 producers,
accepted handle ranges and aliases, later slot state, the remaining
callers and native hardware outcomes. One reading supplies a valid
ordinary allocation at +A0; another supplies a reference, already-free
or otherwise unsupported slot. The local record-bit test does not
separate those cases. Complete producer and caller readings would
settle the code-decided conditions; owner observations are required
where VGA behavior decides the outcome.

The reading that every resident service call passes through 3BD1's
signed at-most-one guard is ruled out by the relocated direct call
in 0A26. This does not prove a native call with a root, negative or
out-of-range handle. No native or emulated result is claimed.

## How to reproduce

Read the approved MZ header's relocation count, table offset and
header-paragraph count. For each relocation, compute its file operand
as header bytes plus sixteen times relocation segment plus relocation
offset. Check the far-call opcode three bytes earlier, then match the
little-endian offset and relative segment to 28C5/0BF3 or 3BD1/1D40.
Keep the query limited to those two targets and report addresses only.
Read 3A8E:0A26 from its entry through the record-bit branch, argument
load, call and immediate continuation; compare FND-CONFIG-168's full
local reading and FND-CONFIG-184's distinct signed wrapper. Follow
FND-CONFIG-194 for the release gates without assuming native input
validity, complete caller coverage or VGA output.
