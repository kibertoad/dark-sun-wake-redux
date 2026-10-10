meta:
  id: fmt_party_001
  title: Character record
  license: MIT
  endian: le
  imports:
    - fmt_party_002
doc: A CHAR resource of the character archive, one per character.
doc-ref: FMT-PARTY-001, FND-PARTY-001, FND-PARTY-003, FND-PARTY-004, FND-PARTY-020, FND-PARTY-049, FND-PARTY-050
seq:
  - id: version
    type: u1
    doc: 1 in every shipped record.
  - id: tail_count
    type: u1
    doc: Number of 33-byte records after the header.
  - id: unk_02
    size: 8
  - id: hit_points
    type: s2
    doc: The character's current hit points, copied to the combatant record.
  - id: unk_0c
    size: 2
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
    size: 3
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
  - id: unk_3b
    size: 20
  - id: tail
    type: fmt_party_002
    repeat: expr
    repeat-expr: tail_count
