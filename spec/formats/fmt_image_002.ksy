meta:
  id: fmt_image_002
  title: Image frame
  license: MIT
  endian: le
doc: |
  One frame of an image resource. body starts with 0xFF and PLAN or PLNR for
  a planar frame, and is a list of rows ended by 0xFF otherwise.
doc-ref: FMT-IMAGE-002, FND-IMAGE-001, FND-IMAGE-002, FND-IMAGE-003
seq:
  - id: width
    type: u2
  - id: height
    type: u2
  - id: body
    size-eos: true
    process: rule_image_001(width, height)
    doc: The encoded pixels, decoded by RULE-IMAGE-001 and RULE-IMAGE-002.
