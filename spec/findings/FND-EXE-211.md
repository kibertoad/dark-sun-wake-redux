---
id: FND-EXE-211
title: Released operand search classifies both known indexed pointer-array publications as writes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A8330..0x004A83EA
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004A83F0..0x004A843D
tool: scientific-method-engine 13.6.0, executable-reader 2.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-022's two pointer-array publications occur at
`0x004A83C3..0x004A83CA` and `0x004A842B..0x004A8432`.
Both use a four-byte indexed memory destination with encoded base
`0x01BE6D60`. The first stores the saved new object; the second stores
the initially selected object. Their ordering remains as in FND-EXE-022.

A separate source-byte operand-candidate search over these two bodies,
with each independently read writer as a mandatory control, recovers exactly
these two candidates. Both classify as verified entry-path memory uses
with write access. It reports zero other verified operands, zero rejected
overlapping decodes and zero unresolved candidate boundaries. All 263
candidate starts in the declared regions are scanned without reaching
the scan, instruction or result limits.

This classifies encoded operands independently of Ghidra reference-type
labels. It does not admit effective addresses for all index values, array
allocation or lifetime. Entry-path verification assumes normal continuation
past calls; it is not runtime reachability evidence.

Containing-section coverage remains partial: bytes outside both bodies,
including their intervening gap, are unsearched. The report retains computed
transfer gaps at the two virtual calls and out-of-region targets at two
direct calls. Those unresolved callees can have additional effects.
Computed displacements, implicit operands, relative branch targets,
segment-value alias proof and runtime reachability are excluded.

## Interpretation

The released search passes FND-EXE-022's actual indexed-writer controls
without filtering references to WRITE or treating DATA as an access
classification. Q-EXE-009 retains array construction, index admission,
prefix bounds, virtual targets and remaining direct/indirect writers.
No complete writer census, complete reading or status promotion follows.

## Alternatives

DATA labels do not prove read-only access. Two recovered controls do
not prove the absence of writers in unsearched code or indirect callees.
Operand classification does not admit valid indexed or object storage.

## How to reproduce

Hash-check shipped interpreter XXH3-128
`09861838aa3018346f9f15c9a4f5925c`. Independently read the controls
with ReportInstructionWindow at `0x004A8330`, count 65, and
`0x004A83F0`, count 28, in the saved project read-only with analysis
disabled. Restrict claims to the cited bodies, excluding later output.

Run `tools/evidence/report.mjs x86-operand-candidates` with sourceKind
`pe32`, query offset `0x01BE6D60`, limit 128, scanLimit 1024 and
instructionLimit 256. Region rotation is shipped offsets
`0x000A7730..0x000A77EA`, entry `0x000A7730`; initialization is
`0x000A77F0..0x000A783D`, entry `0x000A77F0`. Both name FND-EXE-022
as bounds evidence. Mandatory controls are shipped instruction starts
`0x000A77C3` and `0x000A782B`, mapped to the preferred addresses above.
Inspect access, classification, counts, truncation, both region and section
coverage, and gaps separately. Retain exclusions and call-continuation
assumptions. Keep rich reports in the licensed-source store, outside Git.
Execute no original program.
