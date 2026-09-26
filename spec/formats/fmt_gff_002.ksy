meta:
  id: fmt_gff_002
  title: GFF directory
  license: MIT
  endian: le
  imports:
    - fmt_gff_003
    - fmt_gff_005
doc-ref: FMT-GFF-002, FND-GFF-002, FND-GFF-003, FND-GFF-004
seq:
  - id: tag_list_offset
    type: u4
    doc: 8, the offset of tag_count from the directory's start.
  - id: tag_list_end
    type: u4
    doc: Offset from the directory's start of gap_count.
  - id: tag_count
    type: u2
  - id: tag_tables
    type: fmt_gff_003
    repeat: expr
    repeat-expr: tag_count
  - id: gap_count
    type: u2
  - id: gaps
    type: fmt_gff_005
    repeat: expr
    repeat-expr: gap_count
    doc: The ranges of the file's data that no resource uses.
