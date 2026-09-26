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
evidence: [FND-PARTY-002, FND-PARTY-011, FND-PARTY-012]
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
| `0x00` | 1 | `UINT8` | `unk_00` | Purpose unknown. 1, 2, 4, 5, 6 or 7 in the shipped resources, a nonzero combination of bits 0 to 2. | supported | FND-PARTY-002, FND-PARTY-012 |
| `0x01` | | | | Total size 1 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The 19 `PSIN` resources of the installed `CHARSAVE.GFF` and the 8 of the disc's copy: every one
is one byte [FND-PARTY-002].

## Open questions

- Whether each of bits 0 to 2 stands for one of the three psionic disciplines, and which. The
  psionicist of the supplied party has 7, and the gladiator, whose Use screen names
  psychometabolism, has 2 (FND-PARTY-002, FND-PARTY-013, Q-PARTY-004).
