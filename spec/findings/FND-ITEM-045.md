---
id: FND-ITEM-045
title: Decoded literal scalar-setter calls select nine fixed fields but neither item position nor word ten
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
tool: tools/research/exec-census/script_selector_census.py at 723e1335; Python 3.12.10; retained indexed source of FND-COMBAT-100 and scalar API of FND-PARTY-156/FND-ITEM-039
environment: null
---

## Observation

FND-ITEM-039 reads the independent scalar setter's kind-one position
field ten and word +10 hex field eight. This paired operand census asks
which fields the unchanged decoded top-level script function-49 calls
actually select. It does not include nested instructions or infer native
target kind or argument preservation.

The retained listing SHA-256 is
649122067103640f41d526cb3b1c4ca2bb853b4f71444cf5f6547cc809fc6445.
Both queries require the independently recorded 3203 top-level opcode-22
starts with four operands, plus the existing selector/error controls.
The first query classifies operand zero, the second operand two. Join
their rows by the unique resource/offset identities validated by the
reporter; do not join by incidental row number or discard nonliterals.

There are 3202 literal function operands, all from one through 53, and
one nonliteral. Exact function 49 occurs at 253 starts. Every one of
those 253 has a literal field operand, with the following complete counts:

| Field | Starts |
|---|---:|
| 0 | 8 |
| 1 | 1 |
| 3 | 3 |
| 7 | 63 |
| 13 | 8 |
| 19 | 18 |
| 25 | 76 |
| 32 | 40 |
| 33 | 36 |

These sum to 253. Neither field ten nor eight occurs in that selected
literal-function set. As concrete positive source checks, GPL/11 at 007F
selects function 49/field 33, and 0088 selects function 49/field 25.
FND-PARTY-156 independently binds function 49 to the scalar API and
field 33 to its kind-two consumer, distinguishing this from the generic
field accessor API. The census reports actual field values, not zero
defaults for a missing property or unknown expression.

The nonliteral function is GPL/27 at 01C8 hex: global word eight supplies
the function and global word five supplies the field. Neither can be
assumed different from 49/10 or 49/8 without a separate producer reading.
The GPL/37 decoder gap at 0D39..0D58, nested opcode-22 expressions,
changed streams and independent native entry routes remain outside the
selected domain.

Both operand reports reproduce byte-for-byte. Their SHA-256 values are
90e10a8dbbb841ed6311359b2eecd78654521b643901b4b6fdd4e60d74a1578d
for functions and
851c9d2594000fce4863a08987dfb3bd461c7edb0d2dc2b79ccdfee45c8b04ff
for fields. A wrong expected count or listing hash returns exit one with
empty stdout. The reporter's existing synthetic controls cover narrowing,
nonliteral/nested operands, arity, missed controls and malformed input.
No original script or game function was executed.

## Interpretation

Under preserved unchanged operands and scalar API/table identity, these
253 top-level literal-function calls cannot directly select the kind-one
position or word +10 stores. Other selected fields can still have alias
effects under different storage; this is a selector exclusion, not a
universal write-footprint or native absence proof. Q-ITEM-006 retains the
single dynamic-function source, nested/gap/other entries, changed state,
aliases, independent writers and actual use-time admission.

## Alternatives

The existence of scalar field-ten and field-eight branches does not prove
that these literal source calls choose them. Conversely, absence from
the 253 calls does not constrain the dynamic GPL/27 request or nested
instructions. A function or field expression is unknown, never implicit
zero; its actual value needs its own evidence.

## How to reproduce

Use GAME_DIR/analysis/reader-admission-20261029/scripts.txt with the hash
above, regenerated as FND-COMBAT-100 describes from GPLDATA.GFF length
2191945, XXH3-128 2bd1d2b989eef2dcb0545feb95975fca, and OBJEX.GFF
length 6816516, XXH3-128 e08d3cac153c547f6804ca27b155915d.

Run script_selector_census.py LISTING CONFIG twice. Both configurations
require controls total 912, sentinel 218, otherLiteral 588, nonliteral 106,
sentinelLiteralSecond 207, sentinelNonliteralSecond 11, errors 1.
Each operandQueries list has one object: opcode 34 decimal, arity four,
count 3203, operand zero for functions or two for fields. Select exact
literal function 49 and match field rows by resource/offset. Verify the
positive GPL/11 sites and retain the nonliteral GPL/27 row separately.

Under GAME_DIR/analysis/item-removal-20261104/, configurations and reports
are position-script-function-controls.json, position-script-function-domain.json,
position-script-field-controls.json and position-script-field-domain.json. The corresponding repeat files
and position-script-missed-count/hash configurations and refusals remain
there. No decoded script text, source-derived fixture or payload is committed.
