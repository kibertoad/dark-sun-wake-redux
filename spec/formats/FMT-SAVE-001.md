---
id: FMT-SAVE-001
title: Stored character identifier in a CACT resource
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["CHARSAVE.GFF"]
byte_order: little
size: 2
text: false
definition: fmt_save_001.ksy
evidence: [FND-PARTY-011, FND-SAVE-046, FND-SAVE-001, FND-SAVE-002, FND-SAVE-045, FND-SAVE-047]
conflicting: []
split_with: []
related: []
---

## Layout

A `CACT` resource of the character archive, one for each number from 1 to 39 that has held a
stored character. The character's `CHAR`, `SPST`, `PSST` and `PSIN` resources take the same number
(FMT-PARTY-001) [FND-PARTY-011, FND-SAVE-046]. The game and the transfer utility replace the
resource whole, by removing it and writing it again [FND-PARTY-011, FND-SAVE-046].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `character_id` | The identifier of the character stored under this number; 0 when the number is free. Not a `CHAR` resource number. `0x8015` to `0x8018` in the shipped resources that are not 0. | supported | FND-PARTY-011, FND-SAVE-046, FND-SAVE-001, FND-SAVE-002 |
| `0x02` | | | | Total size 2 | | |

The normal generation allocator starts at `0x8015` and selects the first
candidate below `0x8300` that does not match the four party identifiers or
the values supplied by its CACT 1–39 reads. It publishes the whole word to
the character's combatant identifier and details key. The allocator does not
check resource-read results; collision handling on failed reads remains
conditional on the reader's output behavior. Exhaustion executes none of
those identifier publication stores [FND-SAVE-045]. The stored-character
writer takes the combatant identifier as its two-byte CACT payload
[FND-SAVE-046]. This identifies the normal producer; it does not establish
all creation or transfer histories.

## Enumerations and flags

The normal allocator uses high-bit-set whole-word candidates [FND-SAVE-045].
When used by the object-reference selector, such a word is a signed-negative
key: the service searches active objects for a whole-word field-zero match.
A nonnegative input instead names a direct slot. Kind-two field zero is the
combatant's character identifier under the recorded getter conditions.
The companion key-normalization service leaves negative keys unchanged and
converts nonnegative slots through that getter. Bit 15 therefore selects the
key side of these reference interfaces; it is retained during equality,
not stripped to recover a slot. No party-membership or player-control meaning is assigned, and no exhaustive
claim about other consumers is made
[FND-SAVE-047].

## Differences between builds

None known. The disc's copy of `CHARSAVE.GFF` holds no `CACT` resource [FND-SAVE-001].

## Coverage

The eleven `CACT` resources, 29 to 39, of the installed `CHARSAVE.GFF`: seven hold 0 and four hold
an identifier [FND-SAVE-001].

The identified producer and reference interpretation do not constitute an
all-reader/writer complete reading. Actual getter metadata/width/record
admission and arbitrary transfer histories are not established by this format
entry [FND-SAVE-045, FND-SAVE-046, FND-SAVE-047].

## Open questions

None.
