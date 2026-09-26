meta:
  id: fmt_gff_007
  title: GFFI index resource
  license: MIT
  endian: le
  imports:
    - fmt_gff_005
doc-ref: FMT-GFF-007, FND-GFF-003
seq:
  - id: entry_count
    type: u4
    doc: Equal to the indexed table's entry_count.
  - id: entries
    type: fmt_gff_005
    repeat: expr
    repeat-expr: entry_count
    doc: In the order of the table's expanded number_ranges.
