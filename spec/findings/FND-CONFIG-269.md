---
id: FND-CONFIG-269
title: Controlled setup index-word cohorts contain only the twenty post-dispatch stores
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 17FB:03D3..17FB:0668
tool: tools/evidence/report.mjs x86-operand-candidates; Capstone 5.0.9; pypcode 4.0.1
environment: null
---

## Observation

Q-CONFIG-005 retains the relevance of FND-CONFIG-268's DS:EBCE/EBD0
stores to subsequent settings construction. Before following these words
as presumed inputs, this reuses the controlled setup literal-operand
census from FND-CONFIG-259/260. The source is SOUND_DS.EXE,204593 bytes,
XXH3-128236c2dc23c071eca421eb5b427caee57. Numbers below are hexadecimal
except explicit counts. This is a bounded representation census, not
a proof about every possible state access or native execution.

The source,25 declared regions, established entry/transfer declarations,
58208 scanned starts, decoder and limits are unchanged. Query offsets
are EBCE and EBD0 independently, with scanLimit1000000 and limit10000.
Positive controls are the first two instruction-backed stores for each
word from268. Repeated result files match exactly. Negative controls
replace the first positive start with its next byte and retain the second
positive start; both negative configurations fail the positive-control
requirement as expected. They do not produce accepted negative results.

| Literal | Positive starts, file offsets | Interior control rejected | Verified memory uses | Other operands | Overlap rejections | Unresolved boundaries |
| --- | --- | --- | --- | --- | --- | --- |
| EBCE |97A9,97EB |97AA |10 |0 |0 |0 |
| EBD0 |97C2,9804 |97C3 |10 |0 |0 |0 |

Every verified use is a word write in17FB:03D3, matching268's complete
ten-arm instruction reading. No returned verified use is a read. The
file-offset cohorts, in priority order, are:

| Card | EBCE write | EBD0 write |
| --- | --- | --- |
|66 |97A9 |97C2 |
|67 |97EB |9804 |
|73 |982D |9846 |
|7A |986F |9888 |
|79 |98B1 |98CA |
|6A |98F3 |990C |
|7C |9935 |994E |
|7D |9977 |9990 |
|81 |99B9 |99D2 |
|82 |99FA |9A13 |

Both reports are untruncated, declare partialSearch false and retain59
walk gaps. That means the literal search covered the declared region
scope, not that its instruction walk resolved every transfer. Computed
displacements, implicit/block operands, segment aliases, changed streams,
other entry mechanisms and runtime reachability remain excluded or
unresolved. An address loaded into a register or another representation
of these bytes is not settled by these queries.

## Interpretation

The known literal cohorts supply no consumer by which to infer that the
stored words select a constructor input. Q-CONFIG-005 no longer needs
another reading of an unclassified literal EBCE/EBD0 use: all twenty
returned sites are268's assigned stores. A broader consumer/lifetime
claim requires evidence for a mechanism outside this census, rather than
assuming03D3's writes feed the later choices. Preserve the59 gaps and
excluded-access obligations alongside the actual incoming selector,
dialog index and record backing conditions. This does not establish dead
state globally or prove the words irrelevant to native execution.

No original or emulated call was run. No complete-reading declaration,
format-status promotion, queue-item closure or parity validation is claimed.

## Alternatives

No literal read in a bounded cohort is weaker than no read anywhere.
Untruncated output is not complete executable reachability. The positive
controls establish instruction boundaries for known stores, not segment
alias immunity or absence of implicit and computed access.

## How to reproduce

Under GAME_DIR/analysis/config-block-construction-20261108/, copy
ec-query.json's unchanged source/region/entry/transfer configuration.
Set query.offset to EBCE or EBD0 and controls to the respective two
positive file offsets above. index-ebce-query.json and
index-ebd0-query.json retain those inputs. Run report.mjs
x86-operand-candidates twice for each. With the checkout-local locked
EVIDENCE_PYTHON, successful report/repeat SHA256 values are:

EBCE: ff8811bdd1322f1933deeecf9b49742f535660ced8c8075207248b5a2f152349.

EBD0: 60f0b41cd60c1291af168fa768ebbc2a076319c33e07a2597df070a84b1a82b2.

index-ebce-negative-query.json and index-ebd0-negative-query.json retain
the interior-control substitutions; their logs retain the expected
control refusals. Only successful controlled reports support this finding.
Cross-check all returned sites against268's retained instructions.
Keep all original-derived configurations and reports in GAME_DIR.
