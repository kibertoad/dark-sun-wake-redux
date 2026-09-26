meta:
  id: fmt_save_001
  title: Stored character identifier in a CACT resource
  license: MIT
  endian: le
doc: The 2-byte CACT resource of the character archive.
doc-ref: FMT-SAVE-001, FND-PARTY-011, FND-PARTY-012, FND-SAVE-001
seq:
  - id: character_id
    type: u2
    doc: Identifier of the stored character, 0 for a free number.
