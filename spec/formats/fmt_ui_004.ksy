meta:
  id: fmt_ui_004
  title: Application frame resource (APFM)
  license: MIT
  endian: le
doc: An APFM resource of RESOURCE.GFF, a rectangle a window places with no image of its own.
doc-ref: FMT-UI-004, FND-UI-005, FND-UI-006, FND-UI-007
seq:
  - id: tag
    type: str
    size: 4
    encoding: ASCII
    doc: APFM.
  - id: size
    type: u4
  - id: number
    type: u4
  - id: unk_0c
    size: 28
  - id: width
    type: u2
  - id: height
    type: u2
  - id: unk_2c
    size: 44
  - id: event_mask
    type: u2
    doc: Event bits matched against an event (RULE-UI-001).
  - id: unk_5a
    size: 26
