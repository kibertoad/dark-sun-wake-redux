meta:
  id: fmt_combat_003
  title: Effect name record in DSUN.EXE
  license: MIT
  endian: le
doc: One of the 113 effect name records of the executable's resident data.
doc-ref: FMT-COMBAT-003, FND-COMBAT-027, FND-COMBAT-029
seq:
  - id: name
    type: strz
    encoding: ASCII
    size: 29
    doc: The effect's name, padded with NULs.
  - id: icon_resource_id
    type: u2
    doc: ICON resource number; zero omits the effect from the reviewed icon list.
