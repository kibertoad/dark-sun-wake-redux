---
id: FMT-ACTOR-002
title: Object data resource
status: unknown
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["OBJEX.GFF"]
byte_order: little
size: null
text: false
definition: null
evidence: []
conflicting: []
split_with: []
related: []
---

## Layout

An `RDFF` resource of `OBJEX.GFF`. Nothing is claimed about its layout yet. `size` is the
resource's size from the GFF directory.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | `size` | `BYTE[size]` | `unk_00` | Purpose unknown. | unknown | None |
| | | | | Total size `size` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

None.

## Open questions

- What the resource holds. The game requests `RDFF` by the number of an object outside 9,000 to
  13,998 when it loads the object into a slot, and again later for an object already in a slot,
  passing bits 0 to 2 of the object's entity flags with it (FND-ACTOR-003, FND-ACTOR-005). Every
  `OJFF` number outside that range has an `RDFF` resource of the same number (FND-ACTOR-007). (Q-ACTOR-003)

- Its layout. The sizes fall into a 68-byte group and two families of `43 + 33 * n` and
  `145 + 33 * n` bytes, and the label at `OBJEX.GFF#ALL/2` offset 3,611 sits at offset 43 of 23
  resources of the second family (FND-ACTOR-007). These are patterns in the data; the routine
  `2D40:000A` that receives the request has not been read, and overlay code in overlays 178, 188,
  191 and 201 names the tag (FND-ACTOR-006). No decoded resident instruction uses the
  displacement 43 or 76 (FND-ACTOR-010), which weighs against, without ruling out, a resident
  field read at the label's offset. (Q-ACTOR-003)

- Whether `RDFF` data feeds the 13-byte records that the routines of segment `1695` walk. The
  routines that request it use only the 37-byte slot records (FND-ACTOR-011). (Q-ACTOR-003)
