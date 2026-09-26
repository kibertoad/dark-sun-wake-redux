meta:
  id: fmt_gff_001
  title: GFF resource container
  license: MIT
  endian: le
  imports:
    - fmt_gff_002
doc: |
  A whole .GFF file. Resources are referred to as FILE.GFF#TAG/number, with
  the tag's trailing spaces removed.
doc-ref: FMT-GFF-001, FND-GFF-001, FND-GFF-002, FND-GFF-004
seq:
  - id: signature
    contents: 'GFFI'
  - id: version
    type: u4
    doc: 0x00030000 in every shipped file.
  - id: header_size
    type: u4
    doc: 28, the offset of data.
  - id: directory_offset
    type: u4
  - id: directory_size
    type: u4
    doc: Bytes in directory, up to the end of its gap list.
  - id: unk_14
    type: u4
    doc: Purpose unknown.
  - id: unk_18
    type: u4
    doc: Purpose unknown.
  - id: data
    size: directory_offset - 28
    doc: The resources' bytes and the unused ranges the gap list gives.
  - id: directory
    size: directory_size
    type: fmt_gff_002
  - id: trailing
    size-eos: true
    doc: Bytes no resource and no gap covers.
