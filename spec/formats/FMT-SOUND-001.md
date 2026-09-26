---
id: FMT-SOUND-001
title: Voice file with one block of 8-bit samples
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["*.VOC", "CD:SPEECH/*.VOC", "CD:INTR/*.VOC", "RESOURCE.GFF"]
byte_order: little
size: null
text: false
definition: fmt_sound_001.ksy
evidence: [FND-SOUND-001, FND-SOUND-006, FND-SOUND-007, FND-SOUND-008]
conflicting: []
split_with: []
related: []
---

## Layout

Every installed `SOUND*.VOC` and `SPCH*.VOC` file, every file of the disc's `SPEECH` and `INTR`
directories, and every `BVOC` resource of `RESOURCE.GFF` has this layout, a Creative Voice File
with a single block [FND-SOUND-001]. The game plays `BVOC` resources and `SOUND` files as sound
effects and `SPCH` and `INTR` files as speech [FND-SOUND-006, FND-SOUND-007, FND-SOUND-008].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 19 | `char[19]` | `signature` | `Creative Voice File`. | supported | FND-SOUND-001 |
| `0x13` | 1 | `UINT8` | `eof_mark` | `0x1A`. | supported | FND-SOUND-001 |
| `0x14` | 2 | `UINT16LE` | `header_size` | 26, the offset of the first block. | supported | FND-SOUND-001 |
| `0x16` | 2 | `UINT16LE` | `version` | `0x010A`, version 1.10. | supported | FND-SOUND-001 |
| `0x18` | 2 | `UINT16LE` | `version_check` | `0x1129`, which is `0x1234` plus the complement of `version`. | supported | FND-SOUND-001 |
| `0x1A` | 1 | `UINT8` | `block_type` | 1, a block of sound data with its own rate. | supported | FND-SOUND-001 |
| `0x1B` | 3 | `UINT24LE` | `block_length` | The number of bytes after this field up to the terminator: 2 plus the sample count. | supported | FND-SOUND-001 |
| `0x1E` | 1 | `UINT8` | `time_constant` | The sample rate as `256 - 1000000 / rate`: 165, 210 or 131 [FND-SOUND-001]. | supported | FND-SOUND-001 |
| `0x1F` | 1 | `UINT8` | `codec` | 0, unsigned 8-bit samples. | supported | FND-SOUND-001 |
| `0x20` | `block_length - 2` | `BYTE[block_length - 2]` | `samples` | One channel of unsigned 8-bit samples. | supported | FND-SOUND-001 |
| `0x1E + block_length` | 1 | `UINT8` | `terminator` | 0, the block type that ends the file. | supported | FND-SOUND-001 |
| `0x1F + block_length` | | | | Total size `31 + block_length` | | |

## Enumerations and flags

`time_constant`: 165 gives 11,111 samples a second, 210 gives 21,739 and 131 gives 8,000
[FND-SOUND-001].

## Differences between builds

None known.

## Coverage

All 147 installed files, all 190 files of the disc's `SPEECH` and `INTR` directories and all 177
`BVOC` resources [FND-SOUND-001].

## Open questions

- Whether the sound library plays the samples at the rate the time constant gives, and whether it
  reads any field other than the samples (FND-SOUND-001, Q-SOUND-002).
