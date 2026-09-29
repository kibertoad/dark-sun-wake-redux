meta:
  id: fmt_script_001
  title: Script resource (GPL and MAS)
  license: MIT
  endian: be
doc: |
  A GPL or MAS resource of GPLDATA.GFF: script byte code with no header. Words
  inside the code are stored high byte first.
doc-ref: FMT-SCRIPT-001, FND-SCRIPT-001, FND-SCRIPT-005, FND-SCRIPT-019
seq:
  - id: code
    size-eos: true
    doc: |
      Instructions and the data they read. Every shipped GPL resource starts
      with 0x19, and every GPL and MAS resource ends with 0x31.
