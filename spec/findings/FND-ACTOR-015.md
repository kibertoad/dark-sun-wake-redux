---
id: FND-ACTOR-015
title: Every RDFF resource of OBJEX.GFF is a chain of the CHAR load's chunks, which the object load walks with the same routine, and one has bytes after its end header
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x24274..0x66105E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:000A..2D40:03C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000701CA..0x0007042B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007042B..0x00070435
tool: tools/research/exec-census/rdff_chunks.py, resident_listing.py and overlay_listing.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

`OBJEX.GFF` is 6,816,516 bytes, XXH3-128 `e08d3cac153c547f6804ca27b155915d`; `DSUN.EXE` is the
installed one (634,416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`).

**The request.** The object load calls `2D40:000A` with the slot, the object number, the tag
`RDFF` and a byte (FND-ACTOR-003, FND-ACTOR-005). `2D40:000A` passes the tag and the number to
`38FF:05B5` and `38FF:04AB` (`2D40:0062..+00B3`) and from then on reads the bytes the same way
whatever the tag: from offset 0 it walks 10-byte headers, handling each by its byte 0 through
the four words at `2D40:03B9` and stepping on by 10 plus the header's word at 8, until a header
whose byte 0 is 0xFF (FND-PARTY-051, which reads this walk for `CHAR`). The listing of
`2D40:000A..2D40:03C1` shows no comparison with the tag.

**The kinds.** Overlay 187 `+039A` copies a chunk's data by the kind in the slot's entry at
`4F49:0C33` through the five words at `+05FB` (file `0x7042B`), `0x04F8`, `0x045E`, `0x054C`,
`0x05CE` and `0x05B0` for kinds 1 to 5. Kinds 1, 2 and 3 copy 23, 49 and 66 bytes as
FND-PARTY-051 gives. Kind 4 (`+05CE`) copies nothing. Kind 5 (`+05B0..+05CB`) copies 23 bytes into
the record of the returned number in the table of 23-byte records at the far pointer `DS:19D1`.

**The resources.** `rdff_chunks.py` walks each of the 1,643 `RDFF` resources the same way:

- 1,642 end with a header whose byte 0 is 0xFF and which is the resource's last 10 bytes.
  `RDFF/500` (145 bytes) has a type-1 chunk of 23 bytes and the 0xFF header at offset 33,
  followed by 102 bytes that hold the text `Brainsniffer` at offset 44 and, at offset 60, a
  header of type 3, kind 3, field 15 and 66 data bytes.
- The longest chain has 20 chunks, below the walk's limit of 80.
- Every chunk of type 2, 3 or 4 names in its byte 1 an earlier chunk of its resource (2,266).
- Byte 3 of every header is 0.

| Type | Kind (byte 2) | Data bytes | Field (word 6) | Chunks |
|---|---|---|---|---|
| 1 | 1 | 23 | 0 | 1,233 |
| 1 | 2 | 49 | 0 | 352 |
| 1 | 5 | 23 | 0 | 58 |
| 2 | 1 | 23 | 4 | 71 |
| 2 | 1 | 23 | 16 | 101 |
| 2 | 1 | 23 | 17 | 108 |
| 3 | 3 | 66 | 15 | 352 |
| 3 | 4 | 15 | 5 | 1,216 |
| 4 | 1 | 23 | 2 | 418 |

The chains, by their first chunk: 1,216 of a type-1 kind-1 chunk and a type-3 kind-4 chunk (the
68-byte resources); 4 of a type-1 kind-1 chunk alone; 13 of a type-1 kind-1 chunk followed by
chunks of type 2 or 4; 58 of a type-1 kind-5 chunk alone, `RDFF/500` among them; 219 of a
type-1 kind-2 chunk and a type-3 kind-3 chunk; and 133 of those two followed by chunks of
type 2 or 4.

## Interpretation

An `RDFF` resource is a chain of FMT-PARTY-006 chunks. Its first chunk is the object itself: a
23-byte kind-1 record (an item), a 23-byte kind-5 record, or a 49-byte kind-2 combatant record of
FMT-COMBAT-001 with its 66-byte FMT-COMBAT-002 details as a type-3 chunk, followed by the items
it carries as type-2 and type-4 chunks. The second chunk of a 68-byte resource has kind 4, whose
15 data bytes are not copied; the load stores its word at 4 as the record number instead
(FND-PARTY-051). The label at offset 43 of the 23 resources of FND-ACTOR-007 is at offset 0x21 of
the combatant record, its `name`. The 102 bytes after `RDFF/500`'s end header are never read; they
look like the rest of an older chain of the 145-byte shape.

## Alternatives

- That every `RDFF` request goes through `2D40:000A` rests on the three callers of
  FND-ACTOR-003 and FND-ACTOR-005; a caller that reads `RDFF` some other way was not searched for.
- What the table at `DS:19D1` holds, and what kind-5 objects are, was not read.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python: `rdff_chunks.py` with the
installed `OBJEX.GFF`; `resident_listing.py <dsun> 2D40:000A..2D40:0400`; `overlay_listing.py
<dsun> 187 6FE30 704B0` and `187 701CA 70435`; read the five words at `DSUN.EXE` offset
`0x7042B` and the bytes of `RDFF/500`. The outputs are retained under
`GAME_DIR/analysis/actor-rdff-20261019/`. No original program was run.
