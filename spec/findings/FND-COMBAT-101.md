---
id: FND-COMBAT-101
title: Declared literal script entries reach the four local-selector sites whose direct assignments are nonzero
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00000000..0x00217249
tool: tools/research/exec-census/script_decode.py at 33d4aa51 and script_reach.py at 3a9c2f28; reach script SHA256 fd3fb4a69221b77ccbf2258cfce196020544001065e670ed60d31f08e1361fd8
environment: null
---

## Observation

This reading reuses FND-COMBAT-100's unchanged source identities and
decoded script index. It binds declared incoming controls and named
assignment sites for the four local-number selectors, not the seven
named-context selectors or complete runtime admission.

### Entry controls

GPL/4, GPL/119, GPL/137 and GPL/163 all begin at offset zero with
opcode 0x19, the return to the calling script of RULE-SCRIPT-002.
Walks from those four zero offsets stop without reaching a selector
site. These are return controls, not proof that a nonzero entry cannot
reach one.

Literal calls and registrations in the decoded index supply the following
38 distinct nonzero entry controls. The offsets in this table are decimal,
as script_reach.py accepts them; selector instruction offsets are hexadecimal.

| Script | Declared entry controls | Entries reaching its selector site |
|---|---|---|
| GPL/4 | 1, 22, 56, 90, 124, 163, 197, 231, 265, 340, 415, 434, 453, 487 | All except 1 reach 0x02A3 |
| GPL/119 | 1, 146, 336, 422, 456, 531, 1447, 1988, 2252, 2367, 4046, 4061, 4076 | 146, 336, 422 and 456 reach 0x0547 |
| GPL/137 | 1, 1450, 2345, 2729, 3075, 3280 | All six reach 0x1069 |
| GPL/163 | 1, 646, 1091, 1800, 4287 | Only 1 reaches 0x027A |

These controls come from opcode 0x14's calls and the registration
forms in FND-SCRIPT-027 and FND-PARTY-240: offset/script are operands
0/1 for 0x1B, 0x1C, 0x65, 0x66 and 0x6C..0x6F; operands 2/3 for
0x68, 0x69 and 0x70; operands 4/5 for 0x6A and 0x6B.
Examples providing positive controls include MAS/66's call of GPL/4
entry 1 and its box registrations, MAS/65's box registrations of
GPL/119 entries 146, 336, 422 and 456, MAS/50's GPL/137 registrations,
and GPL/184's call of GPL/163 entry 1. This is a declared literal-root
assessment; dynamic registrations, nested calls and restored records
are not excluded by this selection.

The supported walker reports no PROBLEM line for these 38 controls.
It reaches the four requested selector instructions from 13, 4, 6
and 1 controls respectively. It follows both conditional outcomes and
continues after a local call as if the call returned. A hit is therefore
a candidate control-flow route, not a measured execution or a value proof.

### Direct assignment surface

Selecting decoded opcode 0x16 assignments that name the corresponding
local number gives 22 instructions, all assigning a nonzero literal:

| Script and local | Assignment instruction offsets, hexadecimal | Values |
|---|---|---|
| GPL/4 local 11 | 03EE, 0411, 0434, 0457, 047A; 049D, 04C0, 04E3, 0506, 0527 | First five 66; last five 65 |
| GPL/119 local 0 | 013C, 0191, 01B4, 01FF | 66, 51, 66, 66 |
| GPL/137 local 5 | 058E, 0916, 0A8C, 0B64, 0C91, 0D72 | 67, 52, 69, 51, 59, 1 |
| GPL/163 local 0 | 000B, 004C | 60, 59 |

This counts direct named assignments only. It does not count writes
through an operand address, external game functions, buffer replacement
or a changed local-number pointer as though they were direct assignments.

## Interpretation

The four variable-selector sites have a finite set of declared literal
entry controls and explicit nonzero assignment candidates. Neither the
empty entry-zero walks nor the fact that the 22 assignments are nonzero
settles Q-COMBAT-056. Establishing a nonzero selector still needs the
relevant assignment on every admitted path and preservation through
intervening calls and local-storage lifetime changes.

FND-COMBAT-099 retains the final word-width forwarding and current-region
gate. FND-COMBAT-100 retains the other operand classes and decode gap.
This reading introduces no restored-health or native-transition claim.

## Alternatives

- A path can enter after an assignment, or local storage can start or become
  zero before the selector read. Literal source assignments alone do not
  exclude those states. The declared root list is not an exhaustive census
  of native, computed or restored script-entry sources.
- A shortest candidate route printed by the walker does not prove all routes
  pass the same assignment. Conditions, menu selections and local return
  admission remain separate from the walk's over-approximation.
- No unresolved transfer reported for these controls does not repair the
  GPL/37 gap or prove callee preservation. Those limits remain in
  FND-SCRIPT-024, Q-COMBAT-055, Q-COMBAT-056 and Q-COMBAT-057.

## How to reproduce

Use FND-COMBAT-100's source fingerprints, decoder and retained --all
index at GAME_DIR/analysis/reader-admission-20261029/scripts.txt.
For each row above, pass TAG/NUMBER@OFFSET controls to script_reach.py
with that index and wanted opcode 5E, using decimal entry offsets.
Keep the four entry-zero controls separate from the 38 nonzero controls.
Retain the latter output locally as literal-selector-entry-reach.txt;
check all four positive selector hits and every PROBLEM line.
Read each direct assignment in the decoded index, selecting opcode 16
whose destination is the table's exact local-number expression; retain
the 22-count control. Cross-check the root operand positions with
FND-SCRIPT-027 and FND-PARTY-240. No decoded scripts or route listings
are committed.
