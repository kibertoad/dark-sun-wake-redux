---
id: FND-EXE-401
title: Bounded installed resident literal search retains four shared-segment accesses and unresolved routes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0000..1425:1CE3
tool: scientific-method-engine 15.0.0, executable-reader 2.5.0, Capstone 5.0.7
environment: null
---

## Observation

A bounded search of the installed resident region for encoded literal 3F40
returns four instruction-owned memory operands, all using DS and word width:

| Site | Access | Independent control |
| --- | --- | --- |
| 1425:00C2 | Read outgoing argument | FND-EXE-399 |
| 1425:00EE | Read outgoing argument | FND-EXE-399 |
| 1425:0145 | Write returned word | FND-EXE-391 |
| 1425:0148 | Read for zero comparison | FND-EXE-391 |

The search examines 7395 byte starts. No matching immediate operand,
rejected overlapping match or unresolved-boundary match is returned. The
literal scan is neither truncated nor partial within its declared region.
Instruction traversal nevertheless retains 35 undecoded or unmapped edges
and twelve unresolved computed transfers. A complete literal scan is not a
complete traversal or a complete reading of those functions.

A narrower control query over 1425:00AF..1425:01C3 examines 276 byte
starts and returns the same four memory operands. Its traversal retains four
undecoded or unmapped callee edges. Both queries explicitly require all four
known sites as verified memory controls; raw or contested decodes cannot satisfy
those controls.

## Interpretation

This bounds the literal representation searched, not every access to the
storage. Other regions, computed displacements, implicit operands, segment
aliases and runtime-written code remain outside the result. Numerical DS
displacements do not establish a shared runtime segment between callers.
Q-EXE-007 still requires cross-region and computed writers, actual segment
provenance, callers, input production and native contracts. No complete-reading
declaration or whole-program negative writer claim follows.

## Alternatives

Treating the scan-complete flag as traversal completeness would discard 47
unresolved routes. Treating these four literal uses as all possible writers
would exclude computed and aliased accesses without evidence. Treating every
inventory entry as a fully read function would confuse analysis boundaries
with complete behavior evidence.

## How to reproduce

At revision 1cf7491c use the installed DSUN.EXE only: length 634416,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Run
`node tools/evidence/report.mjs x86-operand-candidates <local-config.json>`
with sourceKind mz, loadSegment 4096, query offset 16192, result limit
100, scanLimit 10000 and instructionLimit 20000. Retain configuration
and report in the licensed game's local analysis store, outside Git.

Declare one region with shipped-file start 37968, exclusive end 45363,
ip zero and segment 5157. Its 48 entry offsets, relative to that start,
are hexadecimal:

0044, 007F, 00AF, 00DB, 0107, 01C3, 02E1, 02FE,
031B, 0329, 041A, 0437, 0454, 0462, 04FF, 0591,
060B, 076B, 08E4, 09AD, 0B26, 0BB4, 0C04, 0C54,
0CDD, 0DE9, 0FA3, 1049, 1146, 1198, 12F0, 1383,
138D, 13BE, 13C8, 13D2, 13FE, 1684, 16BF, 16ED,
1720, 1769, 17AE, 1879, 18F6, 1954, 1B0B, 1CC2.

These starts are selected from that revision's committed installed function
inventory; they are traversal seeds, not assertions of complete readings.
Set positive controls to shipped-file offsets 38162, 38206, 38293 and
38296. Inspect the returned source identity, classifications, access widths,
segment choices, coverage, caps and all traversal gaps separately.

For the narrower query use start 38143, exclusive end 38419, ip 175,
segment 5157 and entries 38143, 38187 and 38231. Keep the same
four controls, set scanLimit and instructionLimit to 1000, and retain result
limit 100. Neither query executes original code. Relative branch targets,
computed displacements, implicit operands, segment-value alias proof and
runtime reachability are explicitly excluded.
