---
id: FND-EXE-172
title: Independent physical rel32 scan agrees with the empty-table helper's decoded caller domain
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004011B2..0x004011B7
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00401124..0x00401129
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004010BE..0x004010C3
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005C4500..0x005C4528
tool: Hash-guarded physical PE32 transfer scanner at repository revision c3beeb8
environment: null
---

## Observation

FND-EXE-011's verified shipped PE contains one physically backed executable
search region: file offsets `0x00000400..0x002EEBF4`, mapped to preferred
addresses `0x00401000..0x006EF7F4`. The scan derives that domain from its
validated sections, independent of analyzer function boundaries. It excludes
raw padding beyond the section's virtual extent and virtual-only bytes.

For the half-open target range `0x005C4500..0x005C4528`, including the
helper's interior, the physical five-byte relative-transfer scan finds one
candidate: call at `0x004011B2` to the normal entry `0x005C4500`. It finds
no additional candidate targeting the interior in this declared domain.
The independent positive-control range `0x005C4530..0x005C4537` finds
calls at `0x004010BE` and `0x00401124`, both targeting `0x005C4530`.
All three candidates agree with FND-EXE-171's decoded-reference and
instruction-text search. Three results remain below the combined cap of 128.
A repeated report is byte-for-byte identical; this is not independent reproduction.

Each candidate is computed from a possible E8/E9 start and a signed four-byte
displacement, with 32-bit target wrapping. This calculation does not prove
an instruction boundary, prefix interpretation, reachable path or function
ownership. The decoded evidence in FND-EXE-171 is kept separate from the
physical candidate test; matching reports do not establish an unread caller.

A separate enumeration of resolved call/jump targets across all decoded
instructions finds the same external call into the helper's normal entry
and two internal jumps: `0x005C4508` to `0x005C451E`, and
`0x005C4524` to `0x005C4510`. FND-EXE-171's independently read body
provides controls for both the ordinary and conditional jump classes.
The control range again finds its two known calls. Both queries complete
below their fifty-site caps. No additional decoded resolved flow enters
the helper's interior in that enumeration. Unknown computed destinations,
undecoded bytes and runtime modifications remain excluded; this is a
decoded-flow comparison, not another physical encoding search.

## Interpretation

This closes the independent physical rel32 comparison for the fixed-bound
helper. Together with FND-EXE-171 it rules out an additional five-byte
candidate into that target range within the mapped executable domain.
It does not locate every possible caller or interior entry: rel8/rel16,
conditional, far, indirect and computed transfers, cross-region encodings,
non-executable storage, and runtime-written code remain outside this scan.
Q-EXE-009 retains those admission obligations and the larger startup route.
No original program was executed and no complete_reading is declared.

## Alternatives

An additional E8/E9 four-byte-displacement candidate into the queried
interior, or disagreement between this physical scan and the decoded calls,
is ruled out within the complete declared search domain. Treating a raw
candidate as a verified call, or treating absence in this domain as absence
of every transfer representation, is not justified.

## How to reproduce

At repository revision c3beeb8 run
`node tools/evidence/report.mjs pe-transfers <local-config.json>`.
Set source to FND-EXE-011's verified DOSBOX/DOSBox.exe, sourceKind to `pe32`,
XXH3 to `09861838aa3018346f9f15c9a4f5925c`, and limit to 128. Supply targets
`[{start: 0x005C4500, end: 0x005C4528}]` and independent controls
`[{start: 0x005C4530, end: 0x005C4537}]`. Each control must produce a
candidate; cap overflow fails rather than returning a partial search.
Compare candidate sites and destinations with FND-EXE-171, and repeat the
command to compare reports byte-for-byte. Keep configurations and results
in GAME_DIR; commit neither original bytes nor broad analysis exports.
With Ghidra 12.1.3 PUBLIC and scientific-method-engine 13.5.0, query
ReportCallsToRange with inclusive endpoints `005C4500`, `005C4527`, mode
`all`, and independently with control endpoints `005C4530`, `005C4536`,
mode `all`. Each has a fifty-site cap. Use the verified saved PE project
read-only with automatic analysis disabled. Compare the internal jump
controls with FND-EXE-171's body reading and retain the decoded-flow
exclusions. No analyzer function-boundary list defines the search domain.