---
id: FMT-PARTY-007
title: Class combination table (DATA 1001)
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RESOURCE.GFF"]
byte_order: little
size: 576
text: false
definition: fmt_party_007.ksy
evidence: [FND-PARTY-058, FND-PARTY-068]
conflicting: []
split_with: []
related: [RULE-PARTY-009]
---

## Layout

The `DATA` resource 1001 of `RESOURCE.GFF`, with no header: one mask of classes for each origin,
first class and second class. Overlay 185 `+0000` returns the byte at 72 times the origin code
plus 9 times the first class less 1 plus the second class, the second class being 0 for none
[FND-PARTY-058], and the generation screen enables the class buttons from it [FND-PARTY-068].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x000` | 576 | `UINT8[8][8][9]` | `masks` | Indexed by `origin` (0 to 7, human first), then by the first class's generation code less 1 (0 to 7, Cleric first), then by the second class's generation code (1 to 8), or 0 when there is no second class. Each byte is a set of classes in the bits below: at second class 0, the classes that may be the second; otherwise, the classes that may be the third. | supported | FND-PARTY-058, FND-PARTY-068 |
| `0x240` | | | | Total size 576 | | |

## Enumerations and flags

### `masks`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| `0x80` | `CLASS_BIT_CLERIC` | Cleric, generation code 1. | supported | FND-PARTY-068 |
| `0x40` | `CLASS_BIT_DRUID` | Druid, generation code 2. | supported | FND-PARTY-068 |
| `0x20` | `CLASS_BIT_FIGHTER` | Fighter, generation code 3. | supported | FND-PARTY-068 |
| `0x10` | `CLASS_BIT_GLADIATOR` | Gladiator, generation code 4. | supported | FND-PARTY-068 |
| `0x08` | `CLASS_BIT_PRESERVER` | Preserver, generation code 5. | supported | FND-PARTY-068 |
| `0x04` | `CLASS_BIT_PSIONICIST` | Psionicist, generation code 6. | supported | FND-PARTY-068 |
| `0x02` | `CLASS_BIT_RANGER` | Ranger, generation code 7. | supported | FND-PARTY-068 |
| `0x01` | `CLASS_BIT_THIEF` | Thief, generation code 8. | supported | FND-PARTY-068 |

## Differences between builds

None known.

## Coverage

The one `DATA` 1001 resource of the installed `RESOURCE.GFF`, at `0x186CB`; FND-PARTY-068 gives
every row that is not all 0. The two reads in overlay 183 `+0A3E` are the only ones found
[FND-PARTY-068].

## Open questions

None.
