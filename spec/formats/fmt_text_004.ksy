meta:
  id: fmt_text_004
  title: Preferences text block in DSUN.EXE
  license: MIT
  endian: le
doc: |
  A block of the resident data of DSUN.EXE, at file offset 0x4F6B9 in
  BLD-GOG-EN-1.1. Far pointers are stored as offset then segment, with the
  segment before relocation.
doc-ref: FMT-TEXT-004, FND-TEXT-005
seq:
  - id: difficulty_labels
    type: far_ptr
    repeat: expr
    repeat-expr: 4
  - id: unk_010
    type: u2
    repeat: expr
    repeat-expr: 4
    doc: Purpose unknown.
  - id: about_lines
    type: far_ptr
    repeat: expr
    repeat-expr: 9
  - {id: difficulty_label_0, type: strz, size: 5, encoding: ASCII}
  - {id: difficulty_label_1, type: strz, size: 9, encoding: ASCII}
  - {id: difficulty_label_2, type: strz, size: 5, encoding: ASCII}
  - {id: difficulty_label_3, type: strz, size: 8, encoding: ASCII}
  - {id: resource_path_pattern, type: strz, size: 17, encoding: ASCII}
  - {id: about_pattern, type: strz, size: 9, encoding: ASCII}
  - {id: description_0, type: strz, size: 20, encoding: ASCII}
  - {id: description_1, type: strz, size: 14, encoding: ASCII}
  - {id: description_2, type: strz, size: 21, encoding: ASCII}
  - {id: description_3, type: strz, size: 20, encoding: ASCII}
  - {id: description_4, type: strz, size: 16, encoding: ASCII}
  - {id: description_5, type: strz, size: 18, encoding: ASCII}
  - {id: description_6, type: strz, size: 6, encoding: ASCII}
  - {id: description_7, type: strz, size: 22, encoding: ASCII}
  - {id: description_8, type: strz, size: 10, encoding: ASCII}
  - {id: description_9, type: strz, size: 15, encoding: ASCII}
  - {id: about_line_0, type: strz, size: 30, encoding: ASCII}
  - {id: about_line_1, type: strz, size: 27, encoding: ASCII}
  - {id: about_line_2, type: strz, size: 27, encoding: ASCII}
  - {id: about_line_3, type: strz, size: 34, encoding: ASCII}
  - {id: about_line_4, type: strz, size: 29, encoding: ASCII}
  - {id: about_line_5, type: strz, size: 30, encoding: ASCII}
  - {id: about_line_6, type: strz, size: 25, encoding: ASCII}
  - {id: about_line_7, type: strz, size: 24, encoding: ASCII}
  - {id: about_line_8, type: strz, size: 28, encoding: ASCII}
types:
  far_ptr:
    doc: FARPTR<char[]>, offset then segment.
    seq:
      - id: offset
        type: u2
      - id: segment
        type: u2
