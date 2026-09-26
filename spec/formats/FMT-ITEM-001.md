---
id: FMT-ITEM-001
title: Item translation pair in ITEMS.BIN
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["ITEMS.BIN", "CD:ITEMS.BIN"]
byte_order: little
size: 4
text: false
definition: fmt_item_001.ksy
evidence: [FND-ITEM-001, FND-ITEM-002, FND-ITEM-006]
conflicting: []
split_with: []
related: [RULE-ITEM-006]
---

## Layout

One record of `ITEMS.BIN`. The file is 234 of these records and nothing else, with no header or
count; records 0 to 232 are in increasing order of `old_item`, and record 233 holds the smallest
`old_item` [FND-ITEM-001]. The character transfer utility reads the whole file and looks a record
up by `old_item` [FND-ITEM-006].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `old_item` | The number of a Dark Sun 1 item. Unique across the file, from 775 to 31,030. | supported | FND-ITEM-001, FND-ITEM-006 |
| `0x02` | 2 | `UINT16LE` | `new_item` | The number of the `RDFF` resource of `OBJEX.GFF` that stands for the same item in Dark Sun 2. From 603 to 31,990, 159 different values; 18 records hold a number that no `RDFF` resource of the shipped `OBJEX.GFF` has. | supported | FND-ITEM-001, FND-ITEM-002, FND-ITEM-006 |
| `0x04` | | | | Total size 4 | | |

The value file `FMT-ITEM-001.records.csv` gives all 234 records of the installed file in file
order, one row each, with the columns `record` (the record's index, from 0), `old_item` and
`new_item` [FND-ITEM-001].

## Enumerations and flags

None.

## Differences between builds

None known. The disc's copy of the file is the same [FND-ITEM-001].

## Coverage

All 234 records of the installed file and of the disc's copy [FND-ITEM-001].

## Open questions

- Whether `DSUN.EXE` reads the file at all. It holds no name for it (FND-ITEM-004, Q-ITEM-001).
- What an `old_item` numbers in the Dark Sun 1 saved game, and why the last record is out of
  order (FND-ITEM-001, FND-ITEM-006, Q-ITEM-001).
