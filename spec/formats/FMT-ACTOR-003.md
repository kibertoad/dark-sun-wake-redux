---
id: FMT-ACTOR-003
title: MONR resource
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RESOURCE.GFF"]
byte_order: little
size: 1134
text: false
definition: fmt_actor_003.ksy
evidence: [FND-ACTOR-014]
conflicting: []
split_with: []
related: []
---

## Layout

The one `MONR` resource of `RESOURCE.GFF`, number 1, 1,134 bytes. Script request 8 reads it as
records of 42 bytes, as many as the resource's size divided by 42; the shipped resource holds 27
with nothing left over (FND-ACTOR-014).

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x000` | `42 * n` | `RECORD[n]` | `records` | The records, `n` the resource's size divided by 42; 27 in the shipped resource. | supported | FND-ACTOR-014 |
| `0x46E` | | | | Total size 1,134 bytes | | |

One record:

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `UINT16LE` | `key` | Compared with `current_region`, or with 255 when the request's second byte is not 0; the request uses only records whose key is equal. | supported | FND-ACTOR-014 |
| `0x02` | 40 | `PAIR[10]` | `pairs` | Ten pairs, laid out below. | supported | FND-ACTOR-014 |
| `0x2A` | | | | Total size 42 | | |

One pair, at record offset `2 + 4 * n` for `n` from 0 to 9:

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 2 | `INT16LE` | `value` | The word the request stores at `549B:00C0` when it picks the pair, and compares with that word to find the pair again; a pair whose `value` is 0 is never picked. | supported | FND-ACTOR-014 |
| `0x02` | 2 | `INT16LE` | `rating` | Compared with the party's score to pick a pair, and the divisor of twice the score for the pair after the one found. | supported | FND-ACTOR-014 |
| `0x04` | | | | Total size 4 | | |

In the shipped resource each key occurs once, 255 does not occur, the non-empty pairs come first
and number 7 to 9, and pair 9 is always 0 and 0 (FND-ACTOR-014).

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

The 27 records of `RESOURCE.GFF#MONR/1`, read as the request's code reads them (FND-ACTOR-014).

## Open questions

- What `value` names, what `549B:00C0` holds and which other code reads it, and whether
  request 8 runs in play: no shipped script gives it the literal request 8, and the one computed
  request number, at `GPL/27` `0x01C8`, was not traced (Q-ACTOR-006).
