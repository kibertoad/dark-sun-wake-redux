---
id: FND-SCRIPT-001
title: GPLDATA.GFF holds 330 GPL and 20 MAS resources; every GPL one starts with 0x19 and every one of both ends with 0x31
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x7C345..0x7D361
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x1FEAA5..0x1FEC40
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x14D695..0x14DB75
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

Read through the directory of the installed `GPLDATA.GFF` (FMT-GFF-001), with the indexed tables
resolved through `GFFI`:

| Tag | Resources | Numbers | Sizes | Total bytes |
|---|---|---|---|---|
| `GPL ` | 330 | 1 to 331, without 22 | 111 to 9,985 | 1,462,845 |
| `MAS ` | 20 | 1, 50 to 52, 54 to 63, 65 to 69, 99 | 202 to 1,910 | 18,750 |

The first byte of every `GPL ` resource is `0x19`, and the last byte of every `GPL ` and every
`MAS ` resource is `0x31`. The first bytes of the `MAS ` resources are `0x22` in 16 of them,
`0x6E` in 2, `0x6A` in 1 and `0x0A` in 1.

Examples: `GPLDATA.GFF#GPL/135` is 4,124 bytes at `0x7C345`, `GPLDATA.GFF#MAS/99` is 411 bytes at
`0x1FEAA5`, and `GPLDATA.GFF#MAS/1` is 1,248 bytes at `0x14D695`.

## Interpretation

`GPL ` and `MAS ` resources are scripts in the byte code FND-SCRIPT-005 dispatches. `0x19` is the
instruction that returns from a script and `0x31` the one that stops the interpreter
(FND-SCRIPT-009), so a script entered at offset 0 returns at once, and a script whose last
instruction falls through stops. Script entry points are therefore at offsets above 0 in `GPL `
resources. `MAS ` resources are run from offset 0 (FND-SCRIPT-013), which fits their first bytes
being ordinary instructions.

## Alternatives

That the `MAS ` numbers match region numbers was not checked.

## How to reproduce

Read the directory of `GPLDATA.GFF`, list the `GPL ` and `MAS ` resources with their offsets and
sizes, and count their first and last bytes.
