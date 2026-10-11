meta:
  id: fmt_party_001
  title: Character record
  license: MIT
  endian: le
  imports:
    - fmt_party_006
doc: |
  A CHAR resource of the character archive, one per character: a chain of chunks
  (fmt_party_006) ending at one whose type is 0xFF. Every shipped record starts with a
  type-1 chunk at 0x00 and a type-3 chunk at 0x3B, given here field by field.
doc-ref: FMT-PARTY-001, FND-PARTY-001, FND-PARTY-003, FND-PARTY-020, FND-PARTY-049, FND-PARTY-050, FND-PARTY-051, FND-PARTY-052, FND-PARTY-053, FND-PARTY-055, FND-PARTY-056, FND-PARTY-058, FND-PARTY-059
seq:
  - id: type
    type: u1
    doc: The first chunk's type, 1 in every shipped record.
  - id: chunk_count
    type: u1
    doc: Number of chunks before the end header; the load does not read it.
  - id: kind
    type: u1
  - id: unk_03
    type: u1
  - id: saved_slot
    type: u2
    doc: The party slot the character was written from (the first chunk's record_ref).
  - id: saved_field
    type: u2
    doc: The first chunk's field, which the writer sets to the slot.
  - id: len_data
    type: u2
    doc: 49, the size of the data from 0x0A.
  - id: hit_points
    type: s2
    doc: The character's current hit points, copied to the combatant record.
  - id: psionic_points
    type: u2
    doc: The character's current psionic strength points.
  - id: saved_details_index
    type: u2
    doc: The details_index when the character was written.
  - id: combatant_id
    type: u2
    doc: Copied to the combatant record's character_id.
  - id: saved_child_refs
    type: u2
    repeat: expr
    repeat-expr: 3
    doc: The combatant record's words at 0x08, 0x0A and 0x0C when written; 9999 for none.
  - id: special_attack
    type: u1
    doc: A monster's special attack kind, 0 for none, mapped to a MONS DATA record by overlay 198 +0149.
  - id: immunity_type
    type: u1
    doc: Index into the 18 immunity words at DS:2588.
  - id: object_offset
    type: u2
    doc: 300 plus this is the object number placed for the character.
  - id: unk_1c
    type: u1
  - id: movement_rate
    type: u1
  - id: combat_mark
    type: u1
  - id: combatant_kind
    type: u1
    doc: The combatant's kind, which sets its side; 0 is the party's own kind.
  - id: thac0
    type: u1
    doc: 20 less the best class group's level term.
  - id: unk_21
    type: u1
  - id: control_flags
    type: u1
    doc: Copied to the combatant record's byte at 0x18 (computer_control, control_locked).
  - id: strength
    type: u1
  - id: dexterity
    type: u1
  - id: constitution
    type: u1
  - id: intelligence
    type: u1
  - id: wisdom
    type: u1
  - id: charisma
    type: u1
  - id: facing
    type: u1
    doc: The direction the character faces, 0 to 7.
  - id: unk_2a
    type: u1
  - id: name
    type: strz
    encoding: ASCII
    size: 16
    doc: The character's name, ending at the first NUL; bytes after it may hold leftover text.
  - id: details_header
    size: 10
    doc: A chunk header with type 3, kind 3, field 15 and length 66.
  - id: experience
    type: u4
    doc: The character's experience points, copied to the combatant details record.
  - id: kill_experience
    type: u4
    doc: The experience a kill of this character as an enemy gives, raised to the experience by each award.
  - id: max_hit_points
    type: s2
    doc: The character's greatest hit points, copied to the combatant details record.
  - id: hit_die_total
    type: u2
    doc: The sum of the hit die rolls of the character's levels.
  - id: max_psionic_points
    type: u2
    doc: The character's greatest psionic strength points.
  - id: details_key
    type: s2
  - id: class_flags
    type: u2
    doc: One bit per class counted.
  - id: origin
    type: u1
    doc: Origin counted from 1 (human, dwarf, elf, half-elf, half-giant, halfling, mul, thri-kreen).
  - id: gender
    type: u1
    doc: 1 male, 2 female.
  - id: alignment
    type: u1
    doc: Alignment counted from 1, lawful good to chaotic evil.
  - id: base_scores
    type: u1
    repeat: expr
    repeat-expr: 6
    doc: The six ability scores as generated, origin modifiers included.
  - id: classes
    type: u1
    repeat: expr
    repeat-expr: 3
    doc: Up to three class codes, 0 for none.
  - id: levels
    type: u1
    repeat: expr
    repeat-expr: 3
    doc: The level in each class.
  - id: base_armor_class
    type: s1
    doc: The armor class before items and dexterity.
  - id: base_movement_rate
    type: u1
  - id: magic_resistance
    type: s1
    doc: Magic resistance in percent.
  - id: class_attack_rate
    type: u1
  - id: attack_rate
    type: u1
  - id: natural_attack_rates
    type: u1
    repeat: expr
    repeat-expr: 2
    doc: The rates of natural attacks 1 and 2.
  - id: natural_damage_dice
    type: u1
    repeat: expr
    repeat-expr: 3
    doc: The number of damage dice of natural attacks 0 to 2.
  - id: natural_damage_sides
    type: u1
    repeat: expr
    repeat-expr: 3
    doc: The sides of the damage dice of natural attacks 0 to 2.
  - id: natural_damage_bonuses
    type: s1
    repeat: expr
    repeat-expr: 3
    doc: The damage added to the dice of natural attacks 0 to 2.
  - id: saving_throws
    type: u1
    repeat: expr
    repeat-expr: 5
    doc: Five saving throws.
  - id: unk_7b
    type: u1
  - id: footprint
    type: u1
  - id: spell_list
    type: u1
    doc: List from which overlay 177 prepares a caster's spells.
  - id: greatest_levels
    type: u1
    repeat: expr
    repeat-expr: 3
    doc: The greatest level held in each class position.
  - id: attack_sound
    type: u2
    doc: Sound of the character's natural attack; 0 for the default sound 98.
  - id: damage_sound
    type: u2
    doc: Sound the damage routine plays for the character; 0 for none.
  - id: psionic_list
    type: u1
    doc: A non-party combatant's psionic power list, 2 to 8, among the lists at 5284:0003; 0 or 1 for none.
  - id: unk_86
    type: u1
  - id: chunks
    type: fmt_party_006
    repeat: until
    repeat-until: _.chunk_type == fmt_party_006::chunk_types::end
    doc: The other chunks and the end header.
