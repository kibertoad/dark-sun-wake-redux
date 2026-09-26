meta:
  id: fmt_text_001
  title: Bitmap font resource
  license: MIT
  endian: le
  imports:
    - fmt_text_002
doc: |
  The FONT resource. Glyph i is the record at glyph_offsets[i], counted from
  the start of the resource.
doc-ref: FMT-TEXT-001, FND-TEXT-001
seq:
  - id: glyph_count
    type: u2
  - id: height
    type: u2
  - id: unk_04
    size: 4
    doc: Purpose unknown. All 0.
  - id: char_map
    size: 256
    doc: The glyph for each character code; the identity in the shipped font.
  - id: glyph_offsets
    type: u2
    repeat: expr
    repeat-expr: glyph_count
instances:
  glyph_slots:
    type: glyph_slot(_index)
    repeat: expr
    repeat-expr: glyph_count
types:
  glyph_slot:
    params:
      - id: i
        type: u2
    instances:
      glyph:
        io: _parent._io
        pos: _parent.glyph_offsets[i]
        type: fmt_text_002(_parent.height)
