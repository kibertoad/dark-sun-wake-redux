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
evidence: [FND-PARTY-001, FND-PARTY-003, FND-PARTY-004, FND-PARTY-005, FND-PARTY-013, FND-PARTY-020, FND-PARTY-048, FND-PARTY-049, FND-PARTY-050]
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
| `0x02` | 8 | `BYTE[8]` | `unk_02` | Purpose unknown. The load does not copy these bytes to the FMT-COMBAT-001 record. | supported | FND-PARTY-004, FND-PARTY-049 |
| `0x0A` | 2 | `INT16LE` | `hit_points` | The character's current hit points. A load copies it to the FMT-COMBAT-001 record's `hit_points`; 21 to 165 in the shipped records. | supported | FND-PARTY-049, FND-PARTY-050 |
| `0x0C` | 2 | `BYTE[2]` | `unk_0c` | Purpose unknown. A load copies it to the FMT-COMBAT-001 record's `unk_02`. | supported | FND-PARTY-049 |
| `0x0E` | 2 | `UINT16LE` | `unk_0e` | Purpose unknown. A load copies it to the FMT-COMBAT-001 record's `details_index` and then replaces that with 9,999; 0 to 3 in the shipped records. | supported | FND-PARTY-049, FND-PARTY-050 |
| `0x10` | 2 | `UINT16LE` | `combatant_id` | A load copies it to the FMT-COMBAT-001 record's `character_id`. 32,769 to 33,536 in the shipped records, never the record's own number, and the same in two of them. | supported | FND-PARTY-049, FND-PARTY-050 |
| `0x12` | 8 | `BYTE[8]` | `unk_12` | Purpose unknown. A load copies it to bytes `0x08` to `0x0F` of the FMT-COMBAT-001 record, then replaces the words that came from `0x12`, `0x14` and `0x16` with 9,999. | supported | FND-PARTY-049, FND-PARTY-050 |
| `0x1A` | 2 | `UINT16LE` | `object_offset` | 300 plus this is the object number placed for the character when it is added to the party or supplied by START GAME (RULE-PARTY-006); 0 to 13 in the shipped records. | supported | FND-PARTY-013, FND-PARTY-048, FND-PARTY-049 |
| `0x1C` | 2 | `BYTE[2]` | `unk_1c` | Purpose unknown. A load copies it to bytes `0x12` and `0x13` of the FMT-COMBAT-001 record. | supported | FND-PARTY-049 |
| `0x1E` | 1 | `UINT8` | `combat_mark` | A load copies it to the FMT-COMBAT-001 record's `combat_mark` and makes that 1 when it is 0; 0 or 1 in the shipped records. | supported | FND-PARTY-049, FND-PARTY-050 |
| `0x1F` | 3 | `BYTE[3]` | `unk_1f` | Purpose unknown. A load copies it to the FMT-COMBAT-001 record's `unk_15`. | supported | FND-PARTY-049 |
| `0x22` | 1 | `UINT8` | `control_flags` | A load copies it to the FMT-COMBAT-001 record's byte at `0x18`, whose bits 5 and 6 are `computer_control` and `control_locked`; 0 in the shipped records. | supported | FND-PARTY-049, FND-PARTY-050 |
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

The value file `FMT-PARTY-001.characters.csv` gives, for each of the 19 `CHAR` resources of the
installed `CHARSAVE.GFF`, the columns `character` (the resource number), `version`,
`tail_count`, the six ability scores and `name` (the text before the NUL) [FND-PARTY-001,
FND-PARTY-003, FND-PARTY-004]. The eight records of the disc's copy are the rows 40 to 43 and 50
to 53. The opaque blocks `unk_02`, `unk_29`, `unk_3B` and the tail are left out.

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

- Where the record keeps the gender, origin, alignment, classes, levels, experience, maximum
  hit points and psionic strength points the View Character screen shows, and what `unk_02`,
  `unk_0c`, `unk_0e`, `unk_12`, `unk_1c`, `unk_1f`, `unk_29`, `unk_3B` and the tail records hold
  (FND-PARTY-018, FND-PARTY-020, Q-PARTY-003).
- Whether `hit_points` holds current rather than maximum hit points: it lands on the combatant
  record's `hit_points`, which the status panel shows first (FND-PARTY-050, Q-PARTY-003).
- What `combatant_id` identifies: overlay 184 writes the combatant record's copy of it to a
  `CACT` resource (FND-PARTY-012), and the shipped values are never the record's own number
  (FND-PARTY-050, Q-PARTY-003).
- Whether the scores are stored before or after origin modifiers (Q-PARTY-003).
- Whether the game accepts a `version` other than 1 (Q-PARTY-003).
