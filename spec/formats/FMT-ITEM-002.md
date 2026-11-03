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

The installed generic descriptor mapping is known (FND-ITEM-011). Selectors
87 through 97 address offsets zero through ten respectively. Selector 25
addresses byte eleven, selector 40 the little-endian word at twelve, and
selector 56 byte fourteen. Metadata is one for these byte fields except
offsets ten and fourteen, whose metadata is zero; the word's metadata is
three. With the documented width setup these fields cover all fifteen bytes.
This structural mapping supplies no unexamined semantic names or universal
signedness. Direct consumers can address fields independently of the getter.

## Enumerations and flags

Not yet specified as a complete record contract.

## Differences between builds

None known.

## Coverage

The installed OBJEX.GFF ALL/4 directory span and conditional native base/stride
binding (FND-ACTOR-017). Its full field schema has not been read.

The 1216 known shipped inline kind-four payloads duplicate their indexed
shared rows, with duplicate-row ambiguity retained (FND-ACTOR-018). This
source relationship does not supply unexamined field meanings.

FND-ITEM-011 reads the complete 98-selector descriptor row and all 102
shared records under that structural partition. Semantic reader/writer
coverage remains incomplete.

## Open questions

- The field widths, meanings, flag domains and native transformations of
  these records, including the index/field relationships used by item
  consumers (Q-ITEM-005). Isolated predicate observations above do not
  establish unexamined bytes or a complete reader.
