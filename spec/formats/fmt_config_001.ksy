meta:
  id: fmt_config_001
  title: Sound configuration SOUND.CFG
  license: MIT
  endian: le
doc: The 59-byte SOUND.CFG that the sound setup program writes.
doc-ref: FMT-CONFIG-001, FND-CONFIG-003, FND-CONFIG-213
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
    type: u2
  - id: unk_0a
    type: u2
  - id: unk_0c
    type: u2
  - id: unk_0e
    type: u2
  - id: unk_10
    type: u2
  - id: unk_12
    type: u2
  - id: unk_14
    type: u2
  - id: music_driver
    type: strz
    size: 14
    encoding: ASCII
    doc: Real-mode music driver file name, padded with NULs.
  - id: digital_driver
    type: strz
    size: 14
    encoding: ASCII
    doc: Real-mode digital sound driver file name, padded with NULs.
  - id: unk_32
    type: u2
  - id: unk_34
    type: u2
  - id: unk_36
    type: u2
  - id: unk_38
    type: u2
  - id: unk_3a
    type: u1
