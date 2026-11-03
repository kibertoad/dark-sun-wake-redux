---
id: FMT-ITEM-002
title: Shared item-type record in ALL/4
status: unknown
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["OBJEX.GFF"]
byte_order: little
size: 15
text: false
definition: null
evidence: []
conflicting: []
split_with: []
related: []
---

## Layout

The initializer offers ALL/4 as the shared table at DS:19CD and publishes
its pointer as the kind-four generic-accessor base. Native item-type consumers
use a fifteen-byte stride. The installed resource contains 1530 bytes, or
102 such logical records (FND-ACTOR-017).

The complete field layout remains unknown. FND-PARTY-248/252 record
individual consumers and flag tests, which have not been assembled into
a complete field/width/meaning contract here. No field decoder is specified.

## Enumerations and flags

Not yet specified as a complete record contract.

## Differences between builds

None known.

## Coverage

The installed OBJEX.GFF ALL/4 directory span and conditional native base/stride
binding (FND-ACTOR-017). Its full field schema has not been read.

## Open questions

- The field widths, meanings, flag domains and native transformations of
  these records, including the index/field relationships used by item
  consumers (Q-ITEM-005). Isolated predicate observations above do not
  establish unexamined bytes or a complete reader.
