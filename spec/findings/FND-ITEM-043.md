---
id: FND-ITEM-043
title: Canonical RDFF item-test inputs have bounded source domains and no position six or nine or negative bonus
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00024274..0x0066105E
tool: Inspector rdff-look-chain-census at 723e1335; .NET 10; composition with FND-ACTOR-015, FND-PARTY-259, FND-RNG-035 and FND-ITEM-039/040/041
environment: null
---

## Observation

Q-ITEM-006 retains admitted producers of four attached-record inputs to
the item tests. This census measures their canonical RDFF source domains
across every kind-one 23-byte payload before each first complete end
header. It reuses the bounded archive/chain and source identity controls
of FND-ITEM-040/041. It does not label the unsigned byte's gameplay meaning
or infer field correlations from separate histograms.

Source OBJEX.GFF has length 6816516, XXH3-128
e08d3cac153c547f6804ca27b155915d and SHA-256
8e1e2db24df80dd605bb1d55df709cc550cc8b379dc4d2c2c23becaa5975b9a6.
The first-kind controls remain 1233,352,58 for kinds one,two,five.
There are 1643 RDFF resources, 3909 non-end chunks and 1931 kind-one
payloads. The last count independently agrees with FND-ACTOR-015's
1233 first and 698 later item chunks. Each histogram sums to 1931.
Record offsets below are hexadecimal; values and counts are decimal.

| Field | Read width | Value | Payloads |
|---|---|---:|---:|
| +12 | Unsigned byte | 0 | 85 |
| +12 | Unsigned byte | 3 | 2 |
| +12 | Unsigned byte | 4 | 525 |
| +12 | Unsigned byte | 5 | 426 |
| +12 | Unsigned byte | 6 | 893 |
| +13 | Signed byte | -1 | 1819 |
| +13 | Signed byte | 0 | 112 |
| +16 | Signed byte | 0 | 1785 |
| +16 | Signed byte | 1 | 63 |
| +16 | Signed byte | 2 | 50 |
| +16 | Signed byte | 3 | 31 |
| +16 | Signed byte | 4 | 2 |

Unsigned word +10 has 134 distinct values, from zero through 327.
Zero occurs in 1671 payloads and nonzero values in 260. Twenty-nine
payloads have a value above 255; the source word therefore cannot be
losslessly reduced to a byte. The full numerical histogram remains local.
The source has neither position six nor nine, nor a negative bonus byte.
The post-end bytes in RDFF/500 remain outside these histograms.

Synthetic controls distinguish unsigned 255, signed -128/-1/127 and a
word with a nonzero high byte at the four distinct offsets. Other initial
types are included; the existing first/later/end/trailing controls remain.
The integration gate passed all discovered Node controls and 757 .NET tests.
Two licensed-source reports are byte-identical, SHA-256
ffc738c98dd640dfad4f11b04f784e71728b1cfd99efedc8ac4262bf67301bcb.
A missed first-kind count or wrong source hash returns exit two with empty
stdout. No original function or game process was executed.

## Interpretation

Under FND-PARTY-259's admitted disjoint copy/own-fixup conditions, these
four fields initially retain the source values above. A runtime item with
position six or nine, or a negative bonus, consequently needs a later
write/restore, another source domain or a changed/aliased copy. It is not
explained by an unchanged canonical RDFF payload alone.

FND-ITEM-039's direct initializer writes byte +12 equal to one, outside
the measured source set, and leaves position +13 untouched. It is already
a concrete independent producer, so the source byte histogram is not a
universal runtime bound. FND-PARTY-217/221 likewise give position mutation
and transfer routes; their actual states and lifetimes remain independent.

The marginal counts do not prove which word +10 values share position
zero, nor which payload a native graph/selector returns. They do not assign
one meaning to all +10 values or admit the item tests' RNG/removal path.
Q-ITEM-006 no longer needs another source-domain search for these values;
remaining writers, other input domains, selection and use-time preservation
still require evidence. No confidence, parity or complete-reading promotion
follows.

## Alternatives

Signed -1 is not unsigned 255 in the position/bonus consumers; the
synthetic sign controls preserve that distinction. A source omission does
not prove a runtime value impossible. The known initializer's value one
already disproves treating the byte +12 source set as a universal range.
Separate histograms cannot replace a joint-domain or actual selection proof.

## How to reproduce

Build Inspector at 723e1335 and run rdff-look-chain-census on the licensed
OBJEX.GFF with look-source-controls.json under
GAME_DIR/analysis/item-removal-20261104/. Controls name the source SHA-256
and first-kind counts above. Retain item-input-domains.json and
item-input-domains-repeat.json. The itemInputs entries byte12, position13,
bonus16 and word10 give sorted numerical value/count rows and signedness.
Require each sum 1931, plus the independent chunk/item totals above.
Compare source identity, item/type totals and both complete reports.

Use look-source-missed-count.json and look-source-missed-hash.json for the
refusals; retained results are item-input-missed-count.refusal.txt and
item-input-missed-hash.refusal.txt. Reports stay in GAME_DIR, and no original
payload, complete histogram asset or fixture is committed.
