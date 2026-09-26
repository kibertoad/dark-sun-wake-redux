# ACTOR

Next ID: Q-ACTOR-006

## Static

- Q-ACTOR-001. FMT-ACTOR-002, FMT-ACTOR-001: Which record supplies the first hostile Look
  panel's name, level and available actions? Settles it: the code that fills the Look panel from
  an actor record. Tried: the `RDFF` records that hold the captured label, the hostile `OJFF`
  record's words, and the known `RDFF` and `OJFF` lookup paths; object 9,258 takes no `RDFF`
  request and has no `RDFF` resource (FND-ACTOR-009). Blocks: slice 3.
- Q-ACTOR-002. FMT-ACTOR-001: What do `unk_00`, `unk_06`, `unk_08` and `unk_0B` do, and what do
  the image numbers `31E0:0EFF` puts in place of `image` show? Settles it: a reading of the code
  that uses the 37-byte slot records at `DS:67BB`, fields `0x0`, `0x7`, `0xF` and `0x19`, and of
  the overlay code of overlays 190, 197 and 213 around the `OJFF` tag bytes. Blocks: nothing yet.
- Q-ACTOR-003. FMT-ACTOR-002: What is the layout of an `RDFF` resource? Settles it: a reading of
  `2D40:000A`, which receives the request, and of the overlay code of overlays 178, 188, 191 and
  201 around the `RDFF` tag bytes. Blocks: nothing yet.
- Q-ACTOR-004. FMT-ACTOR-003: Which code reads `MONR`, and what is its layout? Settles it: a
  reading of the code of overlay 204 around the `MONR` tag bytes. Blocks: nothing yet.

## Emulated call

None.

## Agent run

None.

## Live session

- Q-ACTOR-005. RULE-ACTOR-001: Does the game mirror an object whose entity has
  `unk_flags_bit_7` set, and does it show frames other than the first for placed objects?
  Settles it: a capture of a view that holds an entity with bit 7 set, compared as in
  FND-IMAGE-010. Blocks: nothing yet.

## Source

None.

## Blocked

None.
