---
id: FND-ITEM-003
title: No second word of ITEMS.BIN occurs in the OJFF records, and few first words do
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: ITEMS.BIN
    offset: 0x00..0x3A8
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00..0x680304
tool: DarkSunWakeRedux.Inspect object-word-overlap, reproduced with Python 3.14.7
environment: null
---

## Observation

`OBJEX.GFF` holds 4,479 `OJFF` resources (FMT-ACTOR-001), each 16 bytes. The 16-bit words at
offsets 0, 6, 8 and 10 of those records take 4, 1,788, 1,361 and 8 different values. Compared
with the 234 pairs of `ITEMS.BIN` (FND-ITEM-001):

| `OJFF` word | First words found | Second words found |
|---|---|---|
| offset 0 | 0 | 0 |
| offset 6 | 5 | 0 |
| offset 8 | 4 | 0 |
| offset 10 | 0 | 0 |

## Interpretation

Neither column of `ITEMS.BIN` is a field of the `OJFF` records. The few first words found among
the many values of offsets 6 and 8 are few enough to be coincidences.

## Alternatives

A column could still refer to an `OJFF` record by its resource number rather than by one of its
fields (FND-ITEM-002).

## How to reproduce

Read every `OJFF` resource of `OBJEX.GFF` through its directory (FMT-GFF-001), collect the
values of the words at offsets 0, 6, 8 and 10, and count the words of each column of `ITEMS.BIN`
that occur among them.
