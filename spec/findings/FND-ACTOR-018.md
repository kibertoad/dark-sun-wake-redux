---
id: FND-ACTOR-018
title: Every known shipped inline kind-four payload equals its referenced shared type row
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00000000..0x00680304
tool: Inspector rdff-type-census at 24bc2788 with the existing GffArchive reader; FND-ACTOR-017's independent bounded-table control
environment: null
---

## Observation

FND-ACTOR-015 accounts for all 1216 known kind-four chunks in the shipped
68-byte RDFF resources. FND-ACTOR-017 binds their retained references to
ALL/4 and independently compares RDFF/600 with row 61. This census answers
Q-ACTOR-007's remaining full-corpus relationship, not the complete shared-row
field schema or every possible native consumer.

The committed command verifies the source before archive interpretation and
checks explicit controls for 1643 RDFF resources, 1216 inline records and
102 shared rows. Every selected resource passes the known two-header shape,
including field five, fifteen-byte child extent and complete end-header span.
Its unsigned retained reference is compared without normalization or clamping.

| Classification | Count |
|---|---:|
| Total RDFF resources | 1643 |
| Validated known 68-byte inline records | 1216 |
| Other-size resources, outside this model | 427 |
| ALL/4 rows at stride fifteen | 102 |
| Payloads equal to their indexed row | 1216 |
| Payloads matching only other rows | 0 |
| Payloads matching no row | 0 |
| Out-of-range references | 0 |
| Total reported payload-to-row equalities | 1231 |

The 427 exclusions are not treated as parsed or as evidence about other
layouts. FND-ACTOR-015's independent chunk census supplies the known target
set; the new comparison does not infer universal chunk coverage from size alone.

### Duplicate shared rows retain ambiguity

Nine selected payloads also match additional identical shared rows. Their
matching sets and numbers of selected payloads are:

| Matching row indices | Payloads |
|---|---:|
| 1, 4, 5 | 3 |
| 30, 34 | 2 |
| 40, 94 | 1 |
| 62, 65, 66 | 3 |

Their retained reference is always one of the matching indices. Thus all
payloads have indexed equality, while the report retains fifteen additional
equalities. The other-row-only classification being zero does not mean every
payload has a unique matching row. No source strings or payload bytes are
needed to record this distinction.

### Controls and repetition

The independent RDFF/600 control has reference 61, is in range, and matches
row 61 alone, agreeing with FND-ACTOR-017's separate fifteen-field comparison.
Repeated complete reports are byte-identical.

Four controls refuse with status two and empty standard output: wrong source
hash, expected inline count 1215, control reference 62 and control indexed-match
false. None produces a partial successful report. The accepted report has
1231 matching relations, below the command's 131072 relation limit; no result
is truncated. Synthetic tests separately exercise other-row/unmatched cases,
out-of-range references, duplicate rows, unsupported shapes and bound failures.

## Interpretation

For the fingerprinted installed source, every known inline kind-four payload
duplicates the fifteen-byte shared row named by its retained reference. The
reading that some of these 1216 payloads hold differing historical/custom
values is ruled out for this source. FND-ACTOR-017's known importer still
uses the retained index and skips the inline bytes.

This settles the corpus relationship in Q-ACTOR-007. Q-ACTOR-008 retains
the independent question of other native inline-payload consumers; this data
comparison establishes no global absence of such readers. FMT-ITEM-002 and
Q-ITEM-005 retain the full shared-row field meanings. Actual table/record
admission and later mutation remain Q-CONFIG-008 and Q-SCRIPT-022.

No native behavior is inferred solely from equality, no evidence status is
promoted, and no complete-reading declaration is added. No original game ran.

## Alternatives

- A mismatched or other-row-only payload would refute indexed equality.
  The full known set contains neither, with its independent control passing.
- Equality does not establish unique row identity. The nine ambiguous cases
  refute that stronger reading; retain the header's actual reference. Direct
  consumers can use type indices as well as row bytes (FND-PARTY-248).
- The 427 outside-model resources do not prove absence of other layouts or
  readers. Nor does equality prove that inline bytes are globally unused.
- These source copies do not establish equality after runtime table changes,
  different editions, rewritten saves or arbitrary index/alias states.

## How to reproduce

Use approved OBJEX.GFF size 6816516, XXH3
e08d3cac153c547f6804ca27b155915d and SHA-256
8e1e2db24df80dd605bb1d55df709cc550cc8b379dc4d2c2c23becaa5975b9a6.
Build the Inspector at 24bc2788 and run `rdff-type-census ARCHIVE CONTROLS`.
Controls require sourceSha256 above, resourceCount 1643, inlineCount 1216,
typeRows 102, controlResource 600, controlReference 61 and controlIndexedMatch
true. The control is from FND-ACTOR-017, not derived from this report.

Repeat and compare bytes. Check all classification totals and each row's
referenceInRange, indexedMatch and matchingRows. Group matchingRows lists
with more than one index to verify the four ambiguity sets above. Do not
replace the retained reference with the first byte-equal row.

Accepted Windows report SHA-256:
779e72b84298fdbb95ac139dbd7b2c8e61077c01905383a01536d3cdcf5fb5fc.
Controls SHA-256:
db2503ba038a8c095629dd98b18caa75ee9727c3bd9fca12f661b7c09184acb9.
Change sourceSha256 to zero, inlineCount to 1215, controlReference to 62
and controlIndexedMatch to false in separate runs. Require each specific
refusal, status two and empty stdout.

The controls, type-census.json and repeat, four type-*.refusal.txt files
and their configs remain under GAME_DIR/analysis/kind-four-reference-20261103/.
Reuse FND-ACTOR-017's independent header/payload/type-row reports and
FND-ACTOR-015's target-set census. No original archive member or byte array
is committed.
