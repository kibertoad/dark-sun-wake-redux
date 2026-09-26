---
id: FMT-PARTY-002
title: Character record tail entry
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["CHARSAVE.GFF", "CD:CHARSAVE.GFF"]
byte_order: little
size: 33
text: false
definition: fmt_party_002.ksy
evidence: [FND-PARTY-004]
conflicting: []
split_with: []
related: []
---

## Layout

One of the `tail_count` records that follow the 79-byte header of an FMT-PARTY-001 character
record [FND-PARTY-004].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 33 | `BYTE[33]` | `unk_00` | Purpose unknown. | supported | FND-PARTY-004 |
| `0x21` | | | | Total size 33 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The 19 `CHAR` resources of the installed `CHARSAVE.GFF`, which hold 2 to 29 such records each,
and the 8 of the disc's copy: every resource's size is its header and a whole number of these
records [FND-PARTY-004].

## Open questions

- What a record holds. The number of records differs between characters, from 2 to 29, so each
  may be one of a list the character carries, such as its possessions (FND-PARTY-004,
  Q-PARTY-003).
