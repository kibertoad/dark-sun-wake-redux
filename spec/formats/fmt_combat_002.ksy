meta:
  id: fmt_combat_002
  title: Combatant details record
  license: MIT
  endian: le
doc: One 66-byte combatant details record of the table the game keeps in memory.
doc-ref: FMT-COMBAT-002, FND-COMBAT-022, FND-COMBAT-023
seq:
  - id: unk_00
    size: 8
  - id: max_hit_points
    type: s2
    doc: Greatest hit points.
  - id: unk_0a
    size: 4
  - id: unk_0e
    type: u2
  - id: unk_10
    size: 39
  - id: footprint
    type: u1
    doc: Size of the occupied square in the low four bits, corner trim in the high four.
  - id: unk_38
    size: 10
