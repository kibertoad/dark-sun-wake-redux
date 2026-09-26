meta:
  id: fmt_region_002
  title: Region terrain map
  license: MIT
  endian: le
doc: |
  The MAP resource of a region file: 98 rows of 128 cells, row by row. Each
  byte is the number of a TILE resource of the same file.
doc-ref: FMT-REGION-002, FND-REGION-002, FND-REGION-006
seq:
  - id: cells
    type: u1
    repeat: expr
    repeat-expr: 12544
