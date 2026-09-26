meta:
  id: fmt_gff_004
  title: GFF resource entry in a plain tag table
  license: MIT
  endian: le
doc-ref: FMT-GFF-004, FND-GFF-002, FND-GFF-005
seq:
  - id: number
    type: u4
  - id: data_offset
    type: u4
    doc: File offset of the resource's bytes.
  - id: data_size
    type: u4
