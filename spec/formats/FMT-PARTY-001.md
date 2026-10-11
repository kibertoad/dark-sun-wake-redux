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
evidence: [FND-PARTY-001, FND-PARTY-003, FND-PARTY-005, FND-PARTY-013, FND-PARTY-020, FND-PARTY-048, FND-PARTY-049, FND-PARTY-050, FND-PARTY-051, FND-PARTY-052, FND-PARTY-053, FND-PARTY-054, FND-PARTY-055, FND-PARTY-056, FND-PARTY-057, FND-PARTY-058, FND-PARTY-059, FND-PARTY-073, FND-PARTY-066, FND-PARTY-074, FND-PARTY-081, FND-PARTY-082, FND-PARTY-083, FND-PARTY-085, FND-PARTY-086, FND-PARTY-091, FND-PARTY-100, FND-PARTY-096, FND-PARTY-099, FND-PARTY-108, FND-PARTY-071, FND-PARTY-070, FND-PARTY-142, FND-PARTY-095, FND-PARTY-143, FND-PARTY-144, FND-PARTY-145, FND-PARTY-146, FND-PARTY-147, FND-PARTY-148, FND-PARTY-112, FND-PARTY-113, FND-PARTY-149, FND-PARTY-150, FND-PARTY-151, FND-PARTY-152, FND-PARTY-156, FND-PARTY-157, FND-PARTY-158]
conflicting: []
split_with: []
related: [RULE-PARTY-006]
---

## Layout

The layout of a `CHAR` resource of the character archive, one per character, under the
character's number [FND-PARTY-005, FND-PARTY-052]. The same number holds the character's
FMT-PARTY-003, FMT-PARTY-004 and FMT-PARTY-005 resources, and, for a character in the store, its
`CACT` resource [FND-PARTY-012].

The load reads the resource as a chain of FMT-PARTY-006 chunks ending at a chunk whose
`chunk_type` is 0xFF [FND-PARTY-051]. Every shipped record starts with a type-1 chunk at `0x00`
and a type-3 chunk at `0x3B`, and their headers hold the same values in all of them, so the
table gives the bytes of those two chunks at fixed offsets; the rest of the chain follows from
`0x87` [FND-PARTY-052]. The game writes a record with the character's chunk first and its details
chunk second, and a record without the details chunk fails to load [FND-PARTY-054], so these
offsets hold for every record the game writes and reads back.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 1 | `UINT8` | `type` | The first chunk's `chunk_type`, 1 in every shipped record: its data is the character's FMT-COMBAT-001 record. | supported | FND-PARTY-051, FND-PARTY-052 |
| `0x01` | 1 | `UINT8` | `chunk_count` | The number of chunks before the end header, 2 to 29 in the shipped records. The load does not read it. | supported | FND-PARTY-051, FND-PARTY-052 |
| `0x02` | 1 | `UINT8` | `kind` | 2 in every shipped record. | supported | FND-PARTY-051, FND-PARTY-052 |
| `0x03` | 1 | `UINT8` | `unk_03` | 0 in every shipped record. | supported | FND-PARTY-052 |
| `0x04` | 2 | `UINT16LE` | `saved_slot` | The first chunk's `record_ref`: the party slot the character was written from. The load does not read it; 0 to 3 in the shipped records, equal to `saved_field` and `saved_details_index`. | supported | FND-PARTY-051, FND-PARTY-052, FND-PARTY-054, FND-PARTY-148 |
| `0x06` | 2 | `UINT16LE` | `saved_field` | The first chunk's `field`, which the writer sets to the slot. The load does not read it. | supported | FND-PARTY-051, FND-PARTY-054, FND-PARTY-148 |
| `0x08` | 2 | `UINT16LE` | `len_data` | 49, the size of the data from `0x0A`. | supported | FND-PARTY-051, FND-PARTY-052 |
| `0x0A` | 2 | `INT16LE` | `hit_points` | The character's current hit points. A load copies it to the FMT-COMBAT-001 record's `hit_points`; 21 to 165 in the shipped records, never above `max_hit_points`. | supported | FND-PARTY-049, FND-PARTY-050, FND-PARTY-053 |
| `0x0C` | 2 | `UINT16LE` | `psionic_points` | The character's current psionic strength points. A load copies it to the FMT-COMBAT-001 record's `unk_02`, which the character sheet prints after `PSI:`; 11 to 162 in the shipped records, never above `max_psionic_points`. | supported | FND-PARTY-049, FND-PARTY-059 |
| `0x0E` | 2 | `UINT16LE` | `saved_details_index` | The character's `details_index` when it was written, which for a party slot is the slot. A load copies it to the FMT-COMBAT-001 record's `details_index`, replaces that with 9,999, and then with the number of the details record the chunk at `0x3B` fills; 0 to 3 in the shipped records. | supported | FND-PARTY-049, FND-PARTY-050, FND-PARTY-051, FND-PARTY-052 |
| `0x10` | 2 | `UINT16LE` | `combatant_id` | A load copies it to the FMT-COMBAT-001 record's `character_id`. 32,769 to 33,536 in the shipped records, never the record's own number, and the same in two of them. | supported | FND-PARTY-049, FND-PARTY-050 |
| `0x12` | 6 | `UINT16LE[3]` | `saved_child_refs` | The words the FMT-COMBAT-001 record held at `0x08`, `0x0A` and `0x0C` when the character was written: 9,999 where it had no record in field 16, 17 or 4. A load copies them there, then replaces them with 9,999, and the type-2 chunks whose `field` is 16, 17 or 4 store their records' handles there. In the shipped records 9,999 exactly where the record has no type-2 chunk of that field on the first chunk, and otherwise 456 to 519, never the chunk's `record_ref`. | supported | FND-PARTY-049, FND-PARTY-050, FND-PARTY-051, FND-PARTY-052, FND-PARTY-148 |
| `0x18` | 1 | `UINT8` | `special_attack` | A monster's special attack kind, 1 to 20, 0 for none. A load copies it to byte `0x0E` of the FMT-COMBAT-001 record; overlay 198 `+0149` turns it and a mode into one of the `MONS` `DATA` records, which overlay 198 `+0000` queues as a hit, and a creature is immune to its own. Field 18 of script functions 48 and 49 of opcode `0x22`. 0 in the shipped records. | supported | FND-PARTY-049, FND-PARTY-113, FND-PARTY-148, FND-PARTY-156, FND-PARTY-157 |
| `0x19` | 1 | `UINT8` | `immunity_type` | An index into the 18 immunity words at `DS:2588`, which the hit routines test against a hit's mask; type 16, whose word is 0xFFFF, ignores every hit. A load copies it to byte `0x0F` of the FMT-COMBAT-001 record. Field 19 of script functions 48 and 49 of opcode `0x22`. 0 in the shipped records. | supported | FND-PARTY-049, FND-PARTY-112, FND-PARTY-113, FND-PARTY-156, FND-PARTY-157 |
| `0x1A` | 2 | `UINT16LE` | `object_offset` | 300 plus this is the object number placed for the character when it is added to the party or supplied by START GAME (RULE-PARTY-006); 0 to 13 in the shipped records. | supported | FND-PARTY-013, FND-PARTY-048, FND-PARTY-049 |
| `0x1C` | 1 | `UINT8` | `unk_1c` | Purpose unknown. A load copies it to byte `0x12` of the FMT-COMBAT-001 record, which the generation finish sets from `base_armor_class`; no other read or store through the combatant table was found. | supported | FND-PARTY-049, FND-PARTY-145, FND-PARTY-152 |
| `0x1D` | 1 | `UINT8` | `movement_rate` | The character's current movement rate: `base_movement_rate` changed by the slot's records, rebuilt with the ability scores. A load copies it to byte `0x13` of the FMT-COMBAT-001 record; a fight round gives the combatant ten movement points for each unit, and a step costs 10 straight or 14 diagonally. Equal to `base_movement_rate` in 18 of the shipped records and one more in the other. | supported | FND-PARTY-049, FND-PARTY-146 |
| `0x1E` | 1 | `UINT8` | `combat_mark` | A load copies it to the FMT-COMBAT-001 record's `combat_mark` and makes that 1 when it is 0; 0 or 1 in the shipped records. | supported | FND-PARTY-049, FND-PARTY-050 |
| `0x1F` | 1 | `UINT8` | `combatant_kind` | The combatant's kind, which sets its side (FND-PARTY-095). A load copies it to byte `0x15` of the FMT-COMBAT-001 record; 0, the party's own kind, in the shipped records. | supported | FND-PARTY-049, FND-PARTY-095, FND-PARTY-143 |
| `0x20` | 1 | `UINT8` | `thac0` | 20 less the best, over the class groups, of the group's greatest level less 1 times the group's rate (8 priest, 12 warrior, 4 wizard, 6 rogue) over 12. A load copies it to byte `0x16` of the FMT-COMBAT-001 record; storing a character, a class change and each level gained in play set it. The attack routine of overlay 173 takes it as the attacker's THAC0: an attack hits on a roll of 1 to 20 that is 20, or not 1 and at least the THAC0 less bonuses less a value for the target. | supported | FND-PARTY-049, FND-PARTY-085, FND-PARTY-091 |
| `0x21` | 1 | `UINT8` | `unk_21` | Purpose unknown. A load copies it to byte `0x17` of the FMT-COMBAT-001 record, which no read found uses; 7 in the shipped records. | supported | FND-PARTY-049, FND-PARTY-152 |
| `0x22` | 1 | `UINT8` | `control_flags` | A load copies it to the FMT-COMBAT-001 record's byte at `0x18`, whose bits 5 and 6 are `computer_control` and `control_locked`; 0 in the shipped records. | supported | FND-PARTY-049, FND-PARTY-050 |
| `0x23` | 1 | `UINT8` | `strength` | Strength, 12 to 24 in the shipped records. | established | FND-PARTY-003, FND-PARTY-020, FND-PARTY-071, FND-PARTY-142 |
| `0x24` | 1 | `UINT8` | `dexterity` | Dexterity. | established | FND-PARTY-003, FND-PARTY-020, FND-PARTY-071, FND-PARTY-142 |
| `0x25` | 1 | `UINT8` | `constitution` | Constitution. | established | FND-PARTY-003, FND-PARTY-020, FND-PARTY-071, FND-PARTY-142 |
| `0x26` | 1 | `UINT8` | `intelligence` | Intelligence. | established | FND-PARTY-003, FND-PARTY-020, FND-PARTY-071, FND-PARTY-142 |
| `0x27` | 1 | `UINT8` | `wisdom` | Wisdom. | established | FND-PARTY-003, FND-PARTY-020, FND-PARTY-071, FND-PARTY-142 |
| `0x28` | 1 | `UINT8` | `charisma` | Charisma. | established | FND-PARTY-003, FND-PARTY-020, FND-PARTY-071, FND-PARTY-142 |
| `0x29` | 1 | `UINT8` | `facing` | The direction the character faces, 0 to 7. A load copies it to byte `0x1F` of the FMT-COMBAT-001 record; drawing maps the eight directions onto four sprite views. 0 to 6 in the shipped records. | supported | FND-PARTY-049, FND-PARTY-144 |
| `0x2A` | 1 | `UINT8` | `unk_2a` | Purpose unknown. A load copies it to byte `0x20` of the FMT-COMBAT-001 record, which overlay 204's combatant field getter and setter read and write as field 33; those are functions 48 and 49 of script opcode `0x22`, and nothing else found reads or writes it; 0 in the shipped records. | supported | FND-PARTY-049, FND-PARTY-144, FND-PARTY-152, FND-PARTY-156 |
| `0x2B` | 16 | `char[16]` | `name` | The character's name, printable ASCII ending at the first NUL, 6 to 15 characters in the shipped records. Bytes after the NUL may hold leftover text. | established | FND-PARTY-001, FND-PARTY-020 |
| `0x3B` | 10 | FMT-PARTY-006 header | `details_header` | `chunk_type` 3, `target_chunk` 0, `kind` 3, `field` 15 and `len_data` 66 in every shipped record, so the load copies the next 66 bytes into the character's FMT-COMBAT-002 record and stores its number in the character's `details_index`. | supported | FND-PARTY-051, FND-PARTY-052 |
| `0x45` | 4 | `UINT32LE` | `experience` | The character's experience points. A load copies it to the FMT-COMBAT-002 record's first dword, which the character sheet prints after `EXP: ` and which overlay 183 compares with the class thresholds of `DATA` 1000 to raise the levels. In play overlay 210 cuts it to the start of the level four above each class's level before raising the levels, and its level drain sets it to half the sum of two thresholds; 30,000 to 2,475,000 in the shipped records. | supported | FND-PARTY-051, FND-PARTY-058, FND-PARTY-081, FND-PARTY-082 |
| `0x49` | 4 | `UINT32LE` | `kill_experience` | The experience a kill of this character as an enemy gives, divided among the filled party slots. A load copies it to the FMT-COMBAT-002 record's dword at `+0x04`, and each experience award in play raises it to the new `experience` when it is below; 3,000 in eleven shipped records, and equal to or above `experience` in the others. | supported | FND-PARTY-051, FND-PARTY-058, FND-PARTY-086 |
| `0x4D` | 2 | `INT16LE` | `max_hit_points` | The character's greatest hit points. A load copies it to the FMT-COMBAT-002 record's `max_hit_points`; 21 to 165 in the shipped records. | supported | FND-PARTY-051, FND-PARTY-053 |
| `0x4F` | 2 | `UINT16LE` | `hit_die_total` | The sum of the hit die rolls of the character's levels. A load copies it to the FMT-COMBAT-002 record's word at `+0x0A`. Generation sets it from a roll per level, each new level in play adds a roll, and the greatest hit points are set from it times the levels held over the greatest levels reached (`greatest_levels`), divided by the number of classes, plus the constitution bonus. | supported | FND-PARTY-051, FND-PARTY-074, FND-PARTY-081 |
| `0x51` | 2 | `UINT16LE` | `max_psionic_points` | The character's greatest psionic strength points. A load copies it to the FMT-COMBAT-002 record's word at `+0x0C`, which the character sheet prints after the current points; 27 to 202 in the shipped records. | supported | FND-PARTY-051, FND-PARTY-059 |
| `0x53` | 2 | `INT16LE` | `details_key` | The key of the character's details record. A load copies it to the FMT-COMBAT-002 record's `unk_0E`; a party slot whose details record has key 0 holds no character, and creatures loaded beyond the party share a details record whose key equals theirs. Generation sets it to the new `combatant_id`. Equal to `combatant_id` in 14 of the 19 shipped records; -21, -14, -7039, -7036 and -7038 in the other five, whose `combatant_id` is 0x82FD to 0x8300. | supported | FND-PARTY-051, FND-PARTY-053, FND-PARTY-147 |
| `0x55` | 2 | `UINT16LE` | `class_flags` | One bit per class counted (FND-PARTY-085): 1, 2, 4 and 8 for the four cleric codes, 0x10 for a druid, 0x20 fighter, 0x40 gladiator, 0x80 preserver, 0x100 psionicist, 0x200 ranger, 0x400 thief; a human's former class counts only while it is below the current one. A load copies it to the FMT-COMBAT-002 record's word at `0x10`; storing a character, a class change and each level gained in play set it. | supported | FND-PARTY-051, FND-PARTY-085 |
| `0x57` | 1 | `UINT8` | `origin` | The character's origin, counted from 1: human, dwarf, elf, half-elf, half-giant, halfling, mul, thri-kreen. The character sheet prints it from the details record's byte at `0x12`. | supported | FND-PARTY-051, FND-PARTY-055, FND-PARTY-056 |
| `0x58` | 1 | `UINT8` | `gender` | 1 for male, 2 for female; printed from the details record's byte at `0x13`. | supported | FND-PARTY-051, FND-PARTY-055, FND-PARTY-056 |
| `0x59` | 1 | `UINT8` | `alignment` | The alignment, counted from 1: lawful good, lawful neutral, lawful evil, neutral good, true neutral, neutral evil, chaotic good, chaotic neutral, chaotic evil; printed from the details record's byte at `0x14`. | supported | FND-PARTY-051, FND-PARTY-055, FND-PARTY-056 |
| `0x5A` | 6 | `UINT8[6]` | `base_scores` | The six ability scores as generated, origin modifiers included, in the order of `0x23` to `0x28`. A load copies them to bytes `0x15` to `0x1A` of the FMT-COMBAT-002 record, from which overlay 197 `+1E7B` sets the current scores; equal to `0x23` to `0x28` in 18 of the 19 shipped records, and one below the strength in resource 50. | supported | FND-PARTY-051, FND-PARTY-070, FND-PARTY-142 |
| `0x60` | 3 | `UINT8[3]` | `classes` | Up to three class codes (see the enumeration below), 0 for none, nonzero ones first. The character sheet prints a level for each nonzero one. | supported | FND-PARTY-051, FND-PARTY-055, FND-PARTY-056, FND-PARTY-057 |
| `0x63` | 3 | `UINT8[3]` | `levels` | The level in each class of `classes`, 0 where the class is 0; printed from the details record's bytes `0x1E` to `0x20`. In play overlay 210 raises a level while the experience reaches the class's next threshold, up to 15, and its level drain lowers it when a party member receives the effect code 59, which a strike by the item of `DATA` number 313 on an attack roll of 20 sends, as do the spells `DATA` 104 and 225, whose byte `+0x19` is 59, used in a fight on a target that fails its saving throw, and a script through function 23 of opcode `0x22`; the class change of overlay 209 sets the new class to level 1. | supported | FND-PARTY-051, FND-PARTY-055, FND-PARTY-056, FND-PARTY-081, FND-PARTY-082, FND-PARTY-083, FND-PARTY-096, FND-PARTY-108 |
| `0x66` | 1 | `INT8` | `base_armor_class` | The character's armor class before items and dexterity. A load copies it to byte `0x21` of the FMT-COMBAT-002 record, from which overlay 173 `+2C00` computes the armor class an attack is rolled against; 10 or, for five of the six thri-kreen, 5 in the shipped records, with 0 and 7 once each. | supported | FND-PARTY-051, FND-PARTY-145 |
| `0x67` | 1 | `UINT8` | `base_movement_rate` | The character's movement rate before items: 12, or 15 for a thri-kreen, set by generation. A load copies it to byte `0x22` of the FMT-COMBAT-002 record; 12 or 15 in the shipped records, with 64 in three. | supported | FND-PARTY-051, FND-PARTY-146 |
| `0x68` | 1 | `INT8` | `magic_resistance` | The character's magic resistance in percent. A load copies it to byte `0x23` of the FMT-COMBAT-002 record; a spell or power is resisted when 1d100 falls below it after overlay 197 `+16B5`'s adjustments. 0 in the shipped records. | supported | FND-PARTY-051, FND-PARTY-146, FND-PARTY-150 |
| `0x69` | 1 | `UINT8` | `class_attack_rate` | 2, plus 1 for each of a warrior-group level above 0, above 6 and above 12. A load copies it to byte `0x24` of the FMT-COMBAT-002 record; storing a character, a class change and each level gained in play set it. The swing of overlay 173 takes it as an armed attack's rate unless the weapon's own rate applies. | supported | FND-PARTY-051, FND-PARTY-085, FND-PARTY-100 |
| `0x6A` | 1 | `UINT8` | `attack_rate` | The rate of natural attack 0: `class_attack_rate`, or 8 for a thri-kreen. A load copies it to byte `0x25` of the FMT-COMBAT-002 record; set with `class_attack_rate`. The swing of overlay 173 takes it for an attack without a weapon, in the place of a weapon's rate. | supported | FND-PARTY-051, FND-PARTY-085, FND-PARTY-100 |
| `0x6B` | 2 | `UINT8[2]` | `natural_attack_rates` | The rates of natural attacks 1 and 2; overlay 210 sets the second to 2 for a thri-kreen. A load copies them to bytes `0x26` and `0x27` of the FMT-COMBAT-002 record, which the swing reads as `attack_rate` is read. | supported | FND-PARTY-051, FND-PARTY-085, FND-PARTY-100 |
| `0x6D` | 3 | `UINT8[3]` | `natural_damage_dice` | The number of damage dice of natural attacks 0 to 2. A load copies them to bytes `0x28` to `0x2A` of the FMT-COMBAT-002 record, which the swing rolls on a hit. | supported | FND-PARTY-051, FND-PARTY-100 |
| `0x70` | 3 | `UINT8[3]` | `natural_damage_sides` | The sides of the damage dice of natural attacks 0 to 2; copied to bytes `0x2B` to `0x2D`. | supported | FND-PARTY-051, FND-PARTY-100 |
| `0x73` | 3 | `INT8[3]` | `natural_damage_bonuses` | The damage added to the dice of natural attacks 0 to 2; copied to bytes `0x2E` to `0x30`. | supported | FND-PARTY-051, FND-PARTY-100 |
| `0x76` | 5 | `UINT8[5]` | `saving_throws` | Five saves, each the least over the class groups of a starting value less a fall per level, the fall capped (FND-PARTY-085, BUG-PARTY-004). A load copies them to bytes `0x31` to `0x35` of the FMT-COMBAT-002 record; storing a character, a class change and each level gained in play set them. A saving throw against an effect whose `DATA` record names save 1 to 5 compares a d20 roll with the byte of that number (FND-PARTY-099). | supported | FND-PARTY-051, FND-PARTY-085, FND-PARTY-099 |
| `0x7B` | 1 | `UINT8` | `unk_7b` | Purpose unknown. A load copies it to byte `0x36` of the FMT-COMBAT-002 record, which no read found uses; 0, 1 or 4 in the shipped records. | supported | FND-PARTY-051, FND-PARTY-151 |
| `0x7C` | 1 | `UINT8` | `footprint` | A load copies it to the FMT-COMBAT-002 record's `footprint`; 0 in the shipped records. | supported | FND-PARTY-051, FND-PARTY-151 |
| `0x7D` | 1 | `UINT8` | `spell_list` | The number, below 13, of the list from which overlay 177 `+0059` prepares a caster's spells. A load copies it to byte `0x38` of the FMT-COMBAT-002 record; 0 in the shipped records. | supported | FND-PARTY-051, FND-PARTY-151 |
| `0x7E` | 3 | `UINT8[3]` | `greatest_levels` | The greatest level the character has held in each class position. A load copies it to bytes `0x39` to `0x3B` of the FMT-COMBAT-002 record. Generation copies `levels` here, a new level in play above the byte raises it, and the level drain leaves it; a roll is added to `hit_die_total` only while the sum of `levels` is not below the sum of these bytes. | supported | FND-PARTY-051, FND-PARTY-074, FND-PARTY-081, FND-PARTY-082 |
| `0x81` | 2 | `UINT16LE` | `attack_sound` | The number of the sound the character's natural attack makes, 0 for the default sound 98. A load copies it to the FMT-COMBAT-002 record's word at `0x3C`, which overlay 179 `+0D2F` plays; 67 or 68 in four shipped records and 0 in the others. | supported | FND-PARTY-051, FND-PARTY-149 |
| `0x83` | 2 | `UINT16LE` | `damage_sound` | The number of a sound the damage routine plays for the character through overlay 206 `+0000`, 0 for none. A load copies it to the FMT-COMBAT-002 record's word at `0x3E`; 69 in one shipped record and 0 in the others. | supported | FND-PARTY-051, FND-PARTY-149 |
| `0x85` | 1 | `UINT8` | `psionic_list` | The psionic power list of a non-party combatant: 2 to 8 pick one of the lists at `5284:0003`, from which overlays 174 and 198 pick a power at random; 0 or 1 gives none. A load copies it to byte `0x40` of the FMT-COMBAT-002 record. 0 in the shipped records. | supported | FND-PARTY-051, FND-PARTY-149, FND-PARTY-158 |
| `0x86` | 1 | `UINT8` | `unk_86` | Purpose unknown. A load copies it to byte `0x41` of the FMT-COMBAT-002 record, which no read found uses. 0 in the shipped records. | supported | FND-PARTY-051, FND-PARTY-149, FND-PARTY-158 |
| `0x87` | `33 * (chunk_count - 2)` | FMT-PARTY-006`[chunk_count - 2]` | `chunks` | The other chunks, each of `chunk_type` 2 or 4 with 23 bytes of data in the shipped records. | supported | FND-PARTY-051, FND-PARTY-052 |
| | 10 | FMT-PARTY-006 header | `end` | `chunk_type` 0xFF and `len_data` 0, the other bytes as at `0x01` to `0x07`. | supported | FND-PARTY-051, FND-PARTY-052 |
| | | | | Total size `145 + 33 * (chunk_count - 2)` in the shipped records | | |

The scores at `0x23` to `0x28` are the current ones: generation stores them with the origin
modifiers added, and in play overlay 197 `+1E7B` sets them from `base_scores` and the entries at
`4E71:0251` it applies [FND-PARTY-071, FND-PARTY-142].

The value file `FMT-PARTY-001.characters.csv` gives, for each of the 19 `CHAR` resources of the
installed `CHARSAVE.GFF`, the columns `character` (the resource number), `type`,
`chunk_count`, the six ability scores and `name` (the text before the NUL) [FND-PARTY-001,
FND-PARTY-003, FND-PARTY-052]. The eight records of the disc's copy are the rows 40 to 43 and
50 to 53. The other fields and the chunks from `0x87` are left out.

## Enumerations and flags

### `classes`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 1 | `CLASS_CLERIC_AIR` | Cleric of the air sphere, the first choice in the generation screen's sphere window. | supported | FND-PARTY-057, FND-PARTY-073, FND-PARTY-066 |
| 2 | `CLASS_CLERIC_EARTH` | Cleric of the earth sphere, the second choice in the generation screen's sphere window. | supported | FND-PARTY-057, FND-PARTY-073, FND-PARTY-066 |
| 3 | `CLASS_CLERIC_FIRE` | Cleric of the fire sphere, the third choice in the generation screen's sphere window. | supported | FND-PARTY-057, FND-PARTY-073, FND-PARTY-066 |
| 4 | `CLASS_CLERIC_WATER` | Cleric of the water sphere, the fourth choice in the generation screen's sphere window. | supported | FND-PARTY-057, FND-PARTY-073, FND-PARTY-066 |
| 5 | `CLASS_DRUID_AIR` | Druid of the air sphere, the first choice in the generation screen's sphere window. | supported | FND-PARTY-057, FND-PARTY-073, FND-PARTY-066 |
| 6 | `CLASS_DRUID_EARTH` | Druid of the earth sphere, the second choice in the generation screen's sphere window. | supported | FND-PARTY-057, FND-PARTY-073, FND-PARTY-066 |
| 7 | `CLASS_DRUID_FIRE` | Druid of the fire sphere, the third choice in the generation screen's sphere window. | supported | FND-PARTY-057, FND-PARTY-073, FND-PARTY-066 |
| 8 | `CLASS_DRUID_WATER` | Druid of the water sphere, the fourth choice in the generation screen's sphere window. | supported | FND-PARTY-057, FND-PARTY-073, FND-PARTY-066 |
| 9 | `CLASS_FIGHTER` | Fighter. | supported | FND-PARTY-057 |
| 10 | `CLASS_GLADIATOR` | Gladiator. | supported | FND-PARTY-057 |
| 11 | `CLASS_PRESERVER` | Preserver. | supported | FND-PARTY-057 |
| 12 | `CLASS_PSIONICIST` | Psionicist, named `Psionic` on the class line. | supported | FND-PARTY-057 |
| 13 | `CLASS_RANGER_AIR` | Ranger of the air sphere, the first choice in the generation screen's sphere window. | supported | FND-PARTY-057, FND-PARTY-073, FND-PARTY-066 |
| 14 | `CLASS_RANGER_EARTH` | Ranger of the earth sphere, the second choice in the generation screen's sphere window. | supported | FND-PARTY-057, FND-PARTY-073, FND-PARTY-066 |
| 15 | `CLASS_RANGER_FIRE` | Ranger of the fire sphere, the third choice in the generation screen's sphere window. | supported | FND-PARTY-057, FND-PARTY-073, FND-PARTY-066 |
| 16 | `CLASS_RANGER_WATER` | Ranger of the water sphere, the fourth choice in the generation screen's sphere window. | supported | FND-PARTY-057, FND-PARTY-073, FND-PARTY-066 |
| 17 | `CLASS_THIEF` | Thief. | supported | FND-PARTY-057 |

## Differences between builds

None known.

## Coverage

All 19 `CHAR` resources of the installed `CHARSAVE.GFF` and the 8 of the disc's copy, which are
the same bytes as the installed resources of the same numbers: in every one the chain of chunks
ends on the last 10 bytes, the first two chunks have the headers the table gives, and the name
ends within its slot [FND-PARTY-001, FND-PARTY-005, FND-PARTY-052].

## Open questions

- What `unk_1c`, `unk_21`, `unk_2a`,
  `unk_7b` and `unk_86` hold. The two chunks at `0x00` and `0x3B` are the only ones that
  reach the combatant and details records, so the screen's values are in them or in the records of
  the later chunks (FND-PARTY-020, FND-PARTY-051, FND-PARTY-057, FND-PARTY-058,
  FND-PARTY-059, Q-PARTY-003).
- What `combatant_id` identifies: overlay 184 writes the combatant record's copy of it to a
  `CACT` resource (FND-PARTY-012), and the shipped values are never the record's own number
  (FND-PARTY-050, Q-PARTY-003).
