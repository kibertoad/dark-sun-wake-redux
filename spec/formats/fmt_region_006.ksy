meta:
  id: fmt_region_006
  title: Region entity record
  license: MIT
  endian: le
  bit-endian: le
doc: One object placed on a region.
doc-ref: FMT-REGION-006, FND-REGION-004, FND-IMAGE-010
seq:
  - id: x
    type: s2
    doc: World x in pixels.
  - id: y
    type: s2
    doc: World y in pixels.
  - id: vertical_offset
    type: s1
    doc: Pixels the object is drawn above y.
  - id: unk_flags_bits_0_2
    type: b3
  - id: unk_flags_bits_3_4
    type: b2
  - id: unk_flags_bit_5
    type: b1
  - id: unk_flags_bit_6
    type: b1
  - id: unk_flags_bit_7
    type: b1
  - id: object
    type: s2
    doc: Number of an OJFF resource in OBJEX.GFF.
