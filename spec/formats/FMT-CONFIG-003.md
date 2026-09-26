---
id: FMT-CONFIG-003
title: Saved settings in the PREF resource
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["CHARSAVE.GFF"]
byte_order: little
size: 9
text: false
definition: fmt_config_003.ksy
evidence: [FND-CONFIG-001, FND-CONFIG-002, FND-SAVE-004, FND-SAVE-005, FND-SOUND-007, FND-SOUND-008, FND-SOUND-011]
conflicting: []
split_with: []
related: []
---

## Layout

`PREF` resource 100, the only one, which the game removes and writes again each time it saves a
game and reads back each time it loads one [FND-CONFIG-001, FND-CONFIG-002, FND-SAVE-004,
FND-SAVE-005]. Each field is copied from and to one global of the game, given here by its
address in BLD-GOG-EN-1.1.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `unk_00` | Purpose unknown. The word at `DS:143A`. 0 in the shipped resource. | supported | FND-CONFIG-001, FND-SAVE-004, FND-SAVE-005 |
| `0x02` | 1 | `UINT8` | `unk_02` | Purpose unknown. The byte at `DS:26B4`. After loading, when sound is on and `unk_06` is not 0, the game compares it with a value a routine returns for it, and copies it to `unk_04` when that value is smaller. 255 in the shipped resource. | supported | FND-CONFIG-001, FND-SAVE-004, FND-SAVE-005 |
| `0x03` | 1 | `UINT8` | `unk_03` | Purpose unknown. The byte at `DS:26B5`. After loading, when sound is on and `unk_05` is not 0, the game passes it to a routine with 0. 63 in the shipped resource. | supported | FND-CONFIG-001, FND-SAVE-004, FND-SAVE-005 |
| `0x04` | 1 | `UINT8` | `unk_04` | Purpose unknown. The byte at `DS:26B6`. 255 in the shipped resource. | supported | FND-CONFIG-001, FND-SAVE-004, FND-SAVE-005 |
| `0x05` | 1 | `UINT8` | `unk_05` | Purpose unknown. The byte at `DS:1435`, passed after loading to a routine with `unk_06`. The sound-effect routine plays nothing while it is 0. 1 in the shipped resource. | supported | FND-CONFIG-001, FND-SAVE-004, FND-SAVE-005, FND-SOUND-007 |
| `0x06` | 1 | `UINT8` | `unk_06` | Purpose unknown. The byte at `DS:1436`. 1 in the shipped resource. | supported | FND-CONFIG-001, FND-SAVE-004, FND-SAVE-005 |
| `0x07` | 1 | `UINT8` | `unk_07` | Purpose unknown. The byte at `DS:1437`. After loading, the game passes 4, 4, 16, 16 to a routine when it is not 0 and 16, 16, 16, 16 when it is. 0 in the shipped resource. | supported | FND-CONFIG-001, FND-SAVE-004, FND-SAVE-005 |
| `0x08` | 1 | `UINT8` | `unk_08` | Purpose unknown. The byte at `DS:1439`. The speech routine speaks nothing while it is 0. 1 in the shipped resource. | supported | FND-CONFIG-001, FND-SAVE-004, FND-SAVE-005, FND-SOUND-008 |
| `0x09` | | | | Total size 9 | | |

## Enumerations and flags

None.

## Differences between builds

None known. The disc's copy of `CHARSAVE.GFF` holds no `PREF` resource [FND-CONFIG-001].

## Coverage

The one `PREF` resource of the installed `CHARSAVE.GFF` [FND-CONFIG-001].

## Open questions

- Which Preferences setting each field holds. The load routine's use suggests that `unk_05` and
  `unk_06` turn sound effects and music on, `unk_02` and `unk_03` are their volumes, and `unk_07`
  turns animations off, but nothing names them. The sound routines test `unk_05` before an effect
  and `unk_08` before a spoken line, and the startup passes `unk_03` to the library after testing
  `unk_05`, so `unk_08` is likely the voice setting and `unk_03` the effects volume; the difficulty
  is not placed (FND-SAVE-005, FND-SOUND-007, FND-SOUND-008, FND-SOUND-011, Q-CONFIG-002,
  Q-CONFIG-001).
