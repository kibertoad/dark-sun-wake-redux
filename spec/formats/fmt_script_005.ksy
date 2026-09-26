meta:
  id: fmt_script_005
  title: Record with two script entry points in the 19-byte list
  license: MIT
  endian: le
doc: One record of the 19-byte list whose head is the word at 57E0:5AF5.
doc-ref: FMT-SCRIPT-005, FND-SCRIPT-016, FND-SCRIPT-017
seq:
  - id: unk_00
    type: u2
  - id: unk_02
    size: 6
  - id: entry_offset_0
    type: u2
  - id: entry_offset_1
    type: u2
  - id: entry_script_0
    type: u2
  - id: entry_script_1
    type: u2
  - id: unk_10
    type: u1
  - id: previous
    type: s1
  - id: next
    type: s1
