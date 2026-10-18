meta:
  id: fmt_script_002
  title: String in script code
  license: MIT
  endian: be
  bit-endian: be
doc: |
  A string inside script code, after the expression byte 0x92. Kind 5 packs
  7-bit characters high bit first and ends at the character 3. Kind 2 holds
  299 bytes, each read shifted left by two bits.
doc-ref: FMT-SCRIPT-002, FND-SCRIPT-010, FND-SCRIPT-024
seq:
  - id: kind
    type: u1
    enum: string_kind
  - id: packed
    type: b7
    repeat: until
    repeat-until: _ == 3
    if: kind == string_kind::string_packed
    doc: |
      Characters of 7 bits. A value below 0x20 or above 0x7E is read as a
      space. The reader stops after 299 characters.
  - id: shifted
    size: 299
    if: kind == string_kind::string_shifted
    doc: |
      Bytes the reader keeps as (b << 2) & 0xFF; that is never 3, so all 299
      are read.
enums:
  string_kind:
    1: string_character_name
    2: string_shifted
    5: string_packed
