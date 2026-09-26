---
id: FMT-ACTOR-004
title: Object slot record
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: []
byte_order: little
size: 37
text: false
definition: fmt_actor_004.ksy
evidence: [FND-ACTOR-003, FND-ACTOR-005, FND-COMBAT-025, FND-COMBAT-026, FND-EXPLORE-003, FND-EXPLORE-005, FND-INPUT-006, FND-PARTY-013]
conflicting: []
split_with: []
related: [RULE-COMBAT-004, RULE-COMBAT-005]
---

## Layout

One of the records of the table at `57E0:67BB` in BLD-GOG-EN-1.1, `0x25` bytes apart and kept
only in memory. Slot `n` is at `57E0:67BB + n * 37`. Slots 0 to 3 hold the party
[FND-PARTY-013]; `31E0:0EFF` fills a slot from an object's `OJFF` definition and an entry of the
table at the far pointer `57E0:67B7` [FND-ACTOR-003], and treats the slot number -1 as 319.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 1 | `UINT8` | `unk_00` | Purpose unknown. Copied from byte `0x0` of the definition. For a party slot, bit 7 set hides the member: the party loader and key 6 set it, key 5 clears it, and the occupancy routines pass over the slot. Bit 5 set makes the movement routine do nothing. One caller of the shift-key routine looks for a record with bit 4 set. | supported | FND-ACTOR-003, FND-EXPLORE-003, FND-EXPLORE-005, FND-INPUT-006, FND-PARTY-013 |
| `0x01` | 2 | `UINT16LE` | `entry_index` | Index of the 8-byte entry of the table at `57E0:67B7` the slot was filled from. | supported | FND-ACTOR-003, FND-ACTOR-005 |
| `0x03` | 2 | `INT16LE` | `x` | Horizontal place of the object's figure: the entry's first word less the definition's word at `0x2`. Less the word at `57E0:1408`, it is where the end-of-move menu is placed from. | supported | FND-ACTOR-003, FND-COMBAT-025, FND-COMBAT-026 |
| `0x05` | 2 | `INT16LE` | `y` | Vertical place of the object's figure: the entry's second word less the definition's word at `0x4` and its signed byte at `0xA`. Less the word at `57E0:140A`, it is where the end-of-move menu is placed from. | supported | FND-ACTOR-003, FND-COMBAT-025, FND-COMBAT-026 |
| `0x07` | 1 | `UINT8` | `unk_07` | Purpose unknown. Byte `0x2` of the definition. | supported | FND-ACTOR-003 |
| `0x08` | 2 | `UINT16LE` | `unk_08` | Purpose unknown. The definition's word at `0x4`. | supported | FND-ACTOR-003 |
| `0x0A` | 2 | `UINT16LE` | `position_x` | The entry's first word: the object's position, from which the definition's offsets are measured. Shifted right by 4, the column of the object's cell for movement and occupancy. | supported | FND-ACTOR-003, FND-EXPLORE-003 |
| `0x0C` | 2 | `UINT16LE` | `position_y` | The entry's second word: the object's position. Shifted right by 4, the row of the object's cell for movement and occupancy. | supported | FND-ACTOR-003, FND-EXPLORE-003 |
| `0x0E` | 1 | `UINT8` | `unk_0E` | Purpose unknown. The entry's byte at `0x4`. | supported | FND-ACTOR-003 |
| `0x0F` | 1 | `UINT8` | `unk_0F` | Purpose unknown. Byte `0xB` of the definition. | supported | FND-ACTOR-003 |
| `0x10` | 2 | `BYTE[2]` | `unk_10` | Purpose unknown. | supported | FND-ACTOR-003 |
| `0x12` | 1 | `UINT8` | `unk_12` | Purpose unknown. Set to 0 when the slot is filled. | supported | FND-ACTOR-003 |
| `0x13` | 1 | `UINT8` | `width` | Width of the figure: the end-of-move menu goes this far to the right of `x`. | supported | FND-COMBAT-026 |
| `0x14` | 1 | `UINT8` | `height` | Height of the figure: the end-of-move menu is centred on half of it below `y`. | supported | FND-COMBAT-026 |
| `0x15` | 4 | `BYTE[4]` | `unk_15` | Purpose unknown. The first byte at least is set to 0 when the slot is filled. | supported | FND-ACTOR-003 |
| `0x19` | 2 | `UINT16LE` | `image` | The image number the slot draws: the definition's word at `0xC`, or 11,001 to 11,008 for seven object numbers. | supported | FND-ACTOR-003 |
| `0x1B` | 2 | `UINT16LE` | `object_number` | The object number the slot was filled with. | supported | FND-ACTOR-005 |
| `0x1D` | 4 | `UINT32LE` | `unk_1D` | Purpose unknown. Set to 0 when the slot is filled. | supported | FND-ACTOR-003 |
| `0x21` | 4 | `BYTE[4]` | `unk_21` | Purpose unknown. | supported | FND-ACTOR-003 |
| `0x25` | | | | Total size 37 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Read from the code that fills the slots and from the combat code that places the end-of-move
menu [FND-ACTOR-003, FND-COMBAT-026]; no record was observed in memory.

## Open questions

- What the unknown fields and the bits of `unk_00` hold, whether `x` and `y` are map pixels, and
  how many slots the table holds (Q-ACTOR-002).
