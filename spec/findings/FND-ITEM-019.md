---
id: FND-ITEM-019
title: The hit entry's source-declared adjacent relocated pairs enumerate the already-known casting call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00000000..0x0009AE30
tool: tools/evidence/report.mjs x86-pointers at 5e63a2de; executable-reader 4.3.0
environment: null
---

## Observation

Q-ITEM-009 retains incoming-route coverage after FND-ITEM-018's reading of
the three already-recorded direct hit callers. This checks one additional
representation: adjacent offset/segment words whose segment word is a
source-declared MZ relocation or FBOV fixup. The published pointer reporter
already supplies this census; no new scanner or original run was needed.
Reuse FND-SCRIPT-030's independently qualified controls, not a new reading
of their unrelated behavior.

### Source, target and enumeration

The source is installed DSUN.EXE, size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. The query supplies both loaded selector
56A7:006B and independently known canonical target file 6500F, overlay
179 +0D2F (FND-PARTY-096, FND-MAGIC-018). The reader verifies their mapping.
The report's load segment is 1000.

The source tables contain 4703 MZ relocation operands and 8262 FBOV fixup
operands, with 229 descriptors, 49 overlays and 854 trampolines. Of these,
12,844 adjacent pairs are inspected. The target enumeration is:

| Classification | Count | Qualified result |
|---|---|---|
| Exact pair | 1 | FBOV segment operand at file 7E7D1, offset operand at 7E7CF |
| Aliased target | 0 | No matching alternate loaded spelling in this enumeration |
| Unresolved | 0 | None within the selected adjacent-pair representation |
| Excluded | 121 | The explicit domain exclusions below |

The exact pair has stored descriptor word 0598, descriptor 179, stored low
bits zero, loaded selector 56A7:006B and resident trampoline file 4BCDB,
which resolves to canonical file 6500F. The pair is the operands of the
already-read overlay 193 +04DE call (FND-MAGIC-018, FND-PARTY-201), whose
local result handling FND-ITEM-018 records. The pointer report itself
labels it a candidate with instruction ownership/runtime use unread; the
instruction identity comes from those independent code readings.

Thus all target-matching pairs in this untruncated report consist of that
known call operand. This is a positive enumeration, not an absent-target
negative: negativeUsable remains false in the report. It adds no distinct
stored-data incoming route in the declared representation, and is not a
universal absence claim about other ways to reach the function.

The 121 exclusions consist of 120 preceding-word/segment combinations
forming nonwrapping addresses outside the resident load image, and one
pair whose two words do not lie in a single source range. They are retained
as exclusions rather than admitted pointers or unresolved runtime targets.
The report is not truncated. Repeating the complete query produces
byte-identical output.

### Controls

All four required segment-operand sites are inspected and resolve:

| File site | Expected canonical target | Independent evidence |
|---|---|---|
| 41342 | 41321 | MZ data pointer, FND-COMBAT-105 and FND-SCRIPT-030 |
| 0D46E | 8BF29 | MZ trampoline call, FND-CONFIG-136 and FND-SCRIPT-030 |
| 71466 | 2E7A5 | FBOV resident call, FND-SAVE-023 and FND-SCRIPT-030 |
| 7E7D1 | 6500F | Matching hit call, FND-MAGIC-018 and FND-PARTY-201 |

FND-SCRIPT-030 already records separate target-matching and alias controls
for the first three, as well as the cap/refusal behavior for the unchanged
reader/source. This query additionally matches its own independently known
hit control. Moving that required site to 7E7D2 causes an explicit missed,
excluded or unresolved positive-control refusal. No empty or refused query
is treated as evidence of non-use.

## Interpretation

The source-declared adjacent relocated-pair part of incoming coverage is
now enumerated for the hit entry, with the known casting call qualified
independently. Q-ITEM-009 need not repeat that representation scan while
the source, reader and controls remain unchanged.

Direct entry-path coverage, relative calls, split construction, unrelocated
pairs, computed targets, runtime copies and actual indirect use remain
separate obligations. Admission and whole-child effects/preservation are
also unanswered. This census establishes neither all incoming routes nor
any rule's complete_reading, confidence or parity status.

## Alternatives

- A raw pair is not automatically an instruction or runtime callback.
  The sole match is identified by the independent casting-call reading.
- Zero alias rows do not exclude pointers built by separate instructions,
  runtime copies or unrelocated data.
- The 121 excluded source-table combinations are not silently discarded
  admitted inputs; their domain reasons remain part of the report.
- The positive enumeration's false negativeUsable flag is retained;
  no absence certificate or whole-program completeness is claimed.

## How to reproduce

Use the repository wrapper and installed executable-reader 4.3.0 with a local
JSON configuration: source is verified installed DSUN.EXE, sourceKind mz,
xxh3 as above, target 0x6500F, query segment 0x56A7 and offset 0x006B,
limit 10000 and controls [0x41342, 0x0D46E, 0x71466, 0x7E7D1]. The loader's
default load segment is 0x1000, repeated by formatTables.loadSegment.

```text
node tools/evidence/report.mjs x86-pointers <local-config.json>
```

Repeat unchanged and compare output bytes. For the refusal control replace
only the final control with 0x7E7D2 and require a nonzero exit. Reuse
FND-SCRIPT-030's retained matching/alias/cap controls for this same reader
and source. Configurations and reports remain only in
GAME_DIR/analysis/hit-incoming-20261104/: hit-pointers.json,
hit-pointers.report.json, hit-pointers-repeat.report.json, missed-control.json
and missed-control.refusal.txt. The accepted report SHA-256 is
8735ff08c94dc847bbe98c7daa6d35f4aa7403ee1c502d78267de3f77a8d54de.
No original bytes, listing or extracted pointer tables are committed.
