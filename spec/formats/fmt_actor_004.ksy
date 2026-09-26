meta:
  id: fmt_actor_004
  title: Object slot record
  license: MIT
  endian: le
doc: One 37-byte object slot record of the table the game keeps in memory.
doc-ref: FMT-ACTOR-004, FND-ACTOR-003, FND-ACTOR-005, FND-COMBAT-026
seq:
  - id: unk_00
    type: u1
  - id: entry_index
    type: u2
    doc: Index of the 8-byte entry the slot was filled from.
  - id: x
    type: s2
    doc: Horizontal place of the figure.
  - id: y
    type: s2
    doc: Vertical place of the figure.
  - id: unk_07
    type: u1
  - id: unk_08
    type: u2
  - id: unk_0a
    type: u2
  - id: unk_0c
    type: u2
  - id: unk_0e
    type: u1
  - id: unk_0f
    type: u1
  - id: unk_10
    size: 2
  - id: unk_12
    type: u1
  - id: width
    type: u1
    doc: Width of the figure.
  - id: height
    type: u1
    doc: Height of the figure.
  - id: unk_15
    size: 4
  - id: image
    type: u2
    doc: Image number the slot draws.
  - id: object_number
    type: u2
    doc: Object number the slot was filled with.
  - id: unk_1d
    type: u4
  - id: unk_21
    size: 4
