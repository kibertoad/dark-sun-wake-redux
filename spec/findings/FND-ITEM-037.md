---
id: FND-ITEM-037
title: Every canonical type-50 first-item record has a valid name number that masks the independent Look word gate
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
tool: Inspector rdff-look-census at cbb3960b; existing bounded GffArchive reader and RdffKeyInspection first-header/source verification; synthetic and integration controls
environment: null
---

## Observation

FND-ITEM-036 narrows an independent word-+10 contribution to the Look
classifier gate to canonical shared type 50, but leaves actual item inputs
open. This census examines the first kind-one record in each canonical
RDFF resource. It does not search arbitrary byte patterns or infer type
identity from a matching fifteen-byte payload.

### Source, framing and controls

The command verifies OBJEX.GFF SHA-256
8e1e2db24df80dd605bb1d55df709cc550cc8b379dc4d2c2c23becaa5975b9a6
before archive interpretation. This is the approved 6816516-byte source
with XXH3-128 e08d3cac153c547f6804ca27b155915d of FND-ACTOR-015/018.
It uses the existing GFF directory/index reader and reuses complete first
header validation: type one; kind one/two/five; their exact 23/49/23-byte
widths; full available payload; the kind-five next end header; unique
resource numbers and explicit first-kind counts 1233/352/58.

Only validated first kind-one records are selected. The fixed type test
reads attached word +0A and requires 50. It then reads quantity +02,
value +06, word +10 and name number +14 from that same 23-byte record.
All words are unsigned little-endian. The reported name gate is exactly
name number below 9999, as FND-PARTY-252's helper tests.

Synthetic controls exercise distinct values at each offset, names 9998
and 9999, another-type exclusion and both 43- and 208-byte resource
extents. A positive unnamed type-50 case survives selection. Missed
counts, duplicate resources and malformed/short first payloads refuse;
an empty selected type-50 domain refuses rather than reporting a vacuous
negative. Existing source/boundary controls still apply.

### Complete first-item domain

| Measurement | Count |
|---|---:|
| RDFF resources validated | 1643 |
| First kind-one item records | 1233 |
| Selected first items with type word 50 | 631 |
| Selected items with name below 9999 | 631 |
| Selected items with name at least 9999 | 0 |
| Selected 68-byte resources | 629 |
| Selected 43-byte resources | 1 |
| Selected 208-byte resources | 1 |

The 68-byte model alone would omit two selected first records. The
43-byte control is RDFF/5513, with name number 204, value 9999, quantity
zero and word +10 zero. The 208-byte control is RDFF/3124, with name
number 104 and the same three other words. No name text is read or
reported. The selected first records of those other sizes are validated
without claiming that their later bytes or chunks have been interpreted.

Two accepted runs have identical output bytes. The report SHA-256 is
ce094cff2b008697a6ec603645e5aac1e9d1a9d295ab639d05b2d7443cdac3d6.
Changing kindOne to 1234 refuses with the first-kind-count diagnostic.
Changing sourceSha256 to zero refuses before archive interpretation.
Both failures return status two and empty standard output.

### Consequence for unchanged copies

Under the valid disjoint-copy conditions of FND-PARTY-259's kind-one
import, the first 23-byte payload is copied and its words +04/+08 and
cache word +0C are then fixed up. Those own fixups do not change name
word +14. Thus these source inputs provide valid names immediately after
that admitted copy/fixup prefix. Later chunk setters and aliases are not
covered by this prefix statement.

With the unchanged valid name at the property helper, its final assignment
is 80 hex, replacing any earlier 08 from word +10 or another predicate.
For a stable kind-one item using canonical type 50, FND-ITEM-036's
type-domain conditions exclude the caller's extra OR/replacement gates;
the returned classification is therefore nonzero regardless of word +10.
This excludes the independent word contribution for those preserved
first-copy inputs, not a later secret-door image check on the same word.

## Interpretation

Q-PARTY-115 no longer needs an unnamed canonical type-50 first record
from this complete first-item source domain: none exists. Its surviving
producer question is whether other creation/replacement/type-change paths
or later writes admit a type-50 item with name at least 9999, with the
remaining value/quantity/word, fallback and door conditions of FND-ITEM-036.

The census does not examine nonfirst attached items, every CHARSAVE or
SAVE record, script/runtime writes, all later chunks, source selection or
an actual Look invocation. Complete producer/lifetime exclusion remains
open. No source field meaning beyond the existing bindings, entry
confidence, parity or complete-reading promotion follows.

## Alternatives

Type 50 is not absent from the canonical first-item domain; 631 positive
cases disprove that shortcut. Their names, rather than absence of the
type, mask the independent gate. Limiting selection to 68-byte resources
would miss two named cases. Conversely, source names below 9999 do not
prove that every later item remains named. An admitted invalid-name
producer would invalidate any global preserved-source exclusion while
leaving this source census unchanged.

## How to reproduce

Run Test.ps1 to build Inspector at cbb3960b. Invoke its rdff-look-census
command with approved OBJEX.GFF and controls containing sourceSha256
above, kindOne 1233, kindTwo 352 and kindFive 58. For the tested artifacts
layout the command is:

```text
dotnet artifacts/test/bin/DarkSunWakeRedux.Inspect/debug/DarkSunWakeRedux.Inspect.dll rdff-look-census ARCHIVE CONTROLS
```

Accepted controls and reports are under
GAME_DIR/analysis/item-removal-20261104/: look-source-controls.json,
look-source-census.json and look-source-repeat.json. Compare complete
report bytes; inspect the selected resource sizes and the two stated
controls. The missed-count and wrong-hash configurations and refusal
diagnostics remain beside them. The bounded source/header/word procedure
is committed in RdffKeyInspection.AnalyzeLook and Run. No original ran;
licensed bytes and numerical per-resource reports remain only in GAME_DIR.
