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
evidence: [FND-PARTY-006, FND-PARTY-011, FND-PARTY-012, FND-PARTY-104, FND-PARTY-210, FND-PARTY-211, FND-MAGIC-007, FND-MAGIC-008, FND-MAGIC-014]
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
| `0x00` | 34 | `BYTE[34]` | `power_ranks` | One byte for each psionic power 0 to 33: bits 1 to 7 the power's rank, 0 while it is unknown, 1 when learned and 1 more for each enhancement up to 30; bit 0 is set while the power, used successfully, stays in effect, and ending the power clears it; every power in effect ends at the end of a fight and of a rest. Power 0 is never offered by the level-gain learning window. Bytes of 0, 2, 3, 4 or 6 in the shipped resources, mostly 0 and 2. | supported | FND-PARTY-006, FND-PARTY-011, FND-PARTY-012, FND-PARTY-104, FND-PARTY-210, FND-PARTY-211 |
| `0x22` | | | | Total size 34 | | |

The power table's third byte supplies the renewal cost and also gates
marking: 99 suppresses marking after a successful party use. The renewal
path still uses that byte for an already marked power, including 99.
It compares the byte with the member's
points, charges it when it renews the power, and ends a power it cannot
maintain. Zero-cost powers can remain marked without a maintenance spend
(FND-MAGIC-007, FND-MAGIC-008). The separate walker clearer removes active
bits for positive-cost powers and leaves the zero-cost cases; its selected
calls follow the signed clock condition recorded by FND-MAGIC-014. These
findings settle the byte's local role, not universal clock/source stability.

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The 19 `PSST` resources of the installed `CHARSAVE.GFF` and the 8 of the disc's copy: every one
is 34 bytes [FND-PARTY-006].

## Open questions

- The caller and current-member/power-index context of overlay 202
  +0C82's active-bit clear (Q-PARTY-005), beyond FND-PARTY-210's local
  write. The older question about the maintenance-table byte and local
  walker gate is answered by FND-MAGIC-007/008/014; retain their stated
  source/alias limitations rather than treating the clock reading as a
  universal timing guarantee. FND-PARTY-211 already supplies fight/rest
  clearing context. Resource-operation success and unrelated window
  behavior are not added as format blockers.
