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
evidence: [FND-PARTY-011, FND-SAVE-046, FND-SAVE-001, FND-SAVE-002, FND-SAVE-045]
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

No independent flag interpretation of bit 15 is established. The normal
allocator uses high-bit-set whole-word candidates [FND-SAVE-045].

## Differences between builds

None known. The disc's copy of `CHARSAVE.GFF` holds no `CACT` resource [FND-SAVE-001].

## Coverage

The eleven `CACT` resources, 29 to 39, of the installed `CHARSAVE.GFF`: seven hold 0 and four hold
an identifier [FND-SAVE-001].

## Open questions

- Q-SAVE-002: the normal producer is identified by FND-SAVE-045 and its
  persistence input by FND-SAVE-046. Determine whether bit 15 has separate
  meaning in identifier consumers, or merely belongs to an opaque identifier
  namespace. The allocator's high-bit-set range and full-word equality tests
  support the latter locally but do not rule out a separate interpretation
  elsewhere. Identify actual identifier consumers and relevant writers,
  including transfer inputs, and inspect their bit tests, conversions and
  comparisons. An observed bit-dependent consumer would disprove the purely
  opaque interpretation; a complete relevant consumer audit is needed to
  exclude one. Preserve computed and indirect accesses not covered by a
  literal search. Generation UI admission belongs to Q-PARTY-130; unknown
  effects on this identifier contract remain obligations here.
