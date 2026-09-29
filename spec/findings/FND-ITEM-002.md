---
id: FND-ITEM-002
title: Both columns of ITEMS.BIN overlap the resource numbers of several OBJEX.GFF tags
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: ITEMS.BIN
    offset: 0x00..0x3A7
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00..0x680303
tool: DarkSunWakeRedux.Inspect pair-resource-overlap, reproduced with Python 3.14.7
environment: null
---

## Observation

For each of four tags of `OBJEX.GFF`, the number of the 234 pairs of `ITEMS.BIN` (FND-ITEM-001)
whose first word, second word, either word or both words is the number of a resource of that tag
(FMT-GFF-002):

| Tag | First word | Second word | Either | Both |
|---|---|---|---|---|
| `OJFF` | 49 | 212 | 219 | 42 |
| `RDFF` | 50 | 216 | 223 | 43 |
| `SCMD` | 22 | 20 | 41 | 1 |
| `BMP ` | 157 | 165 | 198 | 124 |

The second words that are not `RDFF` numbers take 15 different values; 900 is an `RDFF` number.

## Interpretation

The resource numbers of these tags share ranges, so membership in one set does not show which
kind of resource a column names. The second column is closest to the `RDFF` numbers, which fits
the transfer utility's use of it (FND-ITEM-006).

## Alternatives

Membership counts are circumstantial: they cannot tell a reference from a coincidence of
numbering. The 18 pairs whose second word is not an `RDFF` number are not explained.

## How to reproduce

List the resource numbers of each tag of `OBJEX.GFF` through its directory (FMT-GFF-001), and
count, for each column of `ITEMS.BIN`, the pairs whose word is in each set.
