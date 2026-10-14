---
id: FMT-PARTY-006
title: Character record chunk
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["CHARSAVE.GFF", "CD:CHARSAVE.GFF"]
byte_order: little
size: null
text: false
definition: fmt_party_006.ksy
evidence: [FND-PARTY-051, FND-PARTY-052, FND-PARTY-054, FND-PARTY-181, FND-PARTY-182, FND-PARTY-196, FND-PARTY-216, FND-PARTY-217]
conflicting: []
split_with: []
related: []
---

## Layout

One link of the chain an FMT-PARTY-001 character record is made of: a 10-byte header and the
data its `len_data` gives. The load reads chunks one after the other until one whose `chunk_type`
is 0xFF, numbering them from 0 in that order, and handles each by its `chunk_type`
[FND-PARTY-051]. The game writes the chunks depth first from the character, giving each the
index of its parent chunk, its record number and the field it was reached through [FND-PARTY-054].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 1 | `UINT8` | `chunk_type` | How the load handles the chunk: 1 to 4 (see below); 0xFF ends the record; any other value makes the load fail with -2. | supported | FND-PARTY-051 |
| `0x01` | 1 | `UINT8` | `target_chunk` | For types 2, 3 and 4, the number of the earlier chunk whose object the chunk attaches to. The load does not read it for type 1; in the shipped records the first chunk and the end header hold the number of chunks there. | supported | FND-PARTY-051, FND-PARTY-052 |
| `0x02` | 1 | `UINT8` | `kind` | The kind of record the data is: 2 for type 1, 3 for type 3, 1 for types 2 and 4 in every shipped record. Overlay 187 copies the data into the table of that kind. | supported | FND-PARTY-051, FND-PARTY-052 |
| `0x03` | 1 | `UINT8` | `unk_03` | Purpose unknown. 0 in every shipped record; a type-3 chunk is copied only when the word at `0x02` is 3. | supported | FND-PARTY-051, FND-PARTY-052 |
| `0x04` | 2 | `UINT16LE` | `record_ref` | Types 2, 3 and 4: replaced, in the loaded copy, by the number of the record the data was copied into, which a type-3 chunk then stores in its target's field and a type-4 chunk links in. The load does not read it for type 1. | supported | FND-PARTY-051 |
| `0x06` | 2 | `UINT16LE` | `field` | Types 2 and 3: the number of the field of the target chunk's object that receives the new record, through the `FNFO` field tables. 15, 16, 17 or 4 in the shipped records' chunks of these types, and 2 in their type-4 chunks, for which the load does not read it. | supported | FND-PARTY-051, FND-PARTY-052 |
| `0x08` | 2 | `UINT16LE` | `len_data` | Number of data bytes after the header: 49 for type 1, 66 for type 3, 23 for types 2 and 4, and 0 in the end header, in every shipped record. | supported | FND-PARTY-051, FND-PARTY-052 |
| `0x0A` | `len_data` | `BYTE[len_data]` | `data` | The record the chunk holds (see below). For types 2 and 4, a 23-byte record of the table at `57E0:19C1`: its signed word at `0x00` is an object number, negated, which placing the record's object passes on as the number of the `OJFF` and `RDFF` resources to load, and which is negative and the negation of an `OBJEX.GFF` `RDFF` number in every shipped record; its byte at `0x12` gives bits 0 to 2 of the placed object's entry. In the shipped records its word at `0x04` is 9999 in each record that ends its chain and another value in each record a type-4 chunk follows, its word at `0x08` is 9999 in all but three, and its byte at `0x15` is 0. | supported | FND-PARTY-051, FND-PARTY-181, FND-PARTY-182, FND-PARTY-196 |
| | | | | Total size `10 + len_data` | | |

For a 23-byte kind-1 attached record, the signed little-endian word at
`0x02` is `quantity` under the shipped field descriptors. The placement
reader treats a nonpositive value as one; a partial split writes the moved
amount on the new object and the remainder on the source object. This
does not restrict the stored word to positive values or specify other
quantity consumers [FND-PARTY-216].

Byte `0x13` is `inventory_position`, a position code interpreted as a
signed byte and increased by four to select a logical inventory-map word.
The main inventory draw loop consumes positions 4 through 29; other
branches use positions 42 through 47. A byte of `0xFF` maps to position 3,
outside that main draw loop. Physical slot labels and complete position
producer/consumer behavior remain unassigned [FND-PARTY-217].

## Enumerations and flags

### `chunk_type`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 1 | `CHUNK_COMBATANT` | Copies 49 bytes into the slot's FMT-COMBAT-001 record. | supported | FND-PARTY-051 |
| 2 | `CHUNK_ATTACHED_RECORD` | Copies 23 bytes into a new record of the table at the far pointer `57E0:19C1` and stores its handle in the field `field` names of the object of `target_chunk`. | supported | FND-PARTY-051 |
| 3 | `CHUNK_DETAILS` | Copies 66 bytes into an FMT-COMBAT-002 record, for a slot of 4 or less the slot's own, and stores that record's number in the field `field` names of the object of `target_chunk`. | supported | FND-PARTY-051 |
| 4 | `CHUNK_CHAINED_RECORD` | Copies 23 bytes into a new record of the table at `57E0:19C1` and links it after the record of `target_chunk` through the records' words at `+0x04`. | supported | FND-PARTY-051 |
| 0xFF | `CHUNK_END` | Ends the record. | supported | FND-PARTY-051 |

The fields the shipped records name are, by the `FNFO` 1 resource of `OBJEX.GFF`: field 15 of a
kind-2 object is FMT-COMBAT-001's `details_index` at `0x04`; fields 16, 17 and 4 of a kind-2 object
are its words at `0x08`, `0x0A` and `0x0C`; field 4 of a kind-1 record is its word at `0x08`
[FND-PARTY-052].

## Differences between builds

None known.

## Coverage

The chunks of all 19 `CHAR` resources of the installed `CHARSAVE.GFF`: each walk from offset 0
ends on a `chunk_type` of 0xFF whose header is the last 10 bytes of the resource [FND-PARTY-052].

## Open questions

- What the other fields of a 23-byte record of chunk types 2 and 4 mean, apart from
  `0x00`, quantity at `0x02`, the chain link at `0x04`, byte `0x12`, and
  inventory position at `0x13`.
  FND-PARTY-196 gives the shipped values (Q-PARTY-020).
- What the character's words at `0x08`, `0x0A` and `0x0C` distinguish when the
  possessions and equipment screens traverse them (Q-PARTY-082).
- What a 23-byte record's handle at `0x08` refers to and how its referenced
  records are traversed; it is 9999 in all but three shipped records
  (FND-PARTY-052, FND-PARTY-196, Q-PARTY-083).
- Complete quantity producer/consumer coverage and runtime field/table binding
  stability, beyond the placement use FND-PARTY-216 reads (Q-PARTY-084).
- Physical position labels, all position writers and the projection's complete
  graph/parent/mode/capacity contracts (FND-PARTY-217, Q-PARTY-085).
