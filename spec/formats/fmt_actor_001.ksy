meta:
  id: fmt_actor_001
  title: Object definition
  license: MIT
  endian: le
doc: The OJFF definition of an object that a region can place.
doc-ref: FMT-ACTOR-001, FND-ACTOR-001, FND-ACTOR-003
seq:
  - id: unk_00
    type: u1
  - id: unk_01
    type: u1
  - id: x_offset
    type: s2
    doc: Pixels from the image's left edge to the object's position.
  - id: y_offset
    type: s2
    doc: Pixels from the image's top edge to the object's position, before vertical_offset.
  - id: unk_06
    type: u2
  - id: unk_08
    type: u2
  - id: vertical_offset
    type: s1
    doc: Further pixels the image is drawn above the position.
  - id: unk_0b
    type: u1
  - id: image
    type: u2
    doc: Number of a BMP resource of OBJEX.GFF.
  - id: unk_0e
    type: u2
