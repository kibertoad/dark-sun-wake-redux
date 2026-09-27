meta:
  id: fmt_config_003
  title: Saved settings in the PREF resource
  license: MIT
  endian: le
doc: The 9-byte PREF resource 100 written with every saved game.
doc-ref: FMT-CONFIG-003, FND-CONFIG-001, FND-CONFIG-009, FND-SAVE-004, FND-SAVE-005
seq:
  - id: difficulty_index
    type: u2
  - id: unk_02
    type: u1
  - id: unk_03
    type: u1
  - id: unk_04
    type: u1
  - id: unk_05
    type: u1
  - id: unk_06
    type: u1
  - id: animation_control_state
    type: u1
  - id: unk_08
    type: u1
