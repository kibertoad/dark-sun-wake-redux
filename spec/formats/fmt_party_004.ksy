meta:
  id: fmt_party_004
  title: Character PSST record
  license: MIT
  endian: le
doc: The 34-byte PSST resource kept beside each CHAR resource.
doc-ref: FMT-PARTY-004, FND-PARTY-006, FND-PARTY-012, FND-PARTY-104
seq:
  - id: power_ranks
    size: 34
    doc: One byte per psionic power; bits 1 to 7 are its rank.
