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
doc-ref: FMT-PARTY-001, FND-PARTY-001, FND-PARTY-003, FND-PARTY-020, FND-PARTY-049, FND-PARTY-050, FND-PARTY-051, FND-PARTY-052, FND-PARTY-053
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
  - id: details_header
    size: 10
    doc: A chunk header with type 3, kind 3, field 15 and length 66.
  - id: unk_45
    size: 8
  - id: max_hit_points
    type: s2
    doc: The character's greatest hit points, copied to the combatant details record.
  - id: unk_4f
    size: 4
  - id: unk_53
    type: u2
  - id: unk_55
    size: 50
  - id: chunks
    type: fmt_party_006
    repeat: until
    repeat-until: _.chunk_type == fmt_party_006::chunk_types::end
    doc: The other chunks and the end header.
