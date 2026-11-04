---
id: FND-ITEM-047
title: Printed nested scalar-setter requests select field twenty-five only
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
tool: tools/research/exec-census/script_nested_census.py at aebe9e76; Python 3.12.10; retained decoded index of FND-COMBAT-100
environment: null
---

## Observation

FND-ITEM-045 excludes nested instructions from its top-level scalar request
census. This separate balanced-brace census covers the printed nested domain
of the same unchanged retained index, whose SHA-256 is
649122067103640f41d526cb3b1c4ca2bb853b4f71444cf5f6547cc809fc6445.
It does not execute scripts or decode the GPL/37 gap.

An independent marker query counts 3643 nested instruction markers, including
374 opcode-22 markers. Positive identities are GPL/1 at 0071 and GPL/2 at
010B and 020B, all opcode 22 (hexadecimal offsets/opcode). The committed
balanced parser matches both counts and all three identities, while separately
validating the top-level index hash, ownership and selector/error controls.

Every one of the 374 nested opcode-22 instructions has four operands and a
literal function selector. Five select exact function 49; all five have literal
field selector 25. None selects field eight or ten. The scalar API binding is
FND-PARTY-156/FND-ITEM-039. Parent/depth identities describe source nesting,
not native call histories or proof that a nested expression is evaluated.

The private report SHA-256 is
4165c2a588a3a7aa1f5f832e70999f7911905526d8aa2e7b5e986d9eab3934ea.
Synthetic controls cover recursive identity, signed narrowing, unknown
expressions, absent selected opcodes with unrelated positive controls,
malformed ownership, arity, braces, missed counts/hash/identities and the
32-level depth boundary. The selected tooling gate passed before the source
query. No original function or script was executed.

## Interpretation

Under unchanged operands and the preserved scalar API binding, the printed
nested requests cannot directly select the kind-one position field ten or
word +10 hex field eight. Together with FND-ITEM-045 this removes the printed
literal top-level and nested selector gaps. FND-ITEM-046 separately constrains
sixteen positive setups of the top-level dynamic function request.

This is a source selector exclusion, not exhaustive native writer coverage.
Q-ITEM-006 retains other incoming routes, decoder gaps, changed streams,
intervening state/dispatch effects, aliases, independent stores and actual
selection/storage/lifetime admission. Other fields may affect aliased storage;
the census does not prove a universal write footprint.

## Alternatives

Treating a nested call as missing because it is not a top-level start would
leave the source domain incomplete. Conversely, counting a printed node does
not establish native reachability. Runtime mutations or a different entry may
produce requests absent from this unchanged index; those require independent
evidence rather than interpreting this source absence as native absence.

## How to reproduce

Regenerate the retained index as FND-COMBAT-100 describes. Run
script_nested_census.py LISTING CONFIG with index.listingSha256 as above and
index.controls total 912, sentinel 218, otherLiteral 588, nonliteral 106,
sentinelLiteralSecond 207, sentinelNonliteralSecond 11, errors 1. Require
opcode 34 decimal, arity 4, expectedNodes 3643, expectedSelected 374 and
positiveControls resource/offset/opcode GPL/1/113/34, GPL/2/267/34 and
GPL/2/523/34. Independently count markers with the pattern
`\{([0-9A-F]{4}): ([0-9A-F]{2})(?: |\})` on canonical indexed lines,
then compare counts and positive identities before accepting the census.
Select report rows whose first operand literal equals 49 and count their
third operand literals; retain nonliterals explicitly rather than as zero.

Private configuration nested-scalar-controls.json and report
nested-scalar-domain.json remain under
GAME_DIR/analysis/item-removal-20261104/. No original-derived script text,
fixture or payload is committed.
