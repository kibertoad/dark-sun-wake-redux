meta:
  id: fmt_region_003
  title: Region cell flag map
  license: MIT
  endian: le
  imports:
    - fmt_region_004
doc: The GMAP resource of a region file, one byte per map cell, row by row.
doc-ref: FMT-REGION-003, FND-REGION-003, FND-REGION-005
seq:
  - id: cells
    type: fmt_region_004
    repeat: expr
    repeat-expr: 12544
