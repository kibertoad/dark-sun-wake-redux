meta:
  id: fmt_exe_005
  title: FBOV overlay code block with its fixup list
  license: MIT
  endian: le
doc-ref: FMT-EXE-005, FND-EXE-003, FND-EXE-005
params:
  - id: code_size
    type: u2
    doc: code_size from the overlay's fmt_exe_003 header.
  - id: fixup_size
    type: u2
    doc: fixup_size from the overlay's fmt_exe_003 header.
seq:
  - id: code
    size: code_size
  - id: fixups
    type: u2
    repeat: expr
    repeat-expr: fixup_size / 2
    doc: |
      Offsets in code of 16-bit words that each hold a segment-table index
      shifted left by three.
