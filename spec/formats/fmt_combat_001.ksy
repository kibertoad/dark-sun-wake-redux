meta:
  id: fmt_combat_001
  title: Combatant record
  license: MIT
  endian: le
doc: One 49-byte combatant record of the table the game keeps in memory.
doc-ref: FMT-COMBAT-001, FND-COMBAT-008, FND-COMBAT-022, FND-COMBAT-023
seq:
  - id: hit_points
    type: s2
    doc: Current hit points.
  - id: unk_02
    size: 4
  - id: character_id
    type: u2
    doc: Identifier of the character in the character archive, 0 for an empty slot.
  - id: unk_08
    size: 12
  - id: combat_mark
    type: u1
    doc: Set to 1 for every present party member when combat starts.
  - id: unk_15
    size: 3
  - id: unk_18
    type: u1
  - id: unk_19
    size: 8
  - id: name
    type: strz
    encoding: ASCII
    size: 16
    doc: The character's name, ending at the first NUL.
