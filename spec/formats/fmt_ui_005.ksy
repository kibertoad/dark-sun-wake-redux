meta:
  id: fmt_ui_005
  title: Edit box resource (EBOX)
  license: MIT
  endian: le
doc: An EBOX resource of RESOURCE.GFF, a box a window places that shows text.
doc-ref: FMT-UI-005, FND-UI-005, FND-UI-006
seq:
  - id: tag
    type: str
    size: 4
    encoding: ASCII
    doc: EBOX.
  - id: size
    type: u4
  - id: number
    type: u4
  - id: unk_0c
    type: u4
  - id: unk_10
    type: u2
  - id: unk_12
    type: u2
  - id: unk_14
    type: u2
  - id: unk_16
    type: u2
  - id: number_copy
    type: u4
  - id: unk_1c
    size: 6
  - id: width
    type: u2
  - id: height
    type: u2
  - id: unk_26
    size: 20
  - id: image
    type: u4
    doc: Number of a BMP resource of RESOURCE.GFF drawn under the text, or 0.
  - id: unk_3e
    size: 26
  - id: unk_58
    type: u2
  - id: unk_5a
    type: u4
  - id: unk_5e
    size: 16
  - id: unk_6e
    type: u1
  - id: unk_6f
    size: 3
  - id: unk_72
    type: u2
  - id: unk_74
    size: 4
  - id: unk_78
    type: u1
  - id: unk_79
    size: 5
  - id: unk_7e
    type: u2
  - id: unk_80
    type: u2
  - id: unk_82
    size: 20
  - id: event_mask
    type: u2
    doc: Event bits; the box is found under the pointer only with the value 2 set (RULE-UI-001).
  - id: unk_98
    size: 16
