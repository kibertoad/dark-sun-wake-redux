meta:
  id: fmt_save_002
  title: Saved game state in a GREQ resource
  license: MIT
  endian: le
doc: The 9-byte GREQ resource written for each saved game.
doc-ref: FMT-SAVE-002, FND-SAVE-001, FND-SAVE-004, FND-SAVE-005, FND-SAVE-013
seq:
  - id: subregion_left
    type: u2
  - id: subregion_top
    type: u2
  - id: subregion_right
    type: u2
  - id: subregion_bottom
    type: u2
  - id: map_page
    type: u1
