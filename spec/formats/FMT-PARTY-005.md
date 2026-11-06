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
evidence: [FND-PARTY-006, FND-PARTY-011, FND-SAVE-046, FND-PARTY-104, FND-PARTY-203, FND-PARTY-209, FND-SAVE-015, FND-SAVE-022, FND-RNG-091]
conflicting: []
split_with: []
related: []
---

## Layout

The `SPST` resource of the character archive, one per character under the character's number,
beside its FMT-PARTY-001 record [FND-PARTY-006]. The game reads and writes it as a 15-byte entry
of a four-slot table, and the transfer utility writes a 9-byte one [FND-PARTY-011,
FND-SAVE-046], so the resource is 15 bytes, or 9 when the transfer utility wrote it last.
`resource_size` is the size the archive's directory gives.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 9 | `BYTE[9]` | `known_spells_00` | Bit `n & 7` of byte `n >> 3` is set when the character knows spell `n`, for spells 0 to 71. | supported | FND-PARTY-006, FND-PARTY-011, FND-SAVE-046, FND-PARTY-104 |
| `0x09` | 6 if `resource_size` == 15 | `BYTE[6]` | `known_spells_09` | The same for spells 72 to 119. Present in the resources the game writes. | supported | FND-PARTY-006, FND-SAVE-046, FND-PARTY-104 |
| | | | | Total size `resource_size`, 9 or 15 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The 19 `SPST` resources of the installed `CHARSAVE.GFF` and the 8 of the disc's copy: 16 are 15
bytes and 3 (characters 30, 31 and 33) are 9 bytes [FND-PARTY-006].

## Open questions

- Remaining SPST read and transfer coverage that can affect the byte/bit
  interpretation or tail of a short load (Q-PARTY-128). FND-PARTY-203
  shows the selected reader writes the resource's own length: nine bytes
  leave the slot's pre-read bytes 9 to 14 untouched. Its shipped ADD and
  START cases use fifteen-byte resources; it explicitly does not census
  other SPST readers. FND-PARTY-209 covers literal-base accesses, not all
  pointer-derived or computed-offset paths.
  FND-SAVE-015/022 already identify the whole sixty-byte bank as SAVE
  table entry 20 and its restore route. With a sixty-byte selected payload
  and valid separate backing, that restore replaces all four entries;
  it is not necessarily a zero clear. The retained tail at a later short
  read can therefore be prior saved state, not just a previous character's
  direct spell-bit writes. FND-RNG-091's direct-read accounting retains
  indirect/context exclusions. General I/O success and unrelated gameplay
  effects are outside this remaining representation question.
