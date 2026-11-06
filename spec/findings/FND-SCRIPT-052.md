---
id: FND-SCRIPT-052
title: Twelve counted script-entry producer families supply only the two known GPL37 pairs
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
tool: script_selector_census.py at 78bcfd4c over retained script_decode.py index from FND-SOUND-077; FND-SCRIPT-038, FND-PARTY-261 and FND-SOUND-079 count controls
environment: null
---

## Observation

Q-SCRIPT-024 retains independent entry mechanisms for GPL/37's rejected
atom. This census covers twelve documented script-entry producer families
together, distinguishing their operand layouts rather than searching
every occurrence of the resource number 37. FND-SCRIPT-009 and
RULE-SCRIPT-003 supply the cross-script call semantics; FND-SCRIPT-038
and FND-PARTY-261 supply registration and object-entry setter interfaces.

The reused all-script index has SHA-256
649122067103640f41d526cb3b1c4ca2bb853b4f71444cf5f6547cc809fc6445.
Its source identities are GPLDATA.GFF size2191945, XXH3-128
2bd1d2b989eef2dcb0545feb95975fca, and OBJEX.GFF size6816516,
XXH3-128 e08d3cac153c547f6804ca27b155915d (FND-SOUND-077).
The controlled reporter accepts 104919 top-level indexed instructions,
one GPL/37 decode error at 0D39 and its one inline echo, resuming at
0D58. The omitted span is not decoded coverage.

Two operand queries use identical count controls, first selecting the
GPL-number operand and then the preceding entry-offset operand. The
documented zero-based operand positions and independently recorded
occurrence counts are:

| Opcode | Sites | Offset operand | GPL-number operand |
| --- | --- | --- | --- |
| 14 | 797 | 0 | 1 |
| 1B | 347 | 0 | 1 |
| 1C | 35 | 0 | 1 |
| 65 | 261 | 0 | 1 |
| 66 | 185 | 0 | 1 |
| 68 | 150 | 2 | 3 |
| 6A | 321 | 4 | 5 |
| 6C | 11 | 0 | 1 |
| 6D | 305 | 0 | 1 |
| 6E | 291 | 0 | 1 |
| 6F | 699 | 0 | 1 |
| 70 | 163 | 2 | 3 |

Counts for opcode14 reuse FND-SOUND-079's literal transfer census;
1B/1C reuse FND-PARTY-261; the other rows reuse FND-SCRIPT-038.
Each query is rejected if its expected count or arity differs. Joining
the reports by opcode, resource and instruction offset pairs all 3565
sites one-to-one, with no unmatched offset row. All selected offset and
GPL-number expressions are simple literals; neither report has a
nonliteral selected operand in these families.

Exactly two selected GPL-number operands have low word 37, both literal
37 in opcode6E:

| Registration | Entry offset, low word | GPL-number word |
| --- | --- | --- |
| GPL/48 at 0363 | 0001 | 37 |
| MAS/1 at 0135 | 0A41 | 37 |

These reproduce both independent positive controls from FND-SCRIPT-045.
No other selected row has GPL-number low word 37. In particular, the
797 selected cross-script opcode14 calls supply no additional GPL/37
pair. This is an exclusion within the counted decoded producer forms,
not a census of every way original code can obtain or change an entry.

## Interpretation

The selected shipped top-level entry-producing families have no unresolved
computed offset or resource operand, and supply no third GPL/37 pair.
FND-SCRIPT-051 separately excludes the known offset0001 entry's listed
literal connection to the pre-gap postamble. Consequently Q-SCRIPT-024
can focus on actual admission and retention of the 0A41 registration,
converted entry168 and mechanisms outside this census, rather than
repeating searches for more literal pairs in these same families.

Nested instructions, other opcode mechanisms, the omitted decode span,
native construction, restored records, changed code/cursors/frames and
runtime table/record aliases remain outside this result. A literal source
expression is not a native value observation or proof that its parameter
reader, source buffer or consumer binding is preserved. The existing
state, pool, key, loading and callback obligations remain open. No queue
item, whole-entry complete reading or status is closed or promoted.

## Alternatives

A text search for 37 cannot distinguish an entry's script operand from
unrelated values or account for the different cell/box/pair layouts.
Conversely, a complete result for these 3565 indexed sites cannot exclude
entries introduced by other mechanisms. The two GPL/37 pairs also do not
prove that either registration actually executes or persists.

## How to reproduce

Use script_selector_census.py on the retained index with the SHA-256 above.
Keep configs and reports under GAME_DIR/analysis/handler-census-20261106/:
gpl37-entry-producer-script-config.json and its report, and
gpl37-entry-producer-offset-config.json and its report. Use operandQueries
with the table's opcode/count/operand positions and arities two for14,
four for1B/1C/70, five for68, seven for6A and three for the other rows.

Reuse the explicit selector controls from FND-COMBAT-100/105: total912,
sentinel218, otherLiteral588, nonliteral106, sentinelLiteralSecond207,
sentinelNonliteralSecond11 and errors1. Require the decoder error and
inline echo rather than treating successful report generation as a full
decode. Require all per-family counts, zero nonliteral selected operands,
the one-to-one site join and the two independent GPL/37 positive controls.
Match the GPL operand by low word and recover its corresponding offset
from the joined report, never from a neighboring text line. No new parser,
original-game run or emulated call was used; no derived report is committed.
