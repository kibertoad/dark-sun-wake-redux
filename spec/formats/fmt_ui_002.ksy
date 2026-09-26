meta:
  id: fmt_ui_002
  title: Child record of a window
  license: MIT
  endian: le
doc: One control a WIND places.
doc-ref: FMT-UI-002, FND-UI-002, FND-UI-011
seq:
  - id: control
    type: u4
    doc: FARPTR to the control's record in the loaded window; 0 in the file.
  - id: tag
    type: str
    size: 4
    encoding: ASCII
    doc: BUTN, APFM or EBOX.
  - id: number
    type: u4
    doc: The control's resource number.
  - id: x
    type: s2
    doc: Pixels from the window's left edge.
  - id: y
    type: s2
    doc: Pixels from the window's top edge.
  - id: unk_10
    size: 12
  - id: flags
    type: u2
    doc: 0x8000 makes the pointer search pass over the child.
