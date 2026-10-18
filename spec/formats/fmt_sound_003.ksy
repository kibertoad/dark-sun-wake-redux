meta:
  id: fmt_sound_003
  title: STDPATCH.AD files
  license: MIT
  endian: le
doc: The instrument bank the sound library reads by bank and patch number.
doc-ref: FMT-SOUND-003, FND-SOUND-016
seq:
  - id: entries
    type: timbre_entry
    repeat: until
    repeat-until: _.bank == 255
types:
  timbre_entry:
    seq:
      - id: patch
        type: u1
      - id: bank
        type: u1
        doc: 255 ends the directory.
      - id: offset
        type: u4
        if: bank != 255
    instances:
      timbre:
        pos: offset
        type: timbre
        if: bank != 255
  timbre:
    seq:
      - id: length
        type: u2
        doc: Size of the record, this word included.
      - id: data
        size: length - 2
