---
id: FND-ITEM-011
title: Shipped kind-four descriptors cover every shared type-record byte with fourteen fields
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00020194..0x00020258
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0002031C..0x0002037E
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001F291..0x0001F88B
tool: tools/evidence/report.mjs table at fb3fb3a1; bounded source tables with explicit count evidence
environment: null
---

## Observation

FND-ACTOR-017 binds the installed ALL/4 resource to the native shared
type-table pointer and fifteen-byte stride. FND-CONFIG-153 identifies
FNFO/1 and FNFO/2. FND-PARTY-216 and FND-CONFIG-140 supply the generic
descriptor indexing and getter behaviour; FND-CONFIG-154 records the
conditional width-table setup. This reads the complete installed kind-four
descriptor row rather than inferring fields from frequently accessed offsets.

Of its 98 signed-word descriptors, fourteen differ from minus one:

| Selector | Record offset | Metadata | Conditional width |
|---|---:|---:|---:|
| 87 | 0 | 1 | 1 |
| 88 | 1 | 1 | 1 |
| 89 | 2 | 1 | 1 |
| 90 | 3 | 1 | 1 |
| 91 | 4 | 1 | 1 |
| 92 | 5 | 1 | 1 |
| 93 | 6 | 1 | 1 |
| 94 | 7 | 1 | 1 |
| 95 | 8 | 1 | 1 |
| 96 | 9 | 1 | 1 |
| 97 | 10 | 0 | 1 |
| 25 | 11 | 1 | 1 |
| 40 | 12 | 3 | 2 |
| 56 | 14 | 0 | 1 |

The other 84 descriptors are minus one. The fourteen intervals partition
bytes zero through fourteen without gaps or overlaps. Offset thirteen is
the high byte of selector 40's little-endian word, with no separate field
in this row. All listed metadata values are below ten, so the getter's
metadata-minus-nine temporary-slot conversion is not selected by them.
These statements assume the documented installed tables and width setup;
they establish no arbitrary handle, selector, table lifetime or alias domain.

A separate bounded table reads all 102 installed shared rows, taking one
byte at each offset except twelve, where it takes a little-endian word,
and thirteen, which belongs to that word. The read ends exactly at the
resource end. Two complete reports are byte-identical. This verifies source
coverage for these structural intervals, without assigning gameplay names
or deriving signedness from observed values.

## Interpretation

The structural field/selector/metadata mapping is settled for the installed
source. Q-ITEM-005 still requires the complete meanings, masks and native
transformations. Existing consumers in FND-COMBAT-065/066, FND-PARTY-092,
FND-PARTY-248/252 and FND-EXE-616 are reusable evidence for that work.
The format remains unknown as a complete semantic contract; no decoder,
complete-reading declaration or gameplay implementation is introduced.

## Alternatives

- A padding byte or an independently addressed byte thirteen would leave a
  gap or another descriptor. Neither occurs in this shipped descriptor row.
  This does not exclude direct code addressing an interior byte of a field.
- Values fitting a signed or unsigned range do not determine the native
  arithmetic. Per-consumer extension, masks and operations still need review.
- Resource descriptors describe offered fields, not proven admission or
  immutability. Q-CONFIG-008 retains those independent conditions.

## How to reproduce

Use OBJEX.GFF size 6816516, XXH3 e08d3cac153c547f6804ca27b155915d.
Run `node tools/evidence/report.mjs table CONFIG` for these bounded tables:
kind-four offsets at 0x20194, count 98, stride two, one width-two field;
metadata at 0x2031C, count 98, stride one, one width-one field; shared rows
at 0x1F291, count 102, stride fifteen, fields as described above.
Every config names the source hash and count evidence from the cited entries.
Interpret FFFF descriptors as minus one, join metadata by selector, and use
the documented conditional widths. Check the interval partition separately.

Configs and reports remain in GAME_DIR/analysis/item-type-fields-20261103/.
Offset-report SHA-256 is
9f9e6563d947d6cc95eb8a71f78751f60279003b5a858833b7f85cab456b6a72;
metadata-report SHA-256 is
e4999e59d2aa804f23b2ecc8af6f4b67d2a699a282ad98a26c60cd5a9998a37e;
repeated shared-row-report SHA-256 is
cabd1552e0b68155cbf8eabcf56cd594cd8ee791588562df817315259acfd4cd.
The local base-access-candidates.txt is preliminary search preparation,
not a complete reader census or evidence of absent writers. No original ran.
