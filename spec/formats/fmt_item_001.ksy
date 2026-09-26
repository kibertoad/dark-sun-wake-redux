meta:
  id: fmt_item_001
  title: Item translation pair in ITEMS.BIN
  license: MIT
  endian: le
doc: One 4-byte record of ITEMS.BIN, which is 234 of them with no header.
doc-ref: FMT-ITEM-001, FND-ITEM-001, FND-ITEM-006
seq:
  - id: old_item
    type: u2
    doc: The number of a Dark Sun 1 item.
  - id: new_item
    type: u2
    doc: The number of the RDFF resource of OBJEX.GFF that replaces it in Dark Sun 2.
