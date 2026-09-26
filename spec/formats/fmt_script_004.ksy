meta:
  id: fmt_script_004
  title: Script trigger record
  license: MIT
  endian: le
doc: One of the 200 script trigger records the game keeps in memory.
doc-ref: FMT-SCRIPT-004, FND-SCRIPT-014, FND-SCRIPT-015, FND-SCRIPT-017
seq:
  - id: entry_offset
    type: u2
    doc: Offset of the entry point in the script, or its GPLI entry number.
  - id: entry_script
    type: u2
    doc: Number of the GPL resource that holds the entry point.
  - id: key_1
    type: s2
  - id: key_2
    type: s2
  - id: unk_08
    type: u1
  - id: unk_09
    type: u1
  - id: unk_0a
    type: u1
  - id: next
    type: s2
    doc: Index of the next record on the list, or -1.
