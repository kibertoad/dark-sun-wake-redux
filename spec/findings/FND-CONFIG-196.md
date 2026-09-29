---
id: FND-CONFIG-196
title: A display-state writer replaces four words after conditional graphics release calls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4328:0B01
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4328:0B4C
tool: Python 3.14.7 and Capstone 5.0.7 complete bounded resident 16-bit reading and header-derived MZ relocation checks
environment: null
---

## Observation

The complete resident 4328:0B01 body spans file
0x00038F81 through 0x00038FCB inclusive. It saves BP and SI,
loads its first word argument into SI, then has the runtime
stack-limit guard through 1000:2E48. The guard's callee outcome
remains a condition on continuing, as in FND-CONFIG-163.

The continuing body independently tests current DS:A352 and
DS:33B8 as signed words. Each value greater than one is passed
directly to 1BF3:28C5, A352 first, with the argument removed
after return. At most one call occurs for each field. Neither
returned AX is tested. Values zero, one, FFFF and other signed
negative words skip the respective call. The second field is
read after the first returning call, not snapshotted before it.
The two service segment operands at file 0x00038FA1 and
0x00038FB2 are declared MZ relocations to relative segment 0BF3.
The runtime operand at 0x00038F91 is also a declared relocation.

Every continuing path then makes these own word assignments:

| Destination, at the instruction's current DS | Value |
|---|---|
| A352 | Second stacked word argument, read after the calls |
| 33B8 | Current SI, initially the first argument |
| A03D | The same current SI |
| A2B2 | Third stacked word argument, read after the calls |

It restores SI and BP and far-returns. There is no own rejection
of incoming replacement words, release-success test, rollback,
write to DS:2FB8, or call to the following refresh helper 0B4C.
It does not normalize AX to an overall result. The old-word
signed gates do not validate the new values written afterward.

FND-CONFIG-194 reads 28C5's own DS/SI restoration on normal
return, flag gates, cursor changes, compaction and VGA accesses.
Thus ordinary balanced returns from that service preserve the
retained SI, but valid handles, memory, aliasing, guard outcomes
and native hardware effects remain conditions. Possible aliasing
of argument storage with callee writes is not excluded here.

FND-CONFIG-171's following helper compares current 33B6 with
33B8 and uses nonzero A2B2 to admit a later call. The four writes
above are therefore concrete producers for some of its inputs;
they do not prove a call to the writer or a subsequent refresh.

A header-derived MZ query found no declared relocated far-call
operand matching 4328:0B01. A raw near-call candidate query over
file 0x00038480..0x000396FF found no E8 displacement targeting
0x00038F81. These bounded inventories do not exclude indirect,
unrelocated, other-segment or computed incoming paths, and do
not establish that the writer is unused.

## Interpretation

The encoded writer releases qualifying old fields before replacing
state, independently of returned service results. Its signed gates
protect this local release route from handles at most one; they
are not a general constraint on the state subsequently installed.
A returning no-op release can still be followed by all four writes.
No native refresh, accepted replacement or transactional effect
is established.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain the writer's incoming path,
argument provenance, DS and alias conditions, later field writers
and native hardware outcomes. One reading supplies accepted display
handles and then invokes refresh under its gates; another bypasses
this writer or supplies different state. Direct incoming or pointer
registration evidence, followed through its argument producers,
would distinguish the code-decided parts. The bounded negative
inventories alone distinguish neither reading.

A reading that the writer's replacements depend on a successful
release result is ruled out by the untested calls and common
continuation. A reading that this body refreshes immediately is
ruled out by its complete call list. No native or emulated result
is claimed.

## How to reproduce

Read 4328:0B01 from its entry through the far return at 0B4B,
including the runtime guard, both signed comparisons, release
arguments, untested continuations and four final word writes.
Verify the three far-call segment operands against the MZ
relocation table. Compare FND-CONFIG-194 for the callee's saved
registers and conditional effects, and FND-CONFIG-171 for 0B4C's
33B8/A2B2 consumers. For the incoming inventories, select only
relocated far calls with offset 0B01 and relative segment 3328;
within the stated same-segment range select only E8 operands whose
signed displacement reaches file 0x00038F81. Keep those negative
queries separate from incoming reachability and argument validity.
