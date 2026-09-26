meta:
  id: fmt_region_005
  title: Region entity table
  license: MIT
  endian: le
  imports:
    - fmt_region_006
doc: The ETAB resource of a region file, a list of 8-byte records.
doc-ref: FMT-REGION-005, FND-REGION-004
seq:
  - id: entities
    type: fmt_region_006
    repeat: eos
