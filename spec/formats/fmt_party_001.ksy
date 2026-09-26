meta:
  id: fmt_party_001
  title: Character record
  license: MIT
  endian: le
  imports:
    - fmt_party_002
doc: A CHAR resource of the character archive, one per character.
doc-ref: FMT-PARTY-001, FND-PARTY-001, FND-PARTY-003, FND-PARTY-004, FND-PARTY-020
seq:
  - id: version
    type: u1
    doc: 1 in every shipped record.
  - id: tail_count
    type: u1
    doc: Number of 33-byte records after the header.
  - id: unk_02
    size: 33
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
