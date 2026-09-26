---
id: FMT-PARTY-001
title: Character record
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["CHARSAVE.GFF", "CD:CHARSAVE.GFF"]
byte_order: little
size: null
text: false
definition: fmt_party_001.ksy
evidence: [FND-PARTY-001, FND-PARTY-003, FND-PARTY-004, FND-PARTY-005, FND-PARTY-020]
conflicting: []
split_with: []
related: [RULE-PARTY-006]
---

## Layout

The layout of a `CHAR` resource of the character archive, one per character, under the
character's number [FND-PARTY-004, FND-PARTY-005]. The same number holds the character's
FMT-PARTY-003, FMT-PARTY-004 and FMT-PARTY-005 resources, and, for a character in the store, its
`CACT` resource [FND-PARTY-012].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 1 | `UINT8` | `version` | 1 in every shipped record. | supported | FND-PARTY-004 |
| `0x01` | 1 | `UINT8` | `tail_count` | Number of FMT-PARTY-002 records after the header, 2 to 29 in the shipped records. | supported | FND-PARTY-004 |
| `0x02` | 33 | `BYTE[33]` | `unk_02` | Purpose unknown. | supported | FND-PARTY-004 |
| `0x23` | 1 | `UINT8` | `strength` | Strength, 12 to 24 in the shipped records. | established | FND-PARTY-003, FND-PARTY-020 |
| `0x24` | 1 | `UINT8` | `dexterity` | Dexterity. | established | FND-PARTY-003, FND-PARTY-020 |
| `0x25` | 1 | `UINT8` | `constitution` | Constitution. | established | FND-PARTY-003, FND-PARTY-020 |
| `0x26` | 1 | `UINT8` | `intelligence` | Intelligence. | established | FND-PARTY-003, FND-PARTY-020 |
| `0x27` | 1 | `UINT8` | `wisdom` | Wisdom. | established | FND-PARTY-003, FND-PARTY-020 |
| `0x28` | 1 | `UINT8` | `charisma` | Charisma. | established | FND-PARTY-003, FND-PARTY-020 |
| `0x29` | 2 | `BYTE[2]` | `unk_29` | Purpose unknown. | supported | FND-PARTY-004 |
| `0x2B` | 16 | `char[16]` | `name` | The character's name, printable ASCII ending at the first NUL, 6 to 15 characters in the shipped records. Bytes after the NUL may hold leftover text. | established | FND-PARTY-001, FND-PARTY-020 |
| `0x3B` | 20 | `BYTE[20]` | `unk_3B` | Purpose unknown. | supported | FND-PARTY-004 |
| `0x4F` | `tail_count * 33` | `FMT-PARTY-002[tail_count]` | `tail` | Records of unknown purpose. | supported | FND-PARTY-004 |
| | | | | Total size `79 + tail_count * 33` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 19 `CHAR` resources of the installed `CHARSAVE.GFF` and the 8 of the disc's copy, which are
the same bytes as the installed resources of the same numbers: every one has `version` 1 and the
size the layout gives, and ends its name within the slot [FND-PARTY-001, FND-PARTY-004,
FND-PARTY-005].

## Open questions

- Where the record keeps the gender, origin, alignment, classes, levels, experience, hit points
  and psionic strength points the View Character screen shows, and what `unk_02`, `unk_29`,
  `unk_3B` and the tail records hold (FND-PARTY-018, FND-PARTY-020, Q-PARTY-003).
- Whether the scores are stored before or after origin modifiers (Q-PARTY-003).
- Whether the game accepts a `version` other than 1 (Q-PARTY-003).
