meta:
  id: fmt_sound_004
  title: Disc Copy driver envelope
  license: MIT
  endian: le
doc: |
  FMT-SOUND-004; FND-SOUND-018. Function directory and opaque driver bytes.
  A terminal number has no target word. No driver code is executed or decoded.
seq:
  - id: directory_offset
    type: u2
    valid:
      min: 6
      max: _io.size - 2
  - id: signature
    contents: [0x43, 0x6f, 0x70, 0x79]
  - id: opaque_header
    size: directory_offset - 6
  - id: directory
    type: entry
    repeat: until
    repeat-until: _.number == 0xffff
  - id: opaque_body
    size-eos: true
types:
  entry:
    seq:
      - id: number
        type: u2
      - id: target
        type: u2
        if: number != 0xffff
        valid:
          max: _root._io.size - 1
