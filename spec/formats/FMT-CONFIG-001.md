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
evidence: [FND-CONFIG-003, FND-CONFIG-213, FND-CONFIG-005, FND-CONFIG-236, FND-CONFIG-019, FND-CONFIG-020, FND-CONFIG-021, FND-CONFIG-214, FND-CONFIG-023, FND-CONFIG-024, FND-SOUND-007, FND-SOUND-008, FND-SOUND-013, FND-SOUND-020, FND-SOUND-037, FND-SOUND-047, FND-SOUND-092]
conflicting: []
split_with: []
related: []
---

## Layout

The whole of `SOUND.CFG`, which the sound setup program writes in one piece of 59 bytes
[FND-CONFIG-213] and the game reads into a whole-file buffer through its sound library
[FND-CONFIG-005, FND-CONFIG-019]. The loader does not validate the 59-byte layout. The meanings below
come from the shipped file, its matching record and the parser-to-output handoff
[FND-CONFIG-003, FND-SOUND-020, FND-SOUND-037, FND-SOUND-047].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `unk_00` | `0x220`, the first address of the matching `SOUND.INI` record. Passed with the next three words to two initialization calls; purpose unknown. | supported | FND-CONFIG-003, FND-CONFIG-021 |
| `0x02` | 2 | `UINT16LE` | `unk_02` | 5, the record's first IRQ. Passed with the other first-block words to two initialization calls; purpose unknown. | supported | FND-CONFIG-003, FND-CONFIG-021 |
| `0x04` | 2 | `UINT16LE` | `unk_04` | 1, the record's first DMA channel. Passed with the other first-block words to two initialization calls; purpose unknown. | supported | FND-CONFIG-003, FND-CONFIG-021 |
| `0x06` | 2 | `UINT16LE` | `unk_06` | 1, the record's flag count. Passed with the other first-block words to two initialization calls; purpose unknown. | supported | FND-CONFIG-003, FND-CONFIG-021 |
| `0x08` | 2 | `UINT16LE` | `unk_08` | 122, the matching record's card ID. Values `0x77` and `0x79` select two hardware port writes using `unk_0a`; `0x69` selects a runtime byte value of 90 rather than 100. The game plays no sound effect, speech or song while it is 113, the card ID of "No Sound", unless `unk_14` asks for disc music. Other uses unknown. | supported | FND-CONFIG-003, FND-CONFIG-020, FND-CONFIG-021, FND-SOUND-007, FND-SOUND-008, FND-SOUND-013 |
| `0x0A` | 2 | `UINT16LE` | `unk_0a` | Equal to `unk_00` in the shipped file. Used as an I/O base for `unk_08` values `0x77` or `0x79`; passed with the next three words to two setup calls for selector one. Other uses unknown. | supported | FND-CONFIG-003, FND-CONFIG-020, FND-CONFIG-024 |
| `0x0C` | 2 | `UINT16LE` | `unk_0c` | Equal to `unk_02` in the shipped file. Passed with the other second-block words to two setup calls; purpose unknown. | supported | FND-CONFIG-003, FND-CONFIG-024 |
| `0x0E` | 2 | `UINT16LE` | `unk_0e` | Equal to `unk_04` in the shipped file. Passed with the other second-block words to two setup calls; purpose unknown. | supported | FND-CONFIG-003, FND-CONFIG-024 |
| `0x10` | 2 | `UINT16LE` | `unk_10` | Equal to `unk_06` in the shipped file. Passed with the other second-block words to two setup calls; purpose unknown. | supported | FND-CONFIG-003, FND-CONFIG-024 |
| `0x12` | 2 | `UINT16LE` | `unk_12` | Purpose unknown. Equal to `unk_08`. | supported | FND-CONFIG-003 |
| `0x14` | 2 | `UINT16LE` | `unk_14` | 11, the record's music driver chunk number. Bit `0x08` clears a library word during initialization; bit `0x02` gates a callback error path and disc audio track playback; bit `0x01` gates setup of the second settings block and its `ADV ` resource. Other uses unknown. | supported | FND-CONFIG-003, FND-CONFIG-020, FND-CONFIG-021, FND-CONFIG-024, FND-SOUND-013 |
| `0x16` | 14 | `char[14]` | `music_driver` | The file name of the real-mode music driver, with its extension, padded with NULs. | supported | FND-CONFIG-003 |
| `0x24` | 14 | `char[14]` | `digital_driver` | The file name of the real-mode digital sound driver, with its extension, padded with NULs. | supported | FND-CONFIG-003 |
| `0x32` | 2 | `UINT16LE` | `unk_32` | 1 in the shipped file; setup copies the selected record's CardGroup word `+0x2A`. Initialization replaces 1 or 2 with 4 in the loaded buffer when `DS:3417` equals 1, and those values also gate later string and file calls. A value of 3 caps a music-level request at 90 instead of 100. Setup's summary substitutes -1 for the first block's DMA argument when this word is 4. Other uses unknown. | supported | FND-CONFIG-003, FND-CONFIG-236, FND-CONFIG-020, FND-CONFIG-021, FND-CONFIG-214, FND-SOUND-020, FND-SOUND-037, FND-SOUND-047 |
| `0x34` | 2 | `UINT16LE` | `unk_34` | 4 in the shipped file; setup writes the literal 4. The library compares it with a runtime word using a signed greater-than branch; the compared quantity's purpose is unknown. | supported | FND-CONFIG-003, FND-CONFIG-214, FND-CONFIG-023 |
| `0x36` | 2 | `UINT16LE` | `unk_36` | 8 in the shipped file. Setup copies the selected record's DigitalDriverChunkNumber word `+0xD6`; the library uses it as the `ADV ` number paired with the second settings block. This source-field identity does not prove driver installation or device effects. | supported | FND-CONFIG-003, FND-CONFIG-214, FND-CONFIG-023, FND-CONFIG-024, FND-SOUND-020, FND-SOUND-037, FND-SOUND-047 |
| `0x38` | 2 | `UINT16LE` | `unk_38` | 11 in the shipped file. Setup copies the selected record's MidiDriverChunkNumber word `+0xD8`; the library pairs it with the first settings block. This does not prove driver installation; the unchanged installed flag at `0x14` can bypass that path. | supported | FND-CONFIG-003, FND-CONFIG-214, FND-CONFIG-023, FND-CONFIG-024, FND-SOUND-020, FND-SOUND-037, FND-SOUND-047, FND-SOUND-092 |
| `0x3A` | 1 | `UINT8` | `unk_3a` | 0 in the shipped file. Setup copies only the low byte of the selected record's SamplePlaybackMode at `+0xDA`, despite its parser's word store. Other library effects remain unread. | supported | FND-CONFIG-003, FND-CONFIG-214, FND-CONFIG-023, FND-SOUND-037, FND-SOUND-047 |
| `0x3B` | | | | Total size 59 | | |

## Enumerations and flags

None.

## Differences between builds

None known. The disc has no `SOUND.CFG`; the setup program writes it [FND-CONFIG-003,
FND-CONFIG-213].

## Coverage

The one installed file [FND-CONFIG-003].

## Open questions

- Remaining selected-record settings-block construction and incoming storage
  admission, game-side block roles and field effects (Q-CONFIG-005).
  FND-SOUND-020/037/047 already bind CardGroup, both driver-number fields,
  SamplePlaybackMode and name-copy alternatives; do not repeat those local
  mappings. A digital source-field name is compatible with an actual digital
  block but does not settle its receiving driver calls or device effects.
  Conditional source settings and loaded-buffer rewrites remain distinct.
  The finite setup roots are SOUND_DS.EXE 1429:042D, 1429:063A and
  1BD4:0003 in coverage/BLD-GOG-EN-1.1/SOUND_DS.EXE.tsv. The latter writes
  the whole buffer; the former two share selected-record state with
  FMT-CONFIG-002 and the name/loader contract in FND-SOUND-020. Record and
  output span, aliasing, incoming selection and child preservation remain
  conditions, not established lifetime claims.
  DSUN.EXE roots 4734:0063, 47B9:0236 and initializer4734:0133 are in its
  corresponding inventory. Resource selector47B9:0334 and digital setup
  49EC:015F from FND-CONFIG-024 share sound-driver helpers; their canonical
  inventory ownership and unresolved indirect targets remain unassigned
  here. No CONFIG ownership JSON or complete-reading declaration is claimed.
  Existing inventory coverage does not establish missing branches/callers;
  embedded ADV code still lacks independent inventories. File layout does
  not answer loader short-read/release outcomes, computed/block access,
  changed configuration bindings or native driver behavior.
