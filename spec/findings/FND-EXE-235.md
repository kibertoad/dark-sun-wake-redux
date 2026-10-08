---
id: FND-EXE-235
title: Cleanup rewrites trampoline slots before a conditional unresolved near callback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:061F..4AE5:0637
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:06B1..4AE5:06E4
tool: Ghidra 12.1.3 PUBLIC and executable-reader 2.5.0
environment: null
---

## Observation

FND-EXE-228's callee at `4AE5:061F` has six instructions and 24 bytes,
ending exclusively at `4AE5:0637`. It first calls `4AE5:06B1`, compares
current ES-relative word `0x0018` against `0x04C6`, and conditionally calls
through current DS-relative word `0x0080` when equal. This is a computed
near call, not a direct call to the comparison literal. Both paths then
clear current ES-relative word `0x0010` and return near at `4AE5:0636`
without extra argument cleanup. The clear follows the optional call;
preservation of ES and the intended destination across it is not established.

The direct helper at `4AE5:06B1` has nineteen instructions and 51 bytes,
ending exclusively at `4AE5:06E4`. It tests current ES-relative byte
`0x0020` against interrupt opcode `0xCD` and returns directly on equality.
Otherwise it reads ES into AX, reads current ES-relative word `0x0010`
into DX, clears CX and calls the wrapper at `4AE5:0753`, whose helper and
SS-relative exchange are read in FND-EXE-234. The segment-register instruction
at `4AE5:06B9` reads ES into AX; its shipped encoding confirms this direction.

After that call, it writes returned CX to current ES-relative word `0x0002`.
This is not unconditionally a zero store: the wrapper can exchange CX with
an SS-relative word after a match. It reloads its separate loop count from
current ES-relative word `0x000C`, sets DI to `0x0020` and clears direction.
Every iteration reads the old target offset from ES-relative DI plus one
into DX before the overlapping stores. It reads current DS-relative word
`0x0110` into AX and stores that word through ES:DI, then stores retained DX
as the next word and zero as the next byte. The net DI advance is five
modulo 65536. Sixteen-bit LOOP repeats after the first set of stores; a
zero loaded count therefore gives 65536 iterations if accesses complete.
The helper returns near at `4AE5:06E3` without extra argument cleanup.

The existing target offset is read before the first store overwrites its
low byte. The new first word comes from live DS-relative state on every
iteration, rather than a fixed literal inferred solely from the entry guard.
Its writers, segment identity and output extents are not admitted here.

## Interpretation

This supplies one remaining publisher callee's explicit cleanup order,
including the resolved direct wrapper dependency and the unresolved near
callback. It distinguishes the guard literal from the callback target,
post-call CX from its initial zero, and the input count from a bound on
the number of stores. No caller or saved-storage preservation follows from
assuming that the calls return.

Q-EXE-001 and Q-EXE-010 retain every writer and native binding of the callback
word, caller DS/ES and header admission, SS-relative aliases and saved slots,
live state-word writers, output bounds, interrupt-enabled changes and other
publisher dependencies. No complete_reading or replacement inventory is
established.

## Alternatives

A direct call to the comparison literal is contradicted by the separate
DS-relative computed target. A segment clear before the callback is
contradicted by instruction order. An always-zero header-word store ignores
the wrapper's possible CX exchange. Zero rewrite stores for a zero count
are contradicted by the store-before-LOOP order.

## How to reproduce

At revision `2ddfe62`, use FND-EXE-226's source hash, original-source region
and default x86-bounds limits. Separately set entry and sole entries value
to `0x0004066F` (`4AE5:061F`) and `0x00040701` (`4AE5:06B1`), with no seeds
or summaries. Check the six/24 and nineteen/51 instruction/byte results,
returning-call assumptions and unresolved computed call in the first report.
CFG completion is not a Standard complete reading.

In the resident Ghidra snapshot, read-only with analysis disabled, run
ReportInstructionWindow at `4AE5:061F`, count 36, restricting the root to
its interval above; separately use `4AE5:06B1`, count twenty, restricting
the helper to its interval. Inspect the shipped instruction encoding at
file offset `0x00040709`, length two, for the ES-to-AX direction; never
infer segment-MOV direction from reversed displayed operands. Read
FND-EXE-234 for the wrapper's returned-CX and saved-slot obligations.
Sources, configurations and reports remain in GAME_DIR.
