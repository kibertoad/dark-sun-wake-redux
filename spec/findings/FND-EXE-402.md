---
id: FND-EXE-402
title: Extending the shared-word literal search into resident native helpers retains interrupt and computed boundaries
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0000..1425:1CE0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:0000..15F3:088B
tool: scientific-method-engine 15.0.0, executable-reader 2.5.0, Capstone 5.0.7
environment: null
---

## Observation

Extending FND-EXE-401's literal-3F40 query to the resident native-helper
region examines 9579 byte starts and returns the same four word-width DS
memory uses: reads at 1425:00C2, 00EE and 0148, and the write
at 0145. All four independently identified controls pass. No additional
matching immediate, overlapping candidate or uncertain-boundary candidate appears.
Neither region has unsearched starts, and the result is not truncated.

The two regions meet at shipped-file offset 45360 without overlapping.
The first ends at 1425:1CE0, three bytes before the end used by
FND-EXE-401; those three bytes are searched through the second region's
mapping instead. This is a new query, not a correction to the earlier result.

The combined traversal retains eighteen hardware or interrupt boundaries and
25 unresolved computed transfers. The additional helper entries expose reached
interrupt instructions, replacing the earlier query's unmapped callee edges.
The result does not admit native effects, follow all computed transfers or
establish runtime segment values.

## Interpretation

No encoded literal-3F40 access is added by this helper region. This does
not exclude writes through registers, other displacements, aliases, native effects
or other regions. Q-EXE-007 still requires those accesses, segment provenance,
actual callers and argument producers. The narrower representational search and
its traversal gaps do not establish complete writer coverage or a complete reading.

## Alternatives

An empty additional literal-match set is not absence of additional writers.
Treating interrupt boundaries as admitted native contracts would discard the
unresolved effects. Combining overlapping address mappings would make candidate
ownership ambiguous; this query instead declares adjacent file regions explicitly.

## How to reproduce

At revision 27fc4557 use installed DSUN.EXE, length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Use the reader-backed
`x86-operand-candidates` command and FND-EXE-401's query offset 16192,
four shipped-file positive controls, result limit 100, scanLimit 10000,
instructionLimit 20000 and modeled load segment 4096.

Keep its first region's start 37968, ip zero, segment 5157 and
48 explicit entry seeds, but set its exclusive end to 45360. Add a
second region with start 45360, exclusive end 47547, ip zero and
segment 5619. Its 29 hexadecimal entry offsets relative to its start are:

0039, 00BC, 00E9, 010C, 012E, 014A, 0170, 0191,
01B2, 01C1, 01F3, 020F, 025C, 02A9, 02C3, 0325,
0360, 036F, 0371, 03B3, 0423, 04DA, 05C8, 05EE,
0618, 0632, 06B6, 076B, 0851.

Select those seeds from that revision's installed function inventory; they do
not declare complete readings. Inspect classifications and per-region literal
coverage independently of all traversal gaps. Computed displacements, implicit
operands, relative branch targets, segment-value alias proof and runtime
reachability remain excluded. Keep licensed reports and configurations in the
game's local analysis store. No original code is executed.
