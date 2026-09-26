---
id: FMT-SAVE-001
title: Stored character identifier in a CACT resource
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["CHARSAVE.GFF"]
byte_order: little
size: 2
text: false
definition: fmt_save_001.ksy
evidence: [FND-PARTY-011, FND-PARTY-012, FND-SAVE-001, FND-SAVE-002]
conflicting: []
split_with: []
related: []
---

## Layout

A `CACT` resource of the character archive, one for each number from 1 to 39 that has held a
stored character. The character's `CHAR`, `SPST`, `PSST` and `PSIN` resources take the same number
(FMT-PARTY-001) [FND-PARTY-011, FND-PARTY-012]. The game and the transfer utility replace the
resource whole, by removing it and writing it again [FND-PARTY-011, FND-PARTY-012].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `character_id` | The identifier of the character stored under this number; 0 when the number is free. Not a `CHAR` resource number. `0x8015` to `0x8018` in the shipped resources that are not 0. | supported | FND-PARTY-011, FND-PARTY-012, FND-SAVE-001, FND-SAVE-002 |
| `0x02` | | | | Total size 2 | | |

## Enumerations and flags

None.

## Differences between builds

None known. The disc's copy of `CHARSAVE.GFF` holds no `CACT` resource [FND-SAVE-001].

## Coverage

The eleven `CACT` resources, 29 to 39, of the installed `CHARSAVE.GFF`: seven hold 0 and four hold
an identifier [FND-SAVE-001].

## Open questions

- Where a character's identifier comes from when the game creates the character, and what its top
  bit means (FND-PARTY-012, Q-SAVE-002).
