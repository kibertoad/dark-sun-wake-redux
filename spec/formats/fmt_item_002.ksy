meta:
  id: fmt_item_002
  title: Shared item-type record in ALL/4
  license: MIT
  endian: le
doc: One fifteen-byte record. Neutral names preserve raw storage independently of consumer interpretations.
doc-ref: FMT-ITEM-002, FND-ITEM-011, FND-ITEM-012, FND-ACTOR-017
seq:
  - id: byte_00
    type: u1
  - id: byte_01
    type: u1
  - id: byte_02
    type: u1
  - id: byte_03
    type: u1
  - id: byte_04
    type: u1
  - id: byte_05
    type: u1
  - id: byte_06
    type: u1
  - id: byte_07
    type: u1
  - id: byte_08
    type: u1
  - id: byte_09
    type: u1
  - id: byte_0a
    type: u1
  - id: byte_0b
    type: u1
  - id: word_0c
    type: u2
  - id: byte_0e
    type: u1
instances:
  material:
    value: byte_04 & 0x0f
    enum: material_kind
enums:
  material_kind:
    0: material_wood
    1: material_bone
    2: material_stone
    3: material_obsidian
    4: material_metal
    5: material_leather
  byte_00_local_masks:
    0x01: local_heavy_gate
    0x13: local_description_mask
  byte_04_local_masks:
    0x40: local_material_gate
  byte_0b_local_masks:
    0x40: local_hand_gate
    0x80: local_armor_gate
