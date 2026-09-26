---
id: FMT-CONFIG-001
title: Sound configuration SOUND.CFG
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["SOUND.CFG"]
byte_order: little
size: 59
text: false
definition: fmt_config_001.ksy
evidence: [FND-CONFIG-003, FND-CONFIG-004, FND-CONFIG-005, FND-SOUND-007, FND-SOUND-008, FND-SOUND-013]
conflicting: []
split_with: []
related: []
---

## Layout

The whole of `SOUND.CFG`, which the sound setup program writes in one piece of 59 bytes
[FND-CONFIG-004] and the game reads through its sound library [FND-CONFIG-005]. The meanings below
come from the values of the shipped file and the `SOUND.INI` record they match [FND-CONFIG-003].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `unk_00` | Purpose unknown. `0x220`, the first address of the matching `SOUND.INI` record. | supported | FND-CONFIG-003 |
| `0x02` | 2 | `UINT16LE` | `unk_02` | Purpose unknown. 5, the record's first IRQ. | supported | FND-CONFIG-003 |
| `0x04` | 2 | `UINT16LE` | `unk_04` | Purpose unknown. 1, the record's first DMA channel. | supported | FND-CONFIG-003 |
| `0x06` | 2 | `UINT16LE` | `unk_06` | Purpose unknown. 1, the record's flag count. | supported | FND-CONFIG-003 |
| `0x08` | 2 | `UINT16LE` | `unk_08` | Purpose unknown. 122, the record's card ID. The game plays no sound effect, speech or song while it is 113, the card ID of "No Sound", unless `unk_14` asks for disc music. | supported | FND-CONFIG-003, FND-SOUND-007, FND-SOUND-008, FND-SOUND-013 |
| `0x0A` | 2 | `UINT16LE` | `unk_0a` | Purpose unknown. Equal to `unk_00` in the shipped file. | supported | FND-CONFIG-003 |
| `0x0C` | 2 | `UINT16LE` | `unk_0c` | Purpose unknown. Equal to `unk_02`. | supported | FND-CONFIG-003 |
| `0x0E` | 2 | `UINT16LE` | `unk_0e` | Purpose unknown. Equal to `unk_04`. | supported | FND-CONFIG-003 |
| `0x10` | 2 | `UINT16LE` | `unk_10` | Purpose unknown. Equal to `unk_06`. | supported | FND-CONFIG-003 |
| `0x12` | 2 | `UINT16LE` | `unk_12` | Purpose unknown. Equal to `unk_08`. | supported | FND-CONFIG-003 |
| `0x14` | 2 | `UINT16LE` | `unk_14` | Purpose unknown. 11, the record's music driver chunk number. With bit 1 set the game plays its music as disc audio tracks. | supported | FND-CONFIG-003, FND-SOUND-013 |
| `0x16` | 14 | `char[14]` | `music_driver` | The file name of the real-mode music driver, with its extension, padded with NULs. | supported | FND-CONFIG-003 |
| `0x24` | 14 | `char[14]` | `digital_driver` | The file name of the real-mode digital sound driver, with its extension, padded with NULs. | supported | FND-CONFIG-003 |
| `0x32` | 2 | `UINT16LE` | `unk_32` | Purpose unknown. 1 in the shipped file. | supported | FND-CONFIG-003 |
| `0x34` | 2 | `UINT16LE` | `unk_34` | Purpose unknown. 4. | supported | FND-CONFIG-003 |
| `0x36` | 2 | `UINT16LE` | `unk_36` | Purpose unknown. 8, the record's digital driver chunk number. | supported | FND-CONFIG-003 |
| `0x38` | 2 | `UINT16LE` | `unk_38` | Purpose unknown. 11, the music driver chunk number again. | supported | FND-CONFIG-003 |
| `0x3A` | 1 | `UINT8` | `unk_3a` | Purpose unknown. 0. | supported | FND-CONFIG-003 |
| `0x3B` | | | | Total size 59 | | |

## Enumerations and flags

None.

## Differences between builds

None known. The disc has no `SOUND.CFG`; the setup program writes it [FND-CONFIG-003,
FND-CONFIG-004].

## Coverage

The one installed file [FND-CONFIG-003].

## Open questions

- Which block is for music and which for digital sound, what the two unexplained tail words hold,
  and what the game does with each field (FND-CONFIG-003, FND-CONFIG-005, Q-CONFIG-005).
