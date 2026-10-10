meta:
  id: fmt_party_003
  title: Character psionic byte
  license: MIT
  endian: le
doc: The one-byte PSIN resource kept beside each CHAR resource.
doc-ref: FMT-PARTY-003, FND-PARTY-002, FND-PARTY-012, FND-PARTY-077
seq:
  - id: disciplines
    type: u1
    doc: The psionic disciplines, bit 0 psychokinesis, bit 1 psychometabolism, bit 2 telepathy.
