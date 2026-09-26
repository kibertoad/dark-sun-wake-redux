meta:
  id: fmt_text_002
  title: Bitmap font glyph
  license: MIT
  endian: le
doc: One glyph of the FONT resource. height comes from the font.
doc-ref: FMT-TEXT-002, FND-TEXT-001
params:
  - id: height
    type: u2
seq:
  - id: width
    type: u2
  - id: pixels
    size: width * height
    doc: Palette indices.
