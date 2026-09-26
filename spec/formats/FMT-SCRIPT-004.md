---
id: FMT-SCRIPT-004
title: Script trigger record
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: []
byte_order: little
size: 13
text: false
definition: fmt_script_004.ksy
evidence: [FND-SCRIPT-013, FND-SCRIPT-014, FND-SCRIPT-015, FND-SCRIPT-017]
conflicting: []
split_with: []
related: [RULE-SCRIPT-008]
---

## Layout

One of the 200 records of the table at the far pointer `57E0:40C0` in BLD-GOG-EN-1.1, kept only
in memory [FND-SCRIPT-014]. Each is on the free list or on one of the trigger lists, and names a
script entry point that the game runs when the record's test passes [FND-SCRIPT-013,
FND-SCRIPT-015]. The tests read the key fields in different ways for different lists
[FND-SCRIPT-014].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `entry_offset` | Offset of the entry point in the script, or its `GPLI` entry number while overlay 187 has converted it. | supported | FND-SCRIPT-013, FND-SCRIPT-015, FND-SCRIPT-017 |
| `0x02` | 2 | `UINT16LE` | `entry_script` | Number of the `GPL ` resource that holds the entry point. | supported | FND-SCRIPT-013, FND-SCRIPT-015, FND-SCRIPT-017 |
| `0x04` | 2 | `INT16LE` | `key_1` | The first key: the one key of an attack trigger, the first of a move-tile trigger. Purpose unknown. | supported | FND-SCRIPT-014, FND-SCRIPT-015 |
| `0x06` | 2 | `INT16LE` | `key_2` | The second key of a move-tile trigger. In an attack trigger only the low byte is written: 1 when `MAS ` resource 99 registered it. Purpose unknown. | supported | FND-SCRIPT-014, FND-SCRIPT-015 |
| `0x08` | 1 | `UINT8` | `unk_08` | Purpose unknown. The fifth parameter of a move-tile trigger; one walker uses it as the extent of `key_1`'s range and another as a threshold. | supported | FND-SCRIPT-014, FND-SCRIPT-015 |
| `0x09` | 1 | `UINT8` | `unk_09` | Purpose unknown. One walker uses it as the extent of `key_2`'s range. | supported | FND-SCRIPT-014 |
| `0x0A` | 1 | `UINT8` | `unk_0A` | Purpose unknown. One walker uses it as a threshold. | supported | FND-SCRIPT-014 |
| `0x0B` | 2 | `INT16LE` | `next` | Index of the next record on the same list, or -1 at its end. | supported | FND-SCRIPT-014, FND-SCRIPT-015 |
| `0x0D` | | | | Total size 13 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Read from the code that sets up, walks, fills and converts the records [FND-SCRIPT-014,
FND-SCRIPT-015, FND-SCRIPT-017]; no record was observed in memory.

## Open questions

- What the keys and `unk_08` to `unk_0A` hold for each list, and which lists the other trigger
  instructions fill (Q-SCRIPT-002).
- Where the table is allocated and whether it is saved (Q-SCRIPT-001).
