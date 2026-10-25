---
id: FMT-COMBAT-002
title: Combatant details record
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: []
byte_order: little
size: 66
text: false
definition: fmt_combat_002.ksy
evidence: [FND-COMBAT-018, FND-COMBAT-022, FND-COMBAT-023, FND-EXPLORE-002, FND-COMBAT-068, FND-COMBAT-069, FND-COMBAT-070, FND-COMBAT-072, FND-COMBAT-073, FND-COMBAT-074, FND-MAGIC-012, FND-MAGIC-013, FND-MAGIC-022, FND-PARTY-051, FND-PARTY-055, FND-PARTY-058, FND-PARTY-059, FND-PARTY-074, FND-PARTY-081, FND-PARTY-085, FND-PARTY-086, FND-PARTY-099, FND-PARTY-100, FND-PARTY-145, FND-PARTY-146, FND-PARTY-147, FND-PARTY-149, FND-PARTY-150, FND-PARTY-151, FND-PARTY-158, FND-PARTY-159]
conflicting: []
split_with: []
related: [RULE-EXPLORE-003]
---

## Layout

One of the 17 records of the table at the far pointer `57E0:19C5` in BLD-GOG-EN-1.1, kept only in
memory and allocated by overlay 188 `+0000`. Records 0 to 4 belong to objects 0 to 4, and the
party's buttons index it by party slot [FND-COMBAT-023]; records 5 to 16 are shared by creatures
with the same `details_key`, and the last release sets the key to 0 [FND-COMBAT-072]. The status
panel reads the second index `2D40:3E64` gives for the character whose turn it is
[FND-COMBAT-022]. The `CHAR` load copies `CHAR` offset `0x45 + n` to offset `n` [FND-PARTY-051],
so each field's meaning in full is that of the FMT-PARTY-001 field of the same name; each has a
field number of `FNFO` 1 [FND-COMBAT-074].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `experience` | Field 35: experience points. | supported | FND-PARTY-051, FND-PARTY-058, FND-COMBAT-074 |
| `0x04` | 4 | `UINT32LE` | `kill_experience` | Field 36: the experience a kill of this combatant gives. | supported | FND-PARTY-051, FND-PARTY-058, FND-PARTY-086, FND-COMBAT-074 |
| `0x08` | 2 | `INT16LE` | `max_hit_points` | Field 37. The character's greatest hit points, the second number of the panel's second line; scaled by the difficulty when a record above 4 is loaded. | supported | FND-COMBAT-018, FND-COMBAT-022, FND-COMBAT-070, FND-COMBAT-074 |
| `0x0A` | 2 | `UINT16LE` | `hit_die_total` | Field 38: the sum of the hit die rolls. | supported | FND-PARTY-051, FND-PARTY-074, FND-COMBAT-074 |
| `0x0C` | 2 | `UINT16LE` | `max_psionic_points` | Field 39: the greatest psionic strength points, the cap of `2D40:3EBF`'s gain. | supported | FND-PARTY-051, FND-PARTY-059, FND-COMBAT-073, FND-COMBAT-074 |
| `0x0E` | 2 | `INT16LE` | `details_key` | Field 0: the key creatures share a record by; 0 marks a free record, and a party slot whose key is 0 holds no character, so its leader and character buttons do nothing. | supported | FND-COMBAT-023, FND-PARTY-051, FND-PARTY-147, FND-COMBAT-072, FND-COMBAT-074 |
| `0x10` | 2 | `UINT16LE` | `class_flags` | Field 40: one bit per class counted. | supported | FND-PARTY-051, FND-PARTY-085, FND-COMBAT-074 |
| `0x12` | 1 | `UINT8` | `origin` | Field 41: origin 1 to 8 for party members, the creature kind otherwise (9 for undead). | supported | FND-PARTY-051, FND-MAGIC-022, FND-COMBAT-074 |
| `0x13` | 1 | `UINT8` | `gender` | Field 42. | supported | FND-PARTY-051, FND-PARTY-055, FND-COMBAT-074 |
| `0x14` | 1 | `UINT8` | `alignment` | Field 43. | supported | FND-PARTY-051, FND-PARTY-055, FND-COMBAT-074 |
| `0x15` | 6 | `UINT8[6]` | `base_scores` | Fields 44 to 49: the six scores as generated, which overlay 197 `+1E7B` builds the current ones from. | supported | FND-PARTY-051, FND-MAGIC-013, FND-COMBAT-074 |
| `0x1B` | 3 | `UINT8[3]` | `classes` | Fields 50 to 52: class codes. | supported | FND-PARTY-051, FND-MAGIC-012, FND-COMBAT-074 |
| `0x1E` | 3 | `UINT8[3]` | `levels` | Fields 53 to 55: the level in each class position. | supported | FND-PARTY-051, FND-MAGIC-012, FND-MAGIC-022, FND-COMBAT-074 |
| `0x21` | 1 | `INT8` | `base_armor_class` | Field 56: armour class before items and Dexterity. | supported | FND-PARTY-145, FND-COMBAT-068, FND-COMBAT-074 |
| `0x22` | 1 | `UINT8` | `base_movement_rate` | Field 57. | supported | FND-PARTY-146, FND-COMBAT-074 |
| `0x23` | 1 | `INT8` | `magic_resistance` | Field 58: magic resistance in percent. | supported | FND-PARTY-146, FND-PARTY-150, FND-COMBAT-074 |
| `0x24` | 1 | `UINT8` | `class_attack_rate` | Field 59. | supported | FND-PARTY-085, FND-PARTY-100, FND-COMBAT-074 |
| `0x25` | 3 | `UINT8[3]` | `natural_attack_rates` | Fields 60 to 62: the rates of natural attacks 0 to 2 (FMT-PARTY-001 `attack_rate` and `natural_attack_rates`). | supported | FND-PARTY-085, FND-PARTY-100, FND-COMBAT-074 |
| `0x28` | 3 | `UINT8[3]` | `natural_damage_dice` | Fields 63 to 65. | supported | FND-PARTY-100, FND-COMBAT-074 |
| `0x2B` | 3 | `UINT8[3]` | `natural_damage_sides` | Fields 66 to 68. | supported | FND-PARTY-100, FND-COMBAT-074 |
| `0x2E` | 3 | `INT8[3]` | `natural_damage_bonuses` | Fields 69 to 71. | supported | FND-PARTY-100, FND-COMBAT-074 |
| `0x31` | 5 | `UINT8[5]` | `saving_throws` | Fields 72 to 76: saves 1 to 5. | supported | FND-PARTY-085, FND-PARTY-099, FND-COMBAT-074 |
| `0x36` | 1 | `UINT8` | `unk_36` | Purpose unknown. Field 77. 0, 1, 2, 4, 5 or 7 in the shipped creature records and 0, 1 or 4 in the `CHAR` records; scripts compare it, most often with 7; no executable reader. | supported | FND-PARTY-151, FND-PARTY-159, FND-COMBAT-074 |
| `0x37` | 1 | `UINT8` | `footprint` | Field 78. The size of the square of cells the object occupies, in the low four bits, and how much its corners are trimmed, in the high four (RULE-EXPLORE-003). | supported | FND-EXPLORE-002, FND-PARTY-151 |
| `0x38` | 1 | `UINT8` | `spell_list` | Field 79: the spell list overlay 177 `+0059` prepares from. | supported | FND-PARTY-151, FND-COMBAT-074 |
| `0x39` | 3 | `UINT8[3]` | `greatest_levels` | Fields 80 to 82. | supported | FND-PARTY-074, FND-PARTY-081, FND-COMBAT-074 |
| `0x3C` | 2 | `UINT16LE` | `attack_sound` | Field 83: the natural attack's sound. | supported | FND-PARTY-149, FND-COMBAT-069, FND-COMBAT-074 |
| `0x3E` | 2 | `UINT16LE` | `damage_sound` | Field 84. | supported | FND-PARTY-149, FND-COMBAT-074 |
| `0x40` | 1 | `UINT8` | `psionic_list` | Field 85: a non-party combatant's psionic power list. | supported | FND-PARTY-149, FND-PARTY-158, FND-COMBAT-074 |
| `0x41` | 1 | `UINT8` | `unk_41` | Purpose unknown. Field 86. 0 in every shipped record; no reader in the executable or the scripts. | supported | FND-PARTY-149, FND-PARTY-158, FND-COMBAT-074 |
| `0x42` | | | | Total size 66 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Read from the code that indexes the table with a stride of 66 [FND-COMBAT-022, FND-COMBAT-023],
the allocation and record choice [FND-COMBAT-072] and the `FNFO` 1 field table
[FND-COMBAT-074]; no record was observed in memory.

## Open questions

- What `unk_36` and `unk_41` hold (FND-COMBAT-074, Q-COMBAT-043).
