meta:
  id: fmt_region_004
  title: Region cell flags
  license: MIT
  endian: le
  bit-endian: le
doc: One byte of flags for a map cell.
doc-ref: FMT-REGION-004, FND-REGION-003, FND-REGION-005
seq:
  - id: unk_bits_0_4
    type: b5
    doc: Purpose unknown. 0 in every shipped cell.
  - id: unk_bit_5
    type: b1
    doc: Purpose unknown. Cleared by the region loader.
  - id: unk_bit_6
    type: b1
  - id: unk_bit_7
    type: b1
