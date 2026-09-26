meta:
  id: fmt_sound_001
  title: Voice file with one block of 8-bit samples
  license: MIT
  endian: le
doc: |
  A .VOC file or BVOC resource of the game: a Creative Voice File
  header, one sound-data block and a terminator.
doc-ref: FMT-SOUND-001, FND-SOUND-001
seq:
  - id: signature
    contents: "Creative Voice File"
  - id: eof_mark
    contents: [0x1a]
  - id: header_size
    type: u2
  - id: version
    type: u2
  - id: version_check
    type: u2
  - id: block_type
    type: u1
  - id: block_length_lo
    type: u2
  - id: block_length_hi
    type: u1
  - id: time_constant
    type: u1
    doc: Sample rate as 256 - 1000000 / rate.
  - id: codec
    type: u1
  - id: samples
    size: block_length - 2
    doc: Unsigned 8-bit samples, one channel.
  - id: terminator
    type: u1
instances:
  block_length:
    value: block_length_lo + (block_length_hi << 16)
