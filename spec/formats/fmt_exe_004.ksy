meta:
  id: fmt_exe_004
  title: FBOV overlay trampoline
  license: MIT
  endian: le
doc-ref: FMT-EXE-004, FND-EXE-004
seq:
  - id: trap
    contents: [0xcd, 0x3f]
    doc: The instruction INT 3Fh.
  - id: target
    type: u2
    doc: An offset in the overlay's code, less than its code_size.
  - id: unk_04
    type: u1
    doc: 0 in every shipped trampoline. Purpose unknown.
