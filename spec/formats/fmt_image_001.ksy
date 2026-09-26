meta:
  id: fmt_image_001
  title: Image resource with a list of frames
  license: MIT
  endian: le
  imports:
    - fmt_image_002
doc: |
  A BMP, CBMP, ICON, PORT or TILE resource of a .GFF file. Frame i runs
  from frame_offsets[i] to the next offset, or to size for the last frame.
doc-ref: FMT-IMAGE-001, FND-IMAGE-001
seq:
  - id: size
    type: u4
    doc: Size of the resource in bytes.
  - id: frame_count
    type: u2
  - id: frame_offsets
    type: u4
    repeat: expr
    repeat-expr: frame_count
    doc: Offset of each frame from the start of the resource, rising.
  - id: frames
    size: size - 6 - frame_count * 4
    doc: The frames, in order, with no bytes between them.
instances:
  frame_slots:
    type: frame_slot(_index)
    repeat: expr
    repeat-expr: frame_count
types:
  frame_slot:
    params:
      - id: i
        type: u2
    instances:
      frame:
        io: _parent._io
        pos: _parent.frame_offsets[i]
        size: 'i + 1 < _parent.frame_count ? _parent.frame_offsets[i + 1] - _parent.frame_offsets[i] : _parent.size - _parent.frame_offsets[i]'
        type: fmt_image_002
