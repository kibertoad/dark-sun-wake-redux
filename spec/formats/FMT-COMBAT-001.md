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
evidence: [FND-AI-003, FND-AI-004, FND-AI-005, FND-COMBAT-052, FND-COMBAT-018, FND-COMBAT-022, FND-COMBAT-023, FND-COMBAT-024, FND-PARTY-012, FND-PARTY-013, FND-EXPLORE-002, FND-MAGIC-012, FND-MAGIC-013, FND-PARTY-048, FND-PARTY-049, FND-PARTY-052, FND-PARTY-059, FND-PARTY-085, FND-PARTY-095, FND-PARTY-144, FND-PARTY-145, FND-PARTY-146, FND-PARTY-152, FND-PARTY-156, FND-PARTY-157, FND-PARTY-221, FND-COMBAT-065, FND-COMBAT-068, FND-COMBAT-069, FND-COMBAT-071, FND-COMBAT-072, FND-COMBAT-073, FND-COMBAT-074]
conflicting: []
split_with: []
related: [RULE-AI-001, RULE-COMBAT-004, RULE-COMBAT-005]
---

## Layout

One of the 42 records of the table at the far pointer `57E0:19C9` in BLD-GOG-EN-1.1, kept only in
memory and allocated by overlay 188 `+0000`. Records 0 to 4 belong to objects 0 to 4, and records
0 to 3 are the four party slots [FND-COMBAT-052, FND-PARTY-013]; a creature gets the lowest record
from 5 to 41 whose `character_id` is 0 [FND-COMBAT-072]. `2D40:3E64` gives an object's record
number as its first index [FND-COMBAT-072]. Each field has a field number of `FNFO` 1
[FND-COMBAT-074], and the `CHAR` load copies `CHAR` offset `0x0A + n` to offset `n`
[FND-PARTY-049].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `INT16LE` | `hit_points` | The character's current hit points, the first number of the panel's second line. Field 13. | supported | FND-COMBAT-018, FND-COMBAT-022, FND-COMBAT-074 |
| `0x02` | 2 | `UINT16LE` | `psionic_points` | The current psionic strength points, field 14. A load copies the `CHAR` record's `psionic_points`; `2D40:3EBF` raises it over time up to the details record's `max_psionic_points`. | supported | FND-PARTY-049, FND-PARTY-059, FND-COMBAT-073, FND-COMBAT-074 |
| `0x04` | 2 | `UINT16LE` | `details_index` | Index of the combatant's FMT-COMBAT-002 record, field 15: the object's own number for objects 0 to 4, otherwise the shared record of its `details_key`. `2D40:3E64` gives it as its second index when it is below 17. Equal to the slot for party slots. | supported | FND-COMBAT-022, FND-EXPLORE-002, FND-MAGIC-012, FND-COMBAT-072 |
| `0x06` | 2 | `UINT16LE` | `character_id` | The identifier of the character in the character archive, field 0; 0 in a slot that holds no character, and 0 marks the record free for `2D40:2C87`, which a release stores. | supported | FND-COMBAT-052, FND-PARTY-012, FND-COMBAT-072 |
| `0x08` | 2 | `UINT16LE` | `worn_items` | Field 16: the handle of the first 23-byte record of a list, 9999 for none. The ability rebuild and two spell routines count the items of this list. | supported | FND-PARTY-049, FND-PARTY-052, FND-MAGIC-013, FND-COMBAT-074 |
| `0x0A` | 2 | `UINT16LE` | `held_items` | Field 17: the handle of the first record of the list the attack routine takes its weapons from, 9999 for none. | supported | FND-PARTY-049, FND-PARTY-052, FND-COMBAT-065, FND-COMBAT-074 |
| `0x0C` | 2 | `UINT16LE` | `carried_items` | Field 4: the handle of the first record of the list that neither the armour class nor the attack counts, the target of the recipient transfer, 9999 for none. | supported | FND-PARTY-049, FND-PARTY-052, FND-PARTY-221, FND-COMBAT-068, FND-COMBAT-074 |
| `0x0E` | 1 | `UINT8` | `special_attack` | Field 18: a monster's special attack kind, 0 for none (FMT-PARTY-001 `special_attack`). | supported | FND-PARTY-049, FND-PARTY-157, FND-COMBAT-074 |
| `0x0F` | 1 | `UINT8` | `immunity_type` | Field 19: an index into the 18 immunity words at `DS:2588` (FMT-PARTY-001 `immunity_type`). | supported | FND-PARTY-049, FND-PARTY-157, FND-COMBAT-069, FND-COMBAT-074 |
| `0x10` | 2 | `UINT16LE` | `object_offset` | Field 6: 300 plus this is the object placed for the character (FMT-PARTY-001 `object_offset`). The party loader computes a value from it. | supported | FND-PARTY-013, FND-PARTY-048, FND-PARTY-049, FND-COMBAT-074 |
| `0x12` | 1 | `INT8` | `generation_armor_class` | Field 20. The generation finish stores `base_armor_class` here; nothing in the executable or the scripts reads it. | supported | FND-PARTY-049, FND-PARTY-145, FND-PARTY-152, FND-COMBAT-074 |
| `0x13` | 1 | `UINT8` | `movement_rate` | Field 21: the current movement rate (FMT-PARTY-001 `movement_rate`). | supported | FND-PARTY-049, FND-PARTY-146, FND-COMBAT-074 |
| `0x14` | 1 | `UINT8` | `combat_mark` | Field 22. Set to 1 in every party slot with a character when combat starts. The panel names an effect only when it is 1, and the leader can be changed to a slot only when it is 0 or 1. Values 0, 1, 2, 3, 7 and 8 (FND-COMBAT-071). | supported | FND-COMBAT-052, FND-COMBAT-022, FND-COMBAT-023, FND-PARTY-013, FND-COMBAT-071 |
| `0x15` | 1 | `UINT8` | `combatant_kind` | Field 23: the combatant's kind, which sets its side; the computer's target choice masks candidates by it. | supported | FND-PARTY-049, FND-PARTY-095, FND-COMBAT-071, FND-COMBAT-074 |
| `0x16` | 1 | `INT8` | `thac0` | Field 24: the THAC0 the attack routine starts from. | supported | FND-PARTY-049, FND-PARTY-085, FND-COMBAT-065, FND-COMBAT-074 |
| `0x17` | 1 | `UINT8` | `unk_17` | Purpose unknown. Field 9, the number the 23-byte record uses for its byte `+0x12`; 7 in the shipped `CHAR` records, and nothing reads it. | supported | FND-PARTY-049, FND-PARTY-152, FND-COMBAT-074 |
| `0x18 bits 0..5` | | `bits[5]` | `script_flags_low` | Bits 0 to 4 of field 25: set and tested only by scripts, through function 49 and field paths. | supported | FND-COMBAT-074 |
| `0x18 bits 5..6` | | `bits[1]` | `computer_control` | Set when the computer controls the party member. The computer-control button toggles it, and Space clears it in every slot where `control_locked` is 0. Effects 7, 20, 21, 35 and 102 set it with `control_locked`, and the end of an effect clears both unless another of those remains during a fight. | supported | FND-AI-003, FND-AI-004, FND-AI-005 |
| `0x18 bits 6..7` | | `bits[1]` | `control_locked` | Set when the computer-control setting cannot be changed; the button then shows a message and Space leaves `computer_control` as it is. Set and cleared with `computer_control` by the effect handlers; scripts can store the whole byte. | supported | FND-AI-003, FND-AI-004, FND-AI-005 |
| `0x18 bits 7..8` | | `bits[1]` | `script_flag_high` | Bit 7 of field 25: set and tested only by scripts. | supported | FND-COMBAT-074 |
| `0x19` | 1 | `UINT8` | `strength` | Field 26. The current Strength: a load copies the `CHAR` record's `strength`, and overlay 197 `+1E7B` sets it from `base_scores` changed by effects and items, at most 25. | supported | FND-MAGIC-013, FND-COMBAT-074 |
| `0x1A` | 1 | `UINT8` | `dexterity` | Field 27. The current Dexterity: a load copies the `CHAR` record's `dexterity`, and overlay 197 `+1E7B` sets it from `base_scores` changed by effects and items, at most 25. | supported | FND-MAGIC-013, FND-COMBAT-074 |
| `0x1B` | 1 | `UINT8` | `constitution` | Field 28. The current Constitution: a load copies the `CHAR` record's `constitution`, and overlay 197 `+1E7B` sets it from `base_scores` changed by effects and items, at most 25. At 20 or more `2D40:3EBF` restores hit points over time. | supported | FND-MAGIC-013, FND-COMBAT-073, FND-COMBAT-074 |
| `0x1C` | 1 | `UINT8` | `intelligence` | Field 29. The current Intelligence: a load copies the `CHAR` record's `intelligence`, and overlay 197 `+1E7B` sets it from `base_scores` changed by effects and items, at most 25. | supported | FND-MAGIC-013, FND-COMBAT-074 |
| `0x1D` | 1 | `UINT8` | `wisdom` | Field 30. The current Wisdom: a load copies the `CHAR` record's `wisdom`, and overlay 197 `+1E7B` sets it from `base_scores` changed by effects and items, at most 25. | supported | FND-MAGIC-013, FND-COMBAT-074 |
| `0x1E` | 1 | `UINT8` | `charisma` | Field 31. The current Charisma: a load copies the `CHAR` record's `charisma`, and overlay 197 `+1E7B` sets it from `base_scores` changed by effects and items, at most 25. | supported | FND-MAGIC-013, FND-COMBAT-074 |
| `0x1F` | 1 | `UINT8` | `facing` | Field 32. The direction the character faces; a load copies the `CHAR` record's `facing`. | supported | FND-MAGIC-013, FND-PARTY-049, FND-PARTY-144, FND-COMBAT-074 |
| `0x20` | 1 | `UINT8` | `script_flags` | Field 33: a byte of flags that only scripts read and write, through functions 48 and 49 and field paths. A load copies the `CHAR` record's `unk_2a`. | supported | FND-MAGIC-013, FND-PARTY-049, FND-PARTY-152, FND-PARTY-156, FND-COMBAT-074 |
| `0x21` | 16 | `char[16]` | `name` | Field 34. The character's name, ending at the first NUL. The first line of the panel and the `%Fs` of the combat messages. | supported | FND-COMBAT-018, FND-COMBAT-022, FND-COMBAT-024 |
| `0x31` | | | | Total size 49 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Read from the code that indexes the table with a stride of 49 [FND-AI-003, FND-AI-004,
FND-COMBAT-052, FND-COMBAT-022, FND-COMBAT-023], the allocation and record choice
[FND-COMBAT-072] and the `FNFO` 1 field table [FND-COMBAT-074]; no record was observed in memory.

## Open questions

- What `unk_17` holds (FND-COMBAT-074, Q-COMBAT-043).
- What object 4 holds, and whether it is ever `turn_combatant` (FND-COMBAT-072, Q-COMBAT-042).
- Whether a release frees a creature's shared details record when its own combatant record is the
  last user (FND-COMBAT-072, Q-COMBAT-042).
- Whether the end of any active effect clears a `computer_control` the player set (FND-AI-005,
  Q-AI-005).
