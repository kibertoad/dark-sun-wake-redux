---
id: FND-SCRIPT-039
title: The decoded registration radius domain has 381 small literals and one local-variable operand
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
tool: tools/research/exec-census/script_selector_census.py at a6e8815b with its existing script_local_paths.py parser; Python 3.12.10; retained indexed source of FND-COMBAT-100
environment: null
---

## Observation

FND-SCRIPT-038 and FND-PARTY-261 establish the registration update
`(old & 0x80) | UINT8(p3)`. This census identifies the fourth operands
at every unchanged decoded top-level opcode 1B/1C registration, without
assuming that a nonliteral has value zero or that a source site runs.

The retained index has SHA-256
649122067103640f41d526cb3b1c4ca2bb853b4f71444cf5f6547cc809fc6445.
Independent top-level count controls from the earlier registration reading
are 347 opcode-1B starts and 35 opcode-1C starts, with four operands each.
The existing selector and decoder-error controls are also required before
accepting this index. Nested instructions are not extra top-level starts.

| Query | Starts | Literal fourth operands | Nonliteral | Literal range | Literal low-byte bit seven set |
|---|---:|---:|---:|---|---:|
| Opcode 1B | 347 | 346 | 1 | 3..40 | 0 |
| Opcode 1C | 35 | 35 | 0 | 1..25 | 0 |

The sole nonliteral is GPL/223 offset 1E9C hex. Its fourth operand reads
local word zero; the report retains its expression as nonliteral, with no
invented low word or byte. The registration names callback offset 7474
in that script and object -72. Its preceding indexed fall-through includes
a named assignment of zero to that local at 1E83, followed by compare-end,
two opcode-4F instructions and a positive-value test before the registration.
This nearby assignment is not by itself a proof of the value at the call:
incoming routes, intervening effects and condition feasibility need their
own admission and preservation reading.

The index retains its GPL/37 stop at 0D39 and recovery at 0D58. The gap,
changed streams, other entries and runtime operand changes remain outside
this decoded-domain result. FND-ITEM-042 gives a separate conditional
reading of the malformed instruction, not a repaired source index.

Synthetic controls report an incoming literal high bit, negative narrowing,
high-word-only values, nested/nonliteral operands and missed count/arity
refusals. The selected tooling gate passed. Two actual source-index runs
are byte-identical, SHA-256
3255cba7b84622b7a7147e34c8055200fec79b21839db8da9a2120efecaf4c50.
A wrong index hash or expected registration count returns exit one with
empty stdout. No original game function or script was executed.

## Interpretation

The 381 unchanged literal radius operands cannot set bit seven through
their own incoming byte. The writer still preserves an already-set high
bit; this census does not clear it or exclude independent skip-bit writers.
Only the one nonliteral site needs an additional fourth-operand value
reading within this decoded registration domain. Q-SCRIPT-023 holds that
independently answerable question, leaving Q-SCRIPT-017's other field,
caller-phase, pointer/index and lifetime obligations separate.

No claim is made that the registrations execute, that every source operand
remains unchanged or that the nonliteral can actually set the high bit.
No confidence, complete-reading or parity promotion follows.

## Alternatives

A large literal byte could have activated the skip marker; the positive
synthetic high-bit case is reported, while none occurs among these literals.
The one variable operand could produce a byte with bit seven set or remain
small; the census deliberately leaves both readings open. A preceding zero
assignment alone does not resolve intervening effects or alternate entries.

## How to reproduce

Use the retained GAME_DIR/analysis/reader-admission-20261029/scripts.txt
from FND-COMBAT-100. Its source identities are GPLDATA.GFF length 2191945,
XXH3-128 2bd1d2b989eef2dcb0545feb95975fca, and OBJEX.GFF length 6816516,
XXH3-128 e08d3cac153c547f6804ca27b155915d. Regeneration must preserve
the named listing identity and controls before this result is reused.

Run script_selector_census.py LISTING CONFIG. Config requires the listing
SHA above and controls total 912, sentinel 218, otherLiteral 588,
nonliteral 106, sentinelLiteralSecond 207, sentinelNonliteralSecond 11,
errors 1. operandQueries has two objects with decimal opcodes 27 and 28,
arity 4, operand 3 and positive counts 347 and 35 respectively.
Retained configuration/report files under
GAME_DIR/analysis/walk-script-20261104/ are registration-radius-controls.json,
registration-radius-domain.json and registration-radius-repeat.json.
Wrong-count and wrong-hash configurations/refusals use the corresponding
registration-radius-missed-count and registration-radius-wrong-hash names.
Read only GPL/223's indexed 1E50..1EE0 context for the nearby source
observation; no original listing or payload fixture is committed.
