meta:
  id: fmt_party_005
  title: Character SPST record
  license: MIT
  endian: le
doc: The SPST resource kept beside each CHAR resource, 15 bytes as the game writes it and 9 as the transfer utility writes it.
doc-ref: FMT-PARTY-005, FND-PARTY-006, FND-PARTY-011, FND-PARTY-012, FND-PARTY-104
seq:
  - id: known_spells_00
    size: 9
    doc: Bit n & 7 of byte n >> 3 is set when the character knows spell n.
  - id: known_spells_09
    size: 6
    if: not _io.eof
