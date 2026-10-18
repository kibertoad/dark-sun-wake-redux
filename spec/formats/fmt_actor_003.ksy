meta:
  id: fmt_actor_003
  title: MONR resource
  license: MIT
  endian: le
doc: |
  MONR resource 1 of RESOURCE.GFF, read by script request 8 as 42-byte
  records, as many as the resource's size divided by 42.
doc-ref: FMT-ACTOR-003, FND-ACTOR-014
seq:
  - id: records
    type: record
    repeat: eos
types:
  record:
    seq:
      - id: key
        type: u2
        doc: Compared with current_region, or with 255.
      - id: pairs
        type: pair
        repeat: expr
        repeat-expr: 10
  pair:
    seq:
      - id: value
        type: s2
        doc: Stored at 549B:00C0 when the pair is picked; 0 for an empty pair.
      - id: rating
        type: s2
        doc: Compared with the party's score, or divides twice the score.
