meta:
  id: fmt_sound_005
  title: GOG disc-audio Ogg page envelopes
  license: MIT
  endian: le
doc: |
  FMT-SOUND-005; FND-SOUND-017. Container envelopes only. Packet assembly,
  CRC, stream identity/sequence and Vorbis identification are checked by
  tools/research/ogg-tracks.mjs. No codec payload or audio decoder is modeled.
seq:
  - id: pages
    type: page
    repeat: eos
types:
  page:
    seq:
      - id: capture
        contents: [0x4f, 0x67, 0x67, 0x53]
      - id: version
        type: u1
        valid: 0
      - id: flags
        type: u1
        valid:
          max: 7
      - id: granule
        type: u8
      - id: serial
        type: u4
      - id: sequence
        type: u4
      - id: checksum
        type: u4
      - id: segment_count
        type: u1
      - id: lacing
        type: u1
        repeat: expr
        repeat-expr: segment_count
      - id: packet_segments
        size: lacing.sum
