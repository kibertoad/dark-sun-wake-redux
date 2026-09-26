meta:
  id: fmt_exe_001
  title: FBOV overlay pack at the end of DSUN.EXE
  license: MIT
  endian: le
doc: |
  Parse from the end of the MZ image, the file offset the MZ header's page
  counts give, to the end of the file.
doc-ref: FMT-EXE-001, FND-EXE-001, FND-EXE-002, FND-EXE-003
seq:
  - id: magic
    contents: 'FBOV'
  - id: payload_size
    type: u4
  - id: segment_table_offset
    type: u4
    doc: |
      File offset, in the resident load image, of segment_count records of
      fmt_exe_002.
  - id: segment_count
    type: u4
  - id: payload
    size: payload_size
    doc: |
      One fmt_exe_005 block per overlay, at the payload_offset its fmt_exe_003
      header gives, each padded with zeros to a multiple of 16 bytes.
