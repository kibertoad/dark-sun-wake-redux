---
id: FMT-COMBAT-002
title: Combatant details record
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: []
byte_order: little
size: 66
text: false
definition: fmt_combat_002.ksy
evidence: [FND-COMBAT-018, FND-COMBAT-022, FND-COMBAT-023]
conflicting: []
split_with: []
related: []
---

## Layout

One of the records of the table at the far pointer `57E0:19C5` in BLD-GOG-EN-1.1, kept only in
memory. The party's buttons index it by party slot [FND-COMBAT-023], and the status panel by the
second index `2D40:3E64` gives for the character whose turn it is [FND-COMBAT-022].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 8 | `BYTE[8]` | `unk_00` | Purpose unknown. | supported | FND-COMBAT-022 |
| `0x08` | 2 | `INT16LE` | `max_hit_points` | The character's greatest hit points, the second number of the panel's second line. | supported | FND-COMBAT-018, FND-COMBAT-022 |
| `0x0A` | 4 | `BYTE[4]` | `unk_0A` | Purpose unknown. | supported | FND-COMBAT-022 |
| `0x0E` | 2 | `UINT16LE` | `unk_0E` | Purpose unknown. 0 in a party slot whose leader and character buttons do nothing. | supported | FND-COMBAT-023 |
| `0x10` | 50 | `BYTE[50]` | `unk_10` | Purpose unknown. | supported | FND-COMBAT-022 |
| `0x42` | | | | Total size 66 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Read from the code that indexes the table with a stride of 66 [FND-COMBAT-022, FND-COMBAT-023];
no record was observed in memory.

## Open questions

- What `unk_00`, `unk_0A`, `unk_0E` and `unk_10` hold, and how many records the table holds
  (Q-COMBAT-002).
