meta:
  id: fmt_combat_002
  title: Combatant details record
  license: MIT
  endian: le
doc: One 66-byte combatant details record of the table the game keeps in memory.
doc-ref: FMT-COMBAT-002, FND-COMBAT-022, FND-COMBAT-023, FND-COMBAT-072, FND-COMBAT-074, FND-PARTY-051
seq:
  - id: experience
    type: u4
  - id: kill_experience
    type: u4
  - id: max_hit_points
    type: s2
    doc: Greatest hit points.
  - id: hit_die_total
    type: u2
  - id: max_psionic_points
    type: u2
  - id: details_key
    type: s2
    doc: Key creatures share a record by, 0 for a free record.
  - id: class_flags
    type: u2
  - id: origin
    type: u1
  - id: gender
    type: u1
  - id: alignment
    type: u1
  - id: base_scores
    type: u1
    repeat: expr
    repeat-expr: 6
  - id: classes
    type: u1
    repeat: expr
    repeat-expr: 3
  - id: levels
    type: u1
    repeat: expr
    repeat-expr: 3
  - id: base_armor_class
    type: s1
  - id: base_movement_rate
    type: u1
  - id: magic_resistance
    type: s1
  - id: class_attack_rate
    type: u1
  - id: natural_attack_rates
    type: u1
    repeat: expr
    repeat-expr: 3
  - id: natural_damage_dice
    type: u1
    repeat: expr
    repeat-expr: 3
  - id: natural_damage_sides
    type: u1
    repeat: expr
    repeat-expr: 3
  - id: natural_damage_bonuses
    type: s1
    repeat: expr
    repeat-expr: 3
  - id: saving_throws
    type: u1
    repeat: expr
    repeat-expr: 5
  - id: unk_36
    type: u1
  - id: footprint
    type: u1
    doc: Size of the occupied square in the low four bits, corner trim in the high four.
  - id: spell_list
    type: u1
  - id: greatest_levels
    type: u1
    repeat: expr
    repeat-expr: 3
  - id: attack_sound
    type: u2
  - id: damage_sound
    type: u2
  - id: psionic_list
    type: u1
  - id: unk_41
    type: u1
