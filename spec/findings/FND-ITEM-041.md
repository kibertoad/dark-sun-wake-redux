---
id: FND-ITEM-041
title: Every canonical RDFF item starts with a valid name regardless of its initial type
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
tool: Inspector rdff-look-chain-census at 8182a5b4; .NET 10; composition with FND-ACTOR-015, FND-PARTY-259 and FND-ITEM-036/038/039/040
environment: null
---

## Observation

FND-ITEM-040 excludes unnamed canonical RDFF payloads initially of type 50.
Q-PARTY-115 still allows an item of another initial type to become type 50.
This extends the same bounded chain census to the name word of every
kind-one payload, keeping a sorted type histogram and separate metadata
locations for any unnamed item. It reads no text or additional payload fields.

The source is OBJEX.GFF, length 6816516, XXH3-128
e08d3cac153c547f6804ca27b155915d, SHA-256
8e1e2db24df80dd605bb1d55df709cc550cc8b379dc4d2c2c23becaa5975b9a6.
The unchanged first-kind controls are 1233,352,58 for kinds one,two,five.
Only payloads before the first complete end header are included.

| Measurement | Count |
|---|---:|
| Resources | 1643 |
| Non-end chunks | 3909 |
| Kind-one item payloads | 1931 |
| Distinct initial item types | 87 |
| Item name words below 9999 | 1931 |
| Item name words at least 9999 | 0 |
| Type-50 payloads, all named | 633 |

The observed type numbers range from 1 to 101; not every intervening type
occurs. Summed histogram counts equal 1931 and summed named counts also
equal 1931. The item total independently agrees with FND-ACTOR-015's
1233 first and 698 later kind-one chunks. The 633 type-50 cases agree
with FND-ITEM-040. The one trailing resource is retained without scanning
its post-end bytes.

A synthetic unnamed non-50 item appears in the unnamed-item report and
its own type bucket without becoming an unnamed type-50 item. Distinct
named/unnamed type-50 boundaries and later/trailing controls remain covered.
The tooling gate passed 192 Node and 756 .NET tests. Two source reports
are byte-identical, SHA-256
c990caa98b42e3365c9546d88a5fb0e6d6c03df6fa5f252165a2f367588db30c.
No original game function was executed.

## Interpretation

Under FND-PARTY-259's admitted disjoint copy and own-fixup conditions,
every canonical RDFF item payload starts with a name below 9999. A later
type-only change preserving that name cannot create the invalid-name
prerequisite of FND-ITEM-036's independent type-50 Look gate. This removes
the remaining initial-type distinction within that canonical source domain.

It does not establish lifetime preservation, successful native copying,
all subsequent chunk/callee effects or storage separation for every caller.
A name-invalidating direct/computed write, overlapping copy or alias,
changed runtime stream/metadata or another source domain remains possible.
FND-ITEM-038/039 are conditional known-writer exclusions, not a universal
proof that names cannot change. Actual Look admission, fallback and later
door gates remain Q-PARTY-115. No status or parity promotion follows.

## Alternatives

An unnamed non-50 source might have become type 50 without a name write.
The synthetic control reports that case; none occurs in these 1931 payloads.
Other archives, runtime-created buffers and later replacements are outside
this negative source result. It must not be restated as all runtime items
being named or as the Look branch being unreachable.

## How to reproduce

Build Inspector at 8182a5b4 and run rdff-look-chain-census with the licensed
OBJEX.GFF and look-source-controls.json under
GAME_DIR/analysis/item-removal-20261104/. The controls specify the source
SHA-256 and first-kind counts above. Repeat into look-all-types.json and
look-all-types-repeat.json. Compare identity, aggregate counts, sorted
itemTypes and empty unnamedRows, as well as the existing type-50 rows.
Cross-check total chunks/items against FND-ACTOR-015 and selected type-50
counts against FND-ITEM-040. The committed tool verifies source identity
before interpreting the archive; framing and positive-domain refusals remain
unchanged. All original-derived reports stay in GAME_DIR.
