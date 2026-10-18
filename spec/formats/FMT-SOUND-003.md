---
id: FMT-SOUND-003
title: STDPATCH.AD files
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["STDPATCH.AD", "CD:STDPATCH.AD"]
byte_order: little
size: null
text: false
definition: fmt_sound_003.ksy
evidence: [FND-SOUND-016]
conflicting: []
split_with: []
related: []
---

## Layout

The instrument bank the sound library opens at setup when word `0x32` of `SOUND.CFG` is 1 or 2, and reads one
instrument from at a time, by bank and patch number [FND-SOUND-016].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 6 × n | `timbre_entry[n]` | `entries` | The directory, read in order until an entry's `bank` is 0xFF. 233 entries in the installed file. | supported | FND-SOUND-016 |
| 6 × n | 2 | `UINT8[2]` | `terminator` | `FF FF`: a `patch` byte and a `bank` of 0xFF. The reader reads six bytes here and tests only the second. | supported | FND-SOUND-016 |
| | | `timbre[]` | `timbres` | The instruments, each at the `offset` of its entry. In the installed file they follow the terminator, 14 bytes apart, from 1,400 to the end of the file. | supported | FND-SOUND-016 |
| | | | | Total size variable; 4,662 in the installed file | | |

`timbre_entry`, 6 bytes:

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 1 | `UINT8` | `patch` | Patch number, compared with the requested patch. | supported | FND-SOUND-016 |
| `0x01` | 1 | `UINT8` | `bank` | Bank number, compared with the requested bank; 0xFF ends the directory. The installed file has banks 0 (patches 0 to 127), 127 (patches 35 to 87) and 1 (patches 0 to 51), in that order. | supported | FND-SOUND-016 |
| `0x02` | 4 | `UINT32LE` | `offset` | File offset of the entry's `timbre`. | supported | FND-SOUND-016 |

`timbre`, `length` bytes:

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `length` | Size of the record, this word included: 14 in every installed record. | supported | FND-SOUND-016 |
| `0x02` | `length` - 2 | `BYTE[]` | `data` | Passed whole, with `length`, to the loaded music driver. Its meaning is the driver's. | supported | FND-SOUND-016 |

## Enumerations and flags

None.

## Differences between builds

The disc copy is 5,162 bytes and differs from the installed one (FND-EXE-570); it belongs to
version 1.0 and is not studied.

## Coverage

The installed `STDPATCH.AD` of BLD-GOG-EN-1.1 and the reader in `DSUN.EXE` [FND-SOUND-016].

## Open questions

- What the bytes of `data` set in the music driver (No item: the driver's code is loaded from a
  resource and the field changes nothing the game itself reads).
