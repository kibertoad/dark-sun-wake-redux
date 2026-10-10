---
id: FMT-PARTY-003
title: Character psionic byte
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["CHARSAVE.GFF", "CD:CHARSAVE.GFF"]
byte_order: little
size: 1
text: false
definition: fmt_party_003.ksy
evidence: [FND-PARTY-002, FND-PARTY-011, FND-PARTY-012, FND-PARTY-067]
conflicting: []
split_with: []
related: []
---

## Layout

The `PSIN` resource of the character archive, one per character under the character's number,
beside its FMT-PARTY-001 record [FND-PARTY-002]. The game reads and writes it as one byte of a
four-slot table, one byte per party slot, and the transfer utility writes it the same way
[FND-PARTY-011, FND-PARTY-012].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 1 | `UINT8` | `disciplines` | The character's psionic disciplines, one bit each (see the flags below). Generation sets the bits from the discipline window when it stores the character; 1, 2, 4, 5, 6 or 7 in the shipped resources. | supported | FND-PARTY-002, FND-PARTY-012, FND-PARTY-067 |
| `0x01` | | | | Total size 1 | | |

## Enumerations and flags

### `disciplines`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| `0x01` | `DISCIPLINE_PSYCHOKINESIS` | Psychokinesis, the first button of the generation screen's discipline window. | supported | FND-PARTY-067 |
| `0x02` | `DISCIPLINE_PSYCHOMETABOLISM` | Psychometabolism, the second button. | supported | FND-PARTY-067 |
| `0x04` | `DISCIPLINE_TELEPATHY` | Telepathy, the third button. | supported | FND-PARTY-067 |

## Differences between builds

None known.

## Coverage

The 19 `PSIN` resources of the installed `CHARSAVE.GFF` and the 8 of the disc's copy: every one
is one byte [FND-PARTY-002].

## Open questions

None.
