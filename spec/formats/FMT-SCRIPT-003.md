---
id: FMT-SCRIPT-003
title: Script entry point index (GPLI)
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["GPLDATA.GFF"]
byte_order: little
size: 6
text: false
definition: fmt_script_003.ksy
evidence: [FND-SCRIPT-002, FND-SCRIPT-017]
conflicting: []
split_with: []
related: []
---

## Layout

`GPLDATA.GFF#GPLI/1` is a list of these records with no header, as many as its size divided by
6: 1,316 in BLD-GOG-EN-1.1 [FND-SCRIPT-002]. Each gives a stable number to a place in a script.
Overlay 187 reads the resource and replaces the script entry points of the trigger records with
these numbers, and back [FND-SCRIPT-017].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `entry` | The entry number. The shipped records hold 0 to 1,315, each once, and sorted by it the `script` values never decrease. | supported | FND-SCRIPT-002, FND-SCRIPT-017 |
| `0x02` | 2 | `UINT16LE` | `offset` | Offset of the entry point in the script, below the script's size. | supported | FND-SCRIPT-002, FND-SCRIPT-017 |
| `0x04` | 2 | `UINT16LE` | `script` | Number of the `GPL ` resource that holds the entry point. The first record is `(0, 0, 0)`, the only one whose `script` is not a `GPL ` number. | supported | FND-SCRIPT-002, FND-SCRIPT-017 |
| `0x06` | | | | Total size 6 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The 1,316 records of the installed `GPLDATA.GFF#GPLI/1` [FND-SCRIPT-002]. The disc's copy was not
compared.

## Open questions

- When the game runs the two conversions of overlay 187 (Q-SCRIPT-001).
