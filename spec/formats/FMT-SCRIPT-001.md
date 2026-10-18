---
id: FMT-SCRIPT-001
title: Script resource (GPL and MAS)
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["GPLDATA.GFF"]
byte_order: big
size: null
text: false
definition: fmt_script_001.ksy
evidence: [FND-SCRIPT-001, FND-SCRIPT-005, FND-SCRIPT-019, FND-SCRIPT-009, FND-SCRIPT-024]
conflicting: []
split_with: []
related: [RULE-SCRIPT-010, RULE-SCRIPT-002, RULE-SCRIPT-004]
---

## Layout

A `GPL ` or `MAS ` resource of `GPLDATA.GFF` (FMT-GFF-001): byte code for the game's script
interpreter, with no header [FND-SCRIPT-001]. The interpreter runs a script from an offset its
caller gives. An instruction is an opcode byte from `0x00` to `0x80` followed by its parameters,
each an expression (RULE-SCRIPT-004), and some instructions read further bytes of their own
[FND-SCRIPT-005, FND-SCRIPT-009]; RULE-SCRIPT-002's `read_operands` and the rules it calls give
the operands of each opcode [FND-SCRIPT-024]. Words inside the code are stored high byte first, which is why
`byte_order` is big. The loader copies the resource and puts the stop instruction `0x31` after
it [FND-SCRIPT-019].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | `resource_size` | `BYTE[resource_size]` | `code` | Instructions and the data they read. In every shipped `GPL ` resource the first byte is `0x19`, the script return instruction, and in every `GPL ` and `MAS ` resource the last byte is `0x31`, the stop instruction. | supported | FND-SCRIPT-001, FND-SCRIPT-005, FND-SCRIPT-009 |
| | | | | Total size `resource_size`, the size of the GFF resource | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The 330 `GPL ` and 20 `MAS ` resources of the installed `GPLDATA.GFF`, checked for their first
and last bytes [FND-SCRIPT-001], and decoded instruction by instruction from offset 0: 349
decode to their last byte, and `GPL/37` holds the expression byte `0xB3`, which the reader
rejects, at offset `0x0D38`; every literal jump, call and menu target lands on an instruction
start [FND-SCRIPT-024].

## Open questions

None.
