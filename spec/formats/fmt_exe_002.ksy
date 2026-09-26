meta:
  id: fmt_exe_002
  title: FBOV segment-table descriptor
  license: MIT
  endian: le
doc-ref: FMT-EXE-002, FND-EXE-002
seq:
  - id: segment
    type: u2
    doc: |
      Paragraph relative to the start of the load image. For an overlay,
      segment + 0x1000 is the segment of its fmt_exe_003 header.
  - id: unk_02
    type: u2
    doc: For an overlay, 32 + 5 * trampoline_count. Purpose unknown otherwise.
  - id: flags
    type: u2
    enum: fbov_segment
  - id: unk_06
    type: u2
    doc: 0 for every overlay. Purpose unknown.
enums:
  fbov_segment:
    0: fbov_segment_0
    1: fbov_segment_1
    3: fbov_segment_overlay
    4: fbov_segment_4
