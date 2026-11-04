---
id: FND-ITEM-040
title: All type-fifty item payloads in canonical RDFF chains have valid names including the two later items
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
tool: Inspector rdff-look-chain-census at 5d65424f; .NET 10; composition with FND-ACTOR-015, FND-PARTY-259 and FND-ITEM-036/037/038/039
environment: null
---

## Observation

FND-ITEM-037 counted first-item inputs only. This extends the source-domain
measurement to every kind-one payload before each RDFF chain's first
complete end header. It uses the same bounded archive reader, source hash
and first-kind controls, then validates each ten-byte header, payload width,
backward reference and chain bound. It stops at the end marker, retaining
trailing-resource counts rather than interpreting those bytes as more items.

Source OBJEX.GFF has length 6816516, XXH3-128
e08d3cac153c547f6804ca27b155915d and SHA-256
8e1e2db24df80dd605bb1d55df709cc550cc8b379dc4d2c2c23becaa5975b9a6.
Controls require first-kind counts 1233,352,58 for kinds one,two,five.

| Measurement | Count |
|---|---:|
| RDFF resources | 1643 |
| Non-end chunks | 3909 |
| Kind-one item payloads | 1931 |
| Type-50 item payloads | 633 |
| Selected name words below 9999 | 633 |
| Selected name words at least 9999 | 0 |
| Selected items after the first chunk | 2 |
| Resources with bytes after the end header | 1 |

FND-ACTOR-015's independent chunk table sums to 3909 chunks and gives
1233 first plus 698 later kind-one payloads, matching 1931. The new result
retains FND-ITEM-037's 631 first type-50 items. The other two are chunk
index two (zero-based) in RDFF/451 and RDFF/522. Both have name 129,
quantity zero, value zero and word +10 hex zero. They do not supply the
invalid-name predicate required by FND-ITEM-036's independent Look gate.

The one trailing resource agrees with FND-ACTOR-015's RDFF/500 exception.
Bytes after its first end marker are outside this chain domain.
Synthetic controls include a later unnamed type-50 positive case, a named
boundary case, a trailing item decoy and malformed later headers/data/end
markers. The integration gate passed 192 Node and 755 .NET tests.
Two source runs are byte-identical, SHA-256
2ca0ec470b7f40e60551b4d27152323f530f299bebd22119ad4d4e9934af61fe.
A missed first-kind count or wrong source hash returns exit two with empty
stdout. No original game function was executed.

## Interpretation

Under FND-PARTY-259's valid disjoint kind-one copy and own-fixup conditions,
all these canonical type-50 payloads initially retain a name below 9999.
This removes nonfirst canonical RDFF type-50 payloads as an invalid-name
source from Q-PARTY-115. It does not prove preservation across later chunks,
arbitrary buffers or aliases. FND-ITEM-038/039 supply separate conditional
exclusions for known setters and leaves, not a global memory-write proof.

Runtime copies/restores, direct/computed stores, other input archives,
changed streams/metadata, the undecoded script gap, aliases and actual
Look admission remain. A different initial item type followed by a type
change also needs its own source/name admission. No parity, confidence or
complete-reading promotion follows from this source-domain census.

## Alternatives

Looking only at first records might have missed unnamed carried items.
The full-chain positive later-item control detects that case; both actual
later type-50 items are named. Parsing beyond an end marker could invent
additional items: the trailing-decoy control instead leaves them out.
Neither result excludes a later runtime writer or a different source domain.

## How to reproduce

After building Inspector at 5d65424f, run its rdff-look-chain-census command
with the licensed OBJEX.GFF and controls JSON containing sourceSha256 above,
kindOne 1233, kindTwo 352 and kindFive 58. No other controls are required.
Retained files are under GAME_DIR/analysis/item-removal-20261104/:
look-source-controls.json, look-chain-census.json and look-chain-repeat.json.
The wrong-count and wrong-hash inputs are look-source-missed-count.json
and look-source-missed-hash.json; their new refusals are
look-chain-look-source-missed-count.json.refusal.txt and
look-chain-look-source-missed-hash.json.refusal.txt. Compare aggregate counts
with FND-ACTOR-015 and first-item counts with FND-ITEM-037 independently.
Reports contain numerical metadata only and remain in the local source store.
