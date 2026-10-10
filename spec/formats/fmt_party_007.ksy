meta:
  id: fmt_party_007
  title: Class combination table (DATA 1001)
  license: MIT
  endian: le
doc: The DATA 1001 resource, a mask of classes per origin, first class and second class.
doc-ref: FMT-PARTY-007, FND-PARTY-058, FND-PARTY-068
seq:
  - id: origins
    type: origin_block
    repeat: expr
    repeat-expr: 8
types:
  origin_block:
    seq:
      - id: first_classes
        type: class_row
        repeat: expr
        repeat-expr: 8
  class_row:
    seq:
      - id: masks
        type: u1
        repeat: expr
        repeat-expr: 9
        doc: Index 0, the classes that may be second; index k, those that may be third after second class k. Bit 0x80 Cleric to 0x01 Thief.
