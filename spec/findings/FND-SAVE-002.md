---
id: FND-SAVE-002
title: No 16-bit window of a GREQ or CACT resource holds the number of an installed character
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0xD6C..0x1B47
tool: DarkSunWakeRedux.Inspect resource-word-overlap
environment: null
---

## Observation

The installed `CHARSAVE.GFF` holds 19 `CHAR` resources (FND-PARTY-005). Every two-byte window of
each `GREQ` and `CACT` resource (FND-SAVE-001), read as a little-endian 16-bit value at every
byte offset, is compared with those 19 resource numbers: 8 windows in each of the ten 9-byte
`GREQ` resources and 1 in each of the eleven 2-byte `CACT` resources, 91 in all. None matches.

## Interpretation

Neither family names a character by its `CHAR` resource number. The `CACT` word is the
character's identifier, which is kept apart from its number (FND-PARTY-011).

## Alternatives

A character could still be named in another form, such as a single byte or a slot index.

## How to reproduce

List the `CHAR` resource numbers of `CHARSAVE.GFF`, then compare every two-byte window of each
`GREQ` and `CACT` resource with them.
