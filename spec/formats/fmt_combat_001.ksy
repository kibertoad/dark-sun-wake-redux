meta:
  id: fmt_combat_001
  title: Combatant record
  license: MIT
  endian: le
  bit-endian: le
doc: One 49-byte combatant record of the table the game keeps in memory.
doc-ref: FMT-COMBAT-001, FND-COMBAT-052, FND-COMBAT-022, FND-COMBAT-023, FND-AI-003, FND-AI-004, FND-COMBAT-072, FND-COMBAT-073, FND-COMBAT-074, FND-PARTY-049, FND-MAGIC-013
seq:
  - id: hit_points
    type: s2
    doc: Current hit points.
  - id: psionic_points
    type: u2
    doc: Current psionic strength points.
  - id: details_index
    type: u2
    doc: Index of the combatant's details record.
  - id: character_id
    type: u2
    doc: Identifier of the character in the character archive, 0 for an empty slot.
  - id: worn_items
    type: u2
    doc: Handle of the first record of the list the ability rebuild counts, 9999 for none.
  - id: held_items
    type: u2
    doc: Handle of the first record of the list the attack takes weapons from, 9999 for none.
  - id: carried_items
    type: u2
    doc: Handle of the first record of the list neither armour class nor attack counts, 9999 for none.
  - id: special_attack
    type: u1
  - id: immunity_type
    type: u1
  - id: object_offset
    type: u2
  - id: generation_armor_class
    type: s1
  - id: movement_rate
    type: u1
  - id: combat_mark
    type: u1
    doc: Set to 1 for every present party member when combat starts.
  - id: combatant_kind
    type: u1
  - id: thac0
    type: s1
  - id: unk_17
    type: u1
  - id: script_flags_low
    type: b5
  - id: computer_control
    type: b1
    doc: Set when the computer controls the party member.
  - id: control_locked
    type: b1
    doc: Set when the computer-control setting cannot be changed.
  - id: script_flag_high
    type: b1
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
  - id: script_flags
    type: u1
  - id: name
    type: strz
    encoding: ASCII
    size: 16
    doc: The character's name, ending at the first NUL.
