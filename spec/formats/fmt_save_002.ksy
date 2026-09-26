meta:
  id: fmt_save_002
  title: Saved game state in a GREQ resource
  license: MIT
  endian: le
doc: The 9-byte GREQ resource written for each saved game.
doc-ref: FMT-SAVE-002, FND-SAVE-001, FND-SAVE-004, FND-SAVE-005
seq:
  - id: unk_00
    type: u2
  - id: unk_02
    type: u2
  - id: unk_04
    type: u2
  - id: unk_06
    type: u2
  - id: unk_08
    type: u1
