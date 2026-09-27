meta:
  id: fmt_config_003
  title: Saved settings in the PREF resource
  license: MIT
  endian: le
doc: The 9-byte PREF resource 100 written with every saved game.
doc-ref: FMT-CONFIG-003, FND-CONFIG-001, FND-CONFIG-009, FND-CONFIG-010, FND-CONFIG-012, FND-CONFIG-013, FND-CONFIG-014, FND-SAVE-004, FND-SAVE-005
seq:
  - id: difficulty_index
    type: u2
  - id: music_level_request
    type: u1
  - id: sound_effects_volume
    type: u1
  - id: music_bar_denominator
    type: u1
  - id: sound_effects_enabled
    type: u1
  - id: music_enabled
    type: u1
  - id: animation_control_state
    type: u1
  - id: speech_gate
    type: u1
