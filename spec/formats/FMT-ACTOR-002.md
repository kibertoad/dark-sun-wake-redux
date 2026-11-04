---
id: FMT-ACTOR-002
title: Object data resource
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["OBJEX.GFF"]
byte_order: little
size: null
text: false
definition: fmt_actor_002.ksy
evidence: [FND-ACTOR-015, FND-ACTOR-016, FND-ACTOR-017, FND-ACTOR-018, FND-PARTY-259, FND-ACTOR-003, FND-ACTOR-005, FND-ACTOR-007]
conflicting: []
split_with: []
related: []
---

## Layout

An `RDFF` resource of `OBJEX.GFF`, 1,643 of them, numbered as the objects outside 9,000 to 13,998
(FND-ACTOR-007). The object load reads it with the routine that reads a `CHAR` record, as a chain
of FMT-PARTY-006 chunks from offset 0 up to and including the first chunk whose `chunk_type` is
0xFF (FND-ACTOR-015, FND-PARTY-259). `size` is the resource's size from the GFF directory, and
`m` the offset of the end header.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | `m + 10` | FMT-PARTY-006 chunks | `chunks` | The chain, ending with a 10-byte header whose `chunk_type` is 0xFF; 1 to 20 chunks before it in the shipped resources. | supported | FND-ACTOR-015, FND-PARTY-259 |
| `m + 10` | `size - m - 10` | `BYTE[]` | `unread` | Not read. Empty in every shipped resource but `RDFF/500`, which has 102 bytes here. | supported | FND-ACTOR-015 |
| | | | | Total size `size` | | |

The chunks in the shipped resources (FND-ACTOR-015):

- `chunk_type` 1, `kind` 1, 23 data bytes, `field` 0 (1,233 chunks): the object, a record of
  the table at `57E0:19C1`, as for FMT-PARTY-006's types 2 and 4.
- `chunk_type` 1, `kind` 2, 49 bytes, `field` 0 (352): the object, an FMT-COMBAT-001 combatant
  record.
- `chunk_type` 1, `kind` 5, 23 bytes, `field` 0 (58): the object, a record of the table at the
  far pointer `57E0:19D1`.
- `chunk_type` 2, `kind` 1, 23 bytes, `field` 4, 16 or 17 (71, 101 and 108): an item attached to
  the object of chunk `target_chunk` through `field`.
- `chunk_type` 3, `kind` 3, 66 bytes, `field` 15 (352): the combatant's FMT-COMBAT-002 details.
- `chunk_type` 3, `kind` 4, 15 bytes, `field` 5 (1,216): not copied; its `record_ref` becomes the
  kind-4 record number.
- `chunk_type` 4, `kind` 1, 23 bytes, `field` 2 (418): an item chained after the item of chunk
  `target_chunk`.

Every resource starts with a type-1 chunk. A kind-1 first chunk is followed by one type-3 kind-4
chunk (1,216 resources, 68 bytes each), by nothing (4) or by chunks of type 2 or 4 (13). A kind-2
first chunk is always followed by a type-3 kind-3 chunk, and in 133 resources by chunks of type 2
or 4. A kind-5 first chunk is alone. In every type-2, 3 and 4 chunk `target_chunk` is an earlier
chunk, and `unk_03` is 0 in every header (FND-ACTOR-015).

In all 58 shipped kind-five first chunks, the signed word at data offset zero
equals the negated full RDFF resource identifier. This is field zero under
the shipped FNFO mappings. The native copy offers all 23 payload bytes to
the indexed table through DS:19D1; its conditional getter base is DS:6163.
The local record selector searches twenty logical records and treats a zero
first word as free; the known deletion branch clears that word. Allocation,
input validity and later preservation remain conditional (FND-ACTOR-016).

For the known kind-four import path, record_ref is retained as a shared type
index and assigned to the target's field five; the fifteen payload bytes are
skipped. The native shared table is ALL/4, whose complete field schema is
FMT-ITEM-002. All 1216 known shipped inline payloads exactly match the
shared row named by their retained reference. Nine also match additional
identical shared rows; the actual reference remains distinct from byte
equality (FND-ACTOR-017, FND-ACTOR-018).

## Enumerations and flags

`chunk_type` and `kind` are those of FMT-PARTY-006.

## Differences between builds

None known.

## Coverage

All 1,643 shipped resources walk to an end header; 1,642 end there (FND-ACTOR-015).

All 1216 known 68-byte inline kind-four records have in-range references and
indexed byte equality with the 102-row shared table. Other-size resources
remain outside that comparison model (FND-ACTOR-018).

## Open questions

- Whether another native DSUN.EXE path consumes the inline kind-four bytes
  (Q-ACTOR-008). The known importer skips them (FND-ACTOR-015/017); the
  complete source equality in FND-ACTOR-018 does not establish reader absence.
  One reading limits use to the known importer; another allows an unread
  consumer through a different resource path. No positive additional consumer
  is recorded; a controlled mechanism/consumer census must distinguish them.
- The fields of the 23-byte kind-1 and kind-5 records, beyond those FMT-PARTY-006 gives for
  kind 1 and FND-ACTOR-016 gives for kind five's first word (Q-ACTOR-003).
- Which admitted metadata, record and pointer states reach the trigger getter
  uses of kind-five field zero (Q-SCRIPT-022).
