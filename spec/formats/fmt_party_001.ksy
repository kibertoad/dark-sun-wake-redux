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
  - id: unk_04
    size: 4
  - id: len_data
    type: u2
    doc: 49, the size of the data from 0x0A.
  - id: hit_points
    type: s2
    doc: The character's current hit points, copied to the combatant record.
  - id: psionic_points
    type: u2
    doc: The character's current psionic strength points.
  - id: unk_0e
    type: u2
  - id: combatant_id
    type: u2
    doc: Copied to the combatant record's character_id.
  - id: unk_12
    size: 8
  - id: object_offset
    type: u2
    doc: 300 plus this is the object number placed for the character.
  - id: unk_1c
    size: 2
  - id: combat_mark
    type: u1
  - id: unk_1f
    type: u1
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
  - id: unk_29
    size: 2
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
  - id: unk_53
    type: u2
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
  - id: unk_5a
    size: 6
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
  - id: unk_66
    size: 3
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
    size: 3
  - id: greatest_levels
    type: u1
    repeat: expr
    repeat-expr: 3
    doc: The greatest level held in each class position.
  - id: unk_81
    size: 6
  - id: chunks
    type: fmt_party_006
    repeat: until
    repeat-until: _.chunk_type == fmt_party_006::chunk_types::end
    doc: The other chunks and the end header.
