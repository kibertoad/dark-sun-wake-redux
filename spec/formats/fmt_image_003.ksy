meta:
  id: fmt_image_003
  title: Palette resource
  license: MIT
  endian: le
  imports:
    - fmt_image_004
doc: A PAL resource of 256 colours in the VGA DAC's form.
doc-ref: FMT-IMAGE-003, FND-IMAGE-004
seq:
  - id: colors
    type: fmt_image_004
    repeat: expr
    repeat-expr: 256
