meta:
  id: fmt_ui_003
  title: Button resource (BUTN)
  license: MIT
  endian: le
doc: A BUTN resource of RESOURCE.GFF, one button.
doc-ref: FMT-UI-003, FND-UI-004, FND-UI-006
seq:
  - id: tag
    type: str
    size: 4
    encoding: ASCII
    doc: BUTN.
  - id: size
    type: u4
  - id: number
    type: u4
  - id: unk_0c
    type: u1
  - id: unk_0d
    type: u1
  - id: unk_0e
    type: u1
  - id: unk_0f
    size: 25
  - id: width
    type: u2
  - id: height
    type: u2
  - id: unk_2c
    size: 44
  - id: event_mask
    type: u2
    doc: Event bits; the values 4 and 2 keep the pointer search from choosing the button (RULE-UI-001).
  - id: number_copy
    type: u4
  - id: unk_5e
    size: 6
  - id: icon
    type: u4
    doc: Number of an ICON resource of RESOURCE.GFF, or 0.
  - id: unk_68
    size: 5
  - id: tail_length
    type: u1
  - id: tail
    size: tail_length
