meta:
  id: fmt_exe_003
  title: FBOV overlay header in the resident image
  license: MIT
  endian: le
  imports:
    - fmt_exe_004
doc-ref: FMT-EXE-003, FND-EXE-003, FND-EXE-004
seq:
  - id: trap
    contents: [0xcd, 0x3f]
    doc: The instruction INT 3Fh.
  - id: unk_02
    type: u2
    doc: 0 in every shipped header. Purpose unknown.
  - id: payload_offset
    type: u4
    doc: Offset of the fmt_exe_005 block from the start of the pack's payload.
  - id: code_size
    type: u2
  - id: fixup_size
    type: u2
  - id: trampoline_count
    type: u2
  - id: unk_0e
    size: 18
    doc: All 0 in every shipped header. Purpose unknown.
  - id: trampolines
    type: fmt_exe_004
    repeat: expr
    repeat-expr: trampoline_count
