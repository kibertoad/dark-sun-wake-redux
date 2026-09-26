meta:
  id: fmt_video_001
  title: FLI animation
  license: MIT
  endian: le
doc: |
  A numbered cinematic n.FLI: a 128-byte header, then frame records to
  the end of the file. The game's player shows the first frame_count
  records.
doc-ref: FMT-VIDEO-001, FND-VIDEO-001, FND-VIDEO-002
seq:
  - id: file_size
    type: u4
  - id: magic
    contents: [0x11, 0xaf]
  - id: frame_count
    type: u2
  - id: width
    type: u2
  - id: height
    type: u2
  - id: depth
    type: u2
  - id: flags
    type: u2
  - id: speed
    type: u2
    doc: Not read by the game's player.
  - id: padding
    size: 110
  - id: records
    type: frame_record
    repeat: eos
types:
  frame_record:
    seq:
      - id: record_size
        type: u4
      - id: record_type
        contents: [0xfa, 0xf1]
      - id: chunk_count
        type: u2
      - id: padding
        size: 8
      - id: body
        size: record_size - 16
        type: frame_body
  frame_body:
    seq:
      - id: chunks
        type: chunk
        repeat: expr
        repeat-expr: _parent.chunk_count
  chunk:
    seq:
      - id: chunk_size
        type: u4
      - id: chunk_type
        type: u2
        enum: chunk_type
      - id: data
        size: 'chunk_size - 6 < _io.size - _io.pos ? chunk_size - 6 : _io.size - _io.pos'
        doc: |
          The first record of 5.FLI has a last chunk whose size runs one
          byte past the record; the data stops at the record's end.
enums:
  chunk_type:
    0x0b: colour
    0x0c: line_delta
    0x0d: black
    0x0e: ignored
    0x0f: run_length
    0x10: copy
