---
id: FND-ITEM-060
title: Joint canonical item inputs assign conditional equipment resets to quantity and word zero E
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
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000120E8..0x00017C28
tool: tools/DarkSunWakeRedux.Inspect/RdffKeyInspection.cs at b379b125 and tools/evidence/report.mjs table; composition with FND-ITEM-056/057/059
environment: null
---

## Observation

FND-ITEM-059 leaves reset-destination counts unanswered because the previous
all-item report retained marginal fields while its detailed Look rows covered
only type 50. The extended bounded census retains resource/chunk identity,
type, quantity, word +0E, word +10 and signed bonus together for every item.
This removes that source-pairing obligation from Q-PARTY-109 and Q-ITEM-003.
It does not admit a native call or assume source fields survive child calls.

### Controlled joint domain

The report contains 1931 distinct resource/chunk pairs. Removing the added
itemHookInputs property reproduces every field of the previous census,
including 1643 resources, 3909 chunks, 633 type-50 items and 500 resources
with excluded trailing bytes. The joint W/bonus histograms also reproduce
the earlier marginal domains. All types are below the 102-row source table;
all W values are 0..327 and all bonuses are nonnegative.

Join each item's type to ALL/4 byte +0B, testing mask 02 hex as in
FND-ITEM-059. For ordinary W=1..319 join DATA ID W-1 and take its first
byte's low two bits, as FND-ITEM-057 describes. The complete source partition
is below. Destination names describe what reset would select if called with
these retained fields, including rows whose hook gates would skip reset.

| Source branch | Items | Quantity destination | Word +0E destination |
| --- | ---: | ---: | ---: |
| W zero | 1671 | 1598 | 73 |
| Ordinary, DATA property zero | 52 | 51 | 1 |
| Ordinary, DATA property one | 93 | 92 | 1 |
| Ordinary, DATA property two | 85 | 85 | 0 |
| Ordinary, DATA property three | 10 | 10 | 0 |
| Special W=320..327 | 20 | 20 | 0 |

Of the 92 ordinary property-one quantity candidates, 71 already have
quantity equal to W and 21 differ. The single word-+0E candidate has type
70, and its word +0E differs from W. Of the twenty special candidates, ten
already have quantity equal to W and ten differ. Thus the 113 conditional
reset candidates comprise 81 initially equal destination words and 32
initially different ones. These are source-payload counts, not invocation
counts or observed mutations.

W-zero items return before the hook's reset path. Ordinary properties zero,
two and three do not reach reset under the retained nonnegative-bonus
conditions of FND-ITEM-056. In particular, the additional type-six
word-+0E candidate in property zero is not another accepted dispatch case.
For property one, reset still requires the status gate to return zero and
the DATA path to obtain matching valid content. Special candidates require
the status gate and returning rebuild before reset. Actual source/type and
destination identity at reset remain separate admission obligations.

## Interpretation

The source join rules out assigning all 93 property-one resets to quantity,
or treating all special candidates as the alternative destination. It also
distinguishes equal-value writes from initially value-changing candidates.
It does not prove a live quantity change, charge semantics, dispatch success,
an equipment effect or a rollback defect. Q-PARTY-109 retains actual owner,
record/type/frame preservation, effect-four membership, DATA cache/read and
child effects, reset rereads, dispatch and independent flag writers/consumers.
No status, parity or complete-reading promotion is claimed.

## Alternatives

Multiplying marginal type and W frequencies would lose the observed pairing.
Using the type-50 Look rows would omit almost every conditional reset case.
Counting every nonzero W as an invoked reset would ignore DATA/status gates;
counting every reset as a changed value would ignore the initially equal
destinations. Later mutations can change any of these conditional domains.

## How to reproduce

Use OBJEX.GFF length 6816516, SHA-256
8e1e2db24df80dd605bb1d55df709cc550cc8b379dc4d2c2c23becaa5975b9a6,
and RESOURCE.GFF length 5724669, XXH3-128
92fe031616f36062f99484b0f728210e. Run Inspector's
rdff-look-chain-census with look-source-controls.json: first kinds one 1233,
two 352 and five 58. The existing framing/hash controls precede field reads.
Run twice and compare complete output bytes. Check unique resource/chunk
pairs and compare all old census properties with item-input-domains.json.

Use report.mjs table for OBJEX.GFF, XXH3-128
e08d3cac153c547f6804ca27b155915d, start 1F291 hex, count 102, stride fifteen,
one one-byte resetFlags field at offset eleven; count evidence is ALL/4
length 1530. Reuse FND-ITEM-057's controlled 320-row DATA property report.
For each joint row, partition W as zero, ordinary 1..319 or special 320..327;
ordinary property is DATA[W-1].property & 3. Destination is word0E when
ALL4[type].resetFlags & 2 is nonzero, otherwise quantity. Group and count,
then compare that row's selected destination with its W. Reconcile the sum
with 1931 and each W/bonus histogram with census.itemInputs.

Reports remain under GAME_DIR/analysis/item-removal-20261104/:
item-hook-inputs.json and its -repeat.json, placement-reset-type-controls.json,
placement-reset-types.json and its -repeat.json, and the derived
placement-reset-source-summary.json. Item report SHA-256 is
97fcd73c9ece0d53392ae6daa6a656f10542b4ff7d5789aa8a00a63f1307ac13;
type report SHA-256 is
1be826cc83a6f104ebc392b4c61e60f93fcced0c16e71ccb87b8d2aace76029f.
Both reports repeat byte-for-byte. No licensed payload, fixture or original
code is committed, and the original game is not executed.
