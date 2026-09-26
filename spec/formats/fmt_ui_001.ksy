meta:
  id: fmt_ui_001
  title: Window resource (WIND)
  license: MIT
  endian: le
  imports:
    - fmt_ui_002
doc: A WIND resource of RESOURCE.GFF, a window and the controls it places.
doc-ref: FMT-UI-001, FND-UI-001, FND-UI-002, FND-UI-003, FND-UI-011
seq:
  - id: tag
    type: str
    size: 4
    encoding: ASCII
    doc: WIND.
  - id: size
    type: u4
    doc: The resource's size in bytes.
  - id: number
    type: u4
    doc: The resource's number.
  - id: unk_0c
    size: 138
    doc: Bytes copied from an EBOX or a BUTN record.
  - id: unk_96
    type: s2
    doc: In the loaded record, the window's left edge on the screen.
  - id: unk_98
    type: s2
    doc: In the loaded record, the window's top edge on the screen.
  - id: unk_9a
    size: 4
  - id: flags
    type: u2
    doc: Window flags; 4 skips the window, 0x100 ends the pointer search after it.
  - id: unk_a0
    size: 8
  - id: unk_a8
    size: 22
  - id: width
    type: u2
  - id: height
    type: u2
  - id: image
    type: u2
    doc: Number of a BMP resource of RESOURCE.GFF, or 0.
  - id: unk_c4
    size: 42
  - id: next_window
    type: u4
    doc: FARPTR to the next registered window in the loaded record; 0 in the file.
  - id: unk_f2
    type: u1
  - id: child_count
    type: u2
  - id: unk_f5
    size: 4
  - id: after_children
    type: u4
    doc: FARPTR to a handler in the loaded record; 0 in the file.
  - id: before_children
    type: u4
    doc: FARPTR to a handler in the loaded record; 0 in the file.
  - id: unk_101
    size: 4
  - id: children
    type: fmt_ui_002
    repeat: expr
    repeat-expr: child_count
