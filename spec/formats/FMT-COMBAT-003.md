---
id: FMT-COMBAT-003
title: Effect name record in DSUN.EXE
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["DSUN.EXE"]
byte_order: little
size: 31
text: false
definition: fmt_combat_003.ksy
evidence: [FND-COMBAT-022, FND-COMBAT-027]
conflicting: []
split_with: []
related: [RULE-COMBAT-009]
---

## Layout

One of the 113 records of the resident data of `DSUN.EXE` at `4C87:0000` (file offset `0x41A70`)
in BLD-GOG-EN-1.1, indexed by effect number from 0 [FND-COMBAT-027]. FND-COMBAT-027 lists every
record's name and word.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 29 | `char[29]` | `name` | The effect's name, ASCII, ending at the first NUL and padded with NULs. Empty in records 12 and 88. The status panel's third line shows it. | supported | FND-COMBAT-022, FND-COMBAT-027 |
| `0x1D` | 2 | `UINT16LE` | `unk_1D` | Purpose unknown. 0, or from `0x2B68` to `0x5313`. | supported | FND-COMBAT-027 |
| `0x1F` | | | | Total size 31 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

All 113 records of the installed `DSUN.EXE` of BLD-GOG-EN-1.1 were read with this layout; every
name is printable ASCII followed only by NULs [FND-COMBAT-027].

## Open questions

- What `unk_1D` holds (Q-COMBAT-008).
