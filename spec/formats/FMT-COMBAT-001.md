---
id: FMT-COMBAT-001
title: Combatant record
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: []
byte_order: little
size: 49
text: false
definition: fmt_combat_001.ksy
evidence: [FND-AI-003, FND-AI-004, FND-COMBAT-008, FND-COMBAT-018, FND-COMBAT-022, FND-COMBAT-023, FND-COMBAT-024, FND-PARTY-012, FND-PARTY-013, FND-EXPLORE-002]
conflicting: []
split_with: []
related: [RULE-AI-001, RULE-COMBAT-004, RULE-COMBAT-005]
---

## Layout

One of the records of the table at the far pointer `57E0:19C9` in BLD-GOG-EN-1.1, kept only in
memory. Records 0 to 3 are the four party slots [FND-COMBAT-008, FND-PARTY-013]; the status
panel reads the record whose index `2D40:3E64` gives for the character whose turn it is, so the
table holds the other combatants too [FND-COMBAT-022].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `INT16LE` | `hit_points` | The character's current hit points, the first number of the panel's second line. | supported | FND-COMBAT-018, FND-COMBAT-022 |
| `0x02` | 2 | `BYTE[2]` | `unk_02` | Purpose unknown. | supported | FND-COMBAT-022 |
| `0x04` | 2 | `UINT16LE` | `details_index` | Index of the combatant's FMT-COMBAT-002 record; `2D40:3E64` gives it as its second index when it is below 17. | supported | FND-COMBAT-022, FND-EXPLORE-002 |
| `0x06` | 2 | `UINT16LE` | `character_id` | The identifier of the character in the character archive; 0 in a slot that holds no character. | supported | FND-COMBAT-008, FND-PARTY-012 |
| `0x08` | 12 | `BYTE[12]` | `unk_08` | Purpose unknown. The party loader computes a value from offset `0x10`. | supported | FND-PARTY-013 |
| `0x14` | 1 | `UINT8` | `combat_mark` | Set to 1 in every party slot with a character when combat starts. The panel names an effect only when it is 1, and the leader can be changed to a slot only when it is 0 or 1. | supported | FND-COMBAT-008, FND-COMBAT-022, FND-COMBAT-023, FND-PARTY-013 |
| `0x15` | 3 | `BYTE[3]` | `unk_15` | Purpose unknown. | supported | FND-COMBAT-022 |
| `0x18 bits 0..5` | | `bits[5]` | `unk_18_bits_0_4` | Purpose unknown. | supported | FND-COMBAT-022 |
| `0x18 bits 5..6` | | `bits[1]` | `computer_control` | Set when the computer controls the party member. The computer-control button toggles it, and Space clears it in every slot where `control_locked` is 0. | supported | FND-AI-003, FND-AI-004 |
| `0x18 bits 6..7` | | `bits[1]` | `control_locked` | Set when the computer-control setting cannot be changed; the button then shows a message and Space leaves `computer_control` as it is. | supported | FND-AI-003, FND-AI-004 |
| `0x18 bits 7..8` | | `bits[1]` | `unk_18_bit_7` | Purpose unknown. | supported | FND-COMBAT-022 |
| `0x19` | 8 | `BYTE[8]` | `unk_19` | Purpose unknown. | supported | FND-COMBAT-022 |
| `0x21` | 16 | `char[16]` | `name` | The character's name, ending at the first NUL. The first line of the panel and the `%Fs` of the combat messages. | supported | FND-COMBAT-018, FND-COMBAT-022, FND-COMBAT-024 |
| `0x31` | | | | Total size 49 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Read from the code that indexes the table with a stride of 49 [FND-AI-003, FND-AI-004,
FND-COMBAT-008, FND-COMBAT-022, FND-COMBAT-023]; no record was observed in memory.

## Open questions

- What `unk_02`, `unk_08`, `unk_15`, `unk_18_bits_0_4`, `unk_18_bit_7` and `unk_19` hold, what
  values `combat_mark` takes besides 0 and 1, how many records the table holds, and how
  `2D40:3E64` maps a combatant to a record (Q-COMBAT-002).
- Where the game sets `control_locked`; no instruction found sets the bit on its own (Q-AI-002).
