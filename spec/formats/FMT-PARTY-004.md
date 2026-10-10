---
id: FMT-PARTY-004
title: Character PSST record
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["CHARSAVE.GFF", "CD:CHARSAVE.GFF"]
byte_order: little
size: 34
text: false
definition: fmt_party_004.ksy
evidence: [FND-PARTY-006, FND-PARTY-011, FND-PARTY-012, FND-PARTY-104]
conflicting: []
split_with: []
related: []
---

## Layout

The `PSST` resource of the character archive, one per character under the character's number,
beside its FMT-PARTY-001 record [FND-PARTY-006]. The game and the transfer utility both read and
write it as a 34-byte entry of a four-slot table [FND-PARTY-011, FND-PARTY-012].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 34 | `BYTE[34]` | `power_ranks` | One byte for each psionic power 0 to 33: bits 1 to 7 the power's rank, 0 while it is unknown, 1 when learned and 1 more for each enhancement up to 30; bit 0 is kept by every writer read. Power 0 is never offered. Bytes of 0, 2, 3, 4 or 6 in the shipped resources, mostly 0 and 2. | supported | FND-PARTY-006, FND-PARTY-011, FND-PARTY-012, FND-PARTY-104 |
| `0x22` | | | | Total size 34 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The 19 `PSST` resources of the installed `CHARSAVE.GFF` and the 8 of the disc's copy: every one
is 34 bytes [FND-PARTY-006].

## Open questions

- What bit 0 of a byte holds: the writers of FND-PARTY-104 keep it, and its own writers and
  readers were not read (Q-PARTY-005).
