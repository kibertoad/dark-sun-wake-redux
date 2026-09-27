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
evidence: [FND-CONFIG-003, FND-CONFIG-004, FND-CONFIG-005, FND-CONFIG-012, FND-CONFIG-019, FND-CONFIG-020, FND-CONFIG-021, FND-CONFIG-022, FND-CONFIG-023, FND-SOUND-007, FND-SOUND-008, FND-SOUND-013]
conflicting: []
split_with: []
related: []
---

## Layout

The whole of `SOUND.CFG`, which the sound setup program writes in one piece of 59 bytes
[FND-CONFIG-004] and the game reads into a whole-file buffer through its sound library
[FND-CONFIG-005, FND-CONFIG-019]. The loader does not validate the 59-byte layout. The meanings below
come from the values of the shipped file and the `SOUND.INI` record they match [FND-CONFIG-003].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `unk_00` | `0x220`, the first address of the matching `SOUND.INI` record. Passed with the next three words to two initialization calls; purpose unknown. | supported | FND-CONFIG-003, FND-CONFIG-021 |
| `0x02` | 2 | `UINT16LE` | `unk_02` | 5, the record's first IRQ. Passed with the other first-block words to two initialization calls; purpose unknown. | supported | FND-CONFIG-003, FND-CONFIG-021 |
| `0x04` | 2 | `UINT16LE` | `unk_04` | 1, the record's first DMA channel. Passed with the other first-block words to two initialization calls; purpose unknown. | supported | FND-CONFIG-003, FND-CONFIG-021 |
| `0x06` | 2 | `UINT16LE` | `unk_06` | 1, the record's flag count. Passed with the other first-block words to two initialization calls; purpose unknown. | supported | FND-CONFIG-003, FND-CONFIG-021 |
| `0x08` | 2 | `UINT16LE` | `unk_08` | 122, the matching record's card ID. Values `0x77` and `0x79` select two hardware port writes using `unk_0a`; `0x69` selects a runtime byte value of 90 rather than 100. The game plays no sound effect, speech or song while it is 113, the card ID of "No Sound", unless `unk_14` asks for disc music. Other uses unknown. | supported | FND-CONFIG-003, FND-CONFIG-020, FND-CONFIG-021, FND-SOUND-007, FND-SOUND-008, FND-SOUND-013 |
| `0x0A` | 2 | `UINT16LE` | `unk_0a` | Equal to `unk_00` in the shipped file. Used as an I/O base for `unk_08` values `0x77` or `0x79`; other uses unknown. | supported | FND-CONFIG-003, FND-CONFIG-020 |
| `0x0C` | 2 | `UINT16LE` | `unk_0c` | Purpose unknown. Equal to `unk_02`. | supported | FND-CONFIG-003 |
| `0x0E` | 2 | `UINT16LE` | `unk_0e` | Purpose unknown. Equal to `unk_04`. | supported | FND-CONFIG-003 |
| `0x10` | 2 | `UINT16LE` | `unk_10` | Purpose unknown. Equal to `unk_06`. | supported | FND-CONFIG-003 |
| `0x12` | 2 | `UINT16LE` | `unk_12` | Purpose unknown. Equal to `unk_08`. | supported | FND-CONFIG-003 |
| `0x14` | 2 | `UINT16LE` | `unk_14` | 11, the record's music driver chunk number. Bit `0x08` clears a library word during initialization; bit `0x02` gates a callback error path and disc audio track playback; bit `0x01` gates another callback error path. Other uses unknown. | supported | FND-CONFIG-003, FND-CONFIG-020, FND-CONFIG-021, FND-SOUND-013 |
| `0x16` | 14 | `char[14]` | `music_driver` | The file name of the real-mode music driver, with its extension, padded with NULs. | supported | FND-CONFIG-003 |
| `0x24` | 14 | `char[14]` | `digital_driver` | The file name of the real-mode digital sound driver, with its extension, padded with NULs. | supported | FND-CONFIG-003 |
| `0x32` | 2 | `UINT16LE` | `unk_32` | 1 in the shipped file, copied by setup from an input record at `+0x2A`. Initialization replaces 1 or 2 with 4 in the loaded buffer when `DS:3417` equals 1, and those values also gate later string and file calls. A value of 3 makes the sound library cap a music-level request at 90 instead of 100. Other uses unknown. | supported | FND-CONFIG-003, FND-CONFIG-012, FND-CONFIG-020, FND-CONFIG-021, FND-CONFIG-022 |
| `0x34` | 2 | `UINT16LE` | `unk_34` | 4 in the shipped file; setup writes the literal 4. The library compares it with a runtime word using a signed greater-than branch; the compared quantity's purpose is unknown. | supported | FND-CONFIG-003, FND-CONFIG-022, FND-CONFIG-023 |
| `0x36` | 2 | `UINT16LE` | `unk_36` | 8 in the shipped file, matching the record's digital driver chunk number. Setup copies it from an input record at `+0xD6`; the library uses it as an `ADV ` resource number when a local selector is one. | supported | FND-CONFIG-003, FND-CONFIG-022, FND-CONFIG-023 |
| `0x38` | 2 | `UINT16LE` | `unk_38` | 11 in the shipped file, matching the music driver chunk number. Setup copies it from an input record at `+0xD8`; the library uses it as an `ADV ` resource number when the selector is zero. | supported | FND-CONFIG-003, FND-CONFIG-022, FND-CONFIG-023 |
| `0x3A` | 1 | `UINT8` | `unk_3a` | 0 in the shipped file. Setup copies it from an input record at `+0xDA`; the library tests it for zero and one at several branches whose effects remain unread. | supported | FND-CONFIG-003, FND-CONFIG-022, FND-CONFIG-023 |
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
  and what the game does with each field (FND-CONFIG-003, FND-CONFIG-005,
  FND-CONFIG-019, FND-CONFIG-020, FND-CONFIG-021, FND-CONFIG-022,
  FND-CONFIG-023,
  Q-CONFIG-005). One reading is that the
  second block is the digital device because its first word supplies the
  observed hardware I/O base; the competing reading is that it is another
  music-device configuration, since the shipped blocks are equal. Reading
  the remaining consumers and setup writer field by field would separate them.
