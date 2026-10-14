---
id: FMT-PARTY-005
title: Character SPST record
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["CHARSAVE.GFF", "CD:CHARSAVE.GFF"]
byte_order: little
size: null
text: false
definition: fmt_party_005.ksy
evidence: [FND-PARTY-006, FND-PARTY-011, FND-PARTY-012, FND-PARTY-104, FND-PARTY-203, FND-PARTY-209]
conflicting: []
split_with: []
related: []
---

## Layout

The `SPST` resource of the character archive, one per character under the character's number,
beside its FMT-PARTY-001 record [FND-PARTY-006]. The game reads and writes it as a 15-byte entry
of a four-slot table, and the transfer utility writes a 9-byte one [FND-PARTY-011,
FND-PARTY-012], so the resource is 15 bytes, or 9 when the transfer utility wrote it last.
`resource_size` is the size the archive's directory gives.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 9 | `BYTE[9]` | `known_spells_00` | Bit `n & 7` of byte `n >> 3` is set when the character knows spell `n`, for spells 0 to 71. | supported | FND-PARTY-006, FND-PARTY-011, FND-PARTY-012, FND-PARTY-104 |
| `0x09` | 6 if `resource_size` == 15 | `BYTE[6]` | `known_spells_09` | The same for spells 72 to 119. Present in the resources the game writes. | supported | FND-PARTY-006, FND-PARTY-012, FND-PARTY-104 |
| | | | | Total size `resource_size`, 9 or 15 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The 19 `SPST` resources of the installed `CHARSAVE.GFF` and the 8 of the disc's copy: 16 are 15
bytes and 3 (characters 30, 31 and 33) are 9 bytes [FND-PARTY-006].

## Open questions

- What a 9-byte resource leaves in bytes 9 to 14 of its 15-byte entry: the read writes only the
  first 9 bytes, and no shipped character with one can be loaded (FND-PARTY-203); no instruction
  that names the table clears a whole entry, so they keep the slot's earlier bits
  (FND-PARTY-209). Left: transfers that reach the table without naming it (Q-PARTY-005).
