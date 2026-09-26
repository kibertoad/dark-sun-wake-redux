meta:
  id: fmt_region_004
  title: Region cell flags
  license: MIT
  endian: le
  bit-endian: le
doc: One byte of flags for a map cell.
doc-ref: FMT-REGION-004, FND-REGION-003, FND-REGION-005, FND-EXPLORE-001
seq:
  - id: unk_bits_0_4
    type: b5
    doc: Purpose unknown. 0 in every shipped cell.
  - id: occupied
    type: b1
    doc: Set with blocked by an object that occupies the cell. Cleared by the region loader.
  - id: blocked
    type: b1
    doc: The cell blocks movement.
  - id: unk_bit_7
    type: b1
