meta:
  id: fmt_sound_002
  title: Music table DJ.DAT
  license: MIT
  endian: le
doc: The music table the game's music selector reads at startup.
doc-ref: FMT-SOUND-002, FND-SOUND-011, FND-SOUND-012
seq:
  - id: count
    type: u1
  - id: retry_passes
    type: u2
  - id: records
    type: music_record
    repeat: expr
    repeat-expr: count
types:
  music_record:
    seq:
      - id: region
        type: s1
        doc: Region number, or -1 for any region.
      - id: chance
        type: u1
      - id: health_band
        type: u1
      - id: mode
        type: u2
      - id: song
        type: u1
        doc: Song number; disc track song + 1.
