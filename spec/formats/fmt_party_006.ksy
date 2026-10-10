meta:
  id: fmt_party_006
  title: Character record chunk
  license: MIT
  endian: le
doc: One chunk of a CHAR resource, a 10-byte header and the data its length gives.
doc-ref: FMT-PARTY-006, FND-PARTY-051, FND-PARTY-052
seq:
  - id: chunk_type
    type: u1
    enum: chunk_types
    doc: How the load handles the chunk; 0xFF ends the record.
  - id: target_chunk
    type: u1
    doc: For types 2 to 4, the earlier chunk whose object this chunk attaches to.
  - id: kind
    type: u1
    doc: The kind of record the data is.
  - id: unk_03
    type: u1
  - id: record_ref
    type: u2
    doc: Replaced at load by the number of the record the data was copied into.
  - id: field
    type: u2
    doc: For types 2 and 3, the field of the target's object that receives the record.
  - id: len_data
    type: u2
    doc: Number of data bytes after the header.
  - id: data
    size: len_data
enums:
  chunk_types:
    1: combatant
    2: attached_record
    3: details
    4: chained_record
    255: end
