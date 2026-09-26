meta:
  id: fmt_party_005
  title: Character SPST record
  license: MIT
  endian: le
doc: The SPST resource kept beside each CHAR resource, 15 bytes as the game writes it and 9 as the transfer utility writes it.
doc-ref: FMT-PARTY-005, FND-PARTY-006, FND-PARTY-011, FND-PARTY-012
seq:
  - id: unk_00
    size: 9
  - id: unk_09
    size: 6
    if: not _io.eof
