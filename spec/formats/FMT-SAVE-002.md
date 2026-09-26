---
id: FMT-SAVE-002
title: Saved game state in a GREQ resource
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["CHARSAVE.GFF"]
byte_order: little
size: 9
text: false
definition: fmt_save_002.ksy
evidence: [FND-SAVE-001, FND-SAVE-002, FND-SAVE-004, FND-SAVE-005]
conflicting: []
split_with: []
related: [RULE-SAVE-002]
---

## Layout

A `GREQ` resource, one for each saved game, numbered as the saved game is, from 1 to 10. Saving
writes it from five globals of the game and loading copies it back to them [FND-SAVE-004,
FND-SAVE-005].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `unk_00` | Purpose unknown. Saved from and loaded into the word at `DS:14D7` in BLD-GOG-EN-1.1. 0, 752, 1,024, 1,376 or 1,632 in the shipped resources. | supported | FND-SAVE-001, FND-SAVE-004, FND-SAVE-005 |
| `0x02` | 2 | `UINT16LE` | `unk_02` | Purpose unknown. The word at `DS:14D9`. 0 or 256 in the shipped resources. | supported | FND-SAVE-001, FND-SAVE-004, FND-SAVE-005 |
| `0x04` | 2 | `UINT16LE` | `unk_04` | Purpose unknown. The word at `DS:14DB`. 1,008 or 2,048 in the shipped resources. | supported | FND-SAVE-001, FND-SAVE-004, FND-SAVE-005 |
| `0x06` | 2 | `UINT16LE` | `unk_06` | Purpose unknown. The word at `DS:14DD`. 784, 1,056 or 1,568 in the shipped resources. | supported | FND-SAVE-001, FND-SAVE-004, FND-SAVE-005 |
| `0x08` | 1 | `UINT8` | `unk_08` | Purpose unknown. The byte at `DS:4459`. 0, 1, 2 or 4 in the shipped resources. No 16-bit window of the resource holds a `CHAR` resource number. | supported | FND-SAVE-001, FND-SAVE-002, FND-SAVE-004, FND-SAVE-005 |
| `0x09` | | | | Total size 9 | | |

## Enumerations and flags

None.

## Differences between builds

None known. The disc's copy of `CHARSAVE.GFF` holds no `GREQ` resource [FND-SAVE-001].

## Coverage

The ten `GREQ` resources of the installed `CHARSAVE.GFF`, every one 9 bytes [FND-SAVE-001].

## Open questions

- What the four words and the byte hold (FND-SAVE-001, Q-SAVE-002).
- The routines keep a tenth byte, at `DS:4458`, beside the nine: saving leaves it out, and loading
  copies a byte the read never wrote into it (FND-SAVE-004, FND-SAVE-005, Q-SAVE-002).
