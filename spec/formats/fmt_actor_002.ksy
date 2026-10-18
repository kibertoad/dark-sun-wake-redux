meta:
  id: fmt_actor_002
  title: Object data resource
  license: MIT
  endian: le
  imports:
    - fmt_party_006
doc: |
  An RDFF resource of OBJEX.GFF: a chain of chunks (fmt_party_006) ending at one whose type
  is 0xFF, which the object load walks as it walks a CHAR resource. The bytes after the end
  header are not read; one shipped resource has any.
doc-ref: FMT-ACTOR-002, FND-ACTOR-015, FND-PARTY-051
seq:
  - id: chunks
    type: fmt_party_006
    repeat: until
    repeat-until: _.chunk_type == fmt_party_006::chunk_types::end
  - id: unread
    size-eos: true
    doc: Bytes after the end header, which the load does not reach.
