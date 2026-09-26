meta:
  id: fmt_gff_005
  title: GFF byte range
  license: MIT
  endian: le
doc-ref: FMT-GFF-005, FND-GFF-003, FND-GFF-004
seq:
  - id: data_offset
    type: u4
    doc: File offset of the range's first byte.
  - id: data_size
    type: u4
