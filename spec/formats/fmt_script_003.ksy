meta:
  id: fmt_script_003
  title: Script entry point index (GPLI)
  license: MIT
  endian: le
doc: One record of GPLDATA.GFF#GPLI/1, which is a list of them with no header.
doc-ref: FMT-SCRIPT-003, FND-SCRIPT-002, FND-SCRIPT-017
seq:
  - id: entry
    type: u2
    doc: The entry number.
  - id: offset
    type: u2
    doc: Offset of the entry point in the script.
  - id: script
    type: u2
    doc: Number of the GPL resource that holds the entry point.
