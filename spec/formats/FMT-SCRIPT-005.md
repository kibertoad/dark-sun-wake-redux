---
id: FMT-SCRIPT-005
title: Record with two script entry points in the 19-byte list
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: []
byte_order: little
size: 19
text: false
definition: fmt_script_005.ksy
evidence: [FND-SCRIPT-016, FND-SCRIPT-017]
conflicting: []
split_with: []
related: []
---

## Layout

A record of the table at `4F49:08A3` in BLD-GOG-EN-1.1, kept only in memory, on a list whose head
is the word at `57E0:5AF5` [FND-SCRIPT-016, FND-SCRIPT-017]. Segment `2D40` runs the scripts its
entry points name.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `unk_00` | Purpose unknown. Replaced by a routine called from segment `28C9`. | supported | FND-SCRIPT-016 |
| `0x02` | 6 | `BYTE[6]` | `unk_02` | Purpose unknown. | supported | FND-SCRIPT-016 |
| `0x08` | 2 | `UINT16LE` | `entry_offset_0` | Offset of the first entry point, or its `GPLI` entry number while converted. | supported | FND-SCRIPT-016, FND-SCRIPT-017 |
| `0x0A` | 2 | `UINT16LE` | `entry_offset_1` | Offset of the second entry point, the same way. | supported | FND-SCRIPT-016, FND-SCRIPT-017 |
| `0x0C` | 2 | `UINT16LE` | `entry_script_0` | `GPL ` number of the first entry point. | supported | FND-SCRIPT-016, FND-SCRIPT-017 |
| `0x0E` | 2 | `UINT16LE` | `entry_script_1` | `GPL ` number of the second entry point. | supported | FND-SCRIPT-016, FND-SCRIPT-017 |
| `0x10` | 1 | `UINT8` | `unk_10` | Purpose unknown. | supported | FND-SCRIPT-016 |
| `0x11` | 1 | `INT8` | `previous` | Index of the previous record on the list, or -1. | supported | FND-SCRIPT-016 |
| `0x12` | 1 | `INT8` | `next` | Index of the next record on the list, or -1. | supported | FND-SCRIPT-016, FND-SCRIPT-017 |
| `0x13` | | | | Total size 19 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Read from the code that walks, keeps and converts the records [FND-SCRIPT-016, FND-SCRIPT-017];
no record was observed in memory.

## Open questions

- What the records stand for, what `unk_00`, `unk_02` and `unk_10` hold, and when the game runs
  each entry point (Q-SCRIPT-002).
