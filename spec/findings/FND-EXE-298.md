---
id: FND-EXE-298
title: Bounded direct-writer and near-call searches retain a further allocator caller lead
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00006915..0x0000691F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00006AF0..0x00006AF3
tool: executable-reader 2.5.0, Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

A bounded explicit-CS direct MOV destination scan over shipped resident
half-open 0x00005200..0x00057570, selecting offsets 1363 and
1365, returned two instruction-byte candidates: 0x00006915 stores
DX to CS:1363 and 0x0000691A stores AX to CS:1365. These are
the independently entry-read stores in FND-EXE-295. The scan's control
is that earlier reading, not a location first inferred from this scan.
It does not search other write opcodes, computed addresses, segment
aliases, implicit segment accesses or prefix arrangements that do not
start at the explicit CS prefix. It is not a writer-completeness result.

The relocation-aware incoming query for FND-EXE-295's wrapper at
shipped 0x00039826 returned no matching declared candidates, without
truncation or unresolved candidates, at limit 100. Its independent known
MZ-call control 0x00040DA9 resolved to 1000:15A5. The query excludes
near calls, computed calls, unrelocated pointers and instruction-boundary
verification. There is no independent FBOV-fixup control in this query,
so its empty result does not establish a controlled absence across that
reference kind or prove that the wrapper is unused.

A separate near-call byte scan in the modeled segment-1000 window
0x00005200..0x00015200 selected sixteen-bit relative calls whose
wrapped destination IP is 1702 or 15A5. There were no candidates
for 1702 in that window. For 15A5 it returned 0x00006830,
0x0000695F and 0x00006AF0. The first two are independently read
near calls in FND-EXE-297 and FND-EXE-295, supplying controls for
this relative form. An isolated decode at the third candidate gives a
three-byte near call from modeled IP 18F0 to 15A5. Its incoming
instruction path, argument production and native CS admission remain
unproved here.

## Interpretation

The shared request stores remain tied to FND-EXE-295's local producer;
the selected direct-write form alone does not settle other writers or
aliases. The far-call query and the modeled near-call window are separate
domains and cannot be combined into complete caller coverage. The new
near-call candidate adds a concrete obligation before treating the allocator's
current callers and their request ranges as complete.

Q-EXE-010 retains that candidate's grounded entry/path and arguments,
other segment bindings, computed and indirect transfers, shared-state
aliases and lifetime, and initialized header/extent admission. No complete
reading or general caller/writer-absence claim follows.

## Alternatives

Treating an empty declared incoming query as an unused wrapper ignores
its excluded call forms and missing fixup control. Treating two direct MOV
stores as every writer ignores aliases and other forms. Treating a modeled
relative destination as native reachability ignores the missing entry path
and CS admission. Treating the known allocator callers as exhaustive
ignores the additional byte candidate.

## How to reproduce

At revision 275a54c require FND-EXE-176's installed DSUN.EXE identity,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Repeat
FND-EXE-295's explicit-CS direct MOV scan with selected displacements
1363 and 1365, using its independent entry reading as the control.
Run the committed incoming reporter with loadSegment 1000, target
0x00039826, limit 100 and controls [0x00040DA9]. For the near
scan, examine every start in 0x00005200..0x000151FE whose next
three bytes fit the segment window. Select opcode E8 and calculate
destination IP as start minus 5200 plus three plus signed little-endian
displacement, modulo 65536; select targets 1702 and 15A5. Decode
0x00006AF0..0x00006AF3 in sixteen-bit mode at IP 18F0 with locked
Capstone 5.0.7. Keep the modeled segment and excluded forms explicit.
Source bytes and reports remain outside Git. No original execution or
complete transfer search is claimed.
